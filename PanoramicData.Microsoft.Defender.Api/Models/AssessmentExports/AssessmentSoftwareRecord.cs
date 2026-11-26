using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>The device and product columns that the software inventory and software vulnerability export records share.</summary>
public class AssessmentSoftwareRecord : AssessmentDeviceRecord
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
}
