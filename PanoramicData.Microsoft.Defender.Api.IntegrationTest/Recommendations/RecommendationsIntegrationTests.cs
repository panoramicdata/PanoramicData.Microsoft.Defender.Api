using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Recommendations;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Recommendations;

/// <summary>
/// Read-only checks of the recommendation operations against a live tenant (needs the <c>SecurityRecommendation.Read.All</c>,
/// <c>Vulnerability.Read.All</c> and <c>Software.Read.All</c> application permissions and onboarded devices). Live
/// verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class RecommendationsIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<Recommendation> FirstApplicationAsync()
	{
		var page = await fixture.Client.Recommendations.ListAsync(new ODataListOptions { Filter = "recommendationCategory eq 'Application'", Top = 50 }, Ct);
		return page.Value.FirstOrDefault(r => r.ExposedMachinesCount > 0)
			?? throw new InvalidOperationException("The tenant reports no application recommendation with exposed devices, so the by-recommendation operations cannot be checked. Use a tenant with onboarded devices.");
	}

	[Fact]
	public async Task ListAsync_ReturnsAPageOfRecommendations()
	{
		var page = await fixture.Client.Recommendations.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(5);
		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.Id) && !string.IsNullOrEmpty(r.RecommendationName));
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var page = await fixture.Client.Recommendations.ListAsync(new ODataListOptions { Filter = "remediationType eq 'Update'", Top = 5 }, Ct);

		page.Value.Should().OnlyContain(r => r.RemediationType == "Update");
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameRecommendationAsTheList()
	{
		var listed = await FirstApplicationAsync();

		var fetched = await fixture.Client.Recommendations.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.RecommendationName.Should().Be(listed.RecommendationName);
	}

	[Fact]
	public async Task ListMachineReferencesAsync_ReturnsTheDevices()
	{
		var listed = await FirstApplicationAsync();

		var page = await fixture.Client.Recommendations.ListMachineReferencesAsync(listed.Id!, Ct);

		page.Value.Should().NotBeEmpty().And.OnlyContain(m => !string.IsNullOrEmpty(m.Id));
	}

	[Fact]
	public async Task ListVulnerabilitiesAsync_ReturnsAPage()
	{
		var listed = await FirstApplicationAsync();

		var page = await fixture.Client.Recommendations.ListVulnerabilitiesAsync(listed.Id!, Ct);

		page.Value.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Id));
	}

	[Fact]
	public async Task GetSoftwareAsync_ReturnsTheRelatedProduct()
	{
		var listed = await FirstApplicationAsync();

		var product = await fixture.Client.Recommendations.GetSoftwareAsync(listed.Id!, Ct);

		product.Id.Should().NotBeNullOrEmpty("the documented response is one product; an empty read would mean the service answers with a collection");
	}

	[Fact]
	public async Task GetAllRecommendationsAsync_FollowsTheNextLink()
	{
		var seen = await LiveChecks.CountAsync(fixture.Client.GetAllRecommendationsAsync(new ODataListOptions { Top = 2 }, Ct), 5, recommendation => recommendation.Id);

		seen.Should().BeGreaterThan(2, "more than one page of two was read, so the next link was followed");
	}

	[Fact]
	public async Task AnUnknownRecommendation_IsRaisedAsAnApiException()
	{
		await LiveChecks.ShouldBeNotFoundAsync(() => fixture.Client.Recommendations.GetAsync("va-_-no-such-_-product", Ct));
	}
}
