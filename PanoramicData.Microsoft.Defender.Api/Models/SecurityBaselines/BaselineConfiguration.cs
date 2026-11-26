using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.SecurityBaselines;

/// <summary>A configuration assessed by an active security baseline profile.</summary>
public class BaselineConfiguration
{
	/// <summary>The configuration's identifier across benchmarks.</summary>
	[JsonPropertyName("uniqueId")]
	public string? UniqueId { get; set; }

	/// <summary>The configuration's identifier inside its benchmark.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The benchmark's name.</summary>
	[JsonPropertyName("benchmarkName")]
	public string? BenchmarkName { get; set; }

	/// <summary>The benchmark's version, which may include operating system details.</summary>
	[JsonPropertyName("benchmarkVersion")]
	public string? BenchmarkVersion { get; set; }

	/// <summary>The configuration's name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The configuration's description, which may contain HTML markup.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The configuration's category.</summary>
	[JsonPropertyName("category")]
	public string? Category { get; set; }

	/// <summary>The compliance levels whose benchmark includes the configuration.</summary>
	[JsonPropertyName("complianceLevels")]
	public IReadOnlyList<string> ComplianceLevels { get; set; } = [];

	/// <summary>The configuration's CCE identifier.</summary>
	[JsonPropertyName("cce")]
	public string? Cce { get; set; }

	/// <summary>Why the benchmark recommends it, possibly with HTML markup.</summary>
	[JsonPropertyName("rationale")]
	public string? Rationale { get; set; }

	/// <summary>The registry paths or other places the device setting is read from.</summary>
	[JsonPropertyName("source")]
	public IReadOnlyList<string> Source { get; set; } = [];

	/// <summary>The recommended value for each source, in the same order.</summary>
	[JsonPropertyName("recommendedValue")]
	public IReadOnlyList<string> RecommendedValue { get; set; } = [];

	/// <summary>The remediation steps, possibly with HTML markup.</summary>
	[JsonPropertyName("remediation")]
	public string? Remediation { get; set; }

	/// <summary>Whether the configuration was customised.</summary>
	[JsonPropertyName("isCustom")]
	public bool? IsCustom { get; set; }
}
