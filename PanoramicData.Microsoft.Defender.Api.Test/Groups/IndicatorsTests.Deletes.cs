using PanoramicData.MicrosoftDefender.Api.Models.Indicators;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

public partial class IndicatorsTests
{
	[Fact]
	public async Task Delete_SendsDeleteIndicatorsId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.DeleteAsync("994", t), string.Empty);

		call.ShouldBeRequest(HttpMethod.Delete, "/api/indicators/994");
	}

	[Fact]
	public async Task Delete_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.DeleteAsync("a/b c", t), string.Empty);

		call.Uri.AbsolutePath.Should().Be("/api/indicators/a%2Fb%20c");
	}

	[Fact]
	public async Task Delete_AcceptsA204WithNoBody()
	{
		using var client = TestClient.Create(TestClient.Stub(string.Empty, HttpStatusCode.NoContent));

		await client.Indicators.DeleteAsync("1", TestContext.Current.CancellationToken);
	}

	[Fact]
	public Task Delete_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.DeleteAsync("1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No such indicator"), "No such indicator");

	[Fact]
	public Task Delete_IsReplayedAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Indicators.DeleteAsync("1", t), HttpMethod.Delete, "/api/indicators/1", null);

	[Fact]
	public Task Delete_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Indicators.DeleteAsync("1", t));

	[Fact]
	public async Task BatchDelete_SendsPostIndicatorsBatchDelete_WithACapitalIndicatorIdsProperty()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["1", "2", "5"] }, t), string.Empty);

		call.ShouldBeRequest(HttpMethod.Post, "/api/indicators/BatchDelete", string.Empty, """{"IndicatorIds":["1","2","5"]}""");
	}

	[Fact]
	public void BatchDelete_AcceptsTheMaximum_AndRejectsMore()
	{
		var ids = Enumerable.Range(0, 501).Select(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList();

		new IndicatorBatchDeleteRequest { IndicatorIds = [.. ids.Take(500)] }.IndicatorIds.Should().HaveCount(500);
		var act = () => new IndicatorBatchDeleteRequest { IndicatorIds = ids };
		act.Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("IndicatorIds");
	}

	[Fact]
	public void BatchDelete_RejectsAnEmptyList()
	{
		var act = () => new IndicatorBatchDeleteRequest { IndicatorIds = [] };

		act.Should().Throw<ArgumentException>().WithMessage("IndicatorIds must contain at least one item.*");
	}

	[Fact]
	public Task BatchDelete_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["9"] }, t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "Indicator 9 not found"), "Indicator 9 not found");

	[Fact]
	public Task BatchDelete_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["1"] }, t), HttpMethod.Post, "/api/indicators/BatchDelete", """{"IndicatorIds":["1"]}""");

	[Fact]
	public Task BatchDelete_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["1"] }, t));
}
