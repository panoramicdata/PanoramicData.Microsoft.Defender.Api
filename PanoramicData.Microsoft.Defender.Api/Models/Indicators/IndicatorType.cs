using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>What kind of value an indicator matches.</summary>
public enum IndicatorType
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

	/// <summary>A file SHA-1 hash.</summary>
	[JsonStringEnumMemberName("FileSha1")]
	FileSha1,

	/// <summary>A file SHA-256 hash.</summary>
	[JsonStringEnumMemberName("FileSha256")]
	FileSha256,

	/// <summary>A file MD5 hash.</summary>
	[JsonStringEnumMemberName("FileMd5")]
	FileMd5,

	/// <summary>A certificate thumbprint.</summary>
	[JsonStringEnumMemberName("CertificateThumbprint")]
	CertificateThumbprint,

	/// <summary>An IP address (CIDR ranges are not supported).</summary>
	[JsonStringEnumMemberName("IpAddress")]
	IpAddress,

	/// <summary>A domain name.</summary>
	[JsonStringEnumMemberName("DomainName")]
	DomainName,

	/// <summary>A URL.</summary>
	[JsonStringEnumMemberName("Url")]
	Url
}
