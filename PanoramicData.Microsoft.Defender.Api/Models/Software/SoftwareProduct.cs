using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Software;

/// <summary>A software product known to the vulnerability-management inventory.</summary>
public class SoftwareProduct
{
	/// <summary>The product id, vendor and name joined by -_-, such as microsoft-_-edge.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The product name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The publisher.</summary>
	[JsonPropertyName("vendor")]
	public string? Vendor { get; set; }

	/// <summary>How many vulnerabilities were found in the product.</summary>
	[JsonPropertyName("weaknesses")]
	public long? Weaknesses { get; set; }

	/// <summary>Whether a public exploit exists for any of those vulnerabilities.</summary>
	[JsonPropertyName("publicExploit")]
	public bool? PublicExploit { get; set; }

	/// <summary>Whether an active alert relates to the product.</summary>
	[JsonPropertyName("activeAlert")]
	public bool? ActiveAlert { get; set; }

	/// <summary>How many devices are exposed through the product.</summary>
	[JsonPropertyName("exposedMachines")]
	public long? ExposedMachines { get; set; }

	/// <summary>The product's effect on the exposure score.</summary>
	[JsonPropertyName("impactScore")]
	public double? ImpactScore { get; set; }
}
