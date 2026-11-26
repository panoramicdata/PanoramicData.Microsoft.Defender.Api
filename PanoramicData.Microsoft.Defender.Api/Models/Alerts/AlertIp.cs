using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>An IP address related to an alert (<c>GET alerts/{id}/ips</c>). A minimal stand-in until the IPs category provides its own type.</summary>
public class AlertIp
{
	/// <summary>The IP address.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }
}
