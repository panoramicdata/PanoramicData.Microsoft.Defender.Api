using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>The body that adds or removes one tag on one device. Property names are case-sensitive on the server.</summary>
public class MachineTagRequest
{
	/// <summary>The tag name.</summary>
	[JsonPropertyName("Value")]
	public required string Value { get; set; }

	/// <summary>Whether to add or remove the tag.</summary>
	[JsonPropertyName("Action")]
	public required MachineTagAction Action { get; set; }
}
