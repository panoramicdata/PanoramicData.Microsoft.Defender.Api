using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>
/// The changes to make to an alert (<c>PATCH alerts/{id}</c>). Properties left <see langword="null"/> are not sent and keep
/// their value. The service documents the body names with inconsistent casing (<c>Status</c>, <c>assignedTo</c>,
/// <c>Classification</c> on the page's table, all lower camel case in its example); this library sends the example's
/// lower camel case. Unverified against a live tenant.
/// </summary>
public class AlertUpdate
{
	/// <summary>The new status.</summary>
	[JsonPropertyName("status")]
	public AlertStatus? Status { get; set; }

	/// <summary>Who the alert is assigned to.</summary>
	[JsonPropertyName("assignedTo")]
	public string? AssignedTo { get; set; }

	/// <summary>The analyst verdict.</summary>
	[JsonPropertyName("classification")]
	public AlertClassification? Classification { get; set; }

	/// <summary>The finer verdict, such as <c>Malware</c>, <c>Phishing</c>, <c>SecurityTesting</c>, <c>NotMalicious</c> or <c>InsufficientData</c>. Which values suit which classification is the service's rule.</summary>
	[JsonPropertyName("determination")]
	public string? Determination { get; set; }

	/// <summary>A comment to add; it may be sent without any other change.</summary>
	[JsonPropertyName("comment")]
	public string? Comment { get; set; }
}
