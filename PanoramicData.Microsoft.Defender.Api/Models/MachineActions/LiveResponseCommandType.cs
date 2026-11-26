using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The kind of a live response command.</summary>
public enum LiveResponseCommandType
{
	/// <summary>Not a recognised type.</summary>
	Unknown = 0,

	/// <summary>Put a library file on the device.</summary>
	[JsonStringEnumMemberName("PutFile")]
	PutFile,

	/// <summary>Run a library script.</summary>
	[JsonStringEnumMemberName("RunScript")]
	RunScript,

	/// <summary>Fetch a file from the device.</summary>
	[JsonStringEnumMemberName("GetFile")]
	GetFile
}
