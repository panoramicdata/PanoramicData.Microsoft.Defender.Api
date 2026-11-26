using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The body of a scan history query by session.</summary>
/// <remarks>
/// The documentation types this member as a string, but its example sends an array, which is followed (unverified).
/// </remarks>
public class ScanHistoryBySessionRequest
{
	/// <summary>The scan session ids whose history is wanted.</summary>
	[JsonPropertyName("SessionIds")]
	public required IReadOnlyList<string> SessionIds { get; set; }
}
