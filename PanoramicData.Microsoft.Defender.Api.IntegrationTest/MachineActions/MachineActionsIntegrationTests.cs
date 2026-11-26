using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.MachineActions;

/// <summary>
/// Read-only checks of the machine action listing against a live tenant (needs <c>Machine.ReadWrite.All</c> and at least one
/// earlier action). Live verification is pending: see docs/TESTING.md. Requesting or cancelling an action
/// (isolate, scan, offboard, live response and the rest) is unit-verified only and never run here; the download-link
/// operations need a finished action and are rate limited, so they are not run either.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class MachineActionsIntegrationTests(DefenderFixture fixture)
{
	private const string MissingId = "00000000-0000-0000-0000-000000000000";

	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<MachineAction> FirstActionAsync()
		=> LiveChecks.FirstOrThrow(
			await fixture.Client.MachineActions.ListAsync(new ODataListOptions { Top = 10 }, Ct),
			a => !string.IsNullOrEmpty(a.Id),
			"The tenant has no machine actions, so the operations cannot be checked. Use a tenant where an action has been requested at some point.");

	[Fact]
	public async Task ListAsync_ReturnsAPageOfActions()
	{
		var page = await fixture.Client.MachineActions.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		LiveChecks.ShouldBeACompletePage(page, 5, a => !string.IsNullOrEmpty(a.Id) && !string.IsNullOrEmpty(a.Type));
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var first = await FirstActionAsync();

		var page = await fixture.Client.MachineActions.ListAsync(new ODataListOptions { Filter = $"id eq '{first.Id}'" }, Ct);

		page.Value.Should().ContainSingle().Which.Id.Should().Be(first.Id);
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameActionAsTheList()
	{
		var listed = await FirstActionAsync();

		var fetched = await fixture.Client.MachineActions.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.Type.Should().Be(listed.Type);
		fetched.MachineId.Should().Be(listed.MachineId);
	}

	[Fact]
	public async Task GetAllMachineActionsAsync_FollowsTheNextLink()
	{
		var seen = await LiveChecks.CountAsync(fixture.Client.GetAllMachineActionsAsync(new ODataListOptions { Top = 1 }, Ct), 3, action => action.Id);

		seen.Should().BeGreaterThan(1, "more than one page of one was read, so the next link was followed (needs at least two actions)");
	}

	[Fact]
	public async Task AnUnknownAction_IsRaisedAsAnApiException()
	{
		await LiveChecks.ShouldBeNotFoundAsync(() => fixture.Client.MachineActions.GetAsync(MissingId, Ct));
	}

	[Fact]
	public async Task AnAction_IsRefusedBeforeItIsSent_OnTheReadOnlyClient()
	{
		await LiveChecks.ShouldBeRefusedAsReadOnlyAsync(() => fixture.Client.MachineActions.IsolateAsync("0000000000000000000000000000000000000000", new IsolateRequest { Comment = "must never be sent" }, Ct));
	}
}
