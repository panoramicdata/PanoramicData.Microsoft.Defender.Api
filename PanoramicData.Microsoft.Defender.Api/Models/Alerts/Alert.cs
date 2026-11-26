using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>
/// A Defender alert. This is a minimal stand-in written by the files, domains, IPs and users group so that its list
/// operations compile; the alerts group owns this type and replaces it with the full entity.
/// </summary>
public class Alert
{
	/// <summary>The alert id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The alert title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The severity label.</summary>
	[JsonPropertyName("severity")]
	public string? Severity { get; set; }

	/// <summary>The id of the device the alert concerns.</summary>
	[JsonPropertyName("machineId")]
	public string? MachineId { get; set; }
}
