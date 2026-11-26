using PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups.AuthenticatedScan;

/// <summary>The state-changing operations of the authenticated scan group, and the read-only guard's treatment of the whole group. Never run live.</summary>
public partial class AuthenticatedScanTests
{
	private const string CreateBody = """{"scanType":"Network","scanName":"Test scan","isActive":true,"target":"127.0.0.1","intervalInHours":1,"targetType":"Ip","scannerAgent":{"machineId":"m1"},"scanAuthenticationParams":{"@odata.type":"#microsoft.windowsDefenderATP.api.SnmpAuthParams","type":"AuthPriv","username":"u","authProtocol":"SHA1","authPassword":"ap","privProtocol":"AES","privPassword":"pp"}}""";

	private const string UpdateBody = """{"isActive":false,"target":"127.0.0.1,127.0.0.2"}""";

	private const string DeleteBody = """{"ScanDefinitionIds":["d1","d2"]}""";

	private static CreateScanDefinitionRequest NewCreate() => new()
	{
		ScanName = "Test scan",
		IsActive = true,
		Target = "127.0.0.1",
		IntervalInHours = 1,
		TargetType = "Ip",
		ScannerAgent = new ScanAgentReference { MachineId = "m1" },
		ScanAuthenticationParams = new SnmpAuthParams { Type = "AuthPriv", Username = "u", AuthProtocol = "SHA1", AuthPassword = "ap", PrivProtocol = "AES", PrivPassword = "pp" }
	};

	private static UpdateScanDefinitionRequest NewUpdate() => new() { Target = "127.0.0.1,127.0.0.2", IsActive = false };

