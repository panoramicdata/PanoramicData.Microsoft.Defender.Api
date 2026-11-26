using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>Names the device that should run a scan, inside a create or update request.</summary>
public class ScanAgentReference
{
	/// <summary>The id of the device that hosts the scan agent.</summary>
	[JsonPropertyName("machineId")]
	public required string MachineId { get; set; }
}
