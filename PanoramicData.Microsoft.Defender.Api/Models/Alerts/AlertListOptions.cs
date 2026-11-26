using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>OData options for listing alerts: the common ones plus <c>$expand</c>.</summary>
public class AlertListOptions : ODataListOptions
{
	/// <summary>The only <c>$expand</c> value the service documents: include each alert's evidence.</summary>
	public static string ExpandEvidence => "evidence";

	/// <summary>Related data to include (<c>$expand</c>); use <see cref="ExpandEvidence"/>.</summary>
	[AliasAs("$expand")]
	public string? Expand { get; set; }
}
