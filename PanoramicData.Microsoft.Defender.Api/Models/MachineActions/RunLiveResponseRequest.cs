using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The body that starts a live response session. Property names are case-sensitive on the server.</summary>
public class RunLiveResponseRequest
{
	/// <summary>The commands to run, in order.</summary>
	[JsonPropertyName("Commands")]
	public required IReadOnlyList<LiveResponseCommand> Commands { get; set; }

	/// <summary>Why the session is run; omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("Comment")]
	public string? Comment { get; set; }
}
