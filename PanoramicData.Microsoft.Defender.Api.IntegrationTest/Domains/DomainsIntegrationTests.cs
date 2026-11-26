using PanoramicData.MicrosoftDefender.Api.IntegrationTest.Entities;
using PanoramicData.MicrosoftDefender.Api.Models;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Domains;

/// <summary>
/// Read-only checks of the domains operations against a live tenant. Needs <c>URL.Read.All</c>, <c>Alert.ReadWrite.All</c> and
/// <c>Machine.ReadWrite.All</c> application permissions and the setting <c>Defender:Test:Domain</c> (a domain the tenant's
/// devices have contacted). Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class DomainsIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private string Domain => TestSetting.Required(fixture, "Defender:Test:Domain");

	[Fact]
	public async Task GetStatisticsAsync_ReturnsPrevalence()
	{
		var stats = await fixture.Client.Domains.GetStatisticsAsync(Domain, new EntityStatisticsOptions { LookBackHours = 48 }, Ct);

		stats.Host.Should().BeEquivalentTo(Domain);
		stats.OrganizationPrevalence.Should().BeGreaterThanOrEqualTo(0);
	}

	[Fact]
	public async Task ListAlertsAndMachines_AnswerPages()
	{
		var alerts = await fixture.Client.Domains.ListAlertsAsync(Domain, Ct);
		var machines = await fixture.Client.Domains.ListMachinesAsync(Domain, Ct);

		alerts.Value.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Id));
		machines.Value.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Id)).And.HaveCountLessThanOrEqualTo(500);
	}

	[Fact]
	public async Task GetAllDomainMachinesAsync_EnumeratesThePages()
	{
		var count = 0;

		await foreach (var machine in fixture.Client.GetAllDomainMachinesAsync(Domain, Ct))
		{
			machine.Id.Should().NotBeNullOrEmpty();
			count++;
		}

		count.Should().BeLessThanOrEqualTo(500);
	}
}
