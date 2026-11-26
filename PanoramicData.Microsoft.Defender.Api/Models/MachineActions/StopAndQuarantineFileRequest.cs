using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The body that stops a file's processes and quarantines it. Property names are case-sensitive on the server.</summary>
public class StopAndQuarantineFileRequest
{
	/// <summary>Why the file is quarantined.</summary>
	[JsonPropertyName("Comment")]
	public required string Comment { get; set; }

	/// <summary>The file's SHA-1 hash.</summary>
	[JsonPropertyName("Sha1")]
	public required string Sha1 { get; set; }
}
