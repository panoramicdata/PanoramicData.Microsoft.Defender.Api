using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.SecurityBaselines;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Security baseline configurations and profiles (<c>baselineConfigurations</c>, <c>baselineProfiles</c>).</summary>
	public ISecurityBaselinesApi SecurityBaselines => field ??= For<ISecurityBaselinesApi>();

	/// <summary>Enumerates every baseline configuration, following the page links (<c>GET baselineConfigurations</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every configuration.</returns>
	public IAsyncEnumerable<BaselineConfiguration> GetAllBaselineConfigurationsAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => SecurityBaselines.ListConfigurationsAsync(options, token), cancellationToken);

	/// <summary>Enumerates every baseline profile, following the page links (<c>GET baselineProfiles</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every profile.</returns>
	public IAsyncEnumerable<BaselineProfile> GetAllBaselineProfilesAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => SecurityBaselines.ListProfilesAsync(options, token), cancellationToken);
}
