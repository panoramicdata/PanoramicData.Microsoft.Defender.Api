using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One security-baseline configuration assessed on one device (a row per device, profile and configuration).</summary>
public class BaselineComplianceAssessment : AssessmentDeviceRecord
{
	/// <summary>The record's identifier, built from the device, profile and configuration ids.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The assessed baseline profile.</summary>
	[JsonPropertyName("profileId")]
	public string? ProfileId { get; set; }

	/// <summary>The assessed configuration within the benchmark.</summary>
	[JsonPropertyName("configurationId")]
	public string? ConfigurationId { get; set; }

	/// <summary>Whether the configuration applies to the device.</summary>
	[JsonPropertyName("isApplicable")]
	public bool? IsApplicable { get; set; }

	/// <summary>Whether the device complies.</summary>
	[JsonPropertyName("isCompliant")]
	public bool? IsCompliant { get; set; }

	/// <summary>The operating system platform.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }

	/// <summary>The operating system version.</summary>
	[JsonPropertyName("osVersion")]
	public string? OsVersion { get; set; }

	/// <summary>When the data was collected from the device; absent if nothing was collected.</summary>
	[JsonPropertyName("dataCollectionTimeOffset")]
	public DateTimeOffset? DataCollectionTimeOffset { get; set; }

	/// <summary>When compliance was calculated.</summary>
	[JsonPropertyName("complianceCalculationTimeOffset")]
	public DateTimeOffset? ComplianceCalculationTimeOffset { get; set; }

	/// <summary>The values that would make the setting compliant.</summary>
	[JsonPropertyName("recommendedValue")]
	public IReadOnlyList<string> RecommendedValue { get; set; } = [];

	/// <summary>The values found on the device.</summary>
	[JsonPropertyName("currentValue")]
	public IReadOnlyList<string> CurrentValue { get; set; } = [];

	/// <summary>The registry paths or other places the setting was read from.</summary>
	[JsonPropertyName("source")]
	public IReadOnlyList<string> Source { get; set; } = [];
}
