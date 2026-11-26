using PanoramicData.MicrosoftDefender.Api.IntegrationTest.Entities;
using PanoramicData.MicrosoftDefender.Api.Models;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Ips;

/// <summary>
/// Read-only checks of the IP operations against a live tenant. Needs <c>Ip.Read.All</c> and <c>Alert.ReadWrite.All</c>
/// application permissions and the setting <c>Defender:Test:IpAddress</c> (an address the tenant's devices have contacted).
/// Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class IpsIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private string Ip => TestSetting.Required(fixture, "Defender:Test:IpAddress");

	[Fact]
	public async Task GetStatisticsAsync_ReturnsPrevalence()
	{
		var stats = await fixture.Client.Ips.GetStatisticsAsync(Ip, new EntityStatisticsOptions { LookBackHours = 48 }, Ct);

		stats.IpAddress.Should().Be(Ip);
		stats.OrganizationPrevalence.Should().BeGreaterThanOrEqualTo(0);
	}

	[Fact]
	public async Task ListAlertsAsync_AnswersAPage()
	{
		var alerts = await fixture.Client.Ips.ListAlertsAsync(Ip, Ct);

		alerts.Value.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Id));
	}

	[Fact]
	public async Task AnInvalidAddress_IsRaisedAsAnApiException_WithStatus400()
	{
		var act = () => fixture.Client.Ips.ListAlertsAsync("not-an-ip", Ct);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
	}
}
