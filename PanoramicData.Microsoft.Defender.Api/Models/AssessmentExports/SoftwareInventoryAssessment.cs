using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One software product installed on one device (a row per device, vendor, name and version).</summary>
public class SoftwareInventoryAssessment : AssessmentDeviceRecord
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

	/// <summary>Disk locations that show the product is installed.</summary>
	[JsonPropertyName("diskPaths")]
	public IReadOnlyList<string> DiskPaths { get; set; } = [];

	/// <summary>Registry locations that show the product is installed.</summary>
	[JsonPropertyName("registryPaths")]
	public IReadOnlyList<string> RegistryPaths { get; set; } = [];

	/// <summary>How many weaknesses the product has on this device.</summary>
	[JsonPropertyName("numberOfWeaknesses")]
	public long? NumberOfWeaknesses { get; set; }

	/// <summary>The end-of-support state, such as None, EOS Version, Upcoming EOS Version, EOS Software or Upcoming EOS Software.</summary>
	[JsonPropertyName("endOfSupportStatus")]
	public string? EndOfSupportStatus { get; set; }

	/// <summary>When support for the product ended or will end.</summary>
	[JsonPropertyName("endOfSupportDate")]
	public DateTimeOffset? EndOfSupportDate { get; set; }

	/// <summary>When the product was first seen on the device.</summary>
	[JsonPropertyName("softwareFirstSeenTimestamp")]
	public DateTimeOffset? SoftwareFirstSeenTimestamp { get; set; }
}
