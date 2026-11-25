using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

/// <summary>The read-only guard: it refuses every state-changing request before it is sent, and before a token is fetched.</summary>
public class ReadOnlyHandlerTests
{
	private static HandlerHarness Create(string baseAddress = "https://defender.test/api/")
		=> new(new ReadOnlyHandler(new Uri(baseAddress)));

	[Theory]
	[InlineData("GET", "https://defender.test/api/machines")]
	[InlineData("HEAD", "https://defender.test/api/machines")]
	[InlineData("POST", "https://defender.test/api/advancedqueries/run")]
	[InlineData("POST", "https://defender.test/api/AdvancedQueries/Run")]
	[InlineData("POST", "https://defender.test/api/advancedqueries/run/")]
	[InlineData("POST", "https://defender.test/api/DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId")]
	[InlineData("POST", "https://defender.test/api/DeviceAuthenticatedScanDefinitions/GetScanHistoryBySessionId")]
	public async Task ReadOperations_AreAllowed(string method, string uri)
	{
		using var harness = Create();
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.SendAsync(new HttpMethod(method), uri);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls.Should().ContainSingle();
	}

	[Theory]
	[InlineData("POST", "https://defender.test/api/machines/abc/isolate")]
	[InlineData("POST", "https://defender.test/api/DeviceAuthenticatedScanDefinitions")]
	[InlineData("POST", "https://defender.test/api/DeviceAuthenticatedScanDefinitions/BatchDelete")]
	[InlineData("POST", "https://defender.test/api/alerts/batchUpdate")]
	[InlineData("POST", "https://defender.test/api/advancedqueries/run/more")]
	[InlineData("POST", "https://defender.test/api/x/advancedqueries/run")]
	[InlineData("POST", "https://defender.test/other/advancedqueries/run")]
	[InlineData("PATCH", "https://defender.test/api/alerts/1")]
	[InlineData("PUT", "https://defender.test/api/anything")]
	[InlineData("DELETE", "https://defender.test/api/indicators/1")]
	public async Task StateChangingOperations_AreRefused_BeforeAnythingIsSent(string method, string uri)
	{
		using var harness = Create();

		var act = () => harness.SendAsync(new HttpMethod(method), uri + "?secret=1");

		var thrown = await act.Should().ThrowAsync<DefenderReadOnlyException>();
		thrown.Which.Method.Should().Be(method);
		thrown.Which.Path.Should().Be(uri);
		thrown.Which.Message.Should().Be($"The Defender client is read-only and refused {method} {uri}.");
		harness.Stub.Calls.Should().BeEmpty();
	}

	[Fact]
	public async Task AClientWithIsReadOnly_RefusesWritesWithoutFetchingAToken()
	{
		var clock = TestClient.NewClock();
		var credential = new FakeCredential(clock);
		var stub = new StubHandler();
		using var client = new DefenderClient(TestClient.Options(clock, o =>
		{
			o.Credential = credential;
			o.IsReadOnly = true;
		}), stub);

		var act = () => client.For<IWriteProbe>().WriteAsync(TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<DefenderReadOnlyException>();
		credential.Issued.Should().Be(0);
		stub.Calls.Should().BeEmpty();
	}

	[Fact]
	public async Task AClientWithoutIsReadOnly_AllowsWrites()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);

		await client.For<IWriteProbe>().WriteAsync(TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Delete);
	}

	public interface IWriteProbe
	{
		[Refit.Delete("indicators/1")]
		Task WriteAsync(CancellationToken cancellationToken);
	}
}
