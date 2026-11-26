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
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<Machine> FirstMachineAsync()
	{
		var page = await fixture.Client.Machines.ListAsync(new ODataListOptions { Top = 10 }, Ct);
		return page.Value.FirstOrDefault(m => !string.IsNullOrEmpty(m.Id))
			?? throw new InvalidOperationException("The tenant reports no devices, so the per-device operations cannot be checked. Use a tenant with onboarded devices.");
	}

	[Fact]
	public async Task ListAsync_ReturnsAPageOfMachines()
	{
		var page = await fixture.Client.Machines.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(5);
		page.Value.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Id) && !string.IsNullOrEmpty(m.ComputerDnsName));
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
	public async Task ListLogOnUsersAsync_ReturnsUsers()
	{
		var machine = await FirstMachineAsync();

		var page = await fixture.Client.Machines.ListLogOnUsersAsync(machine.Id!, Ct);

		page.Value.Should().OnlyContain(u => !string.IsNullOrEmpty(u.Id));
	}

	[Fact]
	public async Task ListAlertsAsync_ReturnsAlerts()
	{
		var machine = await FirstMachineAsync();

		var page = await fixture.Client.Machines.ListAlertsAsync(machine.Id!, Ct);

		page.Value.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Id));
	}

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
		var tagged = (await fixture.Client.Machines.ListAsync(new ODataListOptions { Top = 1000 }, Ct)).Value.FirstOrDefault(m => m.MachineTags.Count > 0)
			?? throw new InvalidOperationException("No device among the first 1000 carries a tag, so find-by-tag cannot be checked. Tag a device first.");

		var page = await fixture.Client.Machines.FindByTagAsync(tagged.MachineTags[0], new FindByTagOptions { UseStartsWithFilter = false }, Ct);

		page.Value.Should().Contain(m => m.Id == tagged.Id);
	}

	[Fact]
	public async Task ListVulnerabilitiesAsync_ReturnsVulnerabilities()
	{
		var machine = await FirstMachineAsync();

		var page = await fixture.Client.Machines.ListVulnerabilitiesAsync(machine.Id!, Ct);

		page.Value.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Id));
	}

	[Fact]
	public async Task ListSoftwareAsync_ReturnsSoftware()
	{
		var machine = await FirstMachineAsync();

		var page = await fixture.Client.Machines.ListSoftwareAsync(machine.Id!, Ct);

		page.Value.Should().OnlyContain(s => !string.IsNullOrEmpty(s.Id));
	}

	[Fact]
	public async Task ListMissingKbsAsync_ReturnsKbs()
	{
		var machine = await FirstMachineAsync();

		var page = await fixture.Client.Machines.ListMissingKbsAsync(machine.Id!, Ct);

		page.Value.Should().OnlyContain(k => !string.IsNullOrEmpty(k.Id));
	}

	[Fact]
	public async Task ListRecommendationsAsync_ReturnsRecommendations()
	{
		var machine = await FirstMachineAsync();

		var page = await fixture.Client.Machines.ListRecommendationsAsync(machine.Id!, Ct);

		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.Id));
	}

	[Fact]
	public async Task GetAllMachinesAsync_FollowsTheNextLink()
	{
		var seen = await LiveChecks.CountAsync(fixture.Client.GetAllMachinesAsync(new ODataListOptions { Top = 1 }, Ct), 3, machine => machine.Id);

		seen.Should().BeGreaterThan(1, "more than one page of one was read, so the next link was followed (needs at least two devices)");
	}

	[Fact]
	public async Task AnUnknownMachine_IsRaisedAsAnApiException()
	{
		await LiveChecks.ShouldBeNotFoundAsync(() => fixture.Client.Machines.GetAsync("0000000000000000000000000000000000000000", Ct));
	}

	[Fact]
	public async Task AWrite_IsRefusedBeforeItIsSent_OnTheReadOnlyClient()
	{
		// A made-up id, so that nothing real could be touched even if the guard failed.
		var act = () => fixture.Client.Machines.SetDeviceValueAsync("0000000000000000000000000000000000000000", new SetDeviceValueRequest { DeviceValue = MachineDeviceValue.Normal }, Ct);

		await act.Should().ThrowAsync<DefenderReadOnlyException>();
	}
}
