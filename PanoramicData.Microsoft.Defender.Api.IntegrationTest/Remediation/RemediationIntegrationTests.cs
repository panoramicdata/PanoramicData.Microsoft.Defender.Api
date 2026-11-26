using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Remediation;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Remediation;

/// <summary>
/// Read-only checks of the remediation activity operations against a live tenant (needs the
/// <c>RemediationTasks.Read.All</c> application permission and at least one remediation activity). Live verification is
/// pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class RemediationIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<RemediationTask> FirstTaskAsync()
	{
		var page = await fixture.Client.Remediation.ListAsync(new ODataListOptions { Top = 20 }, Ct);
		return (page.Value.Count > 0 ? page.Value[0] : null)
			?? throw new InvalidOperationException("The tenant has no remediation activity, so the by-id operations cannot be checked. Create one in the Defender portal first.");
	}

	[Fact]
	public async Task ListAsync_ReturnsAPageOfActivities()
	{
		var first = await FirstTaskAsync();

		first.Id.Should().NotBeNullOrEmpty();
		first.Title.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var page = await fixture.Client.Remediation.ListAsync(new ODataListOptions { Filter = "status eq 'Active'", Top = 5 }, Ct);

		page.Value.Should().OnlyContain(t => t.Status == "Active");
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameActivityAsTheList()
	{
		var listed = await FirstTaskAsync();

		var fetched = await fixture.Client.Remediation.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.Title.Should().Be(listed.Title);
	}

	[Fact]
	public async Task ListMachineReferencesAsync_AcceptsAnActivityId()
	{
		var listed = await FirstTaskAsync();

		var page = await fixture.Client.Remediation.ListMachineReferencesAsync(listed.Id!, Ct);

		page.Value.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Id));
	}

	[Fact]
	public async Task GetAllRemediationTasksAsync_EnumeratesActivities()
	{
		var seen = 0;

		await foreach (var task in fixture.Client.GetAllRemediationTasksAsync(new ODataListOptions { Top = 2 }, Ct))
		{
			task.Id.Should().NotBeNullOrEmpty();
			if (++seen == 3)
			{
				break;
			}
		}

		seen.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task AnUnknownActivity_IsRaisedAsAnApiException()
	{
		var act = () => fixture.Client.Remediation.GetAsync("00000000-0000-0000-0000-000000000000", Ct);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
