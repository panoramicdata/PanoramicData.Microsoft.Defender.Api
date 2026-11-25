using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Vulnerabilities and the devices they affect (<c>vulnerabilities</c>).</summary>
	public IVulnerabilitiesApi Vulnerabilities => field ??= For<IVulnerabilitiesApi>();

	/// <summary>Enumerates every vulnerability, following the page links (<c>GET vulnerabilities</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every vulnerability.</returns>
	public IAsyncEnumerable<Vulnerability> GetAllVulnerabilitiesAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Vulnerabilities.ListAsync(options, token), cancellationToken);

	/// <summary>Enumerates every device affected by a vulnerability (<c>GET vulnerabilities/{cveId}/machineReferences</c>).</summary>
	/// <param name="cveId">The CVE identifier.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every affected device.</returns>
	public IAsyncEnumerable<MachineReference> GetAllMachineReferencesAsync(string cveId, CancellationToken cancellationToken)
		=> GetAllAsync(token => Vulnerabilities.ListMachineReferencesAsync(cveId, token), cancellationToken);

	/// <summary>Enumerates every device vulnerability, following the page links (<c>GET vulnerabilities/machinesVulnerabilities</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device vulnerability.</returns>
	public IAsyncEnumerable<MachineVulnerability> GetAllMachineVulnerabilitiesAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Vulnerabilities.ListMachineVulnerabilitiesAsync(options, token), cancellationToken);
}
