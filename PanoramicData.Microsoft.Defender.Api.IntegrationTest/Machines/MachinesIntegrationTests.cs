using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Machines;

/// <summary>
/// Read-only checks of the machines operations against a live tenant (needs <c>Machine.Read.All</c>, <c>User.Read.All</c>,
/// <c>Alert.ReadWrite.All</c>, <c>Vulnerability.Read.All</c>, <c>Software.Read.All</c> and <c>SecurityRecommendation.Read.All</c>,
/// and at least one onboarded device). Live verification is pending: see docs/TESTING.md. The write operations (update,
/// tags, device value) are unit-verified only and never run here.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class MachinesIntegrationTests(DefenderFixture fixture)
{
	private const string MissingId = "0000000000000000000000000000000000000000";

	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<Machine> FirstMachineAsync()
		=> LiveChecks.FirstOrThrow(
			await fixture.Client.Machines.ListAsync(new ODataListOptions { Top = 10 }, Ct),
			m => !string.IsNullOrEmpty(m.Id),
			"The tenant reports no devices, so the per-device operations cannot be checked. Use a tenant with onboarded devices.");

	/// <summary>Runs a per-device list operation against the first device and checks every item it returns has an identifier.</summary>
	private async Task ShouldListIdentifiedItemsAsync<T>(Func<string, CancellationToken, Task<ODataPage<T>>> list, Func<T, string?> identifier)
	{
		var machine = await FirstMachineAsync();

		await LiveChecks.ShouldListIdentifiedItemsAsync(list, machine.Id!, identifier, Ct);
	}

	[Fact]
	public async Task ListAsync_ReturnsAPageOfMachines()
	{
		var page = await fixture.Client.Machines.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		LiveChecks.ShouldBeACompletePage(page, 5, m => !string.IsNullOrEmpty(m.Id) && !string.IsNullOrEmpty(m.ComputerDnsName));
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var first = await FirstMachineAsync();

		var page = await fixture.Client.Machines.ListAsync(new ODataListOptions { Filter = $"id eq '{first.Id}'" }, Ct);

		page.Value.Should().ContainSingle().Which.Id.Should().Be(first.Id);
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameMachineAsTheList()
	{
		var listed = await FirstMachineAsync();

		var fetched = await fixture.Client.Machines.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.ComputerDnsName.Should().Be(listed.ComputerDnsName);
	}

	[Fact]
	public Task ListLogOnUsersAsync_ReturnsUsers()
		=> ShouldListIdentifiedItemsAsync(fixture.Client.Machines.ListLogOnUsersAsync, u => u.Id);

	[Fact]
	public Task ListAlertsAsync_ReturnsAlerts()
		=> ShouldListIdentifiedItemsAsync(fixture.Client.Machines.ListAlertsAsync, a => a.Id);

	[Fact]
	public async Task FindByIpAsync_FindsTheDeviceByItsLastIp()
	{
		var machine = await FirstMachineAsync();
		machine.LastIpAddress.Should().NotBeNullOrEmpty("the device must report an internal IP for the lookup");

		var page = await fixture.Client.Machines.FindByIpAsync(machine.LastIpAddress!, machine.LastSeen ?? DateTimeOffset.UtcNow, Ct);

		page.Value.Should().Contain(m => m.Id == machine.Id);
	}

	[Fact]
	public async Task FindByTagAsync_FindsATaggedDevice()
	{
		var tagged = LiveChecks.FirstOrThrow(
			await fixture.Client.Machines.ListAsync(new ODataListOptions { Top = 1000 }, Ct),
			m => m.MachineTags.Count > 0,
			"No device among the first 1000 carries a tag, so find-by-tag cannot be checked. Tag a device first.");

		var page = await fixture.Client.Machines.FindByTagAsync(tagged.MachineTags[0], new FindByTagOptions { UseStartsWithFilter = false }, Ct);

		page.Value.Should().Contain(m => m.Id == tagged.Id);
	}

	[Fact]
	public Task ListVulnerabilitiesAsync_ReturnsVulnerabilities()
		=> ShouldListIdentifiedItemsAsync(fixture.Client.Machines.ListVulnerabilitiesAsync, v => v.Id);

	[Fact]
	public Task ListSoftwareAsync_ReturnsSoftware()
		=> ShouldListIdentifiedItemsAsync(fixture.Client.Machines.ListSoftwareAsync, s => s.Id);

	[Fact]
	public Task ListMissingKbsAsync_ReturnsKbs()
		=> ShouldListIdentifiedItemsAsync(fixture.Client.Machines.ListMissingKbsAsync, k => k.Id);

	[Fact]
	public Task ListRecommendationsAsync_ReturnsRecommendations()
		=> ShouldListIdentifiedItemsAsync(fixture.Client.Machines.ListRecommendationsAsync, r => r.Id);

	[Fact]
	public async Task GetAllMachinesAsync_FollowsTheNextLink()
	{
		var seen = await LiveChecks.CountAsync(fixture.Client.GetAllMachinesAsync(new ODataListOptions { Top = 1 }, Ct), 3, machine => machine.Id);

		seen.Should().BeGreaterThan(1, "more than one page of one was read, so the next link was followed (needs at least two devices)");
	}

	[Fact]
	public async Task AnUnknownMachine_IsRaisedAsAnApiException()
	{
		await LiveChecks.ShouldBeNotFoundAsync(() => fixture.Client.Machines.GetAsync(MissingId, Ct));
	}

	[Fact]
	public async Task AWrite_IsRefusedBeforeItIsSent_OnTheReadOnlyClient()
	{
		await LiveChecks.ShouldBeRefusedAsReadOnlyAsync(() => fixture.Client.Machines.SetDeviceValueAsync(MissingId, new SetDeviceValueRequest { DeviceValue = MachineDeviceValue.Normal }, Ct));
	}
}
