using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public partial class AuthenticationHandlerTests
{
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
