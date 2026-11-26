using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Software;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The software inventory operations: each pins its request and maps a realistic response.</summary>
public class SoftwareTests
{
	internal const string ProductJson = """
		{"id":"vendor-_-product","name":"product","vendor":"vendor","weaknesses":467,"publicExploit":true,"activeAlert":false,"exposedMachines":172,"impactScore":2.39947438}
		""";

	internal static void AssertProduct(SoftwareProduct p)
	{
		(p.Id, p.Name, p.Vendor, p.Weaknesses, p.PublicExploit, p.ActiveAlert, p.ExposedMachines, p.ImpactScore)
			.Should().Be(("vendor-_-product", "product", "vendor", 467L, true, false, 172L, 2.39947438));
	}

	[Fact]
	public Task List_SendsGetSoftware()
		=> ShouldGetAsync((c, t) => c.Software.ListAsync(null, t), Envelope(""), "/api/Software");

	[Fact]
	public Task List_SendsTheODataOptions()
		=> ShouldGetAsync((c, t) => c.Software.ListAsync(new ODataListOptions { Filter = "vendor eq 'o''neil co'", Top = 10000, Skip = 5 }, t), Envelope(""),
			"/api/Software", "?$filter=vendor%20eq%20%27o%27%27neil%20co%27&$top=10000&$skip=5");

