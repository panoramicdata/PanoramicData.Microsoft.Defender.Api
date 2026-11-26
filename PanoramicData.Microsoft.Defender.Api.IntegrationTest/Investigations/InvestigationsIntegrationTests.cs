using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Investigations;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Investigations;

/// <summary>
/// Read-only checks of the investigations operations against a live tenant (needs <c>Alert.ReadWrite.All</c> and at least one
/// investigation). Live verification is pending: see docs/TESTING.md. Starting an investigation is unit-verified only and
/// never run here.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class InvestigationsIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task ListAsync_ReturnsAPageOfInvestigations()
	{
		var page = await fixture.Client.Investigations.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(5);
		page.Value.Should().OnlyContain(i => !string.IsNullOrEmpty(i.Id) && !string.IsNullOrEmpty(i.State));
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var listed = (await fixture.Client.Investigations.ListAsync(new ODataListOptions { Top = 1 }, Ct)).Value.Should().ContainSingle().Subject;

		var page = await fixture.Client.Investigations.ListAsync(new ODataListOptions { Filter = $"state eq '{listed.State}'", Top = 5 }, Ct);

		page.Value.Should().OnlyContain(i => i.State == listed.State);
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameInvestigationAsTheList()
	{
		var listed = (await fixture.Client.Investigations.ListAsync(new ODataListOptions { Top = 1 }, Ct)).Value.Should().ContainSingle().Subject;

		var fetched = await fixture.Client.Investigations.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.MachineId.Should().Be(listed.MachineId);
	}

	[Fact]
	public async Task GetAllInvestigationsAsync_FollowsTheNextLink()
	{
		var seen = await LiveChecks.CountAsync(fixture.Client.GetAllInvestigationsAsync(new ODataListOptions { Top = 2 }, Ct), 5, investigation => investigation.Id);

		seen.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task AnUnknownInvestigation_IsRaisedAsAnApiException()
	{
		await LiveChecks.ShouldBeNotFoundAsync(() => fixture.Client.Investigations.GetAsync("0", Ct));
	}

	[Fact]
	public async Task StartingOne_IsRefused_ByTheReadOnlyClient_BeforeAnythingIsSent()
	{
		var act = () => fixture.Client.Investigations.StartAsync("0000000000000000000000000000000000000000", new StartInvestigationRequest { Comment = "never sent" }, Ct);

		await act.Should().ThrowAsync<DefenderReadOnlyException>();
	}
}
