using Microsoft.Extensions.Time.Testing;
using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public partial class AuthenticationHandlerTests
{
	private const string Url = "https://defender.test/api/machines";

	private sealed class Harness : IDisposable
	{
		private readonly HandlerHarness _harness;

		public Harness(Action<DefenderClientOptions>? tweak = null, Action<FakeCredential>? configure = null, HttpMessageHandler? inner = null)
		{
			Credential = new FakeCredential(Clock);
			configure?.Invoke(Credential);
			var options = new DefenderClientOptions { Credential = Credential, TimeProvider = Clock };
			tweak?.Invoke(options);
			_harness = new HandlerHarness(new AuthenticationHandler(options), inner);
		}

		public FakeTimeProvider Clock { get; } = TestClient.NewClock();

		public FakeCredential Credential { get; }

		public StubHandler Stub => _harness.Stub;

		public Harness Respond(params HttpStatusCode[] statuses)
		{
			foreach (var status in statuses)
			{
				Stub.Enqueue(status);
			}

			return this;
		}

		public Task<HttpResponseMessage> SendAsync(HttpMethod? method = null, HttpContent? content = null)
			=> _harness.SendAsync(method ?? HttpMethod.Get, Url, content);

		public Task<HttpResponseMessage> Send(HttpRequestMessage request) => _harness.SendAsync(request);

		public Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken cancellationToken) => _harness.SendAsync(request, cancellationToken);

		public void Dispose() => _harness.Dispose();
	}

	[Fact]
	public async Task SendsABearerToken_ForTheScope()
	{
		using var harness = new Harness(o => o.Scope = "https://example.test/.default").Respond(HttpStatusCode.OK);

		using var response = await harness.SendAsync();

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls[0].Authorization.Should().Be("Bearer token-1");
		harness.Credential.RequestedScopes.Should().ContainSingle().Which.Should().Equal("https://example.test/.default");
	}

	[Fact]
	public async Task OverwritesAnAuthorizationHeaderTheCallerSet()
	{
		using var harness = new Harness().Respond(HttpStatusCode.OK);
		using var request = new HttpRequestMessage(HttpMethod.Get, Url);
		request.Headers.Authorization = new("Basic", "caller");

		using var response = await harness.Send(request, TestContext.Current.CancellationToken);

		harness.Stub.Calls[0].Authorization.Should().Be("Bearer token-1");
	}

	[Fact]
	public async Task CachesTheToken_UntilItIsNearExpiry()
	{
		using var harness = new Harness().Respond(HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK);

		(await harness.SendAsync()).Dispose();
		harness.Clock.Advance(TimeSpan.FromMinutes(54));
		(await harness.SendAsync()).Dispose();
		harness.Credential.Issued.Should().Be(1, "the token is valid for another six minutes, longer than the five-minute margin");

		harness.Clock.Advance(TimeSpan.FromMinutes(2));
		(await harness.SendAsync()).Dispose();
		(await harness.SendAsync()).Dispose();

		harness.Credential.Issued.Should().Be(2);
		harness.Stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-1", "Bearer token-2", "Bearer token-2");
	}

	[Fact]
	public async Task TheRefreshMargin_IsConfigurable()
	{
		using var harness = new Harness(o => o.TokenRefreshMargin = TimeSpan.Zero).Respond(HttpStatusCode.OK, HttpStatusCode.OK);

		(await harness.SendAsync()).Dispose();
		harness.Clock.Advance(TimeSpan.FromMinutes(59));
		(await harness.SendAsync()).Dispose();

		harness.Credential.Issued.Should().Be(1);
	}

	/// <summary>A margin longer than any token lifetime means "never reuse a token"; it must not overflow the date arithmetic.</summary>
	[Fact]
	public async Task AHugeRefreshMargin_NeverReusesAToken_WithoutOverflowing()
	{
		using var harness = new Harness(o => o.TokenRefreshMargin = TimeSpan.MaxValue).Respond(HttpStatusCode.OK, HttpStatusCode.OK);

		(await harness.SendAsync()).Dispose();
		(await harness.SendAsync()).Dispose();

		harness.Credential.Issued.Should().Be(2);
		harness.Stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
	}

	[Fact]
	public async Task On401_FetchesANewToken_AndReplaysTheRequestWithItsBody()
	{
		using var harness = new Harness().Respond(HttpStatusCode.Unauthorized, HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Patch, new StringContent("""{"status":"Resolved"}""", System.Text.Encoding.UTF8, "application/json"));

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
		harness.Stub.Calls.Select(c => c.Body).Should().AllBe("""{"status":"Resolved"}""");
		harness.Credential.Issued.Should().Be(2);
	}

	[Fact]
	public async Task On401_WhenTheCredentialReturnsTheSameToken_TheSecond401IsReturned()
	{
		using var harness = new Harness(configure: c => c.FixedToken = "cached").Respond(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized);

		using var response = await harness.SendAsync();

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		harness.Stub.Calls.Should().HaveCount(2, "the request is replayed once, never more");
		harness.Credential.Issued.Should().Be(2, "the stale token is never served from the cache");
	}

	[Fact]
	public async Task On401_WithAStreamBody_IsNotReplayed()
	{
		using var harness = new Harness().Respond(HttpStatusCode.Unauthorized);

		using var response = await harness.SendAsync(HttpMethod.Post, new StreamContent(new MemoryStream("x"u8.ToArray())));

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		harness.Stub.Calls.Should().ContainSingle();
		harness.Credential.Issued.Should().Be(1);
	}

	[Theory]
	[InlineData(HttpStatusCode.Forbidden)]
	[InlineData(HttpStatusCode.OK)]
	public async Task OtherStatuses_AreReturnedUntouched(HttpStatusCode status)
	{
		using var harness = new Harness().Respond(status);

		using var response = await harness.SendAsync();

		response.StatusCode.Should().Be(status);
		harness.Stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task ACredentialFailure_Propagates_AndTheNextRequestTriesAgain()
	{
		var clock = TestClient.NewClock();
		var fail = true;
		var failing = new ThrowingCredential(() => fail ? throw new InvalidOperationException("no token") : new Azure.Core.AccessToken("t", clock.GetUtcNow().AddHours(1)));
		using var harness = new HandlerHarness(new AuthenticationHandler(new DefenderClientOptions { Credential = failing, TimeProvider = clock }));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		var first = () => harness.SendAsync(HttpMethod.Get, Url);
		(await first.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("no token");
		fail = false;
		using var response = await harness.SendAsync(HttpMethod.Get, Url);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls[0].Authorization.Should().Be("Bearer t");
	}

	[Fact]
	public void TheTokenNeverAppearsInDiagnosticText()
	{
		using var harness = new Harness(configure: c => c.FixedToken = "super-secret-token").Respond(HttpStatusCode.OK);
		var options = new DefenderClientOptions { Credential = harness.Credential };

		options.ToString().Should().NotContain("super-secret-token");
	}

	[Fact]
	public async Task CancellingWhileWaitingForTheToken_RaisesOperationCanceled()
	{
		var gate = new TaskCompletionSource();
		using var harness = new Harness(configure: c => c.Gate = gate.Task);
		using var cts = new CancellationTokenSource();
		using var request = new HttpRequestMessage(HttpMethod.Get, Url);

		var sending = harness.Send(request, cts.Token);
		await harness.Credential.FirstRequested.Task;
		await cts.CancelAsync();

		await ((Func<Task>)(() => sending)).Should().ThrowAsync<OperationCanceledException>();
	}

	private sealed class ThrowingCredential(Func<Azure.Core.AccessToken> next) : Azure.Core.TokenCredential
	{
		public override Azure.Core.AccessToken GetToken(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			_ = requestContext;
			return next();
		}

		public override ValueTask<Azure.Core.AccessToken> GetTokenAsync(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(requestContext.Scopes);
			cancellationToken.ThrowIfCancellationRequested();
			return ValueTask.FromResult(next());
		}
	}
}
