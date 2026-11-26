using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>What Defender does when an indicator matches.</summary>
public enum IndicatorAction
{
	/// <summary>The value was missing or not one this library knows.</summary>
	Unknown = 0,

	/// <summary>Warn the user (Defender for Cloud Apps only).</summary>
	[JsonStringEnumMemberName("Warn")]
	Warn,

	/// <summary>Block the item.</summary>
	[JsonStringEnumMemberName("Block")]
	Block,

	/// <summary>Only record the match; needs generateAlert to be true.</summary>
	[JsonStringEnumMemberName("Audit")]
	Audit,

	/// <summary>Raise an alert. Legacy: the service documents it as supported only until January 2022, but it is still accepted and returned.</summary>
	[JsonStringEnumMemberName("Alert")]
	Alert,

	/// <summary>Raise an alert and block. Legacy, as for <see cref="Alert"/>.</summary>
	[JsonStringEnumMemberName("AlertAndBlock")]
	AlertAndBlock,

	/// <summary>Block and remediate the item.</summary>
	[JsonStringEnumMemberName("BlockAndRemediate")]
	BlockAndRemediate,

	/// <summary>Allow the item.</summary>
	[JsonStringEnumMemberName("Allowed")]
	Allowed
}
