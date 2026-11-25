using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The worked example of a group's tests: each operation pins its request exactly and maps a realistic response.</summary>
public class VulnerabilitiesTests
{
	// Shaped like the service's responses (field names and types), with neutral values.
	private const string VulnerabilityJson = """
		{
			"id": "CVE-2024-0001",
			"name": "CVE-2024-0001",
			"description": "A made-up vulnerability used by the tests.",
			"severity": "High",
			"cvssV3": 8.8,
			"cvssVector": "CVSS:3.1/AV:N/AC:L/PR:N/UI:R/S:U/C:H/I:H/A:H",
			"exposedMachines": 23,
			"publishedOn": "2024-07-30T00:00:00Z",
			"updatedOn": "2024-07-31T00:00:00Z",
			"firstDetected": "2024-07-31T01:55:47Z",
			"publicExploit": false,
			"exploitVerified": false,
			"exploitInKit": true,
			"exploitTypes": ["Remote"],
			"exploitUris": ["https://exploits.test/1"],
			"cveSupportability": "Supported",
			"tags": ["tag-a"],
			"epss": 0.632,
			"status": "RemediationRequired"
		}
		""";

	private static string Envelope(string set, string items)
		=> $$"""{"@odata.context":"https://defender.test/api/$metadata#{{set}}","value":[{{items}}]}""";

	private static void AssertVulnerability(Vulnerability v)
	{
		v.Id.Should().Be("CVE-2024-0001");
		v.Name.Should().Be("CVE-2024-0001");
		v.Description.Should().Be("A made-up vulnerability used by the tests.");
		v.Severity.Should().Be(VulnerabilitySeverity.High);
		v.CvssV3.Should().Be(8.8);
		v.CvssVector.Should().Be("CVSS:3.1/AV:N/AC:L/PR:N/UI:R/S:U/C:H/I:H/A:H");
		v.ExposedMachines.Should().Be(23);
		v.PublishedOn.Should().Be(new DateTimeOffset(2024, 7, 30, 0, 0, 0, TimeSpan.Zero));
		v.UpdatedOn.Should().Be(new DateTimeOffset(2024, 7, 31, 0, 0, 0, TimeSpan.Zero));
		v.FirstDetected.Should().Be(new DateTimeOffset(2024, 7, 31, 1, 55, 47, TimeSpan.Zero));
		v.PublicExploit.Should().BeFalse();
		v.ExploitVerified.Should().BeFalse();
		v.ExploitInKit.Should().BeTrue();
		v.ExploitTypes.Should().Equal("Remote");
		v.ExploitUris.Should().Equal("https://exploits.test/1");
		v.CveSupportability.Should().Be("Supported");
		v.Tags.Should().Equal("tag-a");
		v.Epss.Should().Be(0.632);
		v.Status.Should().Be("RemediationRequired");
	}

	[Fact]
	public async Task List_SendsGetVulnerabilities_WithNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Vulnerabilities.ListAsync(null, t), Envelope("Vulnerabilities", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/vulnerabilities");
		call.Authorization.Should().Be("Bearer token-1");
	}

	[Fact]
	public async Task List_SendsTheODataOptions_WithALiteralDollar()
	{
		var options = new ODataListOptions { Filter = "severity eq 'High' and cvssV3 ge 7", Top = 8000, Skip = 16000 };

		var call = await TestClient.CaptureAsync((c, t) => c.Vulnerabilities.ListAsync(options, t), Envelope("Vulnerabilities", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/vulnerabilities", "?$filter=severity%20eq%20%27High%27%20and%20cvssV3%20ge%207&$top=8000&$skip=16000");
	}

	[Fact]
	public async Task List_MapsEveryField_AndTheEnvelope()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Vulnerabilities.ListAsync(null, t),
			$$"""{"@odata.context":"https://defender.test/api/$metadata#Vulnerabilities","value":[{{VulnerabilityJson}}],"@odata.nextLink":"https://defender.test/api/vulnerabilities?$skiptoken=1"}""");

		page.Context.Should().Be("https://defender.test/api/$metadata#Vulnerabilities");
		page.NextLink.Should().Be("https://defender.test/api/vulnerabilities?$skiptoken=1");
		AssertVulnerability(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Vulnerabilities.ListAsync(null, t),
			Envelope("Vulnerabilities", """{"id":"CVE-1","severity":"Catastrophic","cvssV3":"7.5","exposedMachines":"4","publicExploit":"true","exploitTypes":null,"updatedOn":"","epss":null,"extra":{"unmodelled":1}}"""));

		var item = page.Value.Should().ContainSingle().Subject;
		item.Severity.Should().Be(VulnerabilitySeverity.Unknown);
		item.CvssV3.Should().Be(7.5);
		item.ExposedMachines.Should().Be(4);
		item.PublicExploit.Should().BeTrue();
		item.ExploitTypes.Should().BeEmpty();
		item.UpdatedOn.Should().BeNull();
		item.Epss.Should().BeNull();
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Vulnerabilities.ListAsync(null, t), HttpStatusCode.Forbidden,
			"""{"error":{"code":"Forbidden","message":"Missing Vulnerability.Read.All","target":"corr-1"}}""", "Missing Vulnerability.Read.All");

