using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Known software vulnerabilities (CVEs) in the tenant, and which devices they affect (<c>vulnerabilities</c>). Needs the
/// <c>Vulnerability.Read.All</c> application permission. Many list operations answer 404 rather than an empty page when
/// there is nothing to list.
/// </summary>
public interface IVulnerabilitiesApi
{
	/// <summary>Lists the vulnerabilities known to the tenant, one page at a time (<c>GET vulnerabilities</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name id, name, description, cvssV3, publishedOn, severity and updatedOn; <c>$top</c> is limited to 8,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of vulnerabilities.</returns>
	[Get("vulnerabilities")]
	Task<ODataPage<Vulnerability>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one vulnerability by its CVE identifier (<c>GET vulnerabilities/{cveId}</c>).</summary>
	/// <param name="cveId">The CVE identifier, such as <c>CVE-2024-7256</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The vulnerability.</returns>
	[Get("vulnerabilities/{cveId}")]
	Task<Vulnerability> GetAsync(string cveId, CancellationToken cancellationToken);

	/// <summary>Lists the devices affected by a vulnerability (<c>GET vulnerabilities/{cveId}/machineReferences</c>).</summary>
	/// <param name="cveId">The CVE identifier.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of references to the affected devices.</returns>
	[Get("vulnerabilities/{cveId}/machineReferences")]
	Task<ODataPage<MachineReference>> ListMachineReferencesAsync(string cveId, CancellationToken cancellationToken);

	/// <summary>
	/// Lists every vulnerability on every device, one row per device, product and CVE, including the fixing KB where there is
	/// one (<c>GET vulnerabilities/machinesVulnerabilities</c>).
	/// </summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name id, cveId, machineId, fixingKbId, productName, productVersion, severity and productVendor; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of device vulnerabilities.</returns>
	[Get("vulnerabilities/machinesVulnerabilities")]
	Task<ODataPage<MachineVulnerability>> ListMachineVulnerabilitiesAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);
}
