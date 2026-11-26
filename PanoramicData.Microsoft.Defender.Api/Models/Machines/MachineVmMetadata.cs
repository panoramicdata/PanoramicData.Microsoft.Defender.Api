using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>Cloud details of a device that is a virtual machine.</summary>
public class MachineVmMetadata
{
	/// <summary>The virtual machine's id.</summary>
	[JsonPropertyName("vmId")]
	public string? VmId { get; set; }

	/// <summary>The cloud provider hosting it.</summary>
	[JsonPropertyName("cloudProvider")]
	public string? CloudProvider { get; set; }

	/// <summary>The cloud resource id.</summary>
	[JsonPropertyName("resourceId")]
	public string? ResourceId { get; set; }

	/// <summary>The cloud subscription id.</summary>
	[JsonPropertyName("subscriptionId")]
	public string? SubscriptionId { get; set; }
}
