using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The body that deletes scan definitions in one call.</summary>
public class DeleteScanDefinitionsRequest
{
	/// <summary>The ids of the definitions to delete.</summary>
	[JsonPropertyName("ScanDefinitionIds")]
	public required IReadOnlyList<string> ScanDefinitionIds { get; set; }
}
