using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.BrowserExtensions;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Reference data about browser-extension permissions (<c>browserextensions</c>). It complements the per-device browser
/// extension export in <see cref="IAssessmentExportsApi"/>. Needs the <c>Software.Read.All</c> application permission. The
/// documentation marks the page as describing a prerelease surface that may change.
/// </summary>
public interface IBrowserExtensionsApi
{
	/// <summary>Lists what each browser-extension permission allows (<c>GET browserextensions/permissionsinfo</c>).</summary>
	/// <remarks>
	/// The documentation lists the <c>$filter</c> properties of the vulnerabilities page (id, name, description, cvssV3,
	/// publishedOn, severity, updatedOn), which cannot be right for this data, and its response example nests a list inside an
	/// item. The options are passed through and the service decides. Unverified.
	/// </remarks>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of permission descriptions.</returns>
	[Get("browserextensions/permissionsinfo")]
	Task<ODataPage<BrowserExtensionPermissionInfo>> ListPermissionsInfoAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);
}
