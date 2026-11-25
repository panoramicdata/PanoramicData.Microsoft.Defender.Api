using Microsoft.Extensions.Logging;
using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public partial class RetryHandlerTests
{
	/// <summary>A transport that fails the first <paramref name="failures"/> sends with <paramref name="error"/>, then answers 200.</summary>
	private static DelegateHandler FailingThenOk(int failures, HttpRequestError error)
	{
		var sends = 0;
		return new DelegateHandler((_, _) => Interlocked.Increment(ref sends) <= failures
			? throw new HttpRequestException(error, "connection failed")
			: Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
	}

	/// <summary>A transport that never answers: it waits until its cancellation token fires.</summary>
	private static DelegateHandler Hanging()
		=> new(async (_, token) =>
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, token);
			throw new InvalidOperationException("unreachable");
		});

	[Theory]
	[InlineData(HttpRequestError.ConnectionError)]
	[InlineData(HttpRequestError.SecureConnectionError)]
	[InlineData(HttpRequestError.NameResolutionError)]
	public async Task ConnectionFailure_IsRetried_ForAnyVerb_WithBackoff(HttpRequestError error)
	{
		var transport = FailingThenOk(2, error);
		using var harness = new Harness(inner: transport);

		using var response = await harness.SendAsync(HttpMethod.Post, new StringContent("{}"));

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		transport.Requests.Should().HaveCount(3);
		harness.Delays.Should().Equal(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
		harness.Logger.Messages.Should().Contain(m => m.Contains($"connection failed ({error})", StringComparison.Ordinal) && !m.Contains("secret", StringComparison.Ordinal));
	}

	[Fact]
	public async Task ConnectionFailure_IsRetried_WithoutALogger()
	{
		var transport = FailingThenOk(1, HttpRequestError.ConnectionError);
		using var harness = new Harness(o => o.Logger = null, transport);

		using var response = await harness.SendAsync(HttpMethod.Get);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		transport.Requests.Should().HaveCount(2);
	}

	[Fact]
	public async Task ConnectionFailure_BeyondMaxRetries_IsThrown()
	{
		var transport = FailingThenOk(10, HttpRequestError.SecureConnectionError);
		using var harness = new Harness(o => o.MaxRetries = 2, transport);

		var act = () => harness.SendAsync(HttpMethod.Get);

		(await act.Should().ThrowAsync<HttpRequestException>()).Which.HttpRequestError.Should().Be(HttpRequestError.SecureConnectionError);
		transport.Requests.Should().HaveCount(3);
	}

	[Fact]
	public async Task ConnectionFailure_WithStreamBody_IsNotRetried()
	{
		var transport = FailingThenOk(1, HttpRequestError.ConnectionError);
		using var harness = new Harness(inner: transport);

		var act = () => harness.SendAsync(HttpMethod.Post, new StreamContent(new MemoryStream([1, 2, 3])));

		await act.Should().ThrowAsync<HttpRequestException>();
		transport.Requests.Should().ContainSingle();
	}

	[Theory]
	[InlineData(HttpRequestError.ResponseEnded)]
	[InlineData(HttpRequestError.Unknown)]
	public async Task FailureAfterSending_IsNotRetried(HttpRequestError error)
	{
		var transport = FailingThenOk(1, error);
		using var harness = new Harness(inner: transport);

		var act = () => harness.SendAsync(HttpMethod.Get);

		await act.Should().ThrowAsync<HttpRequestException>();
		transport.Requests.Should().ContainSingle();
		RetryHandler.IsConnectionFailure(new HttpRequestException(error)).Should().BeFalse();
	}

	[Fact]
	public async Task AttemptTimeout_RaisesTimeoutException_WhenTheInjectedClockPassesIt()
	{
		using var harness = new Harness(o => o.Timeout = TimeSpan.FromSeconds(10), Hanging());

		var sending = harness.SendAsync(HttpMethod.Get);
		harness.Clock.Advance(TimeSpan.FromSeconds(10));

		var thrown = await ((Func<Task>)(() => sending)).Should().ThrowAsync<TimeoutException>();
		thrown.Which.Message.Should().Be("Defender did not respond within 00:00:10.");
		thrown.Which.InnerException.Should().BeAssignableTo<OperationCanceledException>();
	}

	[Fact]
	public async Task CallerCancellation_RaisesOperationCanceled()
	{
		using var cts = new CancellationTokenSource();
		using var invoker = new HttpMessageInvoker(new RetryHandler(new DefenderClientOptions()) { InnerHandler = Hanging() });

		var sending = invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, Url), cts.Token);
		await cts.CancelAsync();

		await ((Func<Task>)(() => sending)).Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task CallerCancellation_DuringBackOff_RaisesOperationCanceled()
	{
		using var cts = new CancellationTokenSource();
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		var handler = new RetryHandler(new DefenderClientOptions { MaxRetries = 1 })
		{
			InnerHandler = stub,
			Delay = (_, token) =>
			{
				cts.Cancel();
				return Task.Delay(Timeout.InfiniteTimeSpan, token);
			}
		};
		using var invoker = new HttpMessageInvoker(handler);

		var act = () => invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, Url), cts.Token);

		await act.Should().ThrowAsync<OperationCanceledException>();
		stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task DefaultDelay_WaitsOnTheInjectedClock()
	{
		var clock = TestClient.NewClock();
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		stub.Enqueue(HttpStatusCode.OK);
		using var client = new DefenderClient(
			TestClient.Options(clock, o => (o.MaxRetries, o.RetryBaseDelay, o.Timeout) = (1, TimeSpan.FromHours(1), TimeSpan.FromDays(2))),
			stub);

		var sending = client.Vulnerabilities.ListAsync(null, TestContext.Current.CancellationToken);
		while (!sending.IsCompleted)
		{
			// The back-off timer may not exist yet, so keep moving the clock past it until the retry has happened.
			clock.Advance(TimeSpan.FromHours(1));
			await Task.Yield();
		}

		(await sending).Value.Should().BeEmpty();
		stub.Calls.Should().HaveCount(2);
	}

	[Fact]
	public async Task Logging_NamesMethodPathAndAttempt_ButNeverTheQuery()
	{
		using var harness = new Harness().Respond(HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Get);

		harness.Logger.Entries.Should().Equal(
			(LogLevel.Debug, "Defender GET https://defender.test/api/vulnerabilities (attempt 1)"),
			(LogLevel.Warning, "Defender returned 503 for GET https://defender.test/api/vulnerabilities; retrying in 00:00:01"),
			(LogLevel.Debug, "Defender GET https://defender.test/api/vulnerabilities (attempt 2)"));
		harness.Logger.Messages.Should().NotContain(m => m.Contains("secret", StringComparison.Ordinal) || m.Contains('?', StringComparison.Ordinal));
	}

	[Fact]
	public async Task WithoutALogger_StillRetries()
	{
		using var harness = new Harness(o => o.Logger = null).Respond(HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Get);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Logger.Entries.Should().BeEmpty();
	}
}
