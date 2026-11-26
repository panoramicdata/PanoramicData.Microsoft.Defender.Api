using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Domain lookups (<c>domains</c>).</summary>
	public IDomainsApi Domains => field ??= For<IDomainsApi>();

	/// <summary>Enumerates every alert related to a domain, following the page links (<c>GET domains/{domain}/alerts</c>).</summary>
	/// <param name="domain">The domain name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every alert.</returns>
	public IAsyncEnumerable<Alert> GetAllDomainAlertsAsync(string domain, CancellationToken cancellationToken)
		=> GetAllAsync(token => Domains.ListAlertsAsync(domain, token), cancellationToken);

	/// <summary>Enumerates every device that contacted a domain, following the page links (<c>GET domains/{domain}/machines</c>).</summary>
	/// <param name="domain">The domain name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device.</returns>
	public IAsyncEnumerable<Machine> GetAllDomainMachinesAsync(string domain, CancellationToken cancellationToken)
		=> GetAllAsync(token => Domains.ListMachinesAsync(domain, token), cancellationToken);
}
