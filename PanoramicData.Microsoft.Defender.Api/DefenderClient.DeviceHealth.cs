using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.DeviceHealth;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Microsoft Defender Antivirus device health (<c>deviceavinfo</c>).</summary>
	public IDeviceHealthApi DeviceHealth => field ??= For<IDeviceHealthApi>();

	/// <summary>Enumerates the antivirus health of every device, following the page links (<c>GET deviceavinfo</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every device health record.</returns>
	public IAsyncEnumerable<DeviceAntivirusHealth> GetAllAntivirusHealthAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => DeviceHealth.ListAntivirusHealthAsync(options, token), cancellationToken);
}
