using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;

/// <summary>A brief reference to a device affected by a vulnerability.</summary>
public class MachineReference
{
	/// <summary>The device id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The device's fully qualified DNS name.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>The operating system platform.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }

	/// <summary>The name of the device group the device belongs to.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }
}
