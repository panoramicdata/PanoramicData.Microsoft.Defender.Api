using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.SecurityBaselines;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Security baseline assessment: the configurations being assessed and the profiles that select them. The per-device
/// compliance results are in <see cref="IAssessmentExportsApi"/>. The documentation does not state the permission for these
/// two operations; <c>SecurityBaselinesAssessment.Read.All</c>, which the compliance exports need, is assumed. Unverified.
/// </summary>
public interface ISecurityBaselinesApi
{
	/// <summary>Lists the configurations assessed in active baseline profiles (<c>GET baselineConfigurations</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name id, category, name and CCE; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of configurations.</returns>
	[Get("baselineConfigurations")]
	Task<ODataPage<BaselineConfiguration>> ListConfigurationsAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Lists the baseline assessment profiles of the organisation (<c>GET baselineProfiles</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name id, name, operatingSystem, operatingSystemVersion, status, settingsNumber, passedDevices and totalDevices; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of profiles.</returns>
	[Get("baselineProfiles")]
	Task<ODataPage<BaselineProfile>> ListProfilesAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);
}
