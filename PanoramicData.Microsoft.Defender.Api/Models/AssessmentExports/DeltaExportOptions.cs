using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>
/// The query options of the delta export, which reports only what changed since a point in time. Unlike
/// <see cref="ExportByMachineOptions"/>, the time is required.
/// </summary>
public class DeltaExportOptions
{
	/// <summary>The instant to report changes since (<c>sinceTime</c>), sent as UTC ISO 8601 text. The service looks back at most 14 days.</summary>
	[AliasAs("sinceTime")]
	public required DateTimeOffset SinceTime { get; set; }

	/// <summary>The number of records per page (<c>pageSize</c>).</summary>
	[AliasAs("pageSize")]
	public int? PageSize { get; set; }

	/// <summary>The most records to return (<c>$top</c>). A response limited this way carries no next link.</summary>
	[AliasAs("$top")]
	public int? Top { get; set; }
}
