using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>A security update (KB) a device is missing.</summary>
public class MissingKb
{
	/// <summary>The KB number.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The update's name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The operating system build the update applies to.</summary>
	[JsonPropertyName("osBuild")]
	public long? OsBuild { get; set; }

	/// <summary>The products the update applies to.</summary>
	[JsonPropertyName("productsNames")]
	public IReadOnlyList<string> ProductsNames { get; set; } = [];

	/// <summary>Where the update can be found.</summary>
	[JsonPropertyName("url")]
	public string? Url { get; set; }

	/// <summary>How many devices miss the update.</summary>
	[JsonPropertyName("machineMissedOn")]
	public long? MachineMissedOn { get; set; }

	/// <summary>How many CVEs the update addresses.</summary>
	[JsonPropertyName("cveAddressed")]
	public long? CveAddressed { get; set; }
}
