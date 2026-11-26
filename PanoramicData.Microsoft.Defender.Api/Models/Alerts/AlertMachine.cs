using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>The device an alert relates to (<c>GET alerts/{id}/machine</c>). A minimal stand-in until the machines category provides its own device type.</summary>
public class AlertMachine
{
	/// <summary>The device id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The device fully qualified name.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>When the device was first seen.</summary>
	[JsonPropertyName("firstSeen")]
	public DateTimeOffset? FirstSeen { get; set; }

	/// <summary>When the device was last seen.</summary>
	[JsonPropertyName("lastSeen")]
	public DateTimeOffset? LastSeen { get; set; }

	/// <summary>The operating system platform.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }

	/// <summary>The processor architecture.</summary>
	[JsonPropertyName("osProcessor")]
	public string? OsProcessor { get; set; }

	/// <summary>The operating system version.</summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }

	/// <summary>The last internal IP address.</summary>
	[JsonPropertyName("lastIpAddress")]
	public string? LastIpAddress { get; set; }

	/// <summary>The last external IP address.</summary>
	[JsonPropertyName("lastExternalIpAddress")]
	public string? LastExternalIpAddress { get; set; }

	/// <summary>The operating system build number.</summary>
	[JsonPropertyName("osBuild")]
	public long? OsBuild { get; set; }

	/// <summary>The sensor health status.</summary>
	[JsonPropertyName("healthStatus")]
	public string? HealthStatus { get; set; }

	/// <summary>The device value assigned to the device.</summary>
	[JsonPropertyName("deviceValue")]
	public string? DeviceValue { get; set; }

	/// <summary>The device group name.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }

	/// <summary>The risk score.</summary>
	[JsonPropertyName("riskScore")]
	public string? RiskScore { get; set; }

	/// <summary>The exposure level.</summary>
	[JsonPropertyName("exposureLevel")]
	public string? ExposureLevel { get; set; }

	/// <summary>The Entra device id.</summary>
	[JsonPropertyName("aadDeviceId")]
	public string? AadDeviceId { get; set; }

	/// <summary>Tags on the device.</summary>
	[JsonPropertyName("machineTags")]
	public IReadOnlyList<string> MachineTags { get; set; } = [];

	/// <summary>The network interfaces of the device.</summary>
	[JsonPropertyName("ipAddresses")]
	public IReadOnlyList<AlertMachineIpAddress> IpAddresses { get; set; } = [];
}
