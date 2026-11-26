using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Remediation;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Remediation activities (<c>remediationtasks</c>).</summary>
	public IRemediationApi Remediation => field ??= For<IRemediationApi>();

	/// <summary>Enumerates every remediation activity, following the page links (<c>GET remediationtasks</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every activity.</returns>
	public IAsyncEnumerable<RemediationTask> GetAllRemediationTasksAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Remediation.ListAsync(options, token), cancellationToken);

	/// <summary>Enumerates every exposed device of a remediation activity (<c>GET remediationtasks/{id}/machineReferences</c>).</summary>
	/// <param name="id">The id of the activity.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device reference.</returns>
	public IAsyncEnumerable<MachineReference> GetAllRemediationTaskMachineReferencesAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Remediation.ListMachineReferencesAsync(id, token), cancellationToken);
}
