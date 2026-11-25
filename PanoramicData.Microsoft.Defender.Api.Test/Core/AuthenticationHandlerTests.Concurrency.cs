using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public partial class AuthenticationHandlerTests
{
	[Fact]
	public async Task ConcurrentRequests_ShareOneTokenFetch()
	{
		var gate = new TaskCompletionSource();
		var clock = TestClient.NewClock();
		var credential = new FakeCredential(clock) { Gate = gate.Task };
		var transport = new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
		using var invoker = new HttpMessageInvoker(new AuthenticationHandler(new DefenderClientOptions { Credential = credential, TimeProvider = clock }) { InnerHandler = transport });

		var sends = Enumerable.Range(0, 16)
			.Select(_ => Task.Run(() => invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, Url), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken))
			.ToList();
		await credential.FirstRequested.Task;
		gate.SetResult();
		foreach (var response in await Task.WhenAll(sends))
		{
			response.Dispose();
		}

		credential.Issued.Should().Be(1, "concurrent requests wait for the one fetch in flight instead of each asking the credential");
		transport.Authorizations.Should().HaveCount(16).And.AllBe("Bearer token-1");
	}

	[Fact]
	public async Task ConcurrentRequests_ThatAllSaw401_RefreshTheTokenOnce()
	{
		var clock = TestClient.NewClock();
		var credential = new FakeCredential(clock);
		var transport = new DelegateHandler((request, _) => Task.FromResult(new HttpResponseMessage(
			request.Headers.Authorization!.Parameter == "token-1" ? HttpStatusCode.Unauthorized : HttpStatusCode.OK)));
		using var invoker = new HttpMessageInvoker(new AuthenticationHandler(new DefenderClientOptions { Credential = credential, TimeProvider = clock }) { InnerHandler = transport });
		// Prime the cache with token-1 so every request starts from it.
		using var priming = new HttpRequestMessage(HttpMethod.Get, Url);
		(await invoker.SendAsync(priming, TestContext.Current.CancellationToken)).Dispose();

		var sends = Enumerable.Range(0, 8)
			.Select(_ => Task.Run(() => invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, Url), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken))
			.ToList();
		var responses = await Task.WhenAll(sends);

		responses.Select(r => r.StatusCode).Should().OnlyContain(s => s == HttpStatusCode.OK);
		credential.Issued.Should().Be(2, "token-1 was replaced once for everyone");
		foreach (var response in responses)
		{
			response.Dispose();
		}
	}
}
