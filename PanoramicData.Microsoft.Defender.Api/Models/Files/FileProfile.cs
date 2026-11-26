using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Files;

/// <summary>What Defender knows about one file, identified by its hash (the <c>File</c> entity).</summary>
public class FileProfile
{
	/// <summary>The SHA-1 hash.</summary>
	[JsonPropertyName("sha1")]
	public string? Sha1 { get; set; }

	/// <summary>The SHA-256 hash.</summary>
	[JsonPropertyName("sha256")]
	public string? Sha256 { get; set; }

	/// <summary>How many devices worldwide have seen the file.</summary>
	[JsonPropertyName("globalPrevalence")]
	public long? GlobalPrevalence { get; set; }

	/// <summary>When the file was first observed anywhere.</summary>
	[JsonPropertyName("globalFirstObserved")]
	public DateTimeOffset? GlobalFirstObserved { get; set; }

	/// <summary>When the file was last observed anywhere.</summary>
	[JsonPropertyName("globalLastObserved")]
	public DateTimeOffset? GlobalLastObserved { get; set; }

	/// <summary>The size in bytes.</summary>
	[JsonPropertyName("size")]
	public long? Size { get; set; }

	/// <summary>The file type label, for example <c>APP</c>. An open set, so kept as text.</summary>
	[JsonPropertyName("fileType")]
	public string? FileType { get; set; }

	/// <summary>Whether the file is a portable executable.</summary>
	[JsonPropertyName("isPeFile")]
	public bool? IsPeFile { get; set; }

	/// <summary>The publisher named by the file's metadata.</summary>
	[JsonPropertyName("filePublisher")]
	public string? FilePublisher { get; set; }

	/// <summary>The product name named by the file's metadata.</summary>
	[JsonPropertyName("fileProductName")]
	public string? FileProductName { get; set; }

	/// <summary>The signer of the file's certificate.</summary>
	[JsonPropertyName("signer")]
	public string? Signer { get; set; }

	/// <summary>The issuer of the signing certificate.</summary>
	[JsonPropertyName("issuer")]
	public string? Issuer { get; set; }

	/// <summary>The hash of the signing certificate.</summary>
	[JsonPropertyName("signerHash")]
	public string? SignerHash { get; set; }

	/// <summary>Whether the signing certificate is valid.</summary>
	[JsonPropertyName("isValidCertificate")]
	public bool? IsValidCertificate { get; set; }

	/// <summary>The kind of verdict reached for the file, for example <c>Pua</c>. An open set, so kept as text.</summary>
	[JsonPropertyName("determinationType")]
	public string? DeterminationType { get; set; }

	/// <summary>The detection name behind the verdict.</summary>
	[JsonPropertyName("determinationValue")]
	public string? DeterminationValue { get; set; }
}
