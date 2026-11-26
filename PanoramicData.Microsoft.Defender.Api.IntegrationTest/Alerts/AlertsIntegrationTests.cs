using PanoramicData.MicrosoftDefender.Api.Models.Alerts;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Alerts;

/// <summary>
/// Read-only checks of the alerts operations against a live tenant (needs the <c>Alert.ReadWrite.All</c> application
/// permission and at least one alert in the retention period). Live verification is pending: see docs/TESTING.md. The
/// update, batch update and create-by-reference operations are unit-verified only and never run here.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class AlertsIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<Alert> FirstAlertAsync()
	{
		var page = await fixture.Client.Alerts.ListAsync(new AlertListOptions { Top = 20 }, Ct);
		return page.Value.FirstOrDefault(a => a.MachineId is not null)
			?? throw new InvalidOperationException("The tenant reports no alert with a device among the first 20, so the by-id operations cannot be checked. Use a tenant with recent alerts.");
	}

	[Fact]
	public async Task ListAsync_ReturnsAPageOfAlerts()
	{
		var page = await fixture.Client.Alerts.ListAsync(new AlertListOptions { Top = 5 }, Ct);

		page.Value.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(5);
		page.Value.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Id));
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var page = await fixture.Client.Alerts.ListAsync(new AlertListOptions { Filter = "status eq 'New'", Top = 5 }, Ct);

		page.Value.Should().OnlyContain(a => a.Status == AlertStatus.New);
	}

	[Fact]
	public async Task ListAsync_CanExpandEvidence()
	{
		var page = await fixture.Client.Alerts.ListAsync(new AlertListOptions { Top = 5, Expand = AlertListOptions.ExpandEvidence }, Ct);

		page.Value.Should().NotBeEmpty();
		page.Value.Should().Contain(a => a.Evidence.Count > 0);
	}

	[Fact]
	public async Task GetAsync_ReturnsTheSameAlertAsTheList()
	{
		var listed = await FirstAlertAsync();

		var fetched = await fixture.Client.Alerts.GetAsync(listed.Id!, Ct);

		fetched.Id.Should().Be(listed.Id);
		fetched.Title.Should().Be(listed.Title);
	}

	[Fact]
	public async Task GetMachineAsync_ReturnsTheAlertsDevice()
	{
		var listed = await FirstAlertAsync();

		var machine = await fixture.Client.Alerts.GetMachineAsync(listed.Id!, Ct);

		machine.Id.Should().Be(listed.MachineId);
	}

	[Fact]
	public async Task RelatedEntityOperations_AnswerForAnExistingAlert()
	{
		var listed = await FirstAlertAsync();

		// Each may legitimately answer 404 when the alert has no such entity, so only the shape of a success is checked.
		var files = await TryAsync(() => fixture.Client.Alerts.ListFilesAsync(listed.Id!, Ct));
		var ips = await TryAsync(() => fixture.Client.Alerts.ListIpsAsync(listed.Id!, Ct));
		var domains = await TryAsync(() => fixture.Client.Alerts.ListDomainsAsync(listed.Id!, Ct));

		files?.Value.Should().OnlyContain(f => !string.IsNullOrEmpty(f.Sha1) || !string.IsNullOrEmpty(f.Sha256));
		ips?.Value.Should().OnlyContain(i => !string.IsNullOrEmpty(i.Id));
		domains?.Value.Should().OnlyContain(d => !string.IsNullOrEmpty(d.Host));
	}

	[Fact]
	public async Task GetAllAlertsAsync_FollowsTheNextLink()
	{
		var seen = 0;

		await foreach (var alert in fixture.Client.GetAllAlertsAsync(new AlertListOptions { Top = 2 }, Ct))
		{
			alert.Id.Should().NotBeNullOrEmpty();
			if (++seen == 5)
			{
				break;
			}
		}

		seen.Should().BeGreaterThan(2, "more than one page of two was read, so the next link was followed");
	}

	[Fact]
	public async Task AnUnknownAlert_IsRaisedAsAnApiException()
	{
		var act = () => fixture.Client.Alerts.GetAsync("da0-unknown", Ct);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task WritesAreRefused_ByTheReadOnlyClient_BeforeAnythingIsSent()
	{
		var update = () => fixture.Client.Alerts.UpdateAsync("da0-unknown", new AlertUpdate { Comment = "never sent" }, Ct);
		var batch = () => fixture.Client.Alerts.BatchUpdateAsync(new AlertBatchUpdate { AlertIds = ["da0-unknown"] }, Ct);

		await update.Should().ThrowAsync<DefenderReadOnlyException>();
		await batch.Should().ThrowAsync<DefenderReadOnlyException>();
	}

	private static async Task<T?> TryAsync<T>(Func<Task<T>> call) where T : class
	{
		try
		{
			return await call();
		}
		catch (DefenderApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}
}
