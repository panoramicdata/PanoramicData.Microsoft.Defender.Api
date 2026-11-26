using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Recommendations;

/// <summary>A security recommendation affecting the tenant.</summary>
public class Recommendation
{
	/// <summary>The recommendation id, such as va-_-google-_-chrome.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The related software's name.</summary>
	[JsonPropertyName("productName")]
	public string? ProductName { get; set; }

	/// <summary>The recommendation's name.</summary>
	[JsonPropertyName("recommendationName")]
	public string? RecommendationName { get; set; }

	/// <summary>How many vulnerabilities were found.</summary>
	[JsonPropertyName("weaknesses")]
	public long? Weaknesses { get; set; }

	/// <summary>The related vendor.</summary>
	[JsonPropertyName("vendor")]
	public string? Vendor { get; set; }

	/// <summary>The version to move to.</summary>
	[JsonPropertyName("recommendedVersion")]
	public string? RecommendedVersion { get; set; }

	/// <summary>The program to move to.</summary>
	[JsonPropertyName("recommendedProgram")]
	public string? RecommendedProgram { get; set; }

	/// <summary>The vendor of the program to move to.</summary>
	[JsonPropertyName("recommendedVendor")]
	public string? RecommendedVendor { get; set; }

	/// <summary>The category: Accounts, Application, Network, OS or SecurityControls.</summary>
	[JsonPropertyName("recommendationCategory")]
	public string? RecommendationCategory { get; set; }

	/// <summary>The finer category.</summary>
	[JsonPropertyName("subCategory")]
	public string? SubCategory { get; set; }

	/// <summary>The potential effect on the secure score, 1 to 10.</summary>
	[JsonPropertyName("severityScore")]
	public double? SeverityScore { get; set; }

	/// <summary>Whether a public exploit exists.</summary>
	[JsonPropertyName("publicExploit")]
	public bool? PublicExploit { get; set; }

	/// <summary>Whether an active alert relates to it.</summary>
	[JsonPropertyName("activeAlert")]
	public bool? ActiveAlert { get; set; }

	/// <summary>Threat analytics reports tied to the recommendation.</summary>
	[JsonPropertyName("associatedThreats")]
	public IReadOnlyList<string> AssociatedThreats { get; set; } = [];

	/// <summary>The kind of fix: ConfigurationChange, Update, Upgrade or Uninstall.</summary>
	[JsonPropertyName("remediationType")]
	public string? RemediationType { get; set; }

	/// <summary>The exception state: Active or Exception.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The effect on the secure score for devices.</summary>
	[JsonPropertyName("configScoreImpact")]
	public double? ConfigScoreImpact { get; set; }

	/// <summary>The effect on the exposure score.</summary>
	[JsonPropertyName("exposureImpact")]
	public double? ExposureImpact { get; set; }

	/// <summary>How many devices have the product installed.</summary>
	[JsonPropertyName("totalMachineCount")]
	public long? TotalMachineCount { get; set; }

	/// <summary>How many of them are exposed to vulnerabilities.</summary>
	[JsonPropertyName("exposedMachinesCount")]
	public long? ExposedMachinesCount { get; set; }

	/// <summary>How many devices are not affected in their productivity.</summary>
	[JsonPropertyName("nonProductivityImpactedAssets")]
	public long? NonProductivityImpactedAssets { get; set; }

	/// <summary>The related software component.</summary>
	[JsonPropertyName("relatedComponent")]
	public string? RelatedComponent { get; set; }

	/// <summary>Whether an unpatchable CVE is involved. Seen in a response example only.</summary>
	[JsonPropertyName("hasUnpatchableCve")]
	public bool? HasUnpatchableCve { get; set; }

	/// <summary>Tags on the recommendation, such as internetFacing. Seen in response examples only.</summary>
	[JsonPropertyName("tags")]
	public IReadOnlyList<string> Tags { get; set; } = [];

	/// <summary>The count of exposed devices that are critical at any level.</summary>
	[JsonPropertyName("exposedCriticalDevices")]
	public long? ExposedCriticalDevices { get; set; }
}
