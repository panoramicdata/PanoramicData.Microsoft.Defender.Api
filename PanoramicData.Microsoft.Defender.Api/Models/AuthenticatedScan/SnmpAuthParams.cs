using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>
/// The SNMP credentials a network scan authenticates with. Which members matter depends on <see cref="Type"/>: a community
/// string for <c>CommunityString</c>, a user name for the other modes, passwords and protocols for <c>AuthNoPriv</c> and
/// <c>AuthPriv</c>, or a key vault reference instead of literal secrets. The service normally returns this object as
/// <see langword="null"/> on read, so the secrets are not echoed back. The type holds secrets and never prints them.
/// </summary>
/// <remarks>
/// The documentation's property table spells some of these in PascalCase while its JSON examples use camelCase; the examples
/// are followed (unverified).
/// </remarks>
public class SnmpAuthParams
{
	/// <summary>The only documented discriminator value, for SNMP credentials.</summary>
	public static string SnmpODataType => "#microsoft.windowsDefenderATP.api.SnmpAuthParams";

	/// <summary>The OData type discriminator; always <see cref="SnmpODataType"/> for SNMP.</summary>
	[JsonPropertyName("@odata.type")]
	public string ODataType { get; set; } = SnmpODataType;

	/// <summary>The authentication mode: <c>CommunityString</c>, <c>NoAuthNoPriv</c>, <c>AuthNoPriv</c> or <c>AuthPriv</c>.</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>The key vault the scanner reads credentials from, instead of literal ones.</summary>
	[JsonPropertyName("keyVaultUrl")]
	public string? KeyVaultUrl { get; set; }

	/// <summary>The key vault secret holding the credentials.</summary>
	[JsonPropertyName("keyVaultSecretName")]
	public string? KeyVaultSecretName { get; set; }

	/// <summary>The user name, for every mode except <c>CommunityString</c>.</summary>
	[JsonPropertyName("username")]
	public string? Username { get; set; }

	/// <summary>The community string, for <c>CommunityString</c>.</summary>
	[JsonPropertyName("communityString")]
	public string? CommunityString { get; set; }

	/// <summary>The authentication protocol for <c>AuthNoPriv</c> and <c>AuthPriv</c>: <c>MD5</c> or <c>SHA1</c>.</summary>
	[JsonPropertyName("authProtocol")]
	public string? AuthProtocol { get; set; }

	/// <summary>The authentication password for <c>AuthNoPriv</c> and <c>AuthPriv</c>.</summary>
	[JsonPropertyName("authPassword")]
	public string? AuthPassword { get; set; }

	/// <summary>The privacy protocol for <c>AuthPriv</c>: <c>DES</c>, <c>3DES</c> or <c>AES</c>.</summary>
	[JsonPropertyName("privProtocol")]
	public string? PrivProtocol { get; set; }

	/// <summary>The privacy password for <c>AuthPriv</c>.</summary>
	[JsonPropertyName("privPassword")]
	public string? PrivPassword { get; set; }

	/// <summary>Describes the object without any of its secrets.</summary>
	/// <returns>The authentication mode only.</returns>
	public override string ToString() => $"{nameof(SnmpAuthParams)} ({Type})";
}
