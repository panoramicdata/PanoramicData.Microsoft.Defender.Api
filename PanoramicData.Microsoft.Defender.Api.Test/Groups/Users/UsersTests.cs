using PanoramicData.MicrosoftDefender.Api.Test.Support;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups.Users;

/// <summary>The users group: alerts and devices related to a user, addressed by user name rather than UPN.</summary>
public class UsersTests
{
	[Fact]
	public Task ListAlerts_SendsGetUsersNameAlerts()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.Users.ListAlertsAsync("user1", t), "/api/users/user1/alerts");

	[Theory]
	[InlineData("user1", "user1")]
	[InlineData("o'brien smith", "o%27brien%20smith")]
	[InlineData("a/b\\c", "a%2Fb%5Cc")]
	[InlineData("a+b%c#d?e", "a%2Bb%25c%23d%3Fe")]
	public async Task UserNames_AreEscapedAsOneSegment(string userName, string escaped)
	{
		var alerts = await TestClient.CaptureAsync((c, t) => c.Users.ListAlertsAsync(userName, t), RelatedEntityChecks.Page(""));
		var machines = await TestClient.CaptureAsync((c, t) => c.Users.ListMachinesAsync(userName, t), RelatedEntityChecks.Page(""));

		alerts.ShouldBeRequest(HttpMethod.Get, $"/api/users/{escaped}/alerts");
		machines.ShouldBeRequest(HttpMethod.Get, $"/api/users/{escaped}/machines");
	}

	[Fact]
	public async Task AUpnPassedAsTheName_IsSentVerbatimEscaped_NotStrippedToTheUserName()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Users.ListAlertsAsync("user1@contoso.test", t), RelatedEntityChecks.Page(""));

		call.Uri.AbsolutePath.Should().Be("/api/users/user1%40contoso.test/alerts");
	}

	[Fact]
	public Task ListAlerts_MapsTheAlerts()
		=> RelatedEntityChecks.AssertAlertsAsync((c, t) => c.Users.ListAlertsAsync("user1", t));

	[Fact]
	public Task ListAlerts_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Users.ListAlertsAsync("user1", t));

	[Fact]
	public Task ListMachines_SendsGetUsersNameMachines()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.Users.ListMachinesAsync("user1", t), "/api/users/user1/machines");

	[Fact]
	public Task ListMachines_MapsTheMachines()
		=> RelatedEntityChecks.AssertMachinesAsync((c, t) => c.Users.ListMachinesAsync("user1", t));

	[Fact]
	public Task ListMachines_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Users.ListMachinesAsync("user1", t));

	[Fact]
	public Task GetAllUserAlerts_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllUserAlertsAsync("user1", t), "/api/users/user1/alerts", """{"id":"a"}""", """{"id":"b"}""", a => a.Id);

	[Fact]
	public Task GetAllUserMachines_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllUserMachinesAsync("user1", t), "/api/users/user1/machines", """{"id":"a"}""", """{"id":"b"}""", m => m.Id);
}
