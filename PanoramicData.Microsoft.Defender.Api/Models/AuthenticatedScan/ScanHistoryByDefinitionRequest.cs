using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The body of a scan history query by definition.</summary>
/// <remarks>
/// The documentation types this member as a string, but its example sends an array, which is followed (unverified).
/// </remarks>
public class ScanHistoryByDefinitionRequest
{
	/// <summary>The definition ids whose history is wanted.</summary>
	[JsonPropertyName("ScanDefinitionIds")]
	public required IReadOnlyList<string> ScanDefinitionIds { get; set; }
}
