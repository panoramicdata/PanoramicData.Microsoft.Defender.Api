#pragma warning disable CS0618 // Microsoft is retiring this API; the test checks it while it still works.

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.AdvancedHunting;

/// <summary>
/// A read-only check of the advanced hunting query against a live tenant (needs <c>AdvancedQuery.Read.All</c>, and a tenant
/// that has not yet had this retiring API switched off). The query only reads, so the read-only client allows it. Live
/// verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class AdvancedHuntingIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task RunAdvancedHuntingQueryAsync_ReturnsTheSchemaAndRows()
	{
		var result = await fixture.Client.RunAdvancedHuntingQueryAsync("DeviceInfo | project DeviceId, DeviceName | take 2", Ct);

		result.Schema.Select(c => c.Name).Should().Equal("DeviceId", "DeviceName");
		result.Results.Should().HaveCountLessThanOrEqualTo(2);
		result.Results.Should().OnlyContain(row => row.ContainsKey("DeviceId"));
	}

	[Fact]
	public async Task AnInvalidQuery_IsRaisedAsAnApiException_WithStatus400()
	{
		var act = () => fixture.Client.RunAdvancedHuntingQueryAsync("this is not kql |", Ct);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
	}
}
