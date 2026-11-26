using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The exposure score and secure score operations: each pins its request and maps a realistic response.</summary>
public class ScoresTests
{
	private const string ScoreJson = """{"@odata.context":"https://defender.test/api/$metadata#ExposureScore/$entity","time":"2019-12-03T07:23:53.28Z","score":33.5}""";

	[Fact]
	public Task GetExposureScore_SendsGetExposureScore()
		=> ShouldGetAsync((c, t) => c.Scores.GetExposureScoreAsync(t), ScoreJson, "/api/exposureScore");

	[Fact]
	public async Task GetExposureScore_MapsTheScoreAndTime()
	{
		var score = await TestClient.ReadAsync((c, t) => c.Scores.GetExposureScoreAsync(t), ScoreJson);

		(score.Score, score.Time).Should().Be((33.5, new DateTimeOffset(2019, 12, 3, 7, 23, 53, 280, TimeSpan.Zero)));
		(score.RbacGroupName, score.RbacGroupId).Should().Be(((string?)null, (string?)null));
	}

	[Fact]
	public async Task GetExposureScore_ToleratesLooseJson()
	{
		var score = await TestClient.ReadAsync((c, t) => c.Scores.GetExposureScoreAsync(t), """{"score":"12.25","time":"","rbacGroupId":7,"extra":[1]}""");

		(score.Score, score.Time, score.RbacGroupId).Should().Be((12.25, (DateTimeOffset?)null, "7"));
	}

	[Fact]
	public Task GetExposureScore_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Scores.GetExposureScoreAsync(t));

	[Fact]
	public Task ListExposureScoresByMachineGroups_SendsGetExposureScoreByMachineGroups()
		=> ShouldGetAsync((c, t) => c.Scores.ListExposureScoresByMachineGroupsAsync(t), Envelope(""), "/api/exposureScore/ByMachineGroups");

	[Fact]
	public async Task ListExposureScoresByMachineGroups_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Scores.ListExposureScoresByMachineGroupsAsync(t),
			Envelope("""{"time":"2019-12-03T09:51:28Z","score":41.38,"rbacGroupName":"GroupOne","rbacGroupId":"12"},{"time":"2019-12-03T09:51:29Z","score":37.4,"rbacGroupName":"GroupTwo"}"""));

		page.Value.Should().HaveCount(2);
		var first = page.Value[0];
		(first.Score, first.Time, first.RbacGroupName, first.RbacGroupId).Should().Be((41.38, new DateTimeOffset(2019, 12, 3, 9, 51, 28, TimeSpan.Zero), "GroupOne", "12"));
		(page.Value[1].RbacGroupName, page.Value[1].RbacGroupId).Should().Be(("GroupTwo", (string?)null));
	}

	[Fact]
	public Task ListExposureScoresByMachineGroups_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Scores.ListExposureScoresByMachineGroupsAsync(t));

	[Fact]
	public Task GetConfigurationScore_SendsGetConfigurationScore()
		=> ShouldGetAsync((c, t) => c.Scores.GetConfigurationScoreAsync(t), """{"time":"2019-12-03T09:15:58.1665846Z","score":340}""", "/api/configurationScore");

	[Fact]
	public async Task GetConfigurationScore_MapsTheScoreAndTime()
	{
		var score = await TestClient.ReadAsync((c, t) => c.Scores.GetConfigurationScoreAsync(t), """{"time":"2019-12-03T09:15:58Z","score":340}""");

		(score.Score, score.Time).Should().Be((340.0, new DateTimeOffset(2019, 12, 3, 9, 15, 58, TimeSpan.Zero)));
	}

	[Fact]
	public Task GetConfigurationScore_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Scores.GetConfigurationScoreAsync(t));
}
