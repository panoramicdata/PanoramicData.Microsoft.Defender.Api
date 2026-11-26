using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Indicators;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Indicators;

/// <summary>
/// Read-only checks of the indicators operations against a live tenant (needs <c>Ti.ReadWrite.All</c>). Live verification
/// is pending: see docs/TESTING.md. Creating, importing and deleting indicators is unit-verified only and never run here.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class IndicatorsIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task ListAsync_ReturnsAPageOfIndicators()
	{
		var page = await fixture.Client.Indicators.ListAsync(new ODataListOptions { Top = 5 }, Ct);

		page.Value.Should().HaveCountLessThanOrEqualTo(5);
		page.Value.Should().OnlyContain(i => !string.IsNullOrEmpty(i.Id) && !string.IsNullOrEmpty(i.IndicatorValue));
		page.Value.Should().OnlyContain(i => i.IndicatorType != IndicatorType.Unknown, "a type this library does not know would read as Unknown");
	}

	[Fact]
	public async Task ListAsync_HonoursAFilter()
	{
		var page = await fixture.Client.Indicators.ListAsync(new ODataListOptions { Filter = "action eq 'Block'", Top = 5 }, Ct);

		page.Value.Should().OnlyContain(i => i.Action == IndicatorAction.Block);
	}

	[Fact]
	public async Task GetAllIndicatorsAsync_ReadsEveryPage()
	{
		var seen = 0;

		await foreach (var indicator in fixture.Client.GetAllIndicatorsAsync(new ODataListOptions { Top = 2 }, Ct))
		{
			indicator.Id.Should().NotBeNullOrEmpty();
			if (++seen == 5)
			{
				break;
			}
		}

		seen.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task WritesAreRefused_ByTheReadOnlyClient_BeforeAnythingIsSent()
	{
		var create = () => fixture.Client.Indicators.CreateAsync(new IndicatorSubmission { IndicatorValue = "never-sent.example.test", IndicatorType = IndicatorType.DomainName, Action = IndicatorAction.Audit, Title = "t", Description = "d" }, Ct);
		var delete = () => fixture.Client.Indicators.DeleteAsync("0", Ct);
		var batchDelete = () => fixture.Client.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["0"] }, Ct);

		await create.Should().ThrowAsync<DefenderReadOnlyException>();
		await delete.Should().ThrowAsync<DefenderReadOnlyException>();
		await batchDelete.Should().ThrowAsync<DefenderReadOnlyException>();
	}
}
