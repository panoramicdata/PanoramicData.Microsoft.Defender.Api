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
	{
		var page = await fixture.Client.Software.ListAsync(new ODataListOptions { Top = 100 }, Ct);
		return page.Value.FirstOrDefault(p => p.ExposedMachines > 0)
			?? throw new InvalidOperationException("The tenant reports no software with exposed devices among the first 100, so the by-product operations cannot be checked. Use a tenant with onboarded devices.");
	}

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

		page.Value.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(5);
		page.Value.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Id) && !string.IsNullOrEmpty(p.Name));
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

		var page = await fixture.Client.Software.ListDistributionsAsync(listed.Id!, Ct);

		page.Value.Should().NotBeEmpty().And.OnlyContain(d => !string.IsNullOrEmpty(d.Version));
	}

	[Fact]
	public async Task ListMachineReferencesAsync_ReturnsTheDevices()
	{
		var listed = await FirstExposedAsync();

		var page = await fixture.Client.Software.ListMachineReferencesAsync(listed.Id!, Ct);

		page.Value.Should().NotBeEmpty().And.OnlyContain(m => !string.IsNullOrEmpty(m.Id));
	}

	[Fact]
	public async Task ListVulnerabilitiesAsync_ReturnsAPage()
	{
		var listed = await FirstExposedAsync();

		var page = await fixture.Client.Software.ListVulnerabilitiesAsync(listed.Id!, Ct);

		page.Value.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Id));
	}

	[Fact]
	public async Task ListMissingKbsAsync_AcceptsAProductId()
	{
		var listed = await FirstExposedAsync();

		var page = await fixture.Client.Software.ListMissingKbsAsync(listed.Id!, Ct);

		page.Value.Should().OnlyContain(kb => !string.IsNullOrEmpty(kb.Id));
	}

	[Fact]
	public async Task GetAllSoftwareAsync_FollowsTheNextLink()
	{
		var seen = 0;

		await foreach (var product in fixture.Client.GetAllSoftwareAsync(new ODataListOptions { Top = 2 }, Ct))
		{
			product.Id.Should().NotBeNullOrEmpty();
			if (++seen == 5)
			{
				break;
			}
		}

		seen.Should().BeGreaterThan(2, "more than one page of two was read, so the next link was followed");
	}

	[Fact]
	public async Task AnUnknownProduct_IsRaisedAsAnApiException()
	{
		var act = () => fixture.Client.Software.GetAsync("no-such-_-product", Ct);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
