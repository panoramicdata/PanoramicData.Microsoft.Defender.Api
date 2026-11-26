using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Exports;

/// <summary>
/// The response of a "via files" export: download links to gzip-compressed files of JSON records, one record per line.
/// Pass it to <see cref="DefenderClient.StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/> to read the records.
/// </summary>
/// <remarks>
/// The links carry a signature that grants access to the data, so treat them as secrets: this type's <see cref="ToString"/>,
/// and the client's logs and exceptions, never include them. The service has been seen to spell the list's property
/// <c>exportFiles</c> and <c>Export files</c>; both are read.
/// </remarks>
public class ExportFilesResponse
{
	/// <summary>The download links, in order. Each is signed: do not log it.</summary>
	[JsonPropertyName("exportFiles")]
	public IReadOnlyList<string> ExportFiles { get; set; } = [];

	/// <summary>When the snapshot was generated.</summary>
	[JsonPropertyName("generatedTime")]
	public DateTimeOffset? GeneratedTime { get; set; }

	/// <summary>Reads the list when it arrives under the spelling <c>Export files</c>.</summary>
	[JsonInclude]
	[JsonPropertyName("Export files")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	private IReadOnlyList<string>? ExportFilesWithSpace
	{
		// The getter is always null, so the property is never written back: the canonical spelling is the only one emitted.
		get => null;
		set
		{
			// A JSON null leaves the list as it was.
			if (value is not null)
			{
				ExportFiles = value;
			}
		}
	}

	/// <summary>Describes the response without revealing any link.</summary>
	/// <returns>The number of files and the generation time.</returns>
	public override string ToString() => $"ExportFilesResponse {{ Files = {ExportFiles.Count}, GeneratedTime = {GeneratedTime:O} }}";
}
