using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.BrowserExtensions;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Browser-extension permission reference data (<c>browserextensions</c>).</summary>
	public IBrowserExtensionsApi BrowserExtensions => field ??= For<IBrowserExtensionsApi>();

	/// <summary>Enumerates every browser-extension permission description, following the page links (<c>GET browserextensions/permissionsinfo</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every permission description.</returns>
	public IAsyncEnumerable<BrowserExtensionPermissionInfo> GetAllBrowserExtensionPermissionsAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => BrowserExtensions.ListPermissionsInfoAsync(options, token), cancellationToken);
}
