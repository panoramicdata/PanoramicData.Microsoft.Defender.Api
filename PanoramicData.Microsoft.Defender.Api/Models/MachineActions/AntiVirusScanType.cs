using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The kind of antivirus scan to run.</summary>
public enum AntiVirusScanType
{
	/// <summary>Not a recognised type.</summary>
	Unknown = 0,

	/// <summary>A quick scan.</summary>
	[JsonStringEnumMemberName("Quick")]
	Quick,

	/// <summary>A full scan.</summary>
	[JsonStringEnumMemberName("Full")]
	Full
}
