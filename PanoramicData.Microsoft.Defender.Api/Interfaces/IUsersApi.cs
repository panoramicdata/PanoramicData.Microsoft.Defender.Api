using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Finds the alerts and devices connected to a user (<c>users</c>). Users are addressed by their user name only, not
/// their UPN: for <c>user1@contoso.com</c> pass <c>user1</c>. The name is escaped as one path segment, so a name with
/// special characters is sent percent-encoded. The alert list needs <c>Alert.Read.All</c> (or <c>Alert.ReadWrite.All</c>),
/// the device list <c>Machine.ReadWrite.All</c>.
/// </summary>
public interface IUsersApi
{
	/// <summary>Lists the alerts related to a user (<c>GET users/{id}/alerts</c>).</summary>
	/// <param name="userName">The user name without the domain part, for example <c>user1</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of alerts.</returns>
	[Get("users/{userName}/alerts")]
	Task<ODataPage<Alert>> ListAlertsAsync(string userName, CancellationToken cancellationToken);

	/// <summary>Lists the devices related to a user (<c>GET users/{id}/machines</c>).</summary>
	/// <param name="userName">The user name without the domain part, for example <c>user1</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of devices.</returns>
	[Get("users/{userName}/machines")]
	Task<ODataPage<Machine>> ListMachinesAsync(string userName, CancellationToken cancellationToken);
}
