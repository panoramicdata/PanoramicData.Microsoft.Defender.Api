using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Recommendations;

/// <summary>
/// A security recommendation. This is a minimal stand-in written by the machines group so that
/// <c>machines/{id}/recommendations</c> has a result type; the recommendations group owns this type and may replace it.
/// </summary>
public class Recommendation
{
	/// <summary>The recommendation id, such as <c>va-_-google-_-chrome</c>.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The product the recommendation concerns.</summary>
	[JsonPropertyName("productName")]
	public string? ProductName { get; set; }

	/// <summary>The recommendation's name.</summary>
	[JsonPropertyName("recommendationName")]
	public string? RecommendationName { get; set; }

	/// <summary>How many weaknesses it addresses.</summary>
	[JsonPropertyName("weaknesses")]
	public long? Weaknesses { get; set; }

	/// <summary>The vendor.</summary>
	[JsonPropertyName("vendor")]
	public string? Vendor { get; set; }

	/// <summary>The recommended version.</summary>
	[JsonPropertyName("recommendedVersion")]
	public string? RecommendedVersion { get; set; }

	/// <summary>The recommendation category.</summary>
	[JsonPropertyName("recommendationCategory")]
	public string? RecommendationCategory { get; set; }

	/// <summary>The severity score.</summary>
	[JsonPropertyName("severityScore")]
	public double? SeverityScore { get; set; }

	/// <summary>Whether a public exploit exists.</summary>
	[JsonPropertyName("publicExploit")]
	public bool? PublicExploit { get; set; }

	/// <summary>Whether an alert is active.</summary>
	[JsonPropertyName("activeAlert")]
	public bool? ActiveAlert { get; set; }

	/// <summary>The remediation type.</summary>
	[JsonPropertyName("remediationType")]
	public string? RemediationType { get; set; }

	/// <summary>The recommendation's status.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The effect on the configuration score.</summary>
	[JsonPropertyName("configScoreImpact")]
	public double? ConfigScoreImpact { get; set; }

	/// <summary>The effect on the exposure score.</summary>
	[JsonPropertyName("exposureImpact")]
	public double? ExposureImpact { get; set; }

	/// <summary>How many devices it applies to.</summary>
	[JsonPropertyName("totalMachineCount")]
	public long? TotalMachineCount { get; set; }

	/// <summary>How many devices are exposed.</summary>
	[JsonPropertyName("exposedMachinesCount")]
	public long? ExposedMachinesCount { get; set; }
}
