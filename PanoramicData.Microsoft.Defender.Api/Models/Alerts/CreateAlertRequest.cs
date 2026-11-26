using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>
/// An alert to raise on top of an advanced-hunting event (<c>POST alerts/CreateAlertByReference</c>). Every member is
/// required. The event is identified by <see cref="EventTime"/>, <see cref="ReportId"/> and <see cref="MachineId"/>.
/// </summary>
public class CreateAlertRequest
{
	/// <summary>The exact time of the event, as advanced hunting reports it.</summary>
	[JsonPropertyName("eventTime")]
	public required DateTimeOffset EventTime { get; set; }

	/// <summary>The report id of the event.</summary>
	[JsonPropertyName("reportId")]
	public required string ReportId { get; set; }

	/// <summary>The device the event happened on.</summary>
	[JsonPropertyName("machineId")]
	public required string MachineId { get; set; }

	/// <summary>The alert severity; the service accepts <c>Low</c>, <c>Medium</c> and <c>High</c> here.</summary>
	[JsonPropertyName("severity")]
	public required AlertSeverity Severity { get; set; }

	/// <summary>The alert title. An open alert on the same device with the same title absorbs the new one.</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The alert description.</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>What a security analyst should do about it.</summary>
	[JsonPropertyName("recommendedAction")]
	public required string RecommendedAction { get; set; }

	/// <summary>The category: one of <c>General</c>, <c>CommandAndControl</c>, <c>Collection</c>, <c>CredentialAccess</c>, <c>DefenseEvasion</c>, <c>Discovery</c>, <c>Exfiltration</c>, <c>Exploit</c>, <c>Execution</c>, <c>InitialAccess</c>, <c>LateralMovement</c>, <c>Malware</c>, <c>Persistence</c>, <c>PrivilegeEscalation</c>, <c>Ransomware</c> or <c>SuspiciousActivity</c>.</summary>
	[JsonPropertyName("category")]
	public required string Category { get; set; }
}
