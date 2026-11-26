using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models.AdvancedHunting;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Advanced hunting (<c>advancedqueries</c>). Microsoft is retiring this API in favour of Microsoft Graph.</summary>
	public IAdvancedHuntingApi AdvancedHunting => field ??= For<IAdvancedHuntingApi>();

	/// <summary>Runs a KQL hunting query (<c>POST advancedqueries/run</c>).</summary>
	/// <param name="query">The KQL query text.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The result schema and rows.</returns>
	[Obsolete("The Defender for Endpoint advanced hunting API is being retired by Microsoft. Use the Microsoft Graph security runHuntingQuery action instead.")]
	public Task<AdvancedHuntingResult> RunAdvancedHuntingQueryAsync(string query, CancellationToken cancellationToken)
#pragma warning disable CS0618 // The helper is itself obsolete for the same reason as the interface method it calls.
		=> AdvancedHunting.RunAsync(new AdvancedHuntingRequest { Query = query }, cancellationToken);
#pragma warning restore CS0618
}
