using PanoramicData.MicrosoftDefender.Api.IntegrationTest.Entities;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Users;

/// <summary>
/// Read-only checks of the user operations against a live tenant. Needs <c>Alert.ReadWrite.All</c> and
/// <c>Machine.ReadWrite.All</c> application permissions and the setting <c>Defender:Test:UserName</c> (the user name part of
/// a user who signed in to a device, not the UPN). Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class UsersIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private string UserName => TestSetting.Required(fixture, "Defender:Test:UserName");

	[Fact]
	public async Task ListAlertsAsync_AnswersAPage()
	{
		var alerts = await fixture.Client.Users.ListAlertsAsync(UserName, Ct);

		alerts.Value.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Id));
	}

	[Fact]
	public async Task ListMachinesAsync_AnswersAPage()
	{
		var machines = await fixture.Client.Users.ListMachinesAsync(UserName, Ct);

		machines.Value.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Id));
	}

	[Fact]
	public async Task GetAllUserMachinesAsync_EnumeratesThePages()
	{
		var count = 0;

		await foreach (var machine in fixture.Client.GetAllUserMachinesAsync(UserName, Ct))
		{
			machine.Id.Should().NotBeNullOrEmpty();
			count++;
		}

		count.Should().BeGreaterThanOrEqualTo(0);
	}
}
