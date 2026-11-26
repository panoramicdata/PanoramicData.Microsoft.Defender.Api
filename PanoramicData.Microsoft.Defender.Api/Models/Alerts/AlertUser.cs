using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>The user related to an alert (<c>GET alerts/{id}/user</c>). A minimal stand-in until the users category provides its own type.</summary>
public class AlertUser
{
	/// <summary>The user id, written domain, backslash, name.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The account name.</summary>
	[JsonPropertyName("accountName")]
	public string? AccountName { get; set; }

	/// <summary>The account domain.</summary>
	[JsonPropertyName("accountDomain")]
	public string? AccountDomain { get; set; }

	/// <summary>The security identifier.</summary>
	[JsonPropertyName("accountSid")]
	public string? AccountSid { get; set; }

	/// <summary>When the user was first seen.</summary>
	[JsonPropertyName("firstSeen")]
	public DateTimeOffset? FirstSeen { get; set; }

	/// <summary>When the user was last seen.</summary>
	[JsonPropertyName("lastSeen")]
	public DateTimeOffset? LastSeen { get; set; }

	/// <summary>The device the user signed in to most.</summary>
	[JsonPropertyName("mostPrevalentMachineId")]
	public string? MostPrevalentMachineId { get; set; }

	/// <summary>The device the user signed in to least.</summary>
	[JsonPropertyName("leastPrevalentMachineId")]
	public string? LeastPrevalentMachineId { get; set; }

	/// <summary>The kinds of sign-in seen.</summary>
	[JsonPropertyName("logonTypes")]
	public string? LogonTypes { get; set; }

	/// <summary>How many devices the user signed in to.</summary>
	[JsonPropertyName("logOnMachinesCount")]
	public int? LogOnMachinesCount { get; set; }

	/// <summary>Whether the user is a domain admin.</summary>
	[JsonPropertyName("isDomainAdmin")]
	public bool? IsDomainAdmin { get; set; }

	/// <summary>Whether the user only signs in over the network.</summary>
	[JsonPropertyName("isOnlyNetworkUser")]
	public bool? IsOnlyNetworkUser { get; set; }
}
