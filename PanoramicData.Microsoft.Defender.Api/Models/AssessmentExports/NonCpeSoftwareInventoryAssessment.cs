using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One installed product that has no CPE product code, on one device.</summary>
public class NonCpeSoftwareInventoryAssessment : AssessmentDeviceRecord
{
	/// <summary>The software vendor.</summary>
	[JsonPropertyName("softwareVendor")]
	public string? SoftwareVendor { get; set; }

	/// <summary>The product name.</summary>
	[JsonPropertyName("softwareName")]
	public string? SoftwareName { get; set; }

	/// <summary>The product version.</summary>
	[JsonPropertyName("softwareVersion")]
	public string? SoftwareVersion { get; set; }

	/// <summary>The operating system platform.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }

	/// <summary>When the product was last seen on the device.</summary>
	[JsonPropertyName("softwareLastSeenTimestamp")]
	public DateTimeOffset? SoftwareLastSeenTimestamp { get; set; }
}
