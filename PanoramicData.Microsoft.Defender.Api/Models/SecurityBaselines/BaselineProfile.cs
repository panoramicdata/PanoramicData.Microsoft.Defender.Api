using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.SecurityBaselines;

/// <summary>A security baseline assessment profile created by the organisation.</summary>
public class BaselineProfile
{
	/// <summary>The profile's identifier.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The profile's name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The profile's description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The benchmark the profile is based on.</summary>
	[JsonPropertyName("benchmark")]
	public string? Benchmark { get; set; }

	/// <summary>The profile's version.</summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }

	/// <summary>The operating system it applies to.</summary>
	[JsonPropertyName("operatingSystem")]
	public string? OperatingSystem { get; set; }

	/// <summary>The operating system version it applies to.</summary>
	[JsonPropertyName("operatingSystemVersion")]
	public string? OperatingSystemVersion { get; set; }

	/// <summary>Whether the profile is active.</summary>
	[JsonPropertyName("status")]
	public bool? Status { get; set; }

	/// <summary>The compliance level chosen.</summary>
	[JsonPropertyName("complianceLevel")]
	public string? ComplianceLevel { get; set; }

	/// <summary>How many configurations the profile selects.</summary>
	[JsonPropertyName("settingsNumber")]
	public long? SettingsNumber { get; set; }

	/// <summary>Who created the profile.</summary>
	[JsonPropertyName("createdBy")]
	public string? CreatedBy { get; set; }

	/// <summary>Who last changed it.</summary>
	[JsonPropertyName("lastUpdatedBy")]
	public string? LastUpdatedBy { get; set; }

	/// <summary>When it was created, as spelled in the response example.</summary>
	[JsonPropertyName("createdOnTimestampUTC")]
	public DateTimeOffset? CreatedOnTimestampUtc { get; set; }

	/// <summary>When it was last changed, as spelled in the response example.</summary>
	[JsonPropertyName("lastUpdateTimestampUTC")]
	public DateTimeOffset? LastUpdateTimestampUtc { get; set; }

	/// <summary>When it was created, as spelled in the property table.</summary>
	[JsonPropertyName("createdOnTimeOffset")]
	public DateTimeOffset? CreatedOnTimeOffset { get; set; }

	/// <summary>When it was last changed, as spelled in the property table.</summary>
	[JsonPropertyName("lastUpdateTimeOffset")]
	public DateTimeOffset? LastUpdateTimeOffset { get; set; }

	/// <summary>How many applicable devices comply with every configuration.</summary>
	[JsonPropertyName("passedDevices")]
	public long? PassedDevices { get; set; }

	/// <summary>How many devices the profile applies to.</summary>
	[JsonPropertyName("totalDevices")]
	public long? TotalDevices { get; set; }
}
