using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Alerts: listing, reading and the paging helper. Updates are in <c>AlertsTests.Writes.cs</c>, related entities in <c>AlertsTests.Related.cs</c>.</summary>
public partial class AlertsTests
{
	// Shaped like the service's responses (field names and types), with neutral values.
	private const string AlertJson = """
        {
            "id": "da637000000000000000_1000000001",
            "incidentId": 4242,
            "investigationId": 17,
            "assignedTo": "analyst@example.test",
            "severity": "Medium",
            "status": "InProgress",
            "classification": "TruePositive",
            "determination": "Malware",
            "investigationState": "Running",
            "detectionSource": "WindowsDefenderAtp",
            "detectorId": "00000000-0000-0000-0000-000000000001",
            "category": "Execution",
            "threatFamilyName": "Family1",
            "threatName": "Threat1",
            "title": "A made-up alert",
            "description": "A made-up alert used by the tests.",
            "alertCreationTime": "2026-01-02T03:04:05.0123456Z",
            "firstEventTime": "2026-01-02T03:00:00Z",
            "lastEventTime": "2026-01-02T03:01:00Z",
            "lastUpdateTime": "2026-01-02T03:05:00Z",
            "resolvedTime": "2026-01-03T00:00:00Z",
            "machineId": "1111111111111111111111111111111111111111",
            "computerDnsName": "pc-01.example.test",
            "rbacGroupName": "GroupA",
            "aadTenantId": "00000000-0000-0000-0000-000000000002",
            "mitreTechniques": ["T0001", "T0002"],
            "relatedUser": { "userName": "user1", "domainName": "EXAMPLE" },
            "comments": [ { "comment": "first note", "createdBy": "analyst@example.test", "createdTime": "2026-01-02T04:00:00Z" } ],
            "evidence": [
                {
                    "entityType": "Process",
                    "evidenceCreationTime": "2026-01-02T03:04:06Z",
                    "sha1": "aa", "sha256": "bb", "fileName": "tool.exe", "filePath": "C:/tools",
                    "processId": 3276, "processCommandLine": "tool.exe --x", "processCreationTime": "2026-01-02T03:00:01Z",
                    "parentProcessId": 8420, "parentProcessCreationTime": "2026-01-02T03:00:00Z",
                    "parentProcessFileName": "parent.exe", "parentProcessFilePath": "C:/parents",
                    "ipAddress": "192.0.2.1", "url": "https://evidence.test/x",
                    "registryKey": "Key", "registryHive": "HKLM", "registryValueType": "String", "registryValue": "Value",
                    "accountName": "acct", "domainName": "EXAMPLE", "userSid": "S-1-5-21-1",
                    "aadUserId": "00000000-0000-0000-0000-000000000003", "userPrincipalName": "acct@example.test",
                    "detectionStatus": "Detected"
                }
            ]
        }
        """;

	private static string Envelope(string set, string items)
		=> $$"""{"@odata.context":"https://defender.test/api/$metadata#{{set}}","value":[{{items}}]}""";

	private static string Failure(string code, string message)
		=> $$$"""{"error":{"code":"{{{code}}}","message":"{{{message}}}","target":"corr-1"}}""";

