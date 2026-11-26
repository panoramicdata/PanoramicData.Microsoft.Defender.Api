using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The paging options of the scan history by definition query.</summary>
public class ScanHistoryOptions
{
	/// <summary>The most rows to return (<c>$top</c>, at most 10,000).</summary>
	[AliasAs("$top")]
	public int? Top { get; set; }

	/// <summary>How many rows to skip (<c>$skip</c>).</summary>
	[AliasAs("$skip")]
	public int? Skip { get; set; }
}
