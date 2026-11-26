using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>A domain related to an alert (<c>GET alerts/{id}/domains</c>). A minimal stand-in until the domains category provides its own type.</summary>
public class AlertDomain
{
	/// <summary>The host name.</summary>
	[JsonPropertyName("host")]
	public string? Host { get; set; }
}
