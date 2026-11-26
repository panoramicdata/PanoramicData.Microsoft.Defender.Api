using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Users;

/// <summary>
/// A user account seen on devices. This is a minimal stand-in written by the machines group so that
/// <c>machines/{id}/logonusers</c> has a result type; the users group owns this type and may replace it.
/// </summary>
public class User
{
	/// <summary>The user id, normally <c>domain\name</c>.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The account name.</summary>
	[JsonPropertyName("accountName")]
	public string? AccountName { get; set; }

	/// <summary>The account domain.</summary>
	[JsonPropertyName("accountDomain")]
	public string? AccountDomain { get; set; }

	/// <summary>When the user was first seen.</summary>
	[JsonPropertyName("firstSeen")]
	public DateTimeOffset? FirstSeen { get; set; }

	/// <summary>When the user was last seen.</summary>
	[JsonPropertyName("lastSeen")]
	public DateTimeOffset? LastSeen { get; set; }

	/// <summary>The logon types seen, as the service reports them.</summary>
	[JsonPropertyName("logonTypes")]
	public string? LogonTypes { get; set; }

	/// <summary>Whether the user is a domain administrator.</summary>
	[JsonPropertyName("isDomainAdmin")]
	public bool? IsDomainAdmin { get; set; }

	/// <summary>Whether the user has only logged on over the network.</summary>
	[JsonPropertyName("isOnlyNetworkUser")]
	public bool? IsOnlyNetworkUser { get; set; }
}
