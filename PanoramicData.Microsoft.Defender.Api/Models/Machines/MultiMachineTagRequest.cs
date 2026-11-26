using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>The body that adds or removes one tag on many devices at once (up to 500). Property names are case-sensitive on the server.</summary>
public class MultiMachineTagRequest
{
	/// <summary>The tag name.</summary>
	[JsonPropertyName("Value")]
	public required string Value { get; set; }

	/// <summary>Whether to add or remove the tag.</summary>
	[JsonPropertyName("Action")]
	public required MachineTagAction Action { get; set; }

	/// <summary>The ids of the devices to change.</summary>
	[JsonPropertyName("MachineIds")]
	public required IReadOnlyList<string> MachineIds { get; set; }
}
