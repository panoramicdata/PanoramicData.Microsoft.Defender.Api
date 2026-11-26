using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>The user named on an alert.</summary>
public class AlertRelatedUser
{
	/// <summary>The account name.</summary>
	[JsonPropertyName("userName")]
	public string? UserName { get; set; }

	/// <summary>The account domain.</summary>
	[JsonPropertyName("domainName")]
	public string? DomainName { get; set; }
}
