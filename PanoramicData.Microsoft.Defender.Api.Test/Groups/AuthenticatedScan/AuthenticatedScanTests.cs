using PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;
using System.Text.Json;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups.AuthenticatedScan;

/// <summary>The read operations of the authenticated scan group: definitions, agents and the two history queries.</summary>
public partial class AuthenticatedScanTests
{
	// Shaped like the service's responses (field names and types), with neutral values.
	private const string DefinitionJson = """
		{
			"id": "def-1",
			"scanType": "Network",
			"scanName": "Test scan",
			"isActive": true,
			"target": "127.0.0.1,127.0.0.2",
			"orgId": "org-1",
			"intervalInHours": 6,
			"createdBy": "user@contoso.test",
			"targetType": "Ip",
			"scanAuthenticationParams": null,
			"scannerAgent": {
				"id": "org-1_m1",
				"machineId": "m1",
				"machineName": "DESKTOP-TEST",
				"lastSeen": "2025-12-18T14:04:34.2258259Z",
				"assignedApplicationId": "app-1",
				"scannerSoftwareVersion": "9.3.15.19",
				"lastCommandExecutionTimestamp": "2025-12-18T13:29:48Z",
				"mdeClientVersion": null
			},
			"latestScan": {
				"status": "Fail",
				"failureReason": "Unreachable",
				"executionDateTime": "2025-12-18T14:33:49.413596Z"
			},
			"advancedActiveConfiguration": {"mode": "x"}
		}
		""";

	private const string AgentJson = """
		{
			"id": "org-1_m1",
			"machineId": "m1",
			"lastSeen": "2022-05-08T12:18:41.538203Z",
			"computerDnsName": "TEST_DOMAIN",
			"AssignedApplicationId": "app-1",
			"ScannerSoftwareVersion": "7.1.1",
			"LastCommandExecutionTimestamp": "2022-05-08T12:18:41Z",
			"mdeClientVersion": "10.8295.22621.1195"
		}
		""";

	private static string Page(string items) => RelatedEntityChecks.Page(items);

	private static void AssertAgent(ScanAgent agent)
	{
		(agent.Id, agent.MachineId, agent.ComputerDnsName, agent.AssignedApplicationId, agent.ScannerSoftwareVersion, agent.MdeClientVersion)
			.Should().Be(("org-1_m1", "m1", "TEST_DOMAIN", "app-1", "7.1.1", "10.8295.22621.1195"));
		agent.LastSeen.Should().Be(new DateTimeOffset(2022, 5, 8, 12, 18, 41, TimeSpan.Zero).AddTicks(5382030));
		agent.LastCommandExecutionTimestamp.Should().Be(new DateTimeOffset(2022, 5, 8, 12, 18, 41, TimeSpan.Zero));
	}

