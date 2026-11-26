using PanoramicData.MicrosoftDefender.Api.Models.AdvancedHunting;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Runs KQL advanced hunting queries (<c>advancedqueries</c>). Microsoft is retiring this API (retirement began in
/// January 2026) in favour of the Microsoft Graph <c>runHuntingQuery</c> action, so it may stop working.
/// </summary>
public interface IAdvancedHuntingApi
{
	/// <summary>
	/// Runs a hunting query (<c>POST advancedqueries/run</c>). It only reads, so a read-only client allows it. Needs the
	/// <c>AdvancedQuery.Read.All</c> application permission; limited to 45 calls a minute, 30 days of data, 100,000 rows,
	/// 50 MB and 200 seconds per request. Not available in Defender for Business.
	/// </summary>
	/// <param name="request">The query to run.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The result schema and rows.</returns>
	[Obsolete("The Defender for Endpoint advanced hunting API is being retired by Microsoft. Use the Microsoft Graph security runHuntingQuery action instead.")]
	[Post("advancedqueries/run")]
	Task<AdvancedHuntingResult> RunAsync([Body] AdvancedHuntingRequest request, CancellationToken cancellationToken);
}
