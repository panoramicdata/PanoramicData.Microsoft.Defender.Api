using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One certificate found on one device (a row per device, thumbprint and store path). Unverified against a live tenant: the documentation's property table and its example name several columns differently, so both spellings are modelled.</summary>
public class CertificateAssessment : AssessmentDeviceRecord
{
	/// <summary>The certificate's thumbprint.</summary>
	[JsonPropertyName("thumbprint")]
	public string? Thumbprint { get; set; }

	/// <summary>Where the certificate is stored.</summary>
	[JsonPropertyName("path")]
	public string? Path { get; set; }

	/// <summary>The signature's hashing and encryption algorithm.</summary>
	[JsonPropertyName("signatureAlgorithm")]
	public string? SignatureAlgorithm { get; set; }

	/// <summary>The serial number within the issuing authority.</summary>
	[JsonPropertyName("serialNumber")]
	public string? SerialNumber { get; set; }

	/// <summary>Whether the holder is a certificate authority or an end entity.</summary>
	[JsonPropertyName("subjectType")]
	public string? SubjectType { get; set; }

	/// <summary>The key size, as spelled in the response example.</summary>
	[JsonPropertyName("keyLength")]
	public long? KeyLength { get; set; }

	/// <summary>The key size, as spelled in the property table.</summary>
	[JsonPropertyName("keySize")]
	public string? KeySize { get; set; }

	/// <summary>The end of the validity period, as spelled in the response example.</summary>
	[JsonPropertyName("notAfter")]
	public DateTimeOffset? NotAfter { get; set; }

	/// <summary>The start of the validity period, as spelled in the response example.</summary>
	[JsonPropertyName("notBefore")]
	public DateTimeOffset? NotBefore { get; set; }

	/// <summary>The end of the validity period, as spelled in the property table.</summary>
	[JsonPropertyName("expirationDate")]
	public DateTimeOffset? ExpirationDate { get; set; }

	/// <summary>The start of the validity period, as spelled in the property table.</summary>
	[JsonPropertyName("issueDate")]
	public DateTimeOffset? IssueDate { get; set; }

	/// <summary>Whether the certificate is self-signed.</summary>
	[JsonPropertyName("isSelfSigned")]
	public bool? IsSelfSigned { get; set; }

	/// <summary>The holder, as the service sends it (its shape is undocumented, so it is kept as raw JSON).</summary>
	[JsonPropertyName("subjectObject")]
	public JsonElement? SubjectObject { get; set; }

	/// <summary>The issuer, as the service sends it (raw JSON).</summary>
	[JsonPropertyName("issuerObject")]
	public JsonElement? IssuerObject { get; set; }

	/// <summary>The permitted key uses, as the service sends them (raw JSON).</summary>
	[JsonPropertyName("keyUsageArray")]
	public JsonElement? KeyUsageArray { get; set; }

	/// <summary>The extended key uses, as the service sends them (raw JSON).</summary>
	[JsonPropertyName("extendedKeyUsageArray")]
	public JsonElement? ExtendedKeyUsageArray { get; set; }
}
