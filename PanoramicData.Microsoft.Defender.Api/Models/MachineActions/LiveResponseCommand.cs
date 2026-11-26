using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>One command of a live response session, with its parameters.</summary>
public class LiveResponseCommand
{
	/// <summary>The command kind.</summary>
	[JsonPropertyName("type")]
	public required LiveResponseCommandType Type { get; set; }

	/// <summary>The command's parameters.</summary>
	[JsonPropertyName("params")]
	public required IReadOnlyList<LiveResponseParameter> Params { get; set; }

	/// <summary>Builds a command that runs a library script, with optional arguments.</summary>
	/// <param name="scriptName">The script's name in the library.</param>
	/// <param name="args">The arguments, or <see langword="null"/> for none.</param>
	/// <returns>The command.</returns>
	public static LiveResponseCommand RunScript(string scriptName, string? args)
	{
		List<LiveResponseParameter> parameters = [new() { Key = "ScriptName", Value = scriptName }];
		if (args is not null)
		{
			parameters.Add(new() { Key = "Args", Value = args });
		}

		return new() { Type = LiveResponseCommandType.RunScript, Params = parameters };
	}

	/// <summary>Builds a command that fetches a file from the device.</summary>
	/// <param name="path">The file's path on the device.</param>
	/// <returns>The command.</returns>
	public static LiveResponseCommand GetFile(string path)
		=> new() { Type = LiveResponseCommandType.GetFile, Params = [new() { Key = "Path", Value = path }] };

	/// <summary>Builds a command that puts a library file on the device.</summary>
	/// <param name="fileName">The file's name in the library.</param>
	/// <returns>The command.</returns>
	public static LiveResponseCommand PutFile(string fileName)
		=> new() { Type = LiveResponseCommandType.PutFile, Params = [new() { Key = "FileName", Value = fileName }] };
}
