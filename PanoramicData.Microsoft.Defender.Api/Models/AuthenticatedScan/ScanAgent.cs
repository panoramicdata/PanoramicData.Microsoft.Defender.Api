using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>A device that runs authenticated network scans (a scan agent, or the agent embedded in a scan definition).</summary>
public class ScanAgent
{
	/// <summary>The agent id, composed of the organisation and the device id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The id of the device hosting the agent.</summary>
	[JsonPropertyName("machineId")]
	public string? MachineId { get; set; }

	/// <summary>The device's name, when returned inside a scan definition.</summary>
	[JsonPropertyName("machineName")]
	public string? MachineName { get; set; }

	/// <summary>The device's DNS name, when returned by the agent list.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>When the agent was last seen.</summary>
	[JsonPropertyName("lastSeen")]
	public DateTimeOffset? LastSeen { get; set; }

	/// <summary>The id of the application assigned to the agent.</summary>
	[JsonPropertyName("assignedApplicationId")]
	public string? AssignedApplicationId { get; set; }

	/// <summary>The scanner software version.</summary>
	[JsonPropertyName("scannerSoftwareVersion")]
	public string? ScannerSoftwareVersion { get; set; }

	/// <summary>When the agent last executed a command.</summary>
	[JsonPropertyName("lastCommandExecutionTimestamp")]
	public DateTimeOffset? LastCommandExecutionTimestamp { get; set; }

	/// <summary>The Defender client version on the device.</summary>
	[JsonPropertyName("mdeClientVersion")]
	public string? MdeClientVersion { get; set; }
}
