using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>
/// The answer to a single scan agent lookup. The documentation's example answers with the agent wrapped in a one-item
/// <c>value</c> collection although its context URL says entity, so this type reads both shapes: the agent's own members are
/// filled when the service answers a bare entity, and <see cref="Value"/> holds the agent when it answers a collection
/// (unverified which one the service sends).
/// </summary>
public class ScanAgentDetails : ScanAgent
{
	/// <summary>The agents in the response when it is wrapped in a collection; empty for a bare entity.</summary>
	[JsonPropertyName("value")]
	public IReadOnlyList<ScanAgent> Value { get; set; } = [];
}