	[Fact]
	public async Task List_AnEmptyListMayBeA404()
	{
		using var client = TestClient.Create(TestClient.Stub("""{"error":{"code":"ResourceNotFound","message":"No vulnerabilities","target":"c"}}""", HttpStatusCode.NotFound));

		var act = () => client.Vulnerabilities.ListAsync(null, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.Code.Should().Be("ResourceNotFound");
	}

	[Fact]
	public async Task Get_SendsGetVulnerabilitiesCveId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Vulnerabilities.GetAsync("CVE-2024-0001", t), VulnerabilityJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/vulnerabilities/CVE-2024-0001");
	}

	[Fact]
	public async Task Get_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Vulnerabilities.GetAsync("a/b c", t), VulnerabilityJson);

		call.Uri.AbsolutePath.Should().Be("/api/vulnerabilities/a%2Fb%20c");
	}

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertVulnerability(await TestClient.ReadAsync((c, t) => c.Vulnerabilities.GetAsync("CVE-2024-0001", t), VulnerabilityJson));

	[Fact]
	public Task Get_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Vulnerabilities.GetAsync("CVE-0", t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"Vulnerability CVE-0 was not found","target":"c"}}""", "Vulnerability CVE-0 was not found");

	[Fact]
	public async Task ListMachineReferences_SendsGetVulnerabilitiesCveIdMachineReferences()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Vulnerabilities.ListMachineReferencesAsync("CVE-2024-0001", t), Envelope("MachineReferences", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/vulnerabilities/CVE-2024-0001/machineReferences");
	}

	[Fact]
	public async Task ListMachineReferences_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Vulnerabilities.ListMachineReferencesAsync("CVE-1", t),
			Envelope("MachineReferences", """{"id":"235a2e","computerDnsName":"pc-01.example.test","osPlatform":"Windows11","rbacGroupName":"GroupTwo"}"""));

		var reference = page.Value.Should().ContainSingle().Subject;
		(reference.Id, reference.ComputerDnsName, reference.OsPlatform, reference.RbacGroupName)
			.Should().Be(("235a2e", "pc-01.example.test", "Windows11", "GroupTwo"));
	}

	[Fact]
	public Task ListMachineReferences_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Vulnerabilities.ListMachineReferencesAsync("CVE-1", t), HttpStatusCode.Forbidden,
			"""{"error":{"code":"Forbidden","message":"Token expired","target":"c"}}""", "Token expired");

	[Fact]
	public async Task ListMachineVulnerabilities_SendsGetMachinesVulnerabilities_WithTheODataOptions()
	{
		var options = new ODataListOptions { Filter = "machineId eq 'abc'", Top = 10000 };

		var call = await TestClient.CaptureAsync((c, t) => c.Vulnerabilities.ListMachineVulnerabilitiesAsync(options, t), Envelope("Collection(microsoft.windowsDefenderATP.api.PublicAssetVulnerabilityDto)", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/vulnerabilities/machinesVulnerabilities", "?$filter=machineId%20eq%20%27abc%27&$top=10000");
	}

	[Fact]
	public async Task ListMachineVulnerabilities_WithNoOptions_SendsNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Vulnerabilities.ListMachineVulnerabilitiesAsync(null, t), Envelope("x", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/vulnerabilities/machinesVulnerabilities");
	}

	[Fact]
	public async Task ListMachineVulnerabilities_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Vulnerabilities.ListMachineVulnerabilitiesAsync(null, t),
			Envelope("x", """
				{"id":"m1-_-CVE-1-_-vendor-_-product-_-1.0-_-123","cveId":"CVE-1","machineId":"m1","fixingKbId":"123","productName":"product","productVendor":"vendor","productVersion":"1.0","severity":"Low"},
				{"id":"m2","cveId":"CVE-2","machineId":"m2","fixingKbId":null,"productName":"p","productVendor":"v","productVersion":"2","severity":"Critical"}
				"""));

		page.Value.Should().HaveCount(2);
		var first = page.Value[0];
		(first.Id, first.CveId, first.MachineId, first.FixingKbId, first.ProductName, first.ProductVendor, first.ProductVersion, first.Severity)
			.Should().Be(("m1-_-CVE-1-_-vendor-_-product-_-1.0-_-123", "CVE-1", "m1", "123", "product", "vendor", "1.0", VulnerabilitySeverity.Low));
		page.Value[1].FixingKbId.Should().BeNull();
		page.Value[1].Severity.Should().Be(VulnerabilitySeverity.Critical);
	}

	[Fact]
	public Task ListMachineVulnerabilities_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Vulnerabilities.ListMachineVulnerabilitiesAsync(null, t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"ODataError","message":"Invalid $filter","target":"c"}}""", "Invalid $filter");

	[Fact]
	public async Task NoOperation_FallsOutsideTheReadOnlyGuard_Because_AllAreGets()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Envelope("x", ""));
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		(await client.Vulnerabilities.ListAsync(null, TestContext.Current.CancellationToken)).Value.Should().BeEmpty();
	}
}
