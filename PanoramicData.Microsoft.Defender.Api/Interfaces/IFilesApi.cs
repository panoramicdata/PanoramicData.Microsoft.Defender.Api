using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Files;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Looks up a file by its hash and finds the alerts, devices and prevalence statistics connected to it (<c>files</c>).
/// The profile and statistics need the <c>File.Read.All</c> application permission, the alert list
/// <c>Alert.ReadWrite.All</c> and the device list <c>Machine.ReadWrite.All</c>. Each path segment is escaped as one
/// segment.
/// </summary>
public interface IFilesApi
{
	/// <summary>Gets a file's profile by hash (<c>GET files/{id}</c>); answers 404 when the file is unknown.</summary>
	/// <param name="id">The file's SHA-1 or SHA-256 hash.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The file profile.</returns>
	[Get("files/{id}")]
	Task<FileProfile> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the alerts that involve a file (<c>GET files/{id}/alerts</c>). Only a SHA-1 hash is accepted here, and an
	/// unknown file answers 200 with an empty list.
	/// </summary>
	/// <param name="id">The file's SHA-1 hash.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of alerts.</returns>
	[Get("files/{id}/alerts")]
	Task<ODataPage<Alert>> ListAlertsAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the devices on which a file was seen (<c>GET files/{id}/machines</c>). Only a SHA-1 hash is accepted.</summary>
	/// <param name="id">The file's SHA-1 hash.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of devices.</returns>
	[Get("files/{id}/machines")]
	Task<ODataPage<Machine>> ListMachinesAsync(string id, CancellationToken cancellationToken);

	/// <summary>Gets how widely a file was seen (<c>GET files/{id}/stats</c>); answers 404 when the file is unknown.</summary>
	/// <param name="id">The file's SHA-1 or SHA-256 hash.</param>
	/// <param name="options">The look-back window, or <see langword="null"/> for the service default of 30 days (at most 720 hours).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The file statistics.</returns>
	[Get("files/{id}/stats")]
	Task<FileStatistics> GetStatisticsAsync(string id, [Query] EntityStatisticsOptions? options, CancellationToken cancellationToken);
}
