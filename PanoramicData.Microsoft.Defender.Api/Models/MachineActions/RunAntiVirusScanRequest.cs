using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The body that starts an antivirus scan. Property names are case-sensitive on the server.</summary>
public class RunAntiVirusScanRequest
{
	/// <summary>Why the scan is requested.</summary>
	[JsonPropertyName("Comment")]
	public required string Comment { get; set; }

	/// <summary>Quick or full.</summary>
	[JsonPropertyName("ScanType")]
	public required AntiVirusScanType ScanType { get; set; }
}
