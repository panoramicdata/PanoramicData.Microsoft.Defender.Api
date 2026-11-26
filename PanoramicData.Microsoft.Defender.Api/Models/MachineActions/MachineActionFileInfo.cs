using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The file a machine action concerns.</summary>
public class MachineActionFileInfo
{
	/// <summary>The file's hash.</summary>
	[JsonPropertyName("fileIdentifier")]
	public string? FileIdentifier { get; set; }

	/// <summary>The kind of hash, such as <c>Sha1</c>.</summary>
	[JsonPropertyName("fileIdentifierType")]
	public string? FileIdentifierType { get; set; }
}
