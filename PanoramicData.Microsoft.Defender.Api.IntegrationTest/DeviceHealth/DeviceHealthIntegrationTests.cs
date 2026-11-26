using PanoramicData.MicrosoftDefender.Api.Models;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.DeviceHealth;

/// <summary>
/// Read-only checks of the antivirus device health report against a live tenant (needs the <c>Machine.Read.All</c>
/// application permission and onboarded devices). Live verification is pending, and the report path is itself in doubt: see
/// <c>IDeviceHealthApi</c> and docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class DeviceHealthIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task ListAntivirusHealthAsync_ReturnsDeviceRows()
	{
		var page = await fixture.Client.DeviceHealth.ListAntivirusHealthAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty("the tenant needs onboarded devices");
		page.Value.Should().OnlyContain(h => !string.IsNullOrEmpty(h.MachineId) && !string.IsNullOrEmpty(h.ComputerDnsName));
	}

	[Fact]
	public async Task ListAntivirusHealthAsync_HonoursAFilter()
	{
		var page = await fixture.Client.DeviceHealth.ListAntivirusHealthAsync(new ODataListOptions { Filter = "osKind eq 'windows'", Top = 5 }, Ct);

		page.Value.Should().OnlyContain(h => h.OsKind == "windows");
	}

	[Fact]
	public async Task GetAllAntivirusHealthAsync_FollowsTheNextLink()
	{
		var seen = 0;

		await foreach (var health in fixture.Client.GetAllAntivirusHealthAsync(new ODataListOptions { Top = 2 }, Ct))
		{
			health.MachineId.Should().NotBeNullOrEmpty();
			if (++seen == 5)
			{
				break;
			}
		}

		seen.Should().BeGreaterThan(0);
	}
}
