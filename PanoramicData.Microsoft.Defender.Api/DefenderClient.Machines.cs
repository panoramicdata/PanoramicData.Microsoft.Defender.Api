using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using PanoramicData.MicrosoftDefender.Api.Models.Recommendations;
using PanoramicData.MicrosoftDefender.Api.Models.Users;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using SoftwareProduct = PanoramicData.MicrosoftDefender.Api.Models.Software.SoftwareProduct;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Devices: list, find, tag and read what is on them (<c>machines</c>).</summary>
	public IMachinesApi Machines => field ??= For<IMachinesApi>();

	/// <summary>Enumerates every device, following the page links (<c>GET machines</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device.</returns>
	public IAsyncEnumerable<Machine> GetAllMachinesAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.ListAsync(options, token), cancellationToken);

	/// <summary>Enumerates the users that logged on to a device (<c>GET machines/{id}/logonusers</c>).</summary>
	/// <param name="id">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every user.</returns>
	public IAsyncEnumerable<User> GetAllMachineLogOnUsersAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.ListLogOnUsersAsync(id, token), cancellationToken);

	/// <summary>Enumerates the alerts of a device (<c>GET machines/{id}/alerts</c>).</summary>
	/// <param name="id">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every alert.</returns>
	public IAsyncEnumerable<Alert> GetAllMachineAlertsAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.ListAlertsAsync(id, token), cancellationToken);

	/// <summary>Enumerates the devices carrying a tag (<c>GET machines/findbytag</c>).</summary>
	/// <param name="tag">The tag.</param>
	/// <param name="options">Optional switches, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every matching device.</returns>
	public IAsyncEnumerable<Machine> GetAllMachinesByTagAsync(string tag, FindByTagOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.FindByTagAsync(tag, options, token), cancellationToken);

	/// <summary>Enumerates the vulnerabilities of a device (<c>GET machines/{machineId}/vulnerabilities</c>).</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every vulnerability.</returns>
	public IAsyncEnumerable<Vulnerability> GetAllMachineVulnerabilitiesByMachineAsync(string machineId, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.ListVulnerabilitiesAsync(machineId, token), cancellationToken);

	/// <summary>Enumerates the software on a device (<c>GET machines/{machineId}/software</c>).</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every software product.</returns>
	public IAsyncEnumerable<SoftwareProduct> GetAllMachineSoftwareAsync(string machineId, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.ListSoftwareAsync(machineId, token), cancellationToken);

	/// <summary>Enumerates the KBs a device is missing (<c>GET machines/{machineId}/getmissingkbs</c>).</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every missing KB.</returns>
	public IAsyncEnumerable<MissingKb> GetAllMachineMissingKbsAsync(string machineId, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.ListMissingKbsAsync(machineId, token), cancellationToken);

	/// <summary>Enumerates the recommendations for a device (<c>GET machines/{machineId}/recommendations</c>).</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every recommendation.</returns>
	public IAsyncEnumerable<Recommendation> GetAllMachineRecommendationsAsync(string machineId, CancellationToken cancellationToken)
		=> GetAllAsync(token => Machines.ListRecommendationsAsync(machineId, token), cancellationToken);
}
