using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The latest outcome of one scan definition, from a history query by definition.</summary>
public class ScanHistoryByDefinition
{
	/// <summary>The definition's id (the service names this member in the plural but sends one id).</summary>
	[JsonPropertyName("ScanDefinitionIds")]
	public string? ScanDefinitionIds { get; set; }

	/// <summary>When the definition was last scanned.</summary>
	[JsonPropertyName("LastScanned")]
	public DateTimeOffset? LastScanned { get; set; }

	/// <summary>The outcome label, for example <c>Partial Success</c>. An open set, so kept as text.</summary>
	[JsonPropertyName("ScanStatus")]
	public string? ScanStatus { get; set; }

	/// <summary>The id of the scanner that ran it.</summary>
	[JsonPropertyName("ScannerId")]
	public string? ScannerId { get; set; }
}
