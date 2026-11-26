using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>The body of a device update. A property left <see langword="null"/> is not sent.</summary>
public class UpdateMachineRequest
{
	/// <summary>
	/// The complete tag list the device should have afterwards. Tags the device has that are missing from the list are
	/// removed, so include the existing ones to keep them.
	/// </summary>
	[JsonPropertyName("machineTags")]
	public IReadOnlyList<string>? MachineTags { get; set; }

	/// <summary>The new device value.</summary>
	[JsonPropertyName("deviceValue")]
	public MachineDeviceValue? DeviceValue { get; set; }
}
