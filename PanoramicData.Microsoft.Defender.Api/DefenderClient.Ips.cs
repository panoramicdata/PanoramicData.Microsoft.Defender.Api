using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>IP address lookups (<c>ips</c>).</summary>
	public IIpsApi Ips => field ??= For<IIpsApi>();

	/// <summary>Enumerates every alert related to an IP address, following the page links (<c>GET ips/{ip}/alerts</c>).</summary>
	/// <param name="ip">The IP address.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every alert.</returns>
	public IAsyncEnumerable<Alert> GetAllIpAlertsAsync(string ip, CancellationToken cancellationToken)
		=> GetAllAsync(token => Ips.ListAlertsAsync(ip, token), cancellationToken);
}
