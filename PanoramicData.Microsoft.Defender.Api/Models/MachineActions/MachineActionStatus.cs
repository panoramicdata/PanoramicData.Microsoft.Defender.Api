using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The state of a machine action.</summary>
public enum MachineActionStatus
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

	/// <summary>Waiting to start.</summary>
	[JsonStringEnumMemberName("Pending")]
	Pending,

	/// <summary>Running.</summary>
	[JsonStringEnumMemberName("InProgress")]
	InProgress,

	/// <summary>Finished successfully.</summary>
	[JsonStringEnumMemberName("Succeeded")]
	Succeeded,

	/// <summary>Finished with a failure.</summary>
	[JsonStringEnumMemberName("Failed")]
	Failed,

	/// <summary>Timed out.</summary>
	[JsonStringEnumMemberName("TimeOut")]
	TimeOut,

	/// <summary>Cancelled.</summary>
	[JsonStringEnumMemberName("Cancelled")]
	Cancelled
}
