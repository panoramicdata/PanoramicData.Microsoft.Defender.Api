using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Vulnerabilities;

/// <summary>
/// Read-only checks of the vulnerabilities operations against a live tenant (needs the <c>Vulnerability.Read.All</c>
/// application permission and at least one exposed vulnerability). Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class VulnerabilitiesIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<Vulnerability> FirstExposedAsync()
	{
		var page = await fixture.Client.Vulnerabilities.ListAsync(new ODataListOptions { Top = 100 }, Ct);
		return page.Value.FirstOrDefault(v => v.ExposedMachines > 0)
			?? throw new InvalidOperationException("The tenant reports no vulnerability with exposed machines among the first 100, so the by-CVE operations cannot be checked. Use a tenant with onboarded devices.");
	}

	[Fact]
	public async Task ListAsync_ReturnsAPageOfVulnerabilities()
	{
		var page = await fixture.Client.Vulnerabilities.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(5);
		page.Value.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Id));
		page.Context.Should().Contain("Vulnerabilities");
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var page = await fixture.Client.Vulnerabilities.ListAsync(new ODataListOptions { Filter = "severity eq 'Critical'", Top = 5 }, Ct);

		page.Value.Should().OnlyContain(v => v.Severity == VulnerabilitySeverity.Critical);
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameVulnerabilityAsTheList()
	{
		var listed = await FirstExposedAsync();

		var fetched = await fixture.Client.Vulnerabilities.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.Severity.Should().Be(listed.Severity);
	}

	[Fact]
	public async Task ListMachineReferencesAsync_ReturnsTheExposedDevices()
	{
		var listed = await FirstExposedAsync();

		var page = await fixture.Client.Vulnerabilities.ListMachineReferencesAsync(listed.Id!, Ct);

		page.Value.Should().NotBeEmpty().And.OnlyContain(m => !string.IsNullOrEmpty(m.Id));
	}

	[Fact]
	public async Task ListMachineVulnerabilitiesAsync_ReturnsDeviceRows()
	{
		var page = await fixture.Client.Vulnerabilities.ListMachineVulnerabilitiesAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty();
		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.CveId) && !string.IsNullOrEmpty(r.MachineId));
	}

	[Fact]
	public async Task GetAllVulnerabilitiesAsync_FollowsTheNextLink()
	{
		var seen = 0;

		await foreach (var vulnerability in fixture.Client.GetAllVulnerabilitiesAsync(new ODataListOptions { Top = 2 }, Ct))
		{
			vulnerability.Id.Should().NotBeNullOrEmpty();
			if (++seen == 5)
			{
				break;
			}
		}

		seen.Should().BeGreaterThan(2, "more than one page of two was read, so the next link was followed");
	}

	[Fact]
	public async Task AnUnknownCve_IsRaisedAsAnApiException()
	{
		var act = () => fixture.Client.Vulnerabilities.GetAsync("CVE-0000-0000", Ct);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
