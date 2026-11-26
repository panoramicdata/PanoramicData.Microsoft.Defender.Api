using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Scores;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// The tenant exposure score and its secure score for devices. Needs the <c>Score.Read.All</c> application permission. The
/// documentation marks these pages as describing a prerelease surface that may change.
/// </summary>
public interface IScoresApi
{
	/// <summary>Gets the organisation-wide exposure score (<c>GET exposureScore</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The score and when it was calculated.</returns>
	[Get("exposureScore")]
	Task<DeviceScore> GetExposureScoreAsync(CancellationToken cancellationToken);

	/// <summary>Lists the exposure score of each device group (<c>GET exposureScore/ByMachineGroups</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page with one score per device group.</returns>
	[Get("exposureScore/ByMachineGroups")]
	Task<ODataPage<DeviceScore>> ListExposureScoresByMachineGroupsAsync(CancellationToken cancellationToken);

	/// <summary>Gets the organisation-wide Microsoft Secure Score for Devices (<c>GET configurationScore</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The score and when it was calculated.</returns>
	[Get("configurationScore")]
	Task<DeviceScore> GetConfigurationScoreAsync(CancellationToken cancellationToken);
}
