using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>The body that sets a device's value. The property name is case-sensitive on the server.</summary>
public class SetDeviceValueRequest
{
	/// <summary>The value to assign.</summary>
	[JsonPropertyName("DeviceValue")]
	public required MachineDeviceValue DeviceValue { get; set; }
}
