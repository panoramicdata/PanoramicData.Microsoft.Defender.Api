using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Software;

/// <summary>
/// A software product. This is a minimal stand-in written by the machines group so that <c>machines/{id}/software</c>
/// has a result type; the software group owns this type and may replace it.
/// </summary>
public class Software
{
	/// <summary>The product id, such as <c>microsoft-_-edge</c>.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The product name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The vendor.</summary>
	[JsonPropertyName("vendor")]
	public string? Vendor { get; set; }

	/// <summary>How many known weaknesses the product has.</summary>
	[JsonPropertyName("weaknesses")]
	public long? Weaknesses { get; set; }

	/// <summary>Whether a public exploit exists.</summary>
	[JsonPropertyName("publicExploit")]
	public bool? PublicExploit { get; set; }

	/// <summary>Whether an alert is active for it.</summary>
	[JsonPropertyName("activeAlert")]
	public bool? ActiveAlert { get; set; }

	/// <summary>How many devices are exposed.</summary>
	[JsonPropertyName("exposedMachines")]
	public long? ExposedMachines { get; set; }

	/// <summary>How many devices have it installed.</summary>
	[JsonPropertyName("installedMachines")]
	public long? InstalledMachines { get; set; }

	/// <summary>The impact score.</summary>
	[JsonPropertyName("impactScore")]
	public double? ImpactScore { get; set; }

	/// <summary>Whether the product name is normalised.</summary>
	[JsonPropertyName("isNormalized")]
	public bool? IsNormalized { get; set; }
}
