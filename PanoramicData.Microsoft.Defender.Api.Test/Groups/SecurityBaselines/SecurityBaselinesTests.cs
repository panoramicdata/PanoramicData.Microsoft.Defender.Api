using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The security baseline configuration and profile operations: each pins its request and maps a realistic response.</summary>
public class SecurityBaselinesTests
{
	private const string ConfigurationJson = """
		{"id":"9.3.9","uniqueId":"CIS_1.4.0-windows_server_2016_9.3.9","benchmarkName":"CIS","benchmarkVersion":"1.4.0-windows_server_2016","name":"Log dropped packets","description":"<p>desc</p>","category":"Public Profile","complianceLevels":["Level 1","Level 2"],"cce":"CCE-35116-3","rationale":"<p>why</p>","remediation":"<div>fix</div>","recommendedValue":["Equals '1'"],"source":["hkey_local_machine\\software"],"isCustom":false}
		""";

	private const string ProfileJson = """
		{"id":"prof-1","name":"Windows 10 profile","description":"important","benchmark":"CIS","version":"1.0.0","operatingSystem":"Windows 10","operatingSystemVersion":"1909","status":true,"complianceLevel":"Level 1","settingsNumber":51,"createdBy":"user@example.test","lastUpdatedBy":null,"createdOnTimestampUTC":"2024-01-02T03:04:05Z","lastUpdateTimestampUTC":"2024-02-03T04:05:06Z","createdOnTimeOffset":"2024-03-04T05:06:07Z","lastUpdateTimeOffset":"2024-04-05T06:07:08Z","passedDevices":4,"totalDevices":10}
		""";

	[Fact]
	public Task ListConfigurations_SendsGetBaselineConfigurations()
		=> ShouldGetAsync((c, t) => c.SecurityBaselines.ListConfigurationsAsync(null, t), Envelope(""), "/api/baselineConfigurations");

	[Fact]
	public Task ListConfigurations_SendsTheODataOptions()
		=> ShouldGetAsync((c, t) => c.SecurityBaselines.ListConfigurationsAsync(new ODataListOptions { Filter = "CCE eq 'CCE-1'", Top = 10000, Skip = 2 }, t), Envelope(""),
			"/api/baselineConfigurations", "?$filter=CCE%20eq%20%27CCE-1%27&$top=10000&$skip=2");

	[Fact]
	public async Task ListConfigurations_MapsEveryField_AndToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.SecurityBaselines.ListConfigurationsAsync(null, t),
			Envelope(ConfigurationJson + ",{\"id\":\"2\",\"cce\":35116,\"isCustom\":\"true\",\"source\":null,\"recommendedValue\":null,\"complianceLevels\":null}"));

		var c0 = page.Value[0];
		(c0.Id, c0.UniqueId, c0.BenchmarkName, c0.BenchmarkVersion, c0.Name, c0.Description, c0.Category, c0.Cce, c0.Rationale, c0.Remediation, c0.IsCustom)
			.Should().Be(("9.3.9", "CIS_1.4.0-windows_server_2016_9.3.9", "CIS", "1.4.0-windows_server_2016", "Log dropped packets", "<p>desc</p>", "Public Profile", "CCE-35116-3", "<p>why</p>", "<div>fix</div>", false));
		c0.ComplianceLevels.Should().Equal("Level 1", "Level 2");
		c0.RecommendedValue.Should().Equal("Equals '1'");
		c0.Source.Should().Equal("hkey_local_machine\\software");
		var loose = page.Value[1];
		(loose.Cce, loose.IsCustom).Should().Be(("35116", true));
		loose.Source.Should().BeEmpty();
		loose.RecommendedValue.Should().BeEmpty();
		loose.ComplianceLevels.Should().BeEmpty();
	}

	[Fact]
	public Task ListConfigurations_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.SecurityBaselines.ListConfigurationsAsync(null, t));

	[Fact]
	public Task ListProfiles_SendsGetBaselineProfiles()
		=> ShouldGetAsync((c, t) => c.SecurityBaselines.ListProfilesAsync(null, t), Envelope(""), "/api/baselineProfiles");

	[Fact]
	public Task ListProfiles_SendsTheODataOptions()
		=> ShouldGetAsync((c, t) => c.SecurityBaselines.ListProfilesAsync(new ODataListOptions { Filter = "status eq true", Top = 3 }, t), Envelope(""),
			"/api/baselineProfiles", "?$filter=status%20eq%20true&$top=3");

	[Fact]
	public async Task ListProfiles_MapsEveryField_AndToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.SecurityBaselines.ListProfilesAsync(null, t),
			Envelope(ProfileJson + ",{\"id\":\"p2\",\"status\":\"false\",\"settingsNumber\":\"7\",\"createdOnTimestampUTC\":\"0001-01-01T00:00:00Z\",\"passedDevices\":null}"));

		var p = page.Value[0];
		(p.Id, p.Name, p.Description, p.Benchmark, p.Version, p.OperatingSystem, p.OperatingSystemVersion, p.Status, p.ComplianceLevel, p.SettingsNumber)
			.Should().Be(("prof-1", "Windows 10 profile", "important", "CIS", "1.0.0", "Windows 10", "1909", true, "Level 1", 51L));
		(p.CreatedBy, p.LastUpdatedBy, p.PassedDevices, p.TotalDevices).Should().Be(("user@example.test", (string?)null, 4L, 10L));
		(p.CreatedOnTimestampUtc, p.LastUpdateTimestampUtc, p.CreatedOnTimeOffset, p.LastUpdateTimeOffset).Should().Be((
			new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero), new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero),
			new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero), new DateTimeOffset(2024, 4, 5, 6, 7, 8, TimeSpan.Zero)));
		var loose = page.Value[1];
		(loose.Status, loose.SettingsNumber, loose.PassedDevices).Should().Be((false, 7L, (long?)null));
	}

	[Fact]
	public Task ListProfiles_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.SecurityBaselines.ListProfilesAsync(null, t));

	[Fact]
	public async Task GetAllHelpers_WireTheirOperations()
	{
		await ShouldEnumerateAsync((c, t) => c.GetAllBaselineConfigurationsAsync(new ODataListOptions { Top = 1 }, t), "/api/baselineConfigurations", "?$top=1");
		await ShouldEnumerateAsync((c, t) => c.GetAllBaselineProfilesAsync(null, t), "/api/baselineProfiles");
	}
}
