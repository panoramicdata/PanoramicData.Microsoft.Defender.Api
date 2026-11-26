using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Software;

/// <summary>A security update (KB article) that devices are missing for a product.</summary>
public class SoftwareMissingKb
{
	/// <summary>The KB article number.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The update's name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The products the update applies to.</summary>
	[JsonPropertyName("productsNames")]
	public IReadOnlyList<string> ProductsNames { get; set; } = [];

	/// <summary>A link to the update's catalog page.</summary>
	[JsonPropertyName("url")]
	public string? Url { get; set; }

	/// <summary>How many devices are missing the update.</summary>
	[JsonPropertyName("machineMissedOn")]
	public long? MachineMissedOn { get; set; }

	/// <summary>How many CVEs the update addresses.</summary>
	[JsonPropertyName("cveAddressed")]
	public long? CveAddressed { get; set; }
}
