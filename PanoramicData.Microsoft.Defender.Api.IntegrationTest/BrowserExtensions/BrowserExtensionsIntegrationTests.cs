using PanoramicData.MicrosoftDefender.Api.Models;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.BrowserExtensions;

/// <summary>
/// Read-only check of the browser-extension permission reference data against a live tenant (needs the
/// <c>Software.Read.All</c> application permission). Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class BrowserExtensionsIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task ListPermissionsInfoAsync_ReturnsPermissionDescriptions()
	{
		var page = await fixture.Client.BrowserExtensions.ListPermissionsInfoAsync(new ODataListOptions { Top = 5 }, Ct);

		// The documented response nests the permissions one level down, so accept either shape.
		var permissions = page.Value.SelectMany(p => p.Value.Count > 0 ? p.Value : [p]).ToList();
		permissions.Should().NotBeEmpty();
		permissions.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Key) && !string.IsNullOrEmpty(p.PermissionName));
	}
}
