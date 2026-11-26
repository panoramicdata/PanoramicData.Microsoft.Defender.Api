using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;
using System.Net.Http.Headers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public class ErrorMapperTests
{
	private static readonly DateTimeOffset Now = new(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);

	/// <summary>Bounds the wait of a test that would otherwise hang when the code under test is broken; never reached when it passes.</summary>
	private static readonly TimeSpan FailSafe = TimeSpan.FromSeconds(20);

	private static async Task<DefenderApiException> MapAsync(HttpStatusCode status, HttpContent content, string? reason = null, Action<HttpResponseMessage>? configure = null)
	{
		using var response = new HttpResponseMessage(status) { Content = content, ReasonPhrase = reason };
		configure?.Invoke(response);
		var exception = await DefenderErrorMapper.CreateAsync(response, TestClient.NewClock(), TestContext.Current.CancellationToken);
		return exception.Should().BeOfType<DefenderApiException>().Subject;
	}

	private static Task<DefenderApiException> MapAsync(string body, HttpStatusCode status = HttpStatusCode.BadRequest)
		=> MapAsync(status, new StringContent(body));

	[Fact]
	public async Task Success_IsNotAnError()
	{
		using var response = new HttpResponseMessage(HttpStatusCode.NoContent);

		(await DefenderErrorMapper.CreateAsync(response, TimeProvider.System, TestContext.Current.CancellationToken)).Should().BeNull();
	}

	[Fact]
	public async Task ErrorObject_ProvidesCodeMessageAndCorrelationId()
	{
		var exception = await MapAsync(
			"""{"error":{"code":"Forbidden","message":"Missing permission","target":"3a1f-77"}}""",
			HttpStatusCode.Forbidden);

		exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		exception.Code.Should().Be("Forbidden");
		exception.Message.Should().Be("Missing permission");
		exception.CorrelationId.Should().Be("3a1f-77");
		exception.RetryAfter.Should().BeNull();
	}

	[Fact]
	public async Task ErrorText_IsTheMessage()
	{
		var exception = await MapAsync("""{"error":"plain text"}""");

		exception.Message.Should().Be("plain text");
		exception.Code.Should().BeNull();
	}

	[Fact]
	public async Task PartialErrorObject_KeepsWhatIsThere()
	{
		var exception = await MapAsync("""{"error":{"code":"ODataError","message":7}}""");

		exception.Code.Should().Be("ODataError");
		exception.Message.Should().Be("HTTP 400 (Bad Request)");
		exception.CorrelationId.Should().BeNull();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("Internal error")]
	[InlineData("[1,2]")]
	[InlineData("{not json")]
	[InlineData("{}")]
	[InlineData("""{"error":null}""")]
	[InlineData("""{"error":[]}""")]
	[InlineData("<html><body>Bad gateway<br></body></html>")]
	public async Task NoReadableError_FallsBackToTheStatus(string body)
	{
		var exception = await MapAsync(body, HttpStatusCode.BadGateway);

		exception.Message.Should().Be("HTTP 502 (Bad Gateway)");
		exception.Code.Should().BeNull();
		exception.CorrelationId.Should().BeNull();
	}

	[Theory]
	[InlineData(null, "HTTP 599 (599)")]
	[InlineData("Custom", "HTTP 599 (Custom)")]
	public async Task Fallback_NamesTheReasonPhraseOrStatus(string? reason, string expected)
		=> (await MapAsync((HttpStatusCode)599, new StringContent(""), reason)).Message.Should().Be(expected);

	[Fact]
	public async Task RetryAfter_IsReadAsSeconds_OrAsADate()
	{
		var seconds = await MapAsync(HttpStatusCode.TooManyRequests, new StringContent(""), configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(42)));
		var date = await MapAsync(HttpStatusCode.TooManyRequests, new StringContent(""), configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(Now.AddSeconds(90)));
		var past = await MapAsync(HttpStatusCode.TooManyRequests, new StringContent(""), configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(Now.AddSeconds(-90)));

		seconds.RetryAfter.Should().Be(TimeSpan.FromSeconds(42));
		date.RetryAfter.Should().Be(TimeSpan.FromSeconds(90));
		past.RetryAfter.Should().Be(TimeSpan.Zero);
	}

	/// <summary>A body that cannot be read at all (a lost connection) still maps to an exception naming the status.</summary>
	[Fact]
	public async Task UnreadableBody_FallsBackToTheStatus()
	{
		var exception = await MapAsync(HttpStatusCode.InternalServerError, new ThrowingContent());

		exception.Message.Should().Be("HTTP 500 (Internal Server Error)");
	}

	/// <summary>A body whose declared charset cannot be decoded (an <see cref="InvalidOperationException"/> on reading) is handled the same way.</summary>
	[Fact]
	public async Task UndecodableBody_FallsBackToTheStatus()
	{
		using var content = new StringContent("{}");
		content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") { CharSet = "no-such-charset" };

		var exception = await MapAsync(HttpStatusCode.BadRequest, content);

		exception.Message.Should().Be("HTTP 400 (Bad Request)");
	}

	/// <summary>A server that sends the headers and then never finishes the body must not hang the caller for ever.</summary>
	[Fact]
	public async Task AStalledBody_IsAbandonedAfterTheTimeLimit_AndFallsBackToTheStatus()
	{
		var clock = TestClient.NewClock();
		var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StallingContent(started) };

		var mapping = DefenderErrorMapper.CreateAsync(response, clock, TestContext.Current.CancellationToken);
		await started.Task.WaitAsync(TestContext.Current.CancellationToken);
		mapping.IsCompleted.Should().BeFalse();
		clock.Advance(DefenderErrorMapper.BodyTimeout);

		var exception = await mapping.WaitAsync(FailSafe, TestContext.Current.CancellationToken);

		exception.Should().BeOfType<DefenderApiException>().Which.Message.Should().Be("HTTP 500 (Internal Server Error)");
	}

	[Fact]
	public async Task AStalledBody_StopsWhenTheCallerCancels()
	{
		var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
		using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StallingContent(started) };

		var mapping = DefenderErrorMapper.CreateAsync(response, TestClient.NewClock(), cancellation.Token);
		await started.Task.WaitAsync(TestContext.Current.CancellationToken);
		await cancellation.CancelAsync();

		await ((Func<Task>)(() => mapping.WaitAsync(FailSafe, TestContext.Current.CancellationToken))).Should().ThrowAsync<OperationCanceledException>();
	}

	/// <summary>An error body is a few hundred bytes; a huge one is not read into memory or into the exception message.</summary>
	[Fact]
	public async Task ABodyOverTheSizeLimit_FallsBackToTheStatus()
	{
		var body = """{"error":{"message":"[""" + new string('x', (int)DefenderErrorMapper.MaxBodyBytes) + """]"}}""";

		var exception = await MapAsync(body, HttpStatusCode.BadGateway);

		exception.Message.Should().Be("HTTP 502 (Bad Gateway)");
	}

	private sealed class StallingContent(TaskCompletionSource started) : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
			=> SerializeToStreamAsync(stream, context, CancellationToken.None);

		protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
		{
			started.TrySetResult();
			await Task.Delay(Timeout.Infinite, cancellationToken);
		}

		protected override bool TryComputeLength(out long length)
		{
			length = 0;
			return false;
		}
	}

	private sealed class ThrowingContent : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => throw new HttpRequestException("lost");

		protected override bool TryComputeLength(out long length)
		{
			length = 0;
			return false;
		}

		protected override Task<Stream> CreateContentReadStreamAsync() => throw new HttpRequestException("lost");
	}
}
