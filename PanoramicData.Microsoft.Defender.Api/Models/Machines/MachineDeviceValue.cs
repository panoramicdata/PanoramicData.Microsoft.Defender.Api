using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>The value (importance) assigned to a device.</summary>
public enum MachineDeviceValue
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

	/// <summary>Normal value.</summary>
	[JsonStringEnumMemberName("Normal")]
	Normal,

	/// <summary>Low value.</summary>
	[JsonStringEnumMemberName("Low")]
	Low,

	/// <summary>High value.</summary>
	[JsonStringEnumMemberName("High")]
	High
}
