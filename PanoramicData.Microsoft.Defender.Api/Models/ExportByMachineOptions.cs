using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models;

/// <summary>
/// The query options of the "by machine" export operations, which return a paged JSON collection (<see cref="ODataPage{T}"/>).
/// These operations are limited to a few calls per minute: page with <see cref="DefenderClient.GetAllAsync{T}(Func{CancellationToken, Task{ODataPage{T}}}, CancellationToken)"/>
/// rather than polling.
/// </summary>
public class ExportByMachineOptions
{
	/// <summary>The number of records per page (<c>pageSize</c>).</summary>
	[AliasAs("pageSize")]
	public int? PageSize { get; set; }

	/// <summary>The most records to return (<c>$top</c>). A response limited this way carries no next link.</summary>
	[AliasAs("$top")]
	public int? Top { get; set; }

	/// <summary>For the delta exports, the instant to report changes since (<c>sinceTime</c>), sent as UTC ISO 8601 text.</summary>
	[AliasAs("sinceTime")]
	public DateTimeOffset? SinceTime { get; set; }
}
