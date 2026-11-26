namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Entities;

/// <summary>
/// Reads the identifiers the entity lookup tests need (a file hash, a domain, an IP address, a user name, a scan session id).
/// A missing one fails the test naming the key; nothing is skipped.
/// </summary>
internal static class TestSetting
{
	private const string Project = "PanoramicData.Microsoft.Defender.Api.IntegrationTest";

	public static string Required(DefenderFixture fixture, string key)
		=> fixture.Configuration[key] is { Length: > 0 } value
			? value
			: throw new InvalidOperationException(
				$"This integration test needs '{key}': an identifier that exists in the tenant. Set it with 'dotnet user-secrets set {key} <value> --project {Project}', "
				+ "or as an environment variable (use __ for each colon). See docs/TESTING.md.");
}
