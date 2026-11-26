using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Software;

/// <summary>How many installations of one version of a product exist.</summary>
public class SoftwareVersionDistribution
{
	/// <summary>The product version.</summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }

	/// <summary>How many devices have this version installed.</summary>
	[JsonPropertyName("installations")]
	public long? Installations { get; set; }

	/// <summary>How many vulnerabilities affect this version.</summary>
	[JsonPropertyName("vulnerabilities")]
	public long? Vulnerabilities { get; set; }
}
