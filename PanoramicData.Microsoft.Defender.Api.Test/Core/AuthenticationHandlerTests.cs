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

		/// <summary>Sends the default request <paramref name="count"/> times in turn, discarding each response.</summary>
		public async Task SendManyAsync(int count)
		{
			for (var i = 0; i < count; i++)
			{
				(await SendAsync()).Dispose();
			}
		}

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

		await harness.SendManyAsync(1);
		harness.Clock.Advance(TimeSpan.FromMinutes(54));
		await harness.SendManyAsync(1);
		harness.Credential.Issued.Should().Be(1, "the token is valid for another six minutes, longer than the five-minute margin");

		harness.Clock.Advance(TimeSpan.FromMinutes(2));
		await harness.SendManyAsync(2);

		harness.Credential.Issued.Should().Be(2);
		harness.Stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-1", "Bearer token-2", "Bearer token-2");
	}

	[Fact]
	public async Task TheRefreshMargin_IsConfigurable()
	{
		using var harness = new Harness(o => o.TokenRefreshMargin = TimeSpan.Zero).Respond(HttpStatusCode.OK, HttpStatusCode.OK);

		await harness.SendManyAsync(1);
		harness.Clock.Advance(TimeSpan.FromMinutes(59));
		await harness.SendManyAsync(1);

		harness.Credential.Issued.Should().Be(1);
	}

	/// <summary>A margin longer than any token lifetime means "never reuse a token"; it must not overflow the date arithmetic.</summary>
	[Fact]
	public async Task AHugeRefreshMargin_NeverReusesAToken_WithoutOverflowing()
	{
		using var harness = new Harness(o => o.TokenRefreshMargin = TimeSpan.MaxValue).Respond(HttpStatusCode.OK, HttpStatusCode.OK);

		await harness.SendManyAsync(2);

		harness.Credential.Issued.Should().Be(2);
		harness.Stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
	}
}
