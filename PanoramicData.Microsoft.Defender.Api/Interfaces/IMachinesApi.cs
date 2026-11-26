using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using PanoramicData.MicrosoftDefender.Api.Models.Recommendations;
using PanoramicData.MicrosoftDefender.Api.Models.Users;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using Refit;
using SoftwareProduct = PanoramicData.MicrosoftDefender.Api.Models.Software.Software;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Devices (<c>machines</c>): listing and finding them, reading what is installed or exposed on one, and changing its tags
/// and value. Read operations need <c>Machine.Read.All</c> (or the permission named per method); the write operations
/// need <c>Machine.ReadWrite.All</c> and are refused by a read-only client. Many list operations answer 404 rather than an
/// empty page when there is nothing to list.
/// </summary>
public interface IMachinesApi
{
	/// <summary>Lists devices seen recently, one page at a time (<c>GET machines</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name computerDnsName, id, version, deviceValue, aadDeviceId, machineTags, lastSeen, exposureLevel, onboardingStatus, lastIpAddress, healthStatus, osPlatform, riskScore and rbacGroupId; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of devices.</returns>
	[Get("machines")]
	Task<ODataPage<Machine>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one device by its id or computer name (<c>GET machines/{id}</c>).</summary>
	/// <param name="id">The device id or computer name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The device.</returns>
	[Get("machines/{id}")]
	Task<Machine> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Changes a device's tags and/or value (<c>PATCH machines/{machineId}</c>). A write operation. When tags are sent they
	/// replace the existing list.
	/// </summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="request">The new tag list and/or value.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated device.</returns>
	[Patch("machines/{machineId}")]
	Task<Machine> UpdateAsync(string machineId, [Body] UpdateMachineRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the users that have logged on to a device (<c>GET machines/{id}/logonusers</c>). Needs <c>User.Read.All</c>; 404 when the device is unknown.</summary>
	/// <param name="id">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of users.</returns>
	[Get("machines/{id}/logonusers")]
	Task<ODataPage<User>> ListLogOnUsersAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the alerts related to a device (<c>GET machines/{id}/alerts</c>). Needs <c>Alert.ReadWrite.All</c>.</summary>
	/// <param name="id">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of alerts.</returns>
	[Get("machines/{id}/alerts")]
	Task<ODataPage<Alert>> ListAlertsAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Finds the device that used an internal IP address at a time in the past 30 days
	/// (<c>GET machines/find(timestamp={timestamp},key={key})</c>).
	/// </summary>
	/// <remarks>
	/// Application permission only. The documentation's example for this older form uses a different host and path prefix;
	/// <see cref="FindByIpAsync"/> is the documented replacement. The timestamp is sent as UTC ISO 8601 with its colons
	/// escaped as one path segment value. Unverified against a live service.
	/// </remarks>
	/// <param name="timestamp">When the device had the address; within the past 30 days.</param>
	/// <param name="key">The IP address.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of matching devices.</returns>
	[Get("machines/find(timestamp={timestamp},key={key})")]
	Task<ODataPage<Machine>> FindAsync(DateTimeOffset timestamp, string key, CancellationToken cancellationToken);

	/// <summary>
	/// Finds the devices seen with an IP address within 15 minutes either side of a time in the past 30 days
	/// (<c>GET machines/findbyip(ip='{ip}',timestamp={timestamp})</c>). A timestamp older than 30 days answers 400.
	/// </summary>
	/// <remarks>The address is wrapped in single quotes in the path, as documented, and escaped; the timestamp is UTC ISO 8601. Unverified against a live service.</remarks>
	/// <param name="ip">The IP address.</param>
	/// <param name="timestamp">The time of interest.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of matching devices.</returns>
	[Get("machines/findbyip(ip='{ip}',timestamp={timestamp})")]
	Task<ODataPage<Machine>> FindByIpAsync(string ip, DateTimeOffset timestamp, CancellationToken cancellationToken);

	/// <summary>Finds devices carrying a tag (<c>GET machines/findbytag?tag=...</c>).</summary>
	/// <param name="tag">The tag to look for.</param>
	/// <param name="options">Optional switches (prefix matching), or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of matching devices.</returns>
	[Get("machines/findbytag")]
	Task<ODataPage<Machine>> FindByTagAsync(string tag, [Query] FindByTagOptions? options, CancellationToken cancellationToken);

	/// <summary>Adds or removes one tag on a device (<c>POST machines/{id}/tags</c>). A write operation; needs the "Manage security settings" role for user tokens.</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The tag and whether to add or remove it.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated device.</returns>
	[Post("machines/{id}/tags")]
	Task<Machine> AddOrRemoveTagAsync(string id, [Body] MachineTagRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Adds or removes one tag on up to 500 devices in a single call (<c>POST machines/AddOrRemoveTagForMultipleMachines</c>).
	/// A write operation; rate limited to 100 calls a minute.
	/// </summary>
	/// <remarks>The documentation does not describe the response body, so it is ignored.</remarks>
	/// <param name="request">The tag, the action and the device ids.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that completes when the service accepts the change.</returns>
	[Post("machines/AddOrRemoveTagForMultipleMachines")]
	Task AddOrRemoveTagForMultipleMachinesAsync([Body] MultiMachineTagRequest request, CancellationToken cancellationToken);

	/// <summary>Sets a device's value (<c>POST machines/{machineId}/setDeviceValue</c>). A write operation.</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="request">The value to assign.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated device.</returns>
	[Post("machines/{machineId}/setDeviceValue")]
	Task<Machine> SetDeviceValueAsync(string machineId, [Body] SetDeviceValueRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the vulnerabilities found on a device (<c>GET machines/{machineId}/vulnerabilities</c>). Needs <c>Vulnerability.Read.All</c>; 50 calls a minute.</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of vulnerabilities.</returns>
	[Get("machines/{machineId}/vulnerabilities")]
	Task<ODataPage<Vulnerability>> ListVulnerabilitiesAsync(string machineId, CancellationToken cancellationToken);

	/// <summary>Lists the software installed on a device (<c>GET machines/{machineId}/software</c>). Needs <c>Software.Read.All</c>.</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of software products.</returns>
	[Get("machines/{machineId}/software")]
	Task<ODataPage<SoftwareProduct>> ListSoftwareAsync(string machineId, CancellationToken cancellationToken);

	/// <summary>Lists the security updates (KBs) a device is missing (<c>GET machines/{machineId}/getmissingkbs</c>). Needs the <c>Software.Read.All</c> application permission.</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of missing KBs.</returns>
	[Get("machines/{machineId}/getmissingkbs")]
	Task<ODataPage<MissingKb>> ListMissingKbsAsync(string machineId, CancellationToken cancellationToken);

	/// <summary>Lists the security recommendations that apply to a device (<c>GET machines/{machineId}/recommendations</c>). Needs <c>SecurityRecommendation.Read.All</c>.</summary>
	/// <param name="machineId">The device id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of recommendations.</returns>
	[Get("machines/{machineId}/recommendations")]
	Task<ODataPage<Recommendation>> ListRecommendationsAsync(string machineId, CancellationToken cancellationToken);
}
