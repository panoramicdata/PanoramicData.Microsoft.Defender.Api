using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>What a device holds or is exposed to: logged-on users, alerts, vulnerabilities, software, missing KBs and recommendations.</summary>
public partial class MachinesTests
{
	private static string Path(string suffix) => $"/api/machines/{Id}/{suffix}";

	[Fact]
	public async Task ListLogOnUsers_SendsGetMachinesIdLogonusers()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListLogOnUsersAsync(Id, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, Path("logonusers"));
	}

	[Fact]
	public async Task ListLogOnUsers_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListLogOnUsersAsync(Id, t),
			Envelope("""{"id":"example\\user1","accountName":"user1","accountDomain":"example","firstSeen":"2024-01-01T00:00:00Z","lastSeen":"2024-01-02T00:00:00Z","logonTypes":"Interactive","isDomainAdmin":"true","isOnlyNetworkUser":false}"""));

		var u = page.Value.Should().ContainSingle().Subject;
		(u.Id, u.AccountName, u.AccountDomain, u.LogonTypes).Should().Be((@"example\user1", "user1", "example", "Interactive"));
		(u.FirstSeen, u.LastSeen).Should().Be((new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero)));
		(u.IsDomainAdmin, u.IsOnlyNetworkUser).Should().Be((true, false));
	}

	[Fact]
	public Task ListLogOnUsers_MapsAnUnknownDeviceAs404()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.ListLogOnUsersAsync(Id, t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"No such device","target":"c"}}""", "No such device");

	[Fact]
	public async Task GetAllMachineLogOnUsers_EnumeratesThePages()
	{
		using var client = TestClient.Create(TestClient.Stub(Envelope("""{"id":"u1"},{"id":"u2"}""")));

		var ids = await client.GetAllMachineLogOnUsersAsync(Id, TestContext.Current.CancellationToken).Select(u => u.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("u1", "u2");
	}

	[Fact]
	public async Task ListAlerts_SendsGetMachinesIdAlerts()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListAlertsAsync(Id, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, Path("alerts"));
	}

	[Fact]
	public async Task ListAlerts_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListAlertsAsync(Id, t), Envelope("""
			{"id":"da1_1","incidentId":"12","investigationId":34,"assignedTo":"analyst@example.test","severity":"High","status":"New","classification":"TruePositive","determination":"Malware","investigationState":"Running","detectionSource":"WindowsDefenderAv","category":"Malware","threatFamilyName":"Family","title":"A title","description":"A description","alertCreationTime":"2024-01-01T00:00:00Z","firstEventTime":"2024-01-01T01:00:00Z","lastEventTime":"2024-01-01T02:00:00Z","lastUpdateTime":"2024-01-01T03:00:00Z","resolvedTime":"2024-01-01T04:00:00Z","machineId":"m1","computerDnsName":"pc-01.example.test","rbacGroupName":"GroupOne","mitreTechniques":["T1059"]}
			"""));

		var a = page.Value.Should().ContainSingle().Subject;
		(a.Id, a.IncidentId, a.InvestigationId, a.AssignedTo).Should().Be(("da1_1", 12L, 34L, "analyst@example.test"));
		(a.Severity, a.Status, a.Classification, a.Determination, a.InvestigationState).Should().Be(("High", "New", "TruePositive", "Malware", "Running"));
		(a.DetectionSource, a.Category, a.ThreatFamilyName, a.Title, a.Description).Should().Be(("WindowsDefenderAv", "Malware", "Family", "A title", "A description"));
		(a.MachineId, a.ComputerDnsName, a.RbacGroupName).Should().Be(("m1", "pc-01.example.test", "GroupOne"));
		new[] { a.AlertCreationTime, a.FirstEventTime, a.LastEventTime, a.LastUpdateTime, a.ResolvedTime }
			.Should().Equal(Enumerable.Range(0, 5).Select(h => (DateTimeOffset?)new DateTimeOffset(2024, 1, 1, h, 0, 0, TimeSpan.Zero)));
		a.MitreTechniques.Should().Equal("T1059");
	}

	[Fact]
	public Task ListAlerts_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.ListAlertsAsync(Id, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task GetAllMachineAlerts_EnumeratesThePages()
	{
		using var client = TestClient.Create(TestClient.Stub(Envelope("""{"id":"a1"}""")));

		var ids = await client.GetAllMachineAlertsAsync(Id, TestContext.Current.CancellationToken).Select(a => a.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("a1");
	}

	[Fact]
	public async Task ListVulnerabilities_SendsGetMachinesIdVulnerabilities()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListVulnerabilitiesAsync(Id, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, Path("vulnerabilities"));
	}

	[Fact]
	public async Task ListVulnerabilities_MapsTheSharedVulnerabilityModel()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListVulnerabilitiesAsync(Id, t),
			Envelope("""{"id":"CVE-2024-0001","name":"CVE-2024-0001","severity":"Critical","cvssV3":9.8,"exposedMachines":"3"}"""));

		var v = page.Value.Should().ContainSingle().Subject;
		(v.Id, v.Severity, v.CvssV3, v.ExposedMachines).Should().Be(("CVE-2024-0001", VulnerabilitySeverity.Critical, 9.8, 3L));
	}

	[Fact]
	public Task ListVulnerabilities_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.ListVulnerabilitiesAsync(Id, t), HttpStatusCode.TooManyRequests,
			"""{"error":{"code":"TooManyRequests","message":"Slow down","target":"c"}}""", "Slow down");

	[Fact]
	public async Task GetAllMachineVulnerabilitiesByMachine_EnumeratesThePages()
	{
		using var client = TestClient.Create(TestClient.Stub(Envelope("""{"id":"CVE-1"}""")));

		var ids = await client.GetAllMachineVulnerabilitiesByMachineAsync(Id, TestContext.Current.CancellationToken).Select(v => v.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("CVE-1");
	}

	[Fact]
	public async Task ListSoftware_SendsGetMachinesIdSoftware()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListSoftwareAsync(Id, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, Path("software"));
	}

	[Fact]
	public async Task ListSoftware_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListSoftwareAsync(Id, t),
			Envelope("""{"id":"vendor-_-product","name":"product","vendor":"vendor","weaknesses":4,"publicExploit":true,"activeAlert":false,"exposedMachines":2,"installedMachines":"9","impactScore":"1.5","isNormalized":true}"""));

		var s = page.Value.Should().ContainSingle().Subject;
		(s.Id, s.Name, s.Vendor, s.Weaknesses).Should().Be(("vendor-_-product", "product", "vendor", 4L));
		(s.PublicExploit, s.ActiveAlert, s.IsNormalized).Should().Be((true, false, true));
		(s.ExposedMachines, s.InstalledMachines, s.ImpactScore).Should().Be((2L, 9L, 1.5));
	}

	[Fact]
	public Task ListSoftware_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.ListSoftwareAsync(Id, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task GetAllMachineSoftware_EnumeratesThePages()
	{
		using var client = TestClient.Create(TestClient.Stub(Envelope("""{"id":"s1"}""")));

		var ids = await client.GetAllMachineSoftwareAsync(Id, TestContext.Current.CancellationToken).Select(s => s.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("s1");
	}

	[Fact]
	public async Task ListMissingKbs_SendsGetMachinesIdGetmissingkbs()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListMissingKbsAsync(Id, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, Path("getmissingkbs"));
	}

	[Fact]
	public async Task ListMissingKbs_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListMissingKbsAsync(Id, t),
			Envelope("""{"id":"4540673","name":"A security update","osBuild":"22621","productsNames":["windows_10","edge"],"url":"https://catalog.test/KB4540673","machineMissedOn":1,"cveAddressed":97}"""));

		var kb = page.Value.Should().ContainSingle().Subject;
		(kb.Id, kb.Name, kb.OsBuild, kb.Url, kb.MachineMissedOn, kb.CveAddressed).Should().Be(("4540673", "A security update", 22621L, "https://catalog.test/KB4540673", 1L, 97L));
		kb.ProductsNames.Should().Equal("windows_10", "edge");
	}

	[Fact]
	public Task ListMissingKbs_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.ListMissingKbsAsync(Id, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task GetAllMachineMissingKbs_EnumeratesThePages()
	{
		using var client = TestClient.Create(TestClient.Stub(Envelope("""{"id":"k1"}""")));

		var ids = await client.GetAllMachineMissingKbsAsync(Id, TestContext.Current.CancellationToken).Select(k => k.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("k1");
	}

	[Fact]
	public async Task ListRecommendations_SendsGetMachinesIdRecommendations()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListRecommendationsAsync(Id, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, Path("recommendations"));
	}

	[Fact]
	public async Task ListRecommendations_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListRecommendationsAsync(Id, t), Envelope("""
			{"id":"va-_-vendor-_-product","productName":"product","recommendationName":"Update product","weaknesses":3,"vendor":"vendor","recommendedVersion":"2.0","recommendationCategory":"Application","severityScore":"4.5","publicExploit":true,"activeAlert":false,"remediationType":"Update","status":"Active","configScoreImpact":1.25,"exposureImpact":0.5,"totalMachineCount":10,"exposedMachinesCount":4}
			"""));

		var r = page.Value.Should().ContainSingle().Subject;
		(r.Id, r.ProductName, r.RecommendationName, r.Vendor, r.RecommendedVersion).Should().Be(("va-_-vendor-_-product", "product", "Update product", "vendor", "2.0"));
		(r.RecommendationCategory, r.RemediationType, r.Status, r.Weaknesses).Should().Be(("Application", "Update", "Active", 3L));
		(r.SeverityScore, r.ConfigScoreImpact, r.ExposureImpact).Should().Be((4.5, 1.25, 0.5));
		(r.PublicExploit, r.ActiveAlert, r.TotalMachineCount, r.ExposedMachinesCount).Should().Be((true, false, 10L, 4L));
	}

	[Fact]
	public Task ListRecommendations_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.ListRecommendationsAsync(Id, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task GetAllMachineRecommendations_EnumeratesThePages()
	{
		using var client = TestClient.Create(TestClient.Stub(Envelope("""{"id":"r1"}""")));

		var ids = await client.GetAllMachineRecommendationsAsync(Id, TestContext.Current.CancellationToken).Select(r => r.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("r1");
	}
}