	[Fact]
	public async Task CreateDefinition_SendsPostWithTheExactBody()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.CreateDefinitionAsync(NewCreate(), t), DefinitionJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/DeviceAuthenticatedScanDefinitions", body: CreateBody);
	}

	[Fact]
	public async Task CreateDefinition_MapsTheCreatedDefinition()
	{
		var created = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.CreateDefinitionAsync(NewCreate(), t), DefinitionJson);

		(created.Id, created.ScanName).Should().Be(("def-1", "Test scan"));
	}

	[Fact]
	public Task CreateDefinition_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.AuthenticatedScan.CreateDefinitionAsync(NewCreate(), t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"InvalidRequestBody","message":"Bad definition","target":"corr-1"}}""", "Bad definition");

	[Fact]
	public Task CreateDefinition_ResendsTheSameBodyAfterATokenRefresh()
		=> AssertReplayAsync((c, t) => c.AuthenticatedScan.CreateDefinitionAsync(NewCreate(), t), DefinitionJson, CreateBody);

	[Fact]
	public async Task UpdateDefinition_SendsPatchWithOnlyTheGivenMembers()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.UpdateDefinitionAsync("def-1", NewUpdate(), t), DefinitionJson);

		call.ShouldBeRequest(HttpMethod.Patch, "/api/DeviceAuthenticatedScanDefinitions/def-1", body: UpdateBody);
	}

	[Fact]
	public async Task UpdateDefinition_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.UpdateDefinitionAsync("a/b c", NewUpdate(), t), DefinitionJson);

		call.Uri.AbsolutePath.Should().Be("/api/DeviceAuthenticatedScanDefinitions/a%2Fb%20c");
	}

	[Fact]
	public async Task UpdateDefinition_SendsEveryMemberWhenAllAreSet()
	{
		var request = new UpdateScanDefinitionRequest
		{
			ScanType = "Network",
			ScanName = "n",
			IsActive = true,
			Target = "t",
			IntervalInHours = 2,
			TargetType = "Hostname",
			ScannerAgent = new ScanAgentReference { MachineId = "m2" },
			ScanAuthenticationParams = new SnmpAuthParams { Type = "CommunityString", CommunityString = "c", KeyVaultUrl = "https://vault.test", KeyVaultSecretName = "s" }
		};

		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.UpdateDefinitionAsync("d", request, t), DefinitionJson);

		call.ShouldBeRequest(HttpMethod.Patch, "/api/DeviceAuthenticatedScanDefinitions/d", body: """{"scanType":"Network","scanName":"n","isActive":true,"target":"t","intervalInHours":2,"targetType":"Hostname","scannerAgent":{"machineId":"m2"},"scanAuthenticationParams":{"@odata.type":"#microsoft.windowsDefenderATP.api.SnmpAuthParams","type":"CommunityString","keyVaultUrl":"https://vault.test","keyVaultSecretName":"s","communityString":"c"}}""");
	}

	[Fact]
	public async Task UpdateDefinition_MapsTheUpdatedDefinition()
	{
		var updated = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.UpdateDefinitionAsync("def-1", NewUpdate(), t), DefinitionJson);

		updated.Target.Should().Be("127.0.0.1,127.0.0.2");
	}

	[Fact]
	public Task UpdateDefinition_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.AuthenticatedScan.UpdateDefinitionAsync("nope", NewUpdate(), t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"No such definition","target":"corr-1"}}""", "No such definition");

	[Fact]
	public Task UpdateDefinition_ResendsTheSameBodyAfterATokenRefresh()
		=> AssertReplayAsync((c, t) => c.AuthenticatedScan.UpdateDefinitionAsync("def-1", NewUpdate(), t), DefinitionJson, UpdateBody);

	[Fact]
	public async Task DeleteDefinitions_SendsPostBatchDeleteWithTheIds()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.DeleteDefinitionsAsync(new DeleteScanDefinitionsRequest { ScanDefinitionIds = ["d1", "d2"] }, t), "{}");

		call.ShouldBeRequest(HttpMethod.Post, "/api/DeviceAuthenticatedScanDefinitions/BatchDelete", body: DeleteBody);
	}

	[Fact]
	public async Task DeleteDefinitions_AcceptsAnEmptyNoContentAnswer()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, string.Empty);
		using var client = TestClient.Create(stub);

		await client.AuthenticatedScan.DeleteDefinitionsAsync(new DeleteScanDefinitionsRequest { ScanDefinitionIds = ["d1"] }, TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public Task DeleteDefinitions_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.AuthenticatedScan.DeleteDefinitionsAsync(new DeleteScanDefinitionsRequest { ScanDefinitionIds = ["d1"] }, t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"InvalidInput","message":"Bad ids","target":"corr-1"}}""", "Bad ids");

	[Fact]
	public Task DeleteDefinitions_ResendsTheSameBodyAfterATokenRefresh()
		=> AssertReplayAsync((c, t) => c.AuthenticatedScan.DeleteDefinitionsAsync(new DeleteScanDefinitionsRequest { ScanDefinitionIds = ["d1", "d2"] }, t), "{}", DeleteBody);

	[Fact]
	public async Task AReadOnlyClient_RefusesTheThreeWrites_WithoutSendingAnything()
	{
		var stub = new StubHandler();
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);
		var ct = TestContext.Current.CancellationToken;

		var create = () => client.AuthenticatedScan.CreateDefinitionAsync(NewCreate(), ct);
		var update = () => client.AuthenticatedScan.UpdateDefinitionAsync("def-1", NewUpdate(), ct);
		var delete = () => client.AuthenticatedScan.DeleteDefinitionsAsync(new DeleteScanDefinitionsRequest { ScanDefinitionIds = ["d1"] }, ct);

		(await create.Should().ThrowAsync<DefenderReadOnlyException>()).Which.Method.Should().Be("POST");
		(await update.Should().ThrowAsync<DefenderReadOnlyException>()).Which.Method.Should().Be("PATCH");
		(await delete.Should().ThrowAsync<DefenderReadOnlyException>()).Which.Path.Should().EndWith("/DeviceAuthenticatedScanDefinitions/BatchDelete");
		stub.Calls.Should().BeEmpty();
	}

	[Fact]
	public async Task AReadOnlyClient_AllowsTheTwoHistoryPosts_BecauseTheyOnlyRead()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page("""{"ScanDefinitionIds":"d1"}"""));
		stub.Enqueue(HttpStatusCode.OK, Page("""{"SessionIds":"s1"}"""));
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);
		var ct = TestContext.Current.CancellationToken;

		var byDefinition = await client.AuthenticatedScan.GetScanHistoryByDefinitionAsync(new ScanHistoryByDefinitionRequest { ScanDefinitionIds = ["d1"] }, null, ct);
		var bySession = await client.AuthenticatedScan.GetScanHistoryBySessionAsync(new ScanHistoryBySessionRequest { SessionIds = ["s1"] }, ct);

		byDefinition.Value.Single().ScanDefinitionIds.Should().Be("d1");
		bySession.Value.Single().SessionIds.Should().Be("s1");
		stub.Calls.Select(c => c.Method).Should().OnlyContain(m => m == HttpMethod.Post);
	}

	[Fact]
	public async Task AReadOnlyClient_AllowsTheReads()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page(DefinitionJson));
		stub.Enqueue(HttpStatusCode.OK, Page(AgentJson));
		stub.Enqueue(HttpStatusCode.OK, AgentJson);
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);
		var ct = TestContext.Current.CancellationToken;

		(await client.AuthenticatedScan.ListDefinitionsAsync(ct)).Value.Should().ContainSingle();
		(await client.AuthenticatedScan.ListAgentsAsync(ct)).Value.Should().ContainSingle();
		(await client.AuthenticatedScan.GetAgentAsync("org-1_m1", ct)).MachineId.Should().Be("m1");
	}

	[Fact]
	public void TheCredentialsObject_NeverPrintsItsSecrets()
	{
		var text = new SnmpAuthParams { Type = "AuthPriv", AuthPassword = "ap-secret", PrivPassword = "pp-secret", CommunityString = "cs-secret", Username = "u-secret" }.ToString();

		text.Should().Be("SnmpAuthParams (AuthPriv)");
	}

	private static async Task AssertReplayAsync(Func<DefenderClient, CancellationToken, Task> call, string response, string expectedBody)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Unauthorized);
		stub.Enqueue(HttpStatusCode.OK, response);
		using var client = TestClient.Create(stub);

		await call(client, TestContext.Current.CancellationToken);

		stub.Calls.Select(c => c.Body).Should().OnlyContain(b => b == expectedBody);
		stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
	}
}
