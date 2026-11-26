using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The outcome of the most recent run of a scan definition.</summary>
public class LatestScan
{
	/// <summary>The outcome label, for example <c>Fail</c>; <see langword="null"/> before the first run. An open set, so kept as text.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>Why the run failed, when it did.</summary>
	[JsonPropertyName("failureReason")]
	public string? FailureReason { get; set; }

	/// <summary>When the run executed.</summary>
	[JsonPropertyName("executionDateTime")]
	public DateTimeOffset? ExecutionDateTime { get; set; }
}