	private static void AssertAlert(Alert a)
	{
		a.Id.Should().Be("da637000000000000000_1000000001");
		a.IncidentId.Should().Be(4242);
		a.InvestigationId.Should().Be(17);
		a.AssignedTo.Should().Be("analyst@example.test");
		a.Severity.Should().Be(AlertSeverity.Medium);
		a.Status.Should().Be(AlertStatus.InProgress);
		a.Classification.Should().Be("TruePositive");
		a.Determination.Should().Be("Malware");
		a.InvestigationState.Should().Be("Running");
		a.DetectionSource.Should().Be("WindowsDefenderAtp");
		a.DetectorId.Should().Be("00000000-0000-0000-0000-000000000001");
		a.Category.Should().Be("Execution");
		a.ThreatFamilyName.Should().Be("Family1");
		a.ThreatName.Should().Be("Threat1");
		a.Title.Should().Be("A made-up alert");
		a.Description.Should().Be("A made-up alert used by the tests.");
		a.AlertCreationTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero).AddTicks(123456));
		a.FirstEventTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 0, 0, TimeSpan.Zero));
		a.LastEventTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 1, 0, TimeSpan.Zero));
		a.LastUpdateTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 5, 0, TimeSpan.Zero));
		a.ResolvedTime.Should().Be(new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero));
		a.MachineId.Should().Be("1111111111111111111111111111111111111111");
		a.ComputerDnsName.Should().Be("pc-01.example.test");
		a.RbacGroupName.Should().Be("GroupA");
		a.AadTenantId.Should().Be("00000000-0000-0000-0000-000000000002");
		a.MitreTechniques.Should().Equal("T0001", "T0002");
		(a.RelatedUser!.UserName, a.RelatedUser.DomainName).Should().Be(("user1", "EXAMPLE"));
		var comment = a.Comments.Should().ContainSingle().Subject;
		(comment.Comment, comment.CreatedBy, comment.CreatedTime).Should().Be(("first note", "analyst@example.test", new DateTimeOffset(2026, 1, 2, 4, 0, 0, TimeSpan.Zero)));
		AssertEvidence(a.Evidence.Should().ContainSingle().Subject);
	}

	private static void AssertEvidence(AlertEvidence e)
	{
		(e.EntityType, e.Sha1, e.Sha256, e.FileName, e.FilePath).Should().Be(("Process", "aa", "bb", "tool.exe", "C:/tools"));
		e.EvidenceCreationTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 4, 6, TimeSpan.Zero));
		(e.ProcessId, e.ProcessCommandLine, e.ParentProcessId).Should().Be((3276L, "tool.exe --x", 8420L));
		e.ProcessCreationTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 0, 1, TimeSpan.Zero));
		e.ParentProcessCreationTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 0, 0, TimeSpan.Zero));
		(e.ParentProcessFileName, e.ParentProcessFilePath, e.IpAddress, e.Url).Should().Be(("parent.exe", "C:/parents", "192.0.2.1", "https://evidence.test/x"));
		(e.RegistryKey, e.RegistryHive, e.RegistryValueType, e.RegistryValue).Should().Be(("Key", "HKLM", "String", "Value"));
		(e.AccountName, e.DomainName, e.UserSid, e.AadUserId, e.UserPrincipalName, e.DetectionStatus)
			.Should().Be(("acct", "EXAMPLE", "S-1-5-21-1", "00000000-0000-0000-0000-000000000003", "acct@example.test", "Detected"));
	}

	[Fact]
	public async Task List_SendsGetAlerts_WithNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.ListAsync(null, t), Envelope("Alerts", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts");
		call.Authorization.Should().Be("Bearer token-1");
	}

	[Fact]
	public async Task List_SendsEveryOption_WithItsWireName()
	{
		var options = new AlertListOptions { Filter = "severity eq 'High' and title eq 'it''s'", Top = 10000, Skip = 20, Expand = AlertListOptions.ExpandEvidence };

		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.ListAsync(options, t), Envelope("Alerts", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts", "?$expand=evidence&$filter=severity%20eq%20%27High%27%20and%20title%20eq%20%27it%27%27s%27&$top=10000&$skip=20");
	}

	[Fact]
	public async Task List_MapsEveryField_AndTheEnvelope()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Alerts.ListAsync(null, t),
			$$"""{"@odata.context":"https://defender.test/api/$metadata#Alerts","value":[{{AlertJson}}],"@odata.nextLink":"https://defender.test/api/alerts?$skip=1"}""");

		page.Context.Should().Be("https://defender.test/api/$metadata#Alerts");
		page.NextLink.Should().Be("https://defender.test/api/alerts?$skip=1");
		AssertAlert(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Alerts.ListAsync(null, t),
			Envelope("Alerts", """{"id":"a1","severity":"Catastrophic","status":"Frozen","incidentId":"5","investigationId":null,"resolvedTime":"","mitreTechniques":null,"comments":null,"evidence":null,"relatedUser":null,"extra":{"unmodelled":1}}"""));

		var item = page.Value.Should().ContainSingle().Subject;
		item.Severity.Should().Be(AlertSeverity.Unknown);
		item.Status.Should().Be(AlertStatus.Unknown);
		item.IncidentId.Should().Be(5);
		item.InvestigationId.Should().BeNull();
		item.ResolvedTime.Should().BeNull();
		item.MitreTechniques.Should().BeEmpty();
		item.Comments.Should().BeEmpty();
		item.Evidence.Should().BeEmpty();
		item.RelatedUser.Should().BeNull();
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.ListAsync(null, t), HttpStatusCode.Forbidden, Failure("Forbidden", "Missing Alert.ReadWrite.All"), "Missing Alert.ReadWrite.All");

	[Fact]
	public async Task Get_SendsGetAlertsId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.GetAsync("da63_1", t), AlertJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts/da63_1");
	}

	[Fact]
	public async Task Get_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.GetAsync("a/b c", t), AlertJson);

		call.Uri.AbsolutePath.Should().Be("/api/alerts/a%2Fb%20c");
	}

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertAlert(await TestClient.ReadAsync((c, t) => c.Alerts.GetAsync("a1", t), AlertJson));

	[Fact]
	public Task Get_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.GetAsync("a1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "Alert a1 was not found"), "Alert a1 was not found");

	[Fact]
	public async Task GetAllAlerts_FollowsTheNextLink_AndKeepsTheOptions()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, $$"""{"value":[{"id":"a1"}],"@odata.nextLink":"https://defender.test/api/alerts?$skiptoken=2"}""");
		stub.Enqueue(HttpStatusCode.OK, Envelope("Alerts", """{"id":"a2"}"""));
		using var client = TestClient.Create(stub);

		var ids = new List<string?>();
		await foreach (var alert in client.GetAllAlertsAsync(new AlertListOptions { Expand = "evidence", Top = 1 }, TestContext.Current.CancellationToken))
		{
			ids.Add(alert.Id);
		}

		ids.Should().Equal("a1", "a2");
		stub.Calls.Select(c => c.Uri.Query).Should().Equal("?$expand=evidence&$top=1", "?$skiptoken=2");
	}
}
