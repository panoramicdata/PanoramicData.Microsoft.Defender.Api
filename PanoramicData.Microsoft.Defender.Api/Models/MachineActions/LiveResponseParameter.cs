using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>One key and value passed to a live response command.</summary>
public class LiveResponseParameter
{
	/// <summary>The parameter name, such as <c>ScriptName</c>.</summary>
	[JsonPropertyName("key")]
	public required string Key { get; set; }

	/// <summary>The parameter value.</summary>
	[JsonPropertyName("value")]
	public required string Value { get; set; }
}
