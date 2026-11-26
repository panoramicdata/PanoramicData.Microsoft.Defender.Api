using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Remediation;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Remediation activities created in the tenant (<c>remediationtasks</c>). Needs the <c>RemediationTasks.Read.All</c>
/// application permission. The documentation marks these pages as describing a prerelease surface that may change, and spells
/// the path both <c>remediationTasks</c> and <c>remediationtasks</c>; the service is believed to ignore case. Unverified.
/// </summary>
public interface IRemediationApi
{
	/// <summary>Lists the remediation activities, one page at a time (<c>GET remediationtasks</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name createdon and status; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of activities.</returns>
	[Get("remediationtasks")]
	Task<ODataPage<RemediationTask>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one remediation activity by its id (<c>GET remediationtasks/{id}</c>).</summary>
	/// <param name="id">The id of the activity.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The activity.</returns>
	[Get("remediationtasks/{id}")]
	Task<RemediationTask> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the exposed devices of a remediation activity (<c>GET remediationtasks/{id}/machineReferences</c>).</summary>
	/// <param name="id">The id of the activity.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of references to the devices.</returns>
	[Get("remediationtasks/{id}/machineReferences")]
	Task<ODataPage<MachineReference>> ListMachineReferencesAsync(string id, CancellationToken cancellationToken);
}
