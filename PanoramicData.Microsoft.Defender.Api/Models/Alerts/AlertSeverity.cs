using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>How severe an alert is.</summary>
public enum AlertSeverity
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

	/// <summary>No severity was assigned.</summary>
	[JsonStringEnumMemberName("UnSpecified")]
	UnSpecified,

	/// <summary>Informational only.</summary>
	[JsonStringEnumMemberName("Informational")]
	Informational,

	/// <summary>Low severity.</summary>
	[JsonStringEnumMemberName("Low")]
	Low,

	/// <summary>Medium severity.</summary>
	[JsonStringEnumMemberName("Medium")]
	Medium,

	/// <summary>High severity.</summary>
	[JsonStringEnumMemberName("High")]
	High
}
