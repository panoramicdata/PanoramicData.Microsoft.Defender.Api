using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>The outcome of one indicator in an import batch.</summary>
public class IndicatorImportResult
{
	/// <summary>The id given to the indicator when it was stored.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The indicator value this result is for.</summary>
	[JsonPropertyName("indicator")]
	public string? Indicator { get; set; }

	/// <summary>Whether this indicator was rejected.</summary>
	[JsonPropertyName("isFailed")]
	public bool? IsFailed { get; set; }

	/// <summary>Why it was rejected, when it was.</summary>
	[JsonPropertyName("failureReason")]
	public string? FailureReason { get; set; }
}
