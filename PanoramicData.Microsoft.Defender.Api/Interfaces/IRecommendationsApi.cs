using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Recommendations;
using PanoramicData.MicrosoftDefender.Api.Models.Software;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Security recommendations that Defender Vulnerability Management raises for the tenant (<c>recommendations</c>). Needs the
/// <c>SecurityRecommendation.Read.All</c> application permission unless an operation says otherwise. The documentation marks
/// these pages as describing a prerelease surface that may change.
/// </summary>
public interface IRecommendationsApi
{
	/// <summary>Lists the recommendations, one page at a time (<c>GET recommendations</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name id, productName, vendor, recommendedVersion, recommendationCategory, subCategory, severityScore, remediationType, recommendedProgram, recommendedVendor and status; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of recommendations.</returns>
	[Get("recommendations")]
	Task<ODataPage<Recommendation>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one recommendation by its id (<c>GET recommendations/{id}</c>).</summary>
	/// <param name="id">The recommendation id, such as <c>va-_-google-_-chrome</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The recommendation.</returns>
	[Get("recommendations/{id}")]
	Task<Recommendation> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the devices a recommendation applies to (<c>GET recommendations/{id}/machineReferences</c>).</summary>
	/// <param name="id">The recommendation id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of references to the devices.</returns>
	[Get("recommendations/{id}/machineReferences")]
	Task<ODataPage<MachineReference>> ListMachineReferencesAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the vulnerabilities a recommendation would address (<c>GET recommendations/{id}/vulnerabilities</c>). Needs
	/// <c>Vulnerability.Read.All</c>.
	/// </summary>
	/// <param name="id">The recommendation id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of vulnerabilities.</returns>
	[Get("recommendations/{id}/vulnerabilities")]
	Task<ODataPage<Vulnerability>> ListVulnerabilitiesAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the software a recommendation relates to (<c>GET recommendations/{id}/software</c>). Needs
	/// <c>Software.Read.All</c> (application) rather than the recommendation permission.
	/// </summary>
	/// <remarks>
	/// The documentation example for the response is one bare product, not a collection, so a single product is returned. If the
	/// service answers with a collection instead, the result reads as an empty product. Unverified.
	/// </remarks>
	/// <param name="id">The recommendation id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The related product.</returns>
	[Get("recommendations/{id}/software")]
	Task<SoftwareProduct> GetSoftwareAsync(string id, CancellationToken cancellationToken);
}
