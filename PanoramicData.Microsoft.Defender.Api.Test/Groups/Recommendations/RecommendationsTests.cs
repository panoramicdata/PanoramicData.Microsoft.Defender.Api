using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The security recommendation operations: each pins its request and maps a realistic response.</summary>
public class RecommendationsTests
{
	private const string RecommendationJson = """
		{"id":"va-_-vendor-_-product","productName":"product","recommendationName":"Update product","weaknesses":762,"vendor":"vendor","recommendedVersion":"127.0","recommendedVendor":"rv","recommendedProgram":"rp","recommendationCategory":"Application","subCategory":"Sub","severityScore":7.5,"publicExploit":true,"activeAlert":false,"associatedThreats":["thr-1"],"remediationType":"Update","status":"Active","configScoreImpact":1.5,"exposureImpact":1.17,"totalMachineCount":261,"exposedMachinesCount":193,"nonProductivityImpactedAssets":2,"relatedComponent":"Component","hasUnpatchableCve":true,"tags":["internetFacing"],"exposedCriticalDevices":116}
		""";

	private static void AssertRecommendation(Models.Recommendations.Recommendation r)
	{
		(r.Id, r.ProductName, r.RecommendationName, r.Weaknesses, r.Vendor).Should().Be(("va-_-vendor-_-product", "product", "Update product", 762L, "vendor"));
		(r.RecommendedVersion, r.RecommendedVendor, r.RecommendedProgram).Should().Be(("127.0", "rv", "rp"));
		(r.RecommendationCategory, r.SubCategory, r.SeverityScore, r.PublicExploit, r.ActiveAlert).Should().Be(("Application", "Sub", 7.5, true, false));
		r.AssociatedThreats.Should().Equal("thr-1");
		(r.RemediationType, r.Status, r.ConfigScoreImpact, r.ExposureImpact).Should().Be(("Update", "Active", 1.5, 1.17));
		(r.TotalMachineCount, r.ExposedMachinesCount, r.NonProductivityImpactedAssets, r.ExposedCriticalDevices).Should().Be((261L, 193L, 2L, 116L));
		(r.RelatedComponent, r.HasUnpatchableCve).Should().Be(("Component", true));
		r.Tags.Should().Equal("internetFacing");
	}

	[Fact]
	public Task List_SendsGetRecommendations()
		=> ShouldGetAsync((c, t) => c.Recommendations.ListAsync(null, t), Envelope(""), "/api/recommendations");

	[Fact]
	public Task List_SendsTheODataOptions()
		=> ShouldGetAsync((c, t) => c.Recommendations.ListAsync(new ODataListOptions { Filter = "status eq 'Active'", Top = 10, Skip = 20 }, t), Envelope(""),
			"/api/recommendations", "?$filter=status%20eq%20%27Active%27&$top=10&$skip=20");

	[Fact]
	public async Task List_MapsEveryField_AndToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Recommendations.ListAsync(null, t),
			Envelope(RecommendationJson + ",{\"id\":\"r2\",\"associatedThreats\":null,\"tags\":null,\"severityScore\":\"3\",\"publicExploit\":\"true\",\"exposureImpact\":null}"));

		AssertRecommendation(page.Value[0]);
		var loose = page.Value[1];
		loose.AssociatedThreats.Should().BeEmpty();
		loose.Tags.Should().BeEmpty();
		(loose.SeverityScore, loose.PublicExploit, loose.ExposureImpact).Should().Be((3.0, true, (double?)null));
	}

	[Fact]
	public Task List_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Recommendations.ListAsync(null, t));

	[Fact]
	public Task Get_SendsGetRecommendationsId()
		=> ShouldGetAsync((c, t) => c.Recommendations.GetAsync("va-_-google-_-chrome", t), RecommendationJson, "/api/recommendations/va-_-google-_-chrome");

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertRecommendation(await TestClient.ReadAsync((c, t) => c.Recommendations.GetAsync("x", t), RecommendationJson));

	[Fact]
	public Task Get_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Recommendations.GetAsync("x", t));

	[Fact]
	public Task ListMachineReferences_SendsGetRecommendationsIdMachineReferences()
		=> ShouldGetAsync((c, t) => c.Recommendations.ListMachineReferencesAsync("r1", t), Envelope(""), "/api/recommendations/r1/machineReferences");

	[Fact]
	public async Task ListMachineReferences_MapsTheSharedReferenceModel()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Recommendations.ListMachineReferencesAsync("r1", t),
			Envelope("""{"id":"m1","computerDnsName":"niw_pc","osPlatform":"Windows10","rbacGroupName":"GroupTwo"}"""));

		var m = page.Value.Should().ContainSingle().Subject;
		(m.Id, m.ComputerDnsName, m.OsPlatform, m.RbacGroupName).Should().Be(("m1", "niw_pc", "Windows10", "GroupTwo"));
	}

	[Fact]
	public Task ListMachineReferences_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Recommendations.ListMachineReferencesAsync("r1", t));

	[Fact]
	public Task ListVulnerabilities_SendsGetRecommendationsIdVulnerabilities()
		=> ShouldGetAsync((c, t) => c.Recommendations.ListVulnerabilitiesAsync("r1", t), Envelope(""), "/api/recommendations/r1/vulnerabilities");

	[Fact]
	public async Task ListVulnerabilities_MapsTheSharedVulnerabilityModel()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Recommendations.ListVulnerabilitiesAsync("r1", t),
			Envelope("""{"id":"CVE-2024-0002","severity":"Critical","cvssV3":9.8,"exposedMachines":3}"""));

		var v = page.Value.Should().ContainSingle().Subject;
		(v.Id, v.Severity, v.CvssV3, v.ExposedMachines).Should().Be(("CVE-2024-0002", Models.Vulnerabilities.VulnerabilitySeverity.Critical, 9.8, 3L));
	}

	[Fact]
	public Task ListVulnerabilities_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Recommendations.ListVulnerabilitiesAsync("r1", t));

	[Fact]
	public Task GetSoftware_SendsGetRecommendationsIdSoftware()
		=> ShouldGetAsync((c, t) => c.Recommendations.GetSoftwareAsync("va-_-google-_-chrome", t), SoftwareTests.ProductJson, "/api/recommendations/va-_-google-_-chrome/software");

	[Fact]
	public async Task GetSoftware_MapsTheSharedProductModel()
		=> SoftwareTests.AssertProduct(await TestClient.ReadAsync((c, t) => c.Recommendations.GetSoftwareAsync("r1", t), SoftwareTests.ProductJson));

	[Fact]
	public Task GetSoftware_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Recommendations.GetSoftwareAsync("r1", t));

	[Fact]
	public async Task GetAllHelpers_WireTheirOperations()
	{
		await ShouldEnumerateAsync((c, t) => c.GetAllRecommendationsAsync(new ODataListOptions { Top = 1 }, t), "/api/recommendations", "?$top=1");
		await ShouldEnumerateAsync((c, t) => c.GetAllRecommendationMachineReferencesAsync("r1", t), "/api/recommendations/r1/machineReferences");
		await ShouldEnumerateAsync((c, t) => c.GetAllRecommendationVulnerabilitiesAsync("r1", t), "/api/recommendations/r1/vulnerabilities");
	}
}
