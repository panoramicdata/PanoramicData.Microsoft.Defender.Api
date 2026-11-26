using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Software;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Software;

/// <summary>
/// Read-only checks of the software operations against a live tenant (needs the <c>Software.Read.All</c> and
/// <c>Vulnerability.Read.All</c> application permissions and onboarded devices). Live verification is pending: see
/// docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class SoftwareIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<SoftwareProduct> FirstExposedAsync()
		=> LiveChecks.FirstOrThrow(
			await fixture.Client.Software.ListAsync(new ODataListOptions { Top = 100 }, Ct),
			p => p.ExposedMachines > 0,
			"The tenant reports no software with exposed devices among the first 100, so the by-product operations cannot be checked. Use a tenant with onboarded devices.");

	/// <summary>An OData filter matching one vendor exactly; a single quote in the name is escaped by doubling it.</summary>
	private static string VendorFilter(string vendor)
	{
		var escaped = vendor.Replace("'", "''", StringComparison.Ordinal);
		return string.Concat("vendor eq '", escaped, "'");
	}

	[Fact]
	public async Task ListAsync_ReturnsAPageOfProducts()
	{
		var page = await fixture.Client.Software.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		LiveChecks.ShouldBeACompletePage(page, 5, p => !string.IsNullOrEmpty(p.Id) && !string.IsNullOrEmpty(p.Name));
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var first = await FirstExposedAsync();

		var page = await fixture.Client.Software.ListAsync(new ODataListOptions { Filter = VendorFilter(first.Vendor!), Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty().And.OnlyContain(p => p.Vendor == first.Vendor);
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameProductAsTheList()
	{
		var listed = await FirstExposedAsync();

		var fetched = await fixture.Client.Software.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.Name.Should().Be(listed.Name);
	}

	[Fact]
	public async Task ListDistributionsAsync_ReturnsVersions()
	{
		var listed = await FirstExposedAsync();

		await LiveChecks.ShouldListIdentifiedItemsAsync(fixture.Client.Software.ListDistributionsAsync, listed.Id!, d => d.Version, Ct, requireItems: true);
	}

	[Fact]
	public async Task ListMachineReferencesAsync_ReturnsTheDevices()
	{
		var listed = await FirstExposedAsync();

		await LiveChecks.ShouldListIdentifiedItemsAsync(fixture.Client.Software.ListMachineReferencesAsync, listed.Id!, m => m.Id, Ct, requireItems: true);
	}

	[Fact]
	public async Task ListVulnerabilitiesAsync_ReturnsAPage()
	{
		var listed = await FirstExposedAsync();

		await LiveChecks.ShouldListIdentifiedItemsAsync(fixture.Client.Software.ListVulnerabilitiesAsync, listed.Id!, v => v.Id, Ct);
	}

	[Fact]
	public async Task ListMissingKbsAsync_AcceptsAProductId()
	{
		var listed = await FirstExposedAsync();

		await LiveChecks.ShouldListIdentifiedItemsAsync(fixture.Client.Software.ListMissingKbsAsync, listed.Id!, kb => kb.Id, Ct);
	}

	[Fact]
	public async Task GetAllSoftwareAsync_FollowsTheNextLink()
	{
		var seen = await LiveChecks.CountAsync(fixture.Client.GetAllSoftwareAsync(new ODataListOptions { Top = 2 }, Ct), 5, product => product.Id);

		seen.Should().BeGreaterThan(2, "more than one page of two was read, so the next link was followed");
	}

	[Fact]
	public async Task AnUnknownProduct_IsRaisedAsAnApiException()
	{
		await LiveChecks.ShouldBeNotFoundAsync(() => fixture.Client.Software.GetAsync("no-such-_-product", Ct));
	}
}
