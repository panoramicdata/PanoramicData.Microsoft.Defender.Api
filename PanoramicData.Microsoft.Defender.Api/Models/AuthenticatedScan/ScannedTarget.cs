using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The result of scanning one target within a scan session.</summary>
public class ScannedTarget
{
	/// <summary>The target's IP address.</summary>
	[JsonPropertyName("Ip")]
	public string? Ip { get; set; }

	/// <summary>The target's host name.</summary>
	[JsonPropertyName("Hostname")]
	public string? Hostname { get; set; }

	/// <summary>The description the scan found for the device.</summary>
	[JsonPropertyName("ScannedDeviceDescription")]
	public string? ScannedDeviceDescription { get; set; }

	/// <summary>The error message; empty on success.</summary>
	[JsonPropertyName("ErrorMessage")]
	public string? ErrorMessage { get; set; }

	/// <summary>The outcome label. An open set, so kept as text.</summary>
	[JsonPropertyName("ScanStatus")]
	public string? ScanStatus { get; set; }

	/// <summary>How long the scan of this target took, as the service formats it (for example <c>00:08:30</c>).</summary>
	[JsonPropertyName("ScanDuration")]
	public string? ScanDuration { get; set; }
}
