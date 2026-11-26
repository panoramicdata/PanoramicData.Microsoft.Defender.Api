using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The body that cancels a machine action. The property name is case-sensitive on the server.</summary>
public class CancelMachineActionRequest
{
	/// <summary>Why the action is cancelled; omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("Comment")]
	public string? Comment { get; set; }
}
