using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Indicators;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Custom threat-intelligence indicators (<c>indicators</c>).</summary>
	public IIndicatorsApi Indicators => field ??= For<IIndicatorsApi>();

	/// <summary>Enumerates every active indicator, following the page links (<c>GET indicators</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every indicator.</returns>
	public IAsyncEnumerable<Indicator> GetAllIndicatorsAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Indicators.ListAsync(options, token), cancellationToken);
}
