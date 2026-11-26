using PanoramicData.MicrosoftDefender.Api.Models;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.SecurityBaselines;

/// <summary>
/// Read-only checks of the security baseline operations against a live tenant (needs the
/// <c>SecurityBaselinesAssessment.Read.All</c> application permission, assumed because the documentation names none, and at
/// least one baseline profile). Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class SecurityBaselinesIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task ListProfilesAsync_ReturnsTheProfiles()
	{
		var page = await fixture.Client.SecurityBaselines.ListProfilesAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty("the tenant needs at least one baseline profile");
		page.Value.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Id) && !string.IsNullOrEmpty(p.Name));
	}

	[Fact]
	public async Task ListConfigurationsAsync_ReturnsTheAssessedConfigurations()
	{
		var page = await fixture.Client.SecurityBaselines.ListConfigurationsAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty("the tenant needs an active baseline profile");
		page.Value.Should().OnlyContain(c => !string.IsNullOrEmpty(c.Id) && !string.IsNullOrEmpty(c.Name));
	}

	[Fact]
	public async Task GetAllBaselineProfilesAsync_EnumeratesProfiles()
	{
		var seen = 0;

		await foreach (var profile in fixture.Client.GetAllBaselineProfilesAsync(new ODataListOptions { Top = 2 }, Ct))
		{
			profile.Id.Should().NotBeNullOrEmpty();
			seen++;
		}

		seen.Should().BeGreaterThan(0);
	}
}
