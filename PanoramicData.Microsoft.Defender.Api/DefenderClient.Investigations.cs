using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Investigations;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Automated investigations (<c>investigations</c>).</summary>
	public IInvestigationsApi Investigations => field ??= For<IInvestigationsApi>();

	/// <summary>Enumerates every investigation, following the page links (<c>GET investigations</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every investigation.</returns>
	public IAsyncEnumerable<Investigation> GetAllInvestigationsAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Investigations.ListAsync(options, token), cancellationToken);
}
