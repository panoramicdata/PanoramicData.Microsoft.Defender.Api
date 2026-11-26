using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Security alerts and the entities related to them (<c>alerts</c>). Reading needs the <c>Alert.ReadWrite.All</c>
/// application permission (the documented one; there is no read-only alert permission), and the related-entity operations
/// need the permission of the entity they return. Alerts are only available for the configured retention period. The
/// update and create operations change tenant data and are refused by a read-only client.
/// </summary>
public interface IAlertsApi
{
	/// <summary>Lists alerts, one page at a time (<c>GET alerts</c>).</summary>
	/// <param name="options">Options, or <see langword="null"/> for none. <c>$filter</c> may name alertCreationTime, lastUpdateTime, incidentId, InvestigationId, id, assignedTo, detectionSource, lastEventTime, status, severity and category; <c>$top</c> is limited to 10,000; <c>$expand</c> may be <c>evidence</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of alerts.</returns>
	[Get("alerts")]
	Task<ODataPage<Alert>> ListAsync([Query] AlertListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one alert (<c>GET alerts/{id}</c>); answers 404 when it does not exist.</summary>
	/// <param name="id">The alert id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The alert.</returns>
	[Get("alerts/{id}")]
	Task<Alert> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Changes one alert (<c>PATCH alerts/{id}</c>). Changes tenant data: never run it against a tenant you do not intend
	/// to change. Needs the <c>Alerts investigation</c> role when acting as a user.
	/// </summary>
	/// <param name="id">The alert id.</param>
	/// <param name="update">The changes; unset properties are left alone.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated alert.</returns>
	/// <remarks>The documented permission name on the update page (<c>Alerts.ReadWrite.All</c>) is a typo for <c>Alert.ReadWrite.All</c>. Unverified.</remarks>
	[Patch("alerts/{id}")]
	Task<Alert> UpdateAsync(string id, [Body] AlertUpdate update, CancellationToken cancellationToken);

	/// <summary>
	/// Applies the same change to several alerts (<c>POST alerts/batchUpdate</c>). Changes tenant data. Limited to 10 calls a
	/// minute and 500 an hour. The service answers 200 with an empty body.
	/// </summary>
	/// <param name="update">The alert ids and the changes.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that completes when the service has accepted the change.</returns>
	[Post("alerts/batchUpdate")]
	Task BatchUpdateAsync([Body] AlertBatchUpdate update, CancellationToken cancellationToken);

	/// <summary>
	/// Raises an alert on top of an advanced-hunting event (<c>POST alerts/CreateAlertByReference</c>). Changes tenant data
	/// and starts an automated investigation; an open alert on the same device with the same title absorbs the new one.
	/// Limited to 15 calls a minute. Answers 404 when no event matches.
	/// </summary>
	/// <param name="request">The event reference and the alert's content.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The alert.</returns>
	[Post("alerts/CreateAlertByReference")]
	Task<Alert> CreateByReferenceAsync([Body] CreateAlertRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the domains related to an alert (<c>GET alerts/{id}/domains</c>; <c>URL.Read.All</c>).</summary>
	/// <param name="id">The alert id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of domains.</returns>
	[Get("alerts/{id}/domains")]
	Task<ODataPage<AlertDomain>> ListDomainsAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the files related to an alert (<c>GET alerts/{id}/files</c>; <c>File.Read.All</c>).</summary>
	/// <param name="id">The alert id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of files.</returns>
	[Get("alerts/{id}/files")]
	Task<ODataPage<AlertFile>> ListFilesAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the IP addresses related to an alert (<c>GET alerts/{id}/ips</c>; <c>Ip.Read.All</c>).</summary>
	/// <param name="id">The alert id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of IP addresses.</returns>
	[Get("alerts/{id}/ips")]
	Task<ODataPage<AlertIp>> ListIpsAsync(string id, CancellationToken cancellationToken);

	/// <summary>Gets the device related to an alert (<c>GET alerts/{id}/machine</c>; <c>Machine.Read.All</c>); 404 when the alert or device is missing.</summary>
	/// <param name="id">The alert id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The device.</returns>
	[Get("alerts/{id}/machine")]
	Task<AlertMachine> GetMachineAsync(string id, CancellationToken cancellationToken);

	/// <summary>Gets the user related to an alert (<c>GET alerts/{id}/user</c>; <c>User.Read.All</c>); 404 when the alert or user is missing.</summary>
	/// <param name="id">The alert id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The user.</returns>
	[Get("alerts/{id}/user")]
	Task<AlertUser> GetUserAsync(string id, CancellationToken cancellationToken);
}
