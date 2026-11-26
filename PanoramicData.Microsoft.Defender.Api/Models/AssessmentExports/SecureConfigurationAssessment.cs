using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One secure-configuration check on one device (a row per device and configuration).</summary>
public class SecureConfigurationAssessment : AssessmentDeviceRecord
{
	/// <summary>The configuration's identifier, such as scid-10000.</summary>
	[JsonPropertyName("configurationId")]
	public string? ConfigurationId { get; set; }

	/// <summary>The configuration's display name.</summary>
	[JsonPropertyName("configurationName")]
	public string? ConfigurationName { get; set; }

	/// <summary>The category: Application, OS, Network, Accounts or Security controls.</summary>
	[JsonPropertyName("configurationCategory")]
	public string? ConfigurationCategory { get; set; }

	/// <summary>The finer grouping within the category.</summary>
	[JsonPropertyName("configurationSubcategory")]
	public string? ConfigurationSubcategory { get; set; }

	/// <summary>The configuration's weight in the secure score, on a 1 to 10 scale.</summary>
	[JsonPropertyName("configurationImpact")]
	public double? ConfigurationImpact { get; set; }

	/// <summary>Whether the configuration applies to the device.</summary>
	[JsonPropertyName("isApplicable")]
	public bool? IsApplicable { get; set; }

	/// <summary>Whether the device meets the configuration.</summary>
	[JsonPropertyName("isCompliant")]
	public bool? IsCompliant { get; set; }

	/// <summary>Whether applying the configuration is expected to affect users.</summary>
	[JsonPropertyName("isExpectedUserImpact")]
	public bool? IsExpectedUserImpact { get; set; }

	/// <summary>The operating system platform.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }

	/// <summary>The operating system version.</summary>
	[JsonPropertyName("osVersion")]
	public string? OsVersion { get; set; }

	/// <summary>The id of the related security recommendation.</summary>
	[JsonPropertyName("recommendationReference")]
	public string? RecommendationReference { get; set; }

	/// <summary>When the configuration was last seen on the device.</summary>
	[JsonPropertyName("timestamp")]
	public DateTimeOffset? Timestamp { get; set; }
}
