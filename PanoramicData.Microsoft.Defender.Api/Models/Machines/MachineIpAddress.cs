using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>One network interface address a device has reported.</summary>
public class MachineIpAddress
{
	/// <summary>The IP address.</summary>
	[JsonPropertyName("ipAddress")]
	public string? IpAddress { get; set; }

	/// <summary>The interface's MAC address.</summary>
	[JsonPropertyName("macAddress")]
	public string? MacAddress { get; set; }

	/// <summary>The interface type, such as <c>Ethernet</c>.</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>The interface's operational state, such as <c>Up</c>.</summary>
	[JsonPropertyName("operationalStatus")]
	public string? OperationalStatus { get; set; }
}
