using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>A comment an analyst added to an alert.</summary>
public class AlertComment
{
	/// <summary>The comment text.</summary>
	[JsonPropertyName("comment")]
	public string? Comment { get; set; }

	/// <summary>Who wrote the comment.</summary>
	[JsonPropertyName("createdBy")]
	public string? CreatedBy { get; set; }

	/// <summary>When the comment was written.</summary>
	[JsonPropertyName("createdTime")]
	public DateTimeOffset? CreatedTime { get; set; }
}
