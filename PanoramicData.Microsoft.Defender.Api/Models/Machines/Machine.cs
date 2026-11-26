using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>A device known to Defender for Endpoint.</summary>
public class Machine
{
	/// <summary>The device id (a 40-character hexadecimal string).</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The device's fully qualified DNS name.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>When the device was first seen.</summary>
	[JsonPropertyName("firstSeen")]
	public DateTimeOffset? FirstSeen { get; set; }

	/// <summary>When the device was last seen.</summary>
	[JsonPropertyName("lastSeen")]
	public DateTimeOffset? LastSeen { get; set; }

	/// <summary>The operating system platform, such as <c>Windows10</c>.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }

	/// <summary>The onboarding state, such as <c>Onboarded</c> or <c>CanBeOnboarded</c>.</summary>
	[JsonPropertyName("onboardingStatus")]
	public string? OnboardingStatus { get; set; }

	/// <summary>The processor architecture as the service's older field reports it; prefer <see cref="OsArchitecture"/>.</summary>
	[JsonPropertyName("osProcessor")]
	public string? OsProcessor { get; set; }

	/// <summary>The operating system version.</summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }

	/// <summary>The operating system build number.</summary>
	[JsonPropertyName("osBuild")]
	public long? OsBuild { get; set; }

	/// <summary>The last IP address the device used inside the network.</summary>
	[JsonPropertyName("lastIpAddress")]
	public string? LastIpAddress { get; set; }

	/// <summary>The last public IP address the device was seen on.</summary>
	[JsonPropertyName("lastExternalIpAddress")]
	public string? LastExternalIpAddress { get; set; }

	/// <summary>The sensor health state, such as <c>Active</c> or <c>Inactive</c>.</summary>
	[JsonPropertyName("healthStatus")]
	public string? HealthStatus { get; set; }

	/// <summary>The name of the device group the device belongs to.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }

	/// <summary>The id of the device group the device belongs to.</summary>
	[JsonPropertyName("rbacGroupId")]
	public long? RbacGroupId { get; set; }

	/// <summary>The risk score, such as <c>Low</c> or <c>High</c>.</summary>
	[JsonPropertyName("riskScore")]
	public string? RiskScore { get; set; }

	/// <summary>The Microsoft Entra device id, when the device is joined.</summary>
	[JsonPropertyName("aadDeviceId")]
	public string? AadDeviceId { get; set; }

	/// <summary>The tags set on the device.</summary>
	[JsonPropertyName("machineTags")]
	public IReadOnlyList<string> MachineTags { get; set; } = [];

	/// <summary>The exposure level, such as <c>Medium</c>.</summary>
	[JsonPropertyName("exposureLevel")]
	public string? ExposureLevel { get; set; }

	/// <summary>How valuable the device is rated to be.</summary>
	[JsonPropertyName("deviceValue")]
	public MachineDeviceValue DeviceValue { get; set; }

	/// <summary>The network interfaces the device has reported.</summary>
	[JsonPropertyName("ipAddresses")]
	public IReadOnlyList<MachineIpAddress> IpAddresses { get; set; } = [];

	/// <summary>The processor architecture, such as <c>x64</c>.</summary>
	[JsonPropertyName("osArchitecture")]
	public string? OsArchitecture { get; set; }

	/// <summary>Details of the virtual machine, when the device is one hosted in a cloud.</summary>
	[JsonPropertyName("vmMetadata")]
	public MachineVmMetadata? VmMetadata { get; set; }
}
