using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Library;

/// <summary>A file in the live response library.</summary>
public class LibraryFile
{
	/// <summary>The file name.</summary>
	[JsonPropertyName("fileName")]
	public string? FileName { get; set; }

	/// <summary>The SHA-256 hash.</summary>
	[JsonPropertyName("sha256")]
	public string? Sha256 { get; set; }

	/// <summary>The description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>When it was uploaded.</summary>
	[JsonPropertyName("creationTime")]
	public DateTimeOffset? CreationTime { get; set; }

	/// <summary>When it was last changed.</summary>
	[JsonPropertyName("lastUpdatedTime")]
	public DateTimeOffset? LastUpdatedTime { get; set; }

	/// <summary>Who uploaded it.</summary>
	[JsonPropertyName("createdBy")]
	public string? CreatedBy { get; set; }

	/// <summary>Whether the script takes parameters.</summary>
	[JsonPropertyName("hasParameters")]
	public bool? HasParameters { get; set; }

	/// <summary>What the parameters are.</summary>
	[JsonPropertyName("parametersDescription")]
	public string? ParametersDescription { get; set; }
}