	[Fact]
	public async Task List_MapsEveryField_AndToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Software.ListAsync(null, t),
			Envelope(ProductJson + ",{\"id\":\"p2\",\"weaknesses\":\"4\",\"publicExploit\":\"false\",\"impactScore\":null,\"exposedMachines\":\"\"}"));

		AssertProduct(page.Value[0]);
		var loose = page.Value[1];
		(loose.Weaknesses, loose.PublicExploit, loose.ImpactScore, loose.ExposedMachines).Should().Be((4L, false, (double?)null, (long?)null));
	}

	[Fact]
	public Task List_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Software.ListAsync(null, t));

	[Fact]
	public Task Get_SendsGetSoftwareId()
		=> ShouldGetAsync((c, t) => c.Software.GetAsync("vendor-_-product", t), ProductJson, "/api/Software/vendor-_-product");

	[Fact]
	public async Task Get_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Software.GetAsync("a/b c", t), ProductJson);

		call.Uri.AbsolutePath.Should().Be("/api/Software/a%2Fb%20c");
	}

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertProduct(await TestClient.ReadAsync((c, t) => c.Software.GetAsync("x", t), ProductJson));

	[Fact]
	public Task Get_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Software.GetAsync("x", t));

	[Fact]
	public Task ListDistributions_SendsGetSoftwareIdDistributions()
		=> ShouldGetAsync((c, t) => c.Software.ListDistributionsAsync("vendor-_-product", t), Envelope(""), "/api/Software/vendor-_-product/distributions");

	[Fact]
	public async Task ListDistributions_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Software.ListDistributionsAsync("x", t),
			Envelope("""{"version":"11.0.17134.1039","installations":1,"vulnerabilities":11},{"version":"2","installations":"750","vulnerabilities":null}"""));

		(page.Value[0].Version, page.Value[0].Installations, page.Value[0].Vulnerabilities).Should().Be(("11.0.17134.1039", 1L, 11L));
		(page.Value[1].Version, page.Value[1].Installations, page.Value[1].Vulnerabilities).Should().Be(("2", 750L, (long?)null));
	}

	[Fact]
	public Task ListDistributions_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Software.ListDistributionsAsync("x", t));

	[Fact]
	public Task ListMachineReferences_SendsGetSoftwareIdMachineReferences()
		=> ShouldGetAsync((c, t) => c.Software.ListMachineReferencesAsync("vendor-_-product", t), Envelope(""), "/api/Software/vendor-_-product/machineReferences");

	[Fact]
	public async Task ListMachineReferences_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Software.ListMachineReferencesAsync("x", t),
			Envelope("""{"id":"m1","computerDnsName":"pc-01.example.test","osPlatform":"Windows11","rbacGroupName":"GroupTwo"}"""));

		var m = page.Value.Should().ContainSingle().Subject;
		(m.Id, m.ComputerDnsName, m.OsPlatform, m.RbacGroupName).Should().Be(("m1", "pc-01.example.test", "Windows11", "GroupTwo"));
	}

	[Fact]
	public Task ListMachineReferences_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Software.ListMachineReferencesAsync("x", t));

	[Fact]
	public Task ListVulnerabilities_SendsGetSoftwareIdVulnerabilities()
		=> ShouldGetAsync((c, t) => c.Software.ListVulnerabilitiesAsync("vendor-_-product", t), Envelope(""), "/api/Software/vendor-_-product/vulnerabilities");

	[Fact]
	public async Task ListVulnerabilities_MapsTheSharedVulnerabilityModel()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Software.ListVulnerabilitiesAsync("x", t),
			Envelope("""{"id":"CVE-2024-0001","name":"CVE-2024-0001","severity":"Medium","cvssV3":4.2,"exposedMachines":1,"publishedOn":"2017-03-14T00:00:00Z","exploitTypes":[]}"""));

		var v = page.Value.Should().ContainSingle().Subject;
		(v.Id, v.Severity, v.CvssV3, v.ExposedMachines, v.PublishedOn).Should().Be(("CVE-2024-0001", Models.Vulnerabilities.VulnerabilitySeverity.Medium, 4.2, 1L, new DateTimeOffset(2017, 3, 14, 0, 0, 0, TimeSpan.Zero)));
	}

	[Fact]
	public Task ListVulnerabilities_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Software.ListVulnerabilitiesAsync("x", t));

	[Fact]
	public Task ListMissingKbs_SendsGetSoftwareIdGetMissingKbs()
		=> ShouldGetAsync((c, t) => c.Software.ListMissingKbsAsync("vendor-_-product", t), Envelope(""), "/api/Software/vendor-_-product/getmissingkbs");

	[Fact]
	public async Task ListMissingKbs_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Software.ListMissingKbsAsync("x", t),
			Envelope("""{"id":"4540673","name":"March Security Updates","productsNames":["edge"],"url":"https://catalog.test/KB4540673","machineMissedOn":240,"cveAddressed":14},{"id":"2","productsNames":null}"""));

		var kb = page.Value[0];
		(kb.Id, kb.Name, kb.Url, kb.MachineMissedOn, kb.CveAddressed).Should().Be(("4540673", "March Security Updates", "https://catalog.test/KB4540673", 240L, 14L));
		kb.ProductsNames.Should().Equal("edge");
		page.Value[1].ProductsNames.Should().BeEmpty();
	}

	[Fact]
	public Task ListMissingKbs_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Software.ListMissingKbsAsync("x", t));

	[Fact]
	public async Task GetAllHelpers_WireTheirOperations()
	{
		await ShouldEnumerateAsync((c, t) => c.GetAllSoftwareAsync(new ODataListOptions { Top = 3 }, t), "/api/Software", "?$top=3");
		await ShouldEnumerateAsync((c, t) => c.GetAllSoftwareMachineReferencesAsync("p", t), "/api/Software/p/machineReferences");
		await ShouldEnumerateAsync((c, t) => c.GetAllSoftwareVulnerabilitiesAsync("p", t), "/api/Software/p/vulnerabilities");
	}

	[Fact]
	public async Task GetAllSoftware_FollowsTheNextLink()
	{
		var stub = new StubHandler();
		stub.Enqueue(System.Net.HttpStatusCode.OK, Envelope("{\"id\":\"a\"}", "https://defender.test/api/Software?$skiptoken=1"));
		stub.Enqueue(System.Net.HttpStatusCode.OK, Envelope("{\"id\":\"b\"}"));
		using var client = TestClient.Create(stub);

		var ids = (await client.GetAllSoftwareAsync(null, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken)).Select(p => p.Id);

		ids.Should().Equal("a", "b");
	}
}
