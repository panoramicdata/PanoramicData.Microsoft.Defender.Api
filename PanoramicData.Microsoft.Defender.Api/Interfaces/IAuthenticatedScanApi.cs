using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Authenticated network (SNMP) scans: their definitions, the agents that run them and their history
/// (<c>DeviceAuthenticatedScanDefinitions</c>, <c>DeviceAuthenticatedScanAgents</c>). Reads need the <c>Machine.Read.All</c>
/// application permission, changes <c>Machine.ReadWrite.All</c>. Microsoft marks these pages as describing a prereleased
/// product. Three operations change state (create, update, delete) and are refused by a read-only client; the two history
/// queries are POSTs that only read and are allowed.
/// </summary>
public interface IAuthenticatedScanApi
{
	/// <summary>Lists the scan definitions (<c>GET DeviceAuthenticatedScanDefinitions</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of scan definitions.</returns>
	[Get("DeviceAuthenticatedScanDefinitions")]
	Task<ODataPage<ScanDefinition>> ListDefinitionsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Creates a scan definition (<c>POST DeviceAuthenticatedScanDefinitions</c>). Changes state and sends credentials; a
	/// read-only client refuses it.
	/// </summary>
	/// <param name="request">The definition to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created definition (the service answers 200).</returns>
	[Post("DeviceAuthenticatedScanDefinitions")]
	Task<ScanDefinition> CreateDefinitionAsync([Body] CreateScanDefinitionRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a scan definition (<c>PATCH DeviceAuthenticatedScanDefinitions/{id}</c>). Changes state; a read-only client
	/// refuses it. The page documents this operation only by example, so the exact semantics are unverified.
	/// </summary>
	/// <param name="id">The definition id.</param>
	/// <param name="request">The members to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated definition.</returns>
	[Patch("DeviceAuthenticatedScanDefinitions/{id}")]
	Task<ScanDefinition> UpdateDefinitionAsync(string id, [Body] UpdateScanDefinitionRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes scan definitions (<c>POST DeviceAuthenticatedScanDefinitions/BatchDelete</c>). Destructive; a read-only client
	/// refuses it. The page documents it only by example and shows no response, so any success status is accepted and nothing
	/// is read from the body.
	/// </summary>
	/// <param name="request">The ids to delete.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that completes when the service accepts the request.</returns>
	[Post("DeviceAuthenticatedScanDefinitions/BatchDelete")]
	Task DeleteDefinitionsAsync([Body] DeleteScanDefinitionsRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the scan agents (<c>GET DeviceAuthenticatedScanAgents</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of scan agents.</returns>
	[Get("DeviceAuthenticatedScanAgents")]
	Task<ODataPage<ScanAgent>> ListAgentsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets one scan agent (<c>GET DeviceAuthenticatedScanAgents/{id}</c>). The page's request line omits the id although its
	/// example uses one, so the path is inferred (unverified).
	/// </summary>
	/// <param name="id">The agent id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The agent, in whichever of the two documented shapes the service answers.</returns>
	[Get("DeviceAuthenticatedScanAgents/{id}")]
	Task<ScanAgentDetails> GetAgentAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the latest result of each of the given scan definitions (<c>POST
	/// DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId</c>). A POST that only reads: a read-only client
	/// allows it.
	/// </summary>
	/// <param name="request">The definition ids.</param>
	/// <param name="options">Paging options (<c>$top</c> up to 10,000, <c>$skip</c>), or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of history rows.</returns>
	[Post("DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId")]
	Task<ODataPage<ScanHistoryByDefinition>> GetScanHistoryByDefinitionAsync([Body] ScanHistoryByDefinitionRequest request, [Query] ScanHistoryOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the per-target results of the given scan sessions (<c>POST
	/// DeviceAuthenticatedScanDefinitions/GetScanHistoryBySessionId</c>). A POST that only reads: a read-only client allows it.
	/// </summary>
	/// <param name="request">The session ids.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of session results.</returns>
	[Post("DeviceAuthenticatedScanDefinitions/GetScanHistoryBySessionId")]
	Task<ODataPage<ScanHistoryBySession>> GetScanHistoryBySessionAsync([Body] ScanHistoryBySessionRequest request, CancellationToken cancellationToken);
}
