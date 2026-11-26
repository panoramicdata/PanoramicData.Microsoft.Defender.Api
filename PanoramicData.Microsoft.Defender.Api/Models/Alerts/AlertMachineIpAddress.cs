using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>One network interface of a device.</summary>
public class AlertMachineIpAddress
{
	/// <summary>The IP address.</summary>
	[JsonPropertyName("ipAddress")]
	public string? IpAddress { get; set; }

	/// <summary>The MAC address.</summary>
	[JsonPropertyName("macAddress")]
	public string? MacAddress { get; set; }

	/// <summary>The interface status, such as <c>Up</c>.</summary>
	[JsonPropertyName("operationalStatus")]
	public string? OperationalStatus { get; set; }
}
