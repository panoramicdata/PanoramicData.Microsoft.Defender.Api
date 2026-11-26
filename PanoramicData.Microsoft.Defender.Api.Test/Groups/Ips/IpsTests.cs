using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups.Ips;

/// <summary>The IPs group: alerts and statistics for an IP address.</summary>
public class IpsTests
{
	private const string StatisticsJson = """
		{
			"@odata.context": "https://defender.test/api/$metadata#microsoft.windowsDefenderATP.api.InOrgIPStats",
			"ipAddress": "10.209.67.177",
			"organizationPrevalence": 63515,
			"orgFirstSeen": "2017-07-30T13:36:06Z",
			"orgLastSeen": "2017-08-29T13:32:59Z"
		}
		""";

	[Fact]
	public Task ListAlerts_SendsGetIpsIpAlerts()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.Ips.ListAlertsAsync("10.209.67.177", t), "/api/ips/10.209.67.177/alerts");

	[Fact]
	public async Task ListAlerts_EscapesTheAddressAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Ips.ListAlertsAsync("2001:db8::1/64", t), RelatedEntityChecks.Page(""));

		call.Uri.AbsolutePath.Should().Be("/api/ips/2001%3Adb8%3A%3A1%2F64/alerts");
	}

	[Fact]
	public Task ListAlerts_MapsTheAlerts()
		=> RelatedEntityChecks.AssertAlertsAsync((c, t) => c.Ips.ListAlertsAsync("10.209.67.177", t));

	[Fact]
	public Task ListAlerts_AnInvalidAddressIsMappedAsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Ips.ListAlertsAsync("not-an-ip", t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"InvalidIpAddress","message":"Invalid IP address","target":"corr-1"}}""", "Invalid IP address");

	[Fact]
	public async Task GetStatistics_WithNoOptions_SendsNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Ips.GetStatisticsAsync("10.209.67.177", null, t), StatisticsJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/ips/10.209.67.177/stats");
	}

	[Fact]
	public async Task GetStatistics_SendsGetIpsIpStats_WithTheLookBackHours()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Ips.GetStatisticsAsync("10.209.67.177", new EntityStatisticsOptions { LookBackHours = 48 }, t), StatisticsJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/ips/10.209.67.177/stats", "?lookBackHours=48");
	}

	[Fact]
	public async Task GetStatistics_MapsEveryField()
	{
		var stats = await TestClient.ReadAsync((c, t) => c.Ips.GetStatisticsAsync("10.209.67.177", null, t), StatisticsJson);

		(stats.IpAddress, stats.OrganizationPrevalence).Should().Be(("10.209.67.177", 63515L));
		stats.OrgFirstSeen.Should().Be(new DateTimeOffset(2017, 7, 30, 13, 36, 6, TimeSpan.Zero));
		stats.OrgLastSeen.Should().Be(new DateTimeOffset(2017, 8, 29, 13, 32, 59, TimeSpan.Zero));
	}

	[Fact]
	public async Task GetStatistics_AValidButUnseenAddress_ReadsAsAZeroPrevalence()
	{
		var stats = await TestClient.ReadAsync((c, t) => c.Ips.GetStatisticsAsync("192.0.2.1", null, t),
			"""{"ipAddress":"192.0.2.1","organizationPrevalence":"0","orgFirstSeen":""}""");

		(stats.OrganizationPrevalence, stats.OrgFirstSeen).Should().Be((0L, (DateTimeOffset?)null));
	}

	[Fact]
	public Task GetStatistics_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Ips.GetStatisticsAsync("10.209.67.177", null, t));

	[Fact]
	public Task GetAllIpAlerts_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllIpAlertsAsync("10.209.67.177", t), "/api/ips/10.209.67.177/alerts", """{"id":"a"}""", """{"id":"b"}""", a => a.Id);
}
