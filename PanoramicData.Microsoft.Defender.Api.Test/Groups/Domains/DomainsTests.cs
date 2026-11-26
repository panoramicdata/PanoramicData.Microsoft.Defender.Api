using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups.Domains;

/// <summary>The domains group: alerts, devices and statistics for a domain name.</summary>
public class DomainsTests
{
	private const string StatisticsJson = """
		{
			"@odata.context": "https://defender.test/api/$metadata#microsoft.windowsDefenderATP.api.InOrgDomainStats",
			"host": "example.test",
			"organizationPrevalence": 4070,
			"orgFirstSeen": "2017-07-30T13:23:48Z",
			"orgLastSeen": "2017-08-29T13:09:05Z"
		}
		""";

	[Fact]
	public Task ListAlerts_SendsGetDomainsDomainAlerts()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.Domains.ListAlertsAsync("example.test", t), "/api/domains/example.test/alerts");

	[Fact]
	public async Task ListAlerts_EscapesTheDomainAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Domains.ListAlertsAsync("a/b c", t), RelatedEntityChecks.Page(""));

		call.Uri.AbsolutePath.Should().Be("/api/domains/a%2Fb%20c/alerts");
	}

	[Fact]
	public Task ListAlerts_MapsTheAlerts()
		=> RelatedEntityChecks.AssertAlertsAsync((c, t) => c.Domains.ListAlertsAsync("example.test", t));

	[Fact]
	public Task ListAlerts_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Domains.ListAlertsAsync("example.test", t));

	[Fact]
	public Task ListMachines_SendsGetDomainsDomainMachines()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.Domains.ListMachinesAsync("example.test", t), "/api/domains/example.test/machines");

	[Fact]
	public Task ListMachines_MapsTheMachines()
		=> RelatedEntityChecks.AssertMachinesAsync((c, t) => c.Domains.ListMachinesAsync("example.test", t));

	[Fact]
	public Task ListMachines_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Domains.ListMachinesAsync("example.test", t));

	[Fact]
	public async Task GetStatistics_WithNoOptions_SendsNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Domains.GetStatisticsAsync("example.test", null, t), StatisticsJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/domains/example.test/stats");
	}

	[Fact]
	public async Task GetStatistics_SendsGetDomainsDomainStats_WithTheLookBackHours()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Domains.GetStatisticsAsync("example.test", new EntityStatisticsOptions { LookBackHours = 720 }, t), StatisticsJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/domains/example.test/stats", "?lookBackHours=720");
	}

	[Fact]
	public async Task GetStatistics_MapsEveryField()
	{
		var stats = await TestClient.ReadAsync((c, t) => c.Domains.GetStatisticsAsync("example.test", null, t), StatisticsJson);

		(stats.Host, stats.OrganizationPrevalence).Should().Be(("example.test", 4070L));
		stats.OrgFirstSeen.Should().Be(new DateTimeOffset(2017, 7, 30, 13, 23, 48, TimeSpan.Zero));
		stats.OrgLastSeen.Should().Be(new DateTimeOffset(2017, 8, 29, 13, 9, 5, TimeSpan.Zero));
	}

	[Fact]
	public async Task GetStatistics_AnUnknownDomain_ReadsAsAZeroPrevalence()
	{
		var stats = await TestClient.ReadAsync((c, t) => c.Domains.GetStatisticsAsync("unseen.test", null, t),
			"""{"host":"unseen.test","organizationPrevalence":"0","orgFirstSeen":null,"orgLastSeen":""}""");

		(stats.OrganizationPrevalence, stats.OrgFirstSeen, stats.OrgLastSeen).Should().Be((0L, (DateTimeOffset?)null, (DateTimeOffset?)null));
	}

	[Fact]
	public Task GetStatistics_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Domains.GetStatisticsAsync("example.test", null, t));

	[Fact]
	public Task GetAllDomainAlerts_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllDomainAlertsAsync("example.test", t), "/api/domains/example.test/alerts", """{"id":"a"}""", """{"id":"b"}""", a => a.Id);

	[Fact]
	public Task GetAllDomainMachines_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllDomainMachinesAsync("example.test", t), "/api/domains/example.test/machines", """{"id":"a"}""", """{"id":"b"}""", m => m.Id);
}
