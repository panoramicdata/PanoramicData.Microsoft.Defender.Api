using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>
/// The same change applied to many alerts (<c>POST alerts/batchUpdate</c>). Assigning an empty <see cref="AlertIds"/> list
/// throws; the service documents no maximum, so none is enforced.
/// </summary>
public class AlertBatchUpdate : AlertUpdate
{
	/// <summary>The alerts to change; at least one.</summary>
	/// <exception cref="ArgumentException">The list is empty.</exception>
	[JsonPropertyName("alertIds")]
	public required IReadOnlyList<string> AlertIds
	{
		get;
		init => field = BatchGuard.Check(value, null, nameof(AlertIds));
	}
}
