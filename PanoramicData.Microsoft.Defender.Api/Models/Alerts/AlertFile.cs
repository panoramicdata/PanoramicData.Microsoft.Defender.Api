using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>A file related to an alert (<c>GET alerts/{id}/files</c>). A minimal stand-in until the files category provides its own type.</summary>
public class AlertFile
{
	/// <summary>The SHA-1 hash.</summary>
	[JsonPropertyName("sha1")]
	public string? Sha1 { get; set; }

	/// <summary>The SHA-256 hash.</summary>
	[JsonPropertyName("sha256")]
	public string? Sha256 { get; set; }

	/// <summary>The MD5 hash.</summary>
	[JsonPropertyName("md5")]
	public string? Md5 { get; set; }

	/// <summary>How many devices worldwide have seen the file.</summary>
	[JsonPropertyName("globalPrevalence")]
	public long? GlobalPrevalence { get; set; }

	/// <summary>When the file was first observed worldwide.</summary>
	[JsonPropertyName("globalFirstObserved")]
	public DateTimeOffset? GlobalFirstObserved { get; set; }

	/// <summary>When the file was last observed worldwide.</summary>
	[JsonPropertyName("globalLastObserved")]
	public DateTimeOffset? GlobalLastObserved { get; set; }

	/// <summary>The size in bytes.</summary>
	[JsonPropertyName("size")]
	public long? Size { get; set; }

	/// <summary>The file type.</summary>
	[JsonPropertyName("fileType")]
	public string? FileType { get; set; }

	/// <summary>Whether the file is a portable executable.</summary>
	[JsonPropertyName("isPeFile")]
	public bool? IsPeFile { get; set; }

	/// <summary>The publisher.</summary>
	[JsonPropertyName("filePublisher")]
	public string? FilePublisher { get; set; }

	/// <summary>The product name.</summary>
	[JsonPropertyName("fileProductName")]
	public string? FileProductName { get; set; }

	/// <summary>The code signer.</summary>
	[JsonPropertyName("signer")]
	public string? Signer { get; set; }

	/// <summary>The certificate issuer.</summary>
	[JsonPropertyName("issuer")]
	public string? Issuer { get; set; }

	/// <summary>The signer certificate hash.</summary>
	[JsonPropertyName("signerHash")]
	public string? SignerHash { get; set; }

	/// <summary>Whether the signing certificate is valid.</summary>
	[JsonPropertyName("isValidCertificate")]
	public bool? IsValidCertificate { get; set; }

	/// <summary>How the file was judged.</summary>
	[JsonPropertyName("determinationType")]
	public string? DeterminationType { get; set; }

	/// <summary>The value of the judgement.</summary>
	[JsonPropertyName("determinationValue")]
	public string? DeterminationValue { get; set; }
}
