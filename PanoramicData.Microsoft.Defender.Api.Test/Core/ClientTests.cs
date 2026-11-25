using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public class ClientTests
{
	private const string Empty = """{"value":[]}""";

	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private sealed class TrackingHandler : HttpMessageHandler
	{
		public bool Disposed { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Empty), RequestMessage = request });
		}

		protected override void Dispose(bool disposing)
		{
			Disposed = true;
			base.Dispose(disposing);
		}
	}

	private static DefenderClientOptions Valid() => TestClient.Options(TimeProvider.System);

	[Fact]
	public void Constructors_RequireArguments()
	{
		var options = Valid();

		((Action)(() => _ = new DefenderClient(null!))).Should().Throw<ArgumentNullException>();
		((Action)(() => _ = new DefenderClient(null!, new StubHandler()))).Should().Throw<ArgumentNullException>();
		((Action)(() => _ = new DefenderClient(options, null!))).Should().Throw<ArgumentNullException>();
		((Action)(() => _ = new DefenderClient(options, new StubHandler(), null!))).Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ValidatesOptions()
	{
		var act = () => new DefenderClient(new DefenderClientOptions { Credential = null });

		act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("Credential");
	}

	[Fact]
	public void Constructor_WithOwnTransport_CreatesAUsableClient()
	{
		using var client = new DefenderClient(Valid());

		client.BaseAddress.Should().Be(new Uri("https://defender.test/api/"));
		client.Vulnerabilities.Should().NotBeNull().And.BeSameAs(client.Vulnerabilities);
	}

	[Fact]
	public void Constructor_WithOneHandler_UsesAnotherTransportForDownloads()
	{
		var handler = new TrackingHandler();
		using var client = new DefenderClient(Valid(), handler);

		client.BaseAddress.Should().Be(new Uri("https://defender.test/api/"));
	}

	[Theory]
	[InlineData("https://api.security.microsoft.com/api", "https://api.security.microsoft.com/api/")]
	[InlineData("https://api.security.microsoft.com/api/", "https://api.security.microsoft.com/api/")]
	[InlineData("https://proxy.test/defender/api/v1.0", "https://proxy.test/defender/api/v1.0/")]
	public void BaseAddress_AlwaysEndsWithSlash(string baseUri, string expected)
	{
		using var client = TestClient.Create(new StubHandler(), o => o.BaseUri = new Uri(baseUri));

		client.BaseAddress.AbsoluteUri.Should().Be(expected);
	}

	[Theory]
	[InlineData("https://proxy.test/defender/api", "/defender/api/vulnerabilities")]
	[InlineData("https://defender.test/api/", "/api/vulnerabilities")]
	[InlineData("https://defender.test", "/vulnerabilities")]
	public async Task Requests_KeepThePathPrefix(string baseUri, string expectedPath)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Empty);
		using var client = TestClient.Create(stub, o => o.BaseUri = new Uri(baseUri));

		await client.Vulnerabilities.ListAsync(null, Ct);

		stub.Calls[0].Uri.AbsolutePath.Should().Be(expectedPath);
	}

	[Fact]
	public async Task Requests_AreSentWithABearerToken_ForTheConfiguredScope()
	{
		var clock = TestClient.NewClock();
		var credential = new FakeCredential(clock);
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Empty);
		using var client = new DefenderClient(TestClient.Options(clock, o =>
		{
			o.Credential = credential;
			o.Scope = "https://example.test/.default";
		}), stub);

		await client.Vulnerabilities.ListAsync(null, Ct);

		stub.Calls[0].Authorization.Should().Be("Bearer token-1");
		credential.RequestedScopes.Should().ContainSingle().Which.Should().Equal("https://example.test/.default");
	}

	[Fact]
	public void Redirects_AreNeverFollowed_OnEitherTransport()
	{
		using var api = DefenderClient.CreateTransport(Valid());
		using var downloads = DefenderClient.CreateDownloadTransport();

		api.AllowAutoRedirect.Should().BeFalse("a redirect could carry the bearer token to another host");
		downloads.AllowAutoRedirect.Should().BeFalse();
	}

	[Fact]
	public async Task DisposingTheClient_DisposesBothTransports_AndStopsTheClient()
	{
		var api = new TrackingHandler();
		var downloads = new TrackingHandler();
		var owner = new DefenderClient(Valid(), api, downloads);

		owner.Dispose();
		owner.Dispose();
		var act = () => owner.Vulnerabilities.ListAsync(null, Ct);

		api.Disposed.Should().BeTrue();
		downloads.Disposed.Should().BeTrue();
		await act.Should().ThrowAsync<ObjectDisposedException>();
	}

	[Fact]
	public async Task TransportExceptions_SurfaceAsThemselves()
	{
		var stub = new DelegateHandler((_, _) => throw new HttpRequestException("boom"));
		using var client = new DefenderClient(Valid(), stub);

		var act = () => client.Vulnerabilities.ListAsync(null, Ct);

		(await act.Should().ThrowAsync<HttpRequestException>()).Which.Message.Should().Be("boom");
	}
}
