using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>User lookups, by user name (<c>users</c>).</summary>
	public IUsersApi Users => field ??= For<IUsersApi>();

	/// <summary>Enumerates every alert related to a user, following the page links (<c>GET users/{id}/alerts</c>).</summary>
	/// <param name="userName">The user name without the domain part.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every alert.</returns>
	public IAsyncEnumerable<Alert> GetAllUserAlertsAsync(string userName, CancellationToken cancellationToken)
		=> GetAllAsync(token => Users.ListAlertsAsync(userName, token), cancellationToken);

	/// <summary>Enumerates every device related to a user, following the page links (<c>GET users/{id}/machines</c>).</summary>
	/// <param name="userName">The user name without the domain part.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device.</returns>
	public IAsyncEnumerable<Machine> GetAllUserMachinesAsync(string userName, CancellationToken cancellationToken)
		=> GetAllAsync(token => Users.ListMachinesAsync(userName, token), cancellationToken);
}
