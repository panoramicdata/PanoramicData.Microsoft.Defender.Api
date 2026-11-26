using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.MachineActions;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Actions requested on devices (<c>machines/{id}/...</c> to request one, <c>machineactions</c> to follow it). Every
/// operation that requests or cancels an action has a real effect on the device, is refused by a read-only client, and is
/// answered with the resulting action. Requesting an action that is already pending for the device answers 400. Each
/// requesting operation needs its own application permission, named below.
/// </summary>
public interface IMachineActionsApi
{
	/// <summary>Isolates a device from the network (<c>POST machines/{id}/isolate</c>; <c>Machine.Isolate</c>).</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment and optional isolation type.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/isolate")]
	Task<MachineAction> IsolateAsync(string id, [Body] IsolateRequest request, CancellationToken cancellationToken);

	/// <summary>Releases a device from isolation (<c>POST machines/{id}/unisolate</c>; <c>Machine.Isolate</c>).</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/unisolate")]
	Task<MachineAction> UnisolateAsync(string id, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>Restricts a device to Microsoft-signed applications (<c>POST machines/{id}/restrictCodeExecution</c>; <c>Machine.RestrictExecution</c>).</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/restrictCodeExecution")]
	Task<MachineAction> RestrictCodeExecutionAsync(string id, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>Lifts the application restriction on a device (<c>POST machines/{id}/unrestrictCodeExecution</c>; <c>Machine.RestrictExecution</c>).</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/unrestrictCodeExecution")]
	Task<MachineAction> UnrestrictCodeExecutionAsync(string id, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>Starts a Microsoft Defender Antivirus scan on a device (<c>POST machines/{id}/runAntiVirusScan</c>; <c>Machine.Scan</c>).</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment and scan type.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/runAntiVirusScan")]
	Task<MachineAction> RunAntiVirusScanAsync(string id, [Body] RunAntiVirusScanRequest request, CancellationToken cancellationToken);

	/// <summary>Stops a file's processes on a device and quarantines it, found by SHA-1 (<c>POST machines/{id}/StopAndQuarantineFile</c>; <c>Machine.StopAndQuarantine</c>). Destructive.</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment and the file's SHA-1.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/StopAndQuarantineFile")]
	Task<MachineAction> StopAndQuarantineFileAsync(string id, [Body] StopAndQuarantineFileRequest request, CancellationToken cancellationToken);

	/// <summary>Collects an investigation package from a device (<c>POST machines/{id}/collectInvestigationPackage</c>; <c>Machine.CollectForensics</c>). Answers 400 while one is already running.</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/collectInvestigationPackage")]
	Task<MachineAction> CollectInvestigationPackageAsync(string id, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>Offboards a device from Defender for Endpoint (<c>POST machines/{id}/offboard</c>; <c>Machine.Offboard</c>). Destructive.</summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{id}/offboard")]
	Task<MachineAction> OffboardAsync(string id, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Runs a sequence of live response commands on a device (<c>POST machines/{machineId}/runliveresponse</c>;
	/// <c>Machine.LiveResponse</c>). Limited to 10 calls a minute and 50 concurrent sessions.
	/// </summary>
	/// <remarks>The documentation writes the path with an upper-case <c>API</c> prefix; the service matches paths without regard to case (unverified).</remarks>
	/// <param name="machineId">The device id.</param>
	/// <param name="request">The commands and optional comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created action.</returns>
	[Post("machines/{machineId}/runliveresponse")]
	Task<MachineAction> RunLiveResponseAsync(string machineId, [Body] RunLiveResponseRequest request, CancellationToken cancellationToken);

	/// <summary>Lists machine actions, one page at a time (<c>GET machineactions</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/>. <c>$filter</c> may name id, status, machineId, type, requestor and creationDateTimeUtc; <c>$top</c> is limited to 10,000.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of actions.</returns>
	[Get("machineactions")]
	Task<ODataPage<MachineAction>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one machine action (<c>GET machineactions/{id}</c>).</summary>
	/// <param name="id">The action id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The action.</returns>
	[Get("machineactions/{id}")]
	Task<MachineAction> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>Cancels an action that has not finished (<c>POST machineactions/{id}/cancel</c>). Answers with the updated action.</summary>
	/// <param name="id">The action id.</param>
	/// <param name="request">The optional comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated action.</returns>
	[Post("machineactions/{id}/cancel")]
	Task<MachineAction> CancelAsync(string id, [Body] CancelMachineActionRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the signed link to an investigation package once its collection has succeeded
	/// (<c>GET machineactions/{id}/getPackageUri</c>). Limited to 2 calls a minute; answers 404 until the package is ready.
	/// </summary>
	/// <param name="id">The id of the collect-package action.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The download link.</returns>
	[Get("machineactions/{id}/getPackageUri")]
	Task<DownloadLink> GetPackageUriAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the signed link to the result of one command of a live response action
	/// (<c>GET machineactions/{id}/GetLiveResponseResultDownloadLink(index={index})</c>); the link is valid for 30 minutes.
	/// </summary>
	/// <remarks>The index is written into the path as a plain integer. Unverified against a live service.</remarks>
	/// <param name="id">The live response action id.</param>
	/// <param name="index">The zero-based position of the command in the session.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The download link.</returns>
	[Get("machineactions/{id}/GetLiveResponseResultDownloadLink(index={index})")]
	Task<DownloadLink> GetLiveResponseResultDownloadLinkAsync(string id, int index, CancellationToken cancellationToken);
}
