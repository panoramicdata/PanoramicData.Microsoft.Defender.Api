using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The body of the machine actions that take only a comment. The property name is case-sensitive on the server.</summary>
public class CommentRequest
{
	/// <summary>Why the action is requested; recorded with the action.</summary>
	[JsonPropertyName("Comment")]
	public required string Comment { get; set; }
}
