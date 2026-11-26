using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Investigations;

/// <summary>The body for starting an automated investigation on a device.</summary>
public class StartInvestigationRequest
{
	/// <summary>Why the investigation is being started (the property is named <c>Comment</c> on the wire, capital C).</summary>
	[JsonPropertyName("Comment")]
	public required string Comment { get; set; }
}
