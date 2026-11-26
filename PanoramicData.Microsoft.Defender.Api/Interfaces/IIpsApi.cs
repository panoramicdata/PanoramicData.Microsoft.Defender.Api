using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Ips;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Finds the alerts and statistics connected to an IP address (<c>ips</c>). The alert list needs <c>Alert.Read.All</c> (or
/// <c>Alert.ReadWrite.All</c>), the statistics <c>Ip.Read.All</c>. An invalid address answers 400. The address is escaped
/// as one path segment.
/// </summary>
public interface IIpsApi
{
	/// <summary>Lists the alerts related to an IP address (<c>GET ips/{ip}/alerts</c>).</summary>
	/// <param name="ip">The IP address.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of alerts.</returns>
	[Get("ips/{ip}/alerts")]
	Task<ODataPage<Alert>> ListAlertsAsync(string ip, CancellationToken cancellationToken);

	/// <summary>Gets how often the organisation connected to an IP address (<c>GET ips/{ip}/stats</c>).</summary>
	/// <param name="ip">The IP address.</param>
	/// <param name="options">The look-back window, or <see langword="null"/> for the service default of 30 days (at most 720 hours).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The IP statistics.</returns>
	[Get("ips/{ip}/stats")]
	Task<IpStatistics> GetStatisticsAsync(string ip, [Query] EntityStatisticsOptions? options, CancellationToken cancellationToken);
}
