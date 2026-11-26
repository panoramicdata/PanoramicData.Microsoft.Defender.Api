using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>An analyst verdict on an alert, sent when updating it.</summary>
public enum AlertClassification
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

	/// <summary>The alert is a real threat.</summary>
	[JsonStringEnumMemberName("TruePositive")]
	TruePositive,

	/// <summary>The activity is expected and harmless.</summary>
	[JsonStringEnumMemberName("InformationalExpectedActivity")]
	InformationalExpectedActivity,

	/// <summary>The alert is wrong.</summary>
	[JsonStringEnumMemberName("FalsePositive")]
	FalsePositive
}
