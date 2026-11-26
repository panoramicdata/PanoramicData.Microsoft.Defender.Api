using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The browser-extension permission reference operation: it pins its request and maps a realistic response.</summary>
public class BrowserExtensionsTests
{
	[Fact]
	public Task ListPermissionsInfo_SendsGetBrowserExtensionsPermissionsInfo()
		=> ShouldGetAsync((c, t) => c.BrowserExtensions.ListPermissionsInfoAsync(null, t), Envelope(""), "/api/browserextensions/permissionsinfo");

	[Fact]
	public Task ListPermissionsInfo_SendsTheODataOptions()
		=> ShouldGetAsync((c, t) => c.BrowserExtensions.ListPermissionsInfoAsync(new ODataListOptions { Filter = "id eq 'tabs'", Top = 10000, Skip = 3 }, t), Envelope(""),
			"/api/browserextensions/permissionsinfo", "?$filter=id%20eq%20%27tabs%27&$top=10000&$skip=3");

	[Fact]
	public async Task ListPermissionsInfo_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.BrowserExtensions.ListPermissionsInfoAsync(null, t),
			Envelope("""{"key":"audioCapture","permissionName":"Capture audio","description":"Records sound."}"""));

		var p = page.Value.Should().ContainSingle().Subject;
		(p.Key, p.PermissionName, p.Description).Should().Be(("audioCapture", "Capture audio", "Records sound."));
		p.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task ListPermissionsInfo_ReadsTheNestedShapeTheDocumentationShows()
	{
		var page = await TestClient.ReadAsync((c, t) => c.BrowserExtensions.ListPermissionsInfoAsync(null, t),
			Envelope("""{"value":[{"key":"browsingData","permissionName":"Clear data","description":"d"},{"key":"tabs","value":null}]}"""));

		var wrapper = page.Value.Should().ContainSingle().Subject;
		wrapper.Key.Should().BeNull();
		wrapper.Value.Select(p => p.Key).Should().Equal("browsingData", "tabs");
		wrapper.Value[1].Value.Should().BeEmpty();
	}

	[Fact]
	public Task ListPermissionsInfo_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.BrowserExtensions.ListPermissionsInfoAsync(null, t));

	[Fact]
	public Task GetAllBrowserExtensionPermissions_WiresTheOperation()
		=> ShouldEnumerateAsync((c, t) => c.GetAllBrowserExtensionPermissionsAsync(new ODataListOptions { Top = 4 }, t), "/api/browserextensions/permissionsinfo", "?$top=4");
}