	[Fact]
	public Task ListDefinitions_SendsGetDeviceAuthenticatedScanDefinitions()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.AuthenticatedScan.ListDefinitionsAsync(t), "/api/DeviceAuthenticatedScanDefinitions");

	[Fact]
	public async Task ListDefinitions_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.ListDefinitionsAsync(t), Page(DefinitionJson));

		var d = page.Value.Should().ContainSingle().Subject;
		(d.Id, d.ScanType, d.ScanName, d.IsActive, d.Target, d.OrgId, d.IntervalInHours, d.CreatedBy, d.TargetType)
			.Should().Be(("def-1", "Network", "Test scan", true, "127.0.0.1,127.0.0.2", "org-1", 6, "user@contoso.test", "Ip"));
		d.ScanAuthenticationParams.Should().BeNull();
		(d.ScannerAgent!.Id, d.ScannerAgent.MachineId, d.ScannerAgent.MachineName, d.ScannerAgent.AssignedApplicationId, d.ScannerAgent.ScannerSoftwareVersion, d.ScannerAgent.MdeClientVersion)
			.Should().Be(("org-1_m1", "m1", "DESKTOP-TEST", "app-1", "9.3.15.19", null));
		d.ScannerAgent.LastSeen.Should().Be(new DateTimeOffset(2025, 12, 18, 14, 4, 34, TimeSpan.Zero).AddTicks(2258259));
		d.ScannerAgent.LastCommandExecutionTimestamp.Should().Be(new DateTimeOffset(2025, 12, 18, 13, 29, 48, TimeSpan.Zero));
		(d.LatestScan!.Status, d.LatestScan.FailureReason).Should().Be(("Fail", "Unreachable"));
		d.LatestScan.ExecutionDateTime.Should().Be(new DateTimeOffset(2025, 12, 18, 14, 33, 49, TimeSpan.Zero).AddTicks(4135960));
		d.AdvancedActiveConfiguration!.Value.GetProperty("mode").GetString().Should().Be("x");
	}

	[Fact]
	public async Task ListDefinitions_MapsReturnedAuthenticationParams()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.ListDefinitionsAsync(t),
			Page("""{"id":"d","isActive":"false","intervalInHours":"3","scanAuthenticationParams":{"@odata.type":"#microsoft.windowsDefenderATP.api.SnmpAuthParams","type":"CommunityString","communityString":"c","keyVaultUrl":"https://vault.test","keyVaultSecretName":"s"},"scannerAgent":null,"latestScan":{"status":null,"executionDateTime":""},"advancedActiveConfiguration":null}"""));

		var d = page.Value.Should().ContainSingle().Subject;
		(d.IsActive, d.IntervalInHours, d.ScannerAgent, d.AdvancedActiveConfiguration).Should().Be((false, 3, null, (JsonElement?)null));
		var p = d.ScanAuthenticationParams!;
		(p.ODataType, p.Type, p.CommunityString, p.KeyVaultUrl, p.KeyVaultSecretName).Should().Be((SnmpAuthParams.SnmpODataType, "CommunityString", "c", "https://vault.test", "s"));
		d.LatestScan!.ExecutionDateTime.Should().BeNull();
	}

	[Fact]
	public Task ListDefinitions_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.AuthenticatedScan.ListDefinitionsAsync(t));

	[Fact]
	public Task GetAllScanDefinitions_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllScanDefinitionsAsync(t), "/api/DeviceAuthenticatedScanDefinitions", """{"id":"a"}""", """{"id":"b"}""", d => d.Id);

	[Fact]
	public Task ListAgents_SendsGetDeviceAuthenticatedScanAgents()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.AuthenticatedScan.ListAgentsAsync(t), "/api/DeviceAuthenticatedScanAgents");

	[Fact]
	public async Task ListAgents_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.ListAgentsAsync(t), Page(AgentJson));

		AssertAgent(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public Task ListAgents_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.AuthenticatedScan.ListAgentsAsync(t));

	[Fact]
	public Task GetAllScanAgents_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllScanAgentsAsync(t), "/api/DeviceAuthenticatedScanAgents", """{"id":"a"}""", """{"id":"b"}""", a => a.Id);

	[Fact]
	public async Task GetAgent_SendsGetDeviceAuthenticatedScanAgentsId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.GetAgentAsync("org-1_m1", t), AgentJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/DeviceAuthenticatedScanAgents/org-1_m1");
	}

	[Fact]
	public async Task GetAgent_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.GetAgentAsync("a/b c", t), AgentJson);

		call.Uri.AbsolutePath.Should().Be("/api/DeviceAuthenticatedScanAgents/a%2Fb%20c");
	}

	[Fact]
	public async Task GetAgent_ReadsABareEntity()
	{
		var agent = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.GetAgentAsync("org-1_m1", t), AgentJson);

		AssertAgent(agent);
		agent.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAgent_ReadsTheCollectionWrappedShapeTheDocumentationShows()
	{
		var agent = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.GetAgentAsync("org-1_m1", t),
			$$"""{"@odata.context":"https://defender.test/api/$metadata#DeviceAuthenticatedScanAgents/$entity","value":[{{AgentJson}}]}""");

		agent.Id.Should().BeNull();
		AssertAgent(agent.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public Task GetAgent_AnUnknownIdIsMappedAsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.AuthenticatedScan.GetAgentAsync("nope", t), HttpStatusCode.NotFound);

	[Fact]
	public async Task GetScanHistoryByDefinition_SendsPostWithTheIdsBody_AndNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.GetScanHistoryByDefinitionAsync(new ScanHistoryByDefinitionRequest { ScanDefinitionIds = ["d1", "d2"] }, null, t), Page(""));

		call.ShouldBeRequest(HttpMethod.Post, "/api/DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId", body: """{"ScanDefinitionIds":["d1","d2"]}""");
	}

	[Fact]
	public async Task GetScanHistoryByDefinition_SendsTopAndSkipWithALiteralDollar()
	{
		var options = new ScanHistoryOptions { Top = 1, Skip = 2 };

		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.GetScanHistoryByDefinitionAsync(new ScanHistoryByDefinitionRequest { ScanDefinitionIds = ["d1"] }, options, t), Page(""));

		call.ShouldBeRequest(HttpMethod.Post, "/api/DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId", "?$top=1&$skip=2", """{"ScanDefinitionIds":["d1"]}""");
	}

	[Fact]
	public async Task GetScanHistoryByDefinition_MapsEveryField()
	{
		var page = await TestClient.ReadAsync(
			(c, t) => c.AuthenticatedScan.GetScanHistoryByDefinitionAsync(new ScanHistoryByDefinitionRequest { ScanDefinitionIds = ["d1"] }, null, t),
			Page("""{"ScanDefinitionIds":"d1","LastScanned":"2022-12-20T11:14:24.5561791Z","ScanStatus":"Partial Success","ScannerId":"s1"}"""));

		var row = page.Value.Should().ContainSingle().Subject;
		(row.ScanDefinitionIds, row.ScanStatus, row.ScannerId).Should().Be(("d1", "Partial Success", "s1"));
		row.LastScanned.Should().Be(new DateTimeOffset(2022, 12, 20, 11, 14, 24, TimeSpan.Zero).AddTicks(5561791));
	}

	[Fact]
	public Task GetScanHistoryByDefinition_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.AuthenticatedScan.GetScanHistoryByDefinitionAsync(new ScanHistoryByDefinitionRequest { ScanDefinitionIds = ["d1"] }, null, t));

	[Fact]
	public async Task GetScanHistoryBySession_SendsPostWithTheIdsBody()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AuthenticatedScan.GetScanHistoryBySessionAsync(new ScanHistoryBySessionRequest { SessionIds = ["s1"] }, t), Page(""));

		call.ShouldBeRequest(HttpMethod.Post, "/api/DeviceAuthenticatedScanDefinitions/GetScanHistoryBySessionId", body: """{"SessionIds":["s1"]}""");
	}

	[Fact]
	public async Task GetScanHistoryBySession_MapsEveryField_WithTargetsAsAnArray()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.GetScanHistoryBySessionAsync(new ScanHistoryBySessionRequest { SessionIds = ["s1"] }, t),
			Page(SessionJson("[" + TargetJson("127.0.0.1", "00:08:30") + "," + TargetJson("127.0.0.2", "00:08:00") + "]")));

		var row = page.Value.Should().ContainSingle().Subject;
		(row.OrgId, row.ScanDefinitionIds, row.SessionIds, row.NumberOfSuccessfullyScannedTargets, row.NumberOfTargets, row.ScanStatus)
			.Should().Be(("org-1", "d1", "s1", 2, 3, "Success"));
		row.LastScanned.Should().Be(new DateTimeOffset(2022, 12, 19, 15, 14, 24, TimeSpan.Zero).AddTicks(5561791));
		row.ListScannedTargets.Select(t => (t.Ip, t.ScanDuration)).Should().Equal(("127.0.0.1", "00:08:30"), ("127.0.0.2", "00:08:00"));
		var target = row.ListScannedTargets[0];
		(target.Hostname, target.ScannedDeviceDescription, target.ErrorMessage, target.ScanStatus).Should().Be(("DESKTOP-TEST", "Network device", "", "Success"));
	}

	[Fact]
	public async Task GetScanHistoryBySession_ReadsASingleTargetObjectAsAOneItemList()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.GetScanHistoryBySessionAsync(new ScanHistoryBySessionRequest { SessionIds = ["s1"] }, t),
			Page(SessionJson(TargetJson("127.0.0.9", "00:01:00"))));

		page.Value.Single().ListScannedTargets.Should().ContainSingle().Which.Ip.Should().Be("127.0.0.9");
	}

	[Theory]
	[InlineData("null")]
	[InlineData("\"\"")]
	[InlineData("[]")]
	[InlineData("7")]
	public async Task GetScanHistoryBySession_ReadsAnythingElseAsAnEmptyList(string targets)
	{
		var page = await TestClient.ReadAsync((c, t) => c.AuthenticatedScan.GetScanHistoryBySessionAsync(new ScanHistoryBySessionRequest { SessionIds = ["s1"] }, t),
			Page(SessionJson(targets)));

		page.Value.Single().ListScannedTargets.Should().BeEmpty();
	}

	[Fact]
	public Task GetScanHistoryBySession_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.AuthenticatedScan.GetScanHistoryBySessionAsync(new ScanHistoryBySessionRequest { SessionIds = ["s1"] }, t));

	[Fact]
	public void TheScannedTargetList_WritesAsAnArray()
	{
		var row = new ScanHistoryBySession { ListScannedTargets = [new ScannedTarget { Ip = "127.0.0.1" }] };

		JsonSerializer.Serialize(row, Serialization.DefenderJson.Options).Should().Contain("\"ListScannedTargets\":[{\"Ip\":\"127.0.0.1\"}]");
	}

	private static string TargetJson(string ip, string duration)
		=> $$"""{"Ip":"{{ip}}","Hostname":"DESKTOP-TEST","ScannedDeviceDescription":"Network device","ErrorMessage":"","ScanStatus":"Success","ScanDuration":"{{duration}}"}""";

	private static string SessionJson(string targets)
		=> $$"""{"orgId":"org-1","ScanDefinitionIds":"d1","SessionIds":"s1","NumberOfSuccessfullyScannedTargets":2,"NumberOfTargets":3,"ScanStatus":"Success","LastScanned":"2022-12-19T15:14:24.5561791Z","ListScannedTargets":{{targets}}}""";
}
