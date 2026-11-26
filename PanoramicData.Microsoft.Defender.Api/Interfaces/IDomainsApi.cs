using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Domains;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Finds the alerts, devices and statistics connected to a domain name (<c>domains</c>). The list operations need the
/// <c>Alert.ReadWrite.All</c> and <c>Machine.ReadWrite.All</c> application permissions, the statistics <c>URL.Read.All</c>.
/// The domain is escaped as one path segment.
/// </summary>
public interface IDomainsApi
{
	/// <summary>Lists the alerts related to a domain (<c>GET domains/{domain}/alerts</c>); an unknown domain gives an empty list.</summary>
	/// <param name="domain">The domain name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of alerts.</returns>
	[Get("domains/{domain}/alerts")]
	Task<ODataPage<Alert>> ListAlertsAsync(string domain, CancellationToken cancellationToken);

	/// <summary>Lists the devices that contacted a domain (<c>GET domains/{domain}/machines</c>); the service returns at most 500.</summary>
	/// <param name="domain">The domain name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of devices.</returns>
	[Get("domains/{domain}/machines")]
	Task<ODataPage<Machine>> ListMachinesAsync(string domain, CancellationToken cancellationToken);

	/// <summary>
	/// Gets how often the organisation contacted a domain (<c>GET domains/{domain}/stats</c>). An unknown domain answers 200
	/// with a prevalence of zero.
	/// </summary>
	/// <param name="domain">The domain name.</param>
	/// <param name="options">The look-back window, or <see langword="null"/> for the service default of 30 days (at most 720 hours).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The domain statistics.</returns>
	[Get("domains/{domain}/stats")]
	Task<DomainStatistics> GetStatisticsAsync(string domain, [Query] EntityStatisticsOptions? options, CancellationToken cancellationToken);
}
