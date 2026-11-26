using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models;

/// <summary>The one query option the file, domain and IP statistics operations accept.</summary>
public class EntityStatisticsOptions
{
	/// <summary>
	/// How many hours back to count observations (<c>lookBackHours</c>). The service defaults to 30 days and allows at most
	/// 720 hours; leave <see langword="null"/> to omit it.
	/// </summary>
	[AliasAs("lookBackHours")]
	public int? LookBackHours { get; set; }
}
