using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>How far a device is cut off from the network when isolated.</summary>
public enum IsolationType
{
	/// <summary>Not a recognised type.</summary>
	Unknown = 0,

	/// <summary>Full isolation.</summary>
	[JsonStringEnumMemberName("Full")]
	Full,

	/// <summary>Selective isolation, which keeps some applications connected.</summary>
	[JsonStringEnumMemberName("Selective")]
	Selective,

	/// <summary>Isolation of an unmanaged device.</summary>
	[JsonStringEnumMemberName("UnManagedDevice")]
	UnManagedDevice
}
