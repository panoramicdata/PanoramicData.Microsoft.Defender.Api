using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>The severity attached to an indicator.</summary>
public enum IndicatorSeverity
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

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
