using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>Whether a tag operation adds or removes the tag.</summary>
public enum MachineTagAction
{
	/// <summary>Not a recognised action.</summary>
	Unknown = 0,

	/// <summary>Add the tag.</summary>
	[JsonStringEnumMemberName("Add")]
	Add,

	/// <summary>Remove the tag.</summary>
	[JsonStringEnumMemberName("Remove")]
	Remove
}
