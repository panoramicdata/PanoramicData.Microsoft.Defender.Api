using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Recommendations;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Security recommendations (<c>recommendations</c>).</summary>
	public IRecommendationsApi Recommendations => field ??= For<IRecommendationsApi>();

	/// <summary>Enumerates every recommendation, following the page links (<c>GET recommendations</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every recommendation.</returns>
	public IAsyncEnumerable<Recommendation> GetAllRecommendationsAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Recommendations.ListAsync(options, token), cancellationToken);

	/// <summary>Enumerates every device a recommendation applies to (<c>GET recommendations/{id}/machineReferences</c>).</summary>
	/// <param name="id">The recommendation id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device reference.</returns>
	public IAsyncEnumerable<MachineReference> GetAllRecommendationMachineReferencesAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Recommendations.ListMachineReferencesAsync(id, token), cancellationToken);

	/// <summary>Enumerates every vulnerability a recommendation would address (<c>GET recommendations/{id}/vulnerabilities</c>).</summary>
	/// <param name="id">The recommendation id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every vulnerability.</returns>
	public IAsyncEnumerable<Vulnerability> GetAllRecommendationVulnerabilitiesAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Recommendations.ListVulnerabilitiesAsync(id, token), cancellationToken);
}
