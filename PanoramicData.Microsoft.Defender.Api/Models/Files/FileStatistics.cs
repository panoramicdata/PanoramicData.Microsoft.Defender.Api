using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Files;

/// <summary>How often a file was seen in the organisation and worldwide over the look-back window.</summary>
public class FileStatistics
{
	/// <summary>The SHA-1 hash of the file.</summary>
	[JsonPropertyName("sha1")]
	public string? Sha1 { get; set; }

	/// <summary>How many devices in the organisation have seen the file.</summary>
	[JsonPropertyName("organizationPrevalence")]
	public long? OrganizationPrevalence { get; set; }

	/// <summary>When the organisation first saw the file.</summary>
	[JsonPropertyName("orgFirstSeen")]
	public DateTimeOffset? OrgFirstSeen { get; set; }

	/// <summary>When the organisation last saw the file.</summary>
	[JsonPropertyName("orgLastSeen")]
	public DateTimeOffset? OrgLastSeen { get; set; }

	/// <summary>How many devices worldwide have seen the file.</summary>
	[JsonPropertyName("globallyPrevalence")]
	public long? GloballyPrevalence { get; set; }

	/// <summary>When the file was first observed anywhere.</summary>
	[JsonPropertyName("globalFirstObserved")]
	public DateTimeOffset? GlobalFirstObserved { get; set; }

	/// <summary>When the file was last observed anywhere.</summary>
	[JsonPropertyName("globalLastObserved")]
	public DateTimeOffset? GlobalLastObserved { get; set; }

	/// <summary>The names the file most often carried.</summary>
	[JsonPropertyName("topFileNames")]
	public IReadOnlyList<string> TopFileNames { get; set; } = [];
}
