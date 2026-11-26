using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Alerts and the entities related to them (<c>alerts</c>).</summary>
	public IAlertsApi Alerts => field ??= For<IAlertsApi>();

	/// <summary>Enumerates every alert, following the page links (<c>GET alerts</c>).</summary>
	/// <param name="options">Options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every alert.</returns>
	public IAsyncEnumerable<Alert> GetAllAlertsAsync(AlertListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Alerts.ListAsync(options, token), cancellationToken);
}
