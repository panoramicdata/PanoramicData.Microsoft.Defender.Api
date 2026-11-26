using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Software;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// The software inventory of the tenant and what is known about each product (<c>Software</c>). Needs the
/// <c>Software.Read.All</c> application permission unless an operation says otherwise. The documentation marks these pages as
/// describing a prerelease surface that may change.
/// </summary>
public interface ISoftwareApi
{
	/// <summary>Lists the products in the software inventory, one page at a time (<c>GET Software</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name id, name and vendor; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of products.</returns>
	[Get("Software")]
	Task<ODataPage<SoftwareProduct>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one product by its id (<c>GET Software/{id}</c>).</summary>
	/// <param name="id">The product id, such as <c>microsoft-_-edge</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The product.</returns>
	[Get("Software/{id}")]
	Task<SoftwareProduct> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists how many installations of each version of a product exist (<c>GET Software/{id}/distributions</c>).</summary>
	/// <param name="id">The product id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of version distributions.</returns>
	[Get("Software/{id}/distributions")]
	Task<ODataPage<SoftwareVersionDistribution>> ListDistributionsAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the devices that have a product installed (<c>GET Software/{id}/machineReferences</c>).</summary>
	/// <param name="id">The product id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of references to the devices.</returns>
	[Get("Software/{id}/machineReferences")]
	Task<ODataPage<MachineReference>> ListMachineReferencesAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the vulnerabilities found in a product (<c>GET Software/{id}/vulnerabilities</c>). Needs
	/// <c>Vulnerability.Read.All</c>.
	/// </summary>
	/// <param name="id">The product id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of vulnerabilities.</returns>
	[Get("Software/{id}/vulnerabilities")]
	Task<ODataPage<Vulnerability>> ListVulnerabilitiesAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the security updates (KB articles) that devices are missing for a product (<c>GET Software/{id}/getmissingkbs</c>).</summary>
	/// <param name="id">The product id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of missing updates.</returns>
	[Get("Software/{id}/getmissingkbs")]
	Task<ODataPage<SoftwareMissingKb>> ListMissingKbsAsync(string id, CancellationToken cancellationToken);
}
