using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AdvancedHunting;

/// <summary>The body of an advanced hunting query: the KQL text to run.</summary>
public class AdvancedHuntingRequest
{
	/// <summary>The KQL query. The service reads only data from the last 30 days.</summary>
	[JsonPropertyName("Query")]
	public required string Query { get; set; }
}
