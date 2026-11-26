using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>File lookups by hash (<c>files</c>).</summary>
	public IFilesApi Files => field ??= For<IFilesApi>();

	/// <summary>Enumerates every alert involving a file, following the page links (<c>GET files/{id}/alerts</c>).</summary>
	/// <param name="id">The file's SHA-1 hash.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every alert.</returns>
	public IAsyncEnumerable<Alert> GetAllFileAlertsAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Files.ListAlertsAsync(id, token), cancellationToken);

	/// <summary>Enumerates every device that saw a file, following the page links (<c>GET files/{id}/machines</c>).</summary>
	/// <param name="id">The file's SHA-1 hash.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device.</returns>
	public IAsyncEnumerable<Machine> GetAllFileMachinesAsync(string id, CancellationToken cancellationToken)
		=> GetAllAsync(token => Files.ListMachinesAsync(id, token), cancellationToken);
}
