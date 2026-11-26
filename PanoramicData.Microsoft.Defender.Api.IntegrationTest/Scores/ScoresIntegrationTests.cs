namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Scores;

/// <summary>
/// Read-only checks of the score operations against a live tenant (needs the <c>Score.Read.All</c> application permission).
/// Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class ScoresIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task GetExposureScoreAsync_ReturnsAScoreWithATime()
	{
		var score = await fixture.Client.Scores.GetExposureScoreAsync(Ct);

		score.Score.Should().BeGreaterThanOrEqualTo(0);
		score.Time.Should().NotBeNull();
	}

	[Fact]
	public async Task ListExposureScoresByMachineGroupsAsync_ReturnsOneScorePerGroup()
	{
		var page = await fixture.Client.Scores.ListExposureScoresByMachineGroupsAsync(Ct);

		page.Value.Should().NotBeEmpty();
		page.Value.Should().OnlyContain(s => s.Score >= 0 && !string.IsNullOrEmpty(s.RbacGroupName));
	}

	[Fact]
	public async Task GetConfigurationScoreAsync_ReturnsAScoreWithATime()
	{
		var score = await fixture.Client.Scores.GetConfigurationScoreAsync(Ct);

		score.Score.Should().BeGreaterThanOrEqualTo(0);
		score.Time.Should().NotBeNull();
	}
}
