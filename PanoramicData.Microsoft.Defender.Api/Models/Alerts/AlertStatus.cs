using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>Where an alert is in its life cycle.</summary>
public enum AlertStatus
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

	/// <summary>Not yet looked at.</summary>
	[JsonStringEnumMemberName("New")]
	New,

	/// <summary>Being worked on.</summary>
	[JsonStringEnumMemberName("InProgress")]
	InProgress,

	/// <summary>Dealt with.</summary>
	[JsonStringEnumMemberName("Resolved")]
	Resolved
}
