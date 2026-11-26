using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Authenticated network scans (<c>DeviceAuthenticatedScanDefinitions</c>, <c>DeviceAuthenticatedScanAgents</c>).</summary>
	public IAuthenticatedScanApi AuthenticatedScan => field ??= For<IAuthenticatedScanApi>();

	/// <summary>Enumerates every scan definition, following the page links (<c>GET DeviceAuthenticatedScanDefinitions</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every scan definition.</returns>
	public IAsyncEnumerable<ScanDefinition> GetAllScanDefinitionsAsync(CancellationToken cancellationToken)
		=> GetAllAsync(AuthenticatedScan.ListDefinitionsAsync, cancellationToken);

	/// <summary>Enumerates every scan agent, following the page links (<c>GET DeviceAuthenticatedScanAgents</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every scan agent.</returns>
	public IAsyncEnumerable<ScanAgent> GetAllScanAgentsAsync(CancellationToken cancellationToken)
		=> GetAllAsync(AuthenticatedScan.ListAgentsAsync, cancellationToken);
}
