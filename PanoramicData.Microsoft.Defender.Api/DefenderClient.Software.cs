using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Software;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>The software inventory and per-product details (<c>Software</c>).</summary>
	public ISoftwareApi Software => field ??= For<ISoftwareApi>();

	/// <summary>Enumerates every product in the software inventory, following the page links (<c>GET Software</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every product.</returns>
	public IAsyncEnumerable<SoftwareProduct> GetAllSoftwareAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Software.ListAsync(options, token), cancellationToken);

	/// <summary>Enumerates every device that has a product installed (<c>GET Software/{id}/machineReferences</c>).</summary>
	/// <param name="id">The product id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device reference.</returns>
	public IAsyncEnumerable<MachineReference> GetAllSoftwareMachineReferencesAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Software.ListMachineReferencesAsync(id, token), cancellationToken);

	/// <summary>Enumerates every vulnerability found in a product (<c>GET Software/{id}/vulnerabilities</c>).</summary>
	/// <param name="id">The product id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every vulnerability.</returns>
	public IAsyncEnumerable<Vulnerability> GetAllSoftwareVulnerabilitiesAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Software.ListVulnerabilitiesAsync(id, token), cancellationToken);
}
