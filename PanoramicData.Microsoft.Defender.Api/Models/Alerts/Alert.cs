using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>A security alert raised on a device, as returned by the alerts operations.</summary>
public class Alert
{
	/// <summary>The alert identifier, for example <c>da637472900382838869_1364969609</c>.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The incident the alert belongs to.</summary>
	[JsonPropertyName("incidentId")]
	public long? IncidentId { get; set; }

	/// <summary>The automated investigation linked to the alert, if any.</summary>
	[JsonPropertyName("investigationId")]
	public long? InvestigationId { get; set; }

	/// <summary>Who the alert is assigned to.</summary>
	[JsonPropertyName("assignedTo")]
	public string? AssignedTo { get; set; }

	/// <summary>How severe the alert is.</summary>
	[JsonPropertyName("severity")]
	public AlertSeverity Severity { get; set; }

	/// <summary>Where the alert is in its life cycle.</summary>
	[JsonPropertyName("status")]
	public AlertStatus Status { get; set; }

	/// <summary>The analyst verdict (<c>TruePositive</c>, <c>FalsePositive</c> or an informational value), kept as text because the service spells one value inconsistently.</summary>
	[JsonPropertyName("classification")]
	public string? Classification { get; set; }

	/// <summary>The finer verdict within the classification, such as <c>Malware</c> or <c>SecurityTesting</c>. An open set, so kept as text.</summary>
	[JsonPropertyName("determination")]
	public string? Determination { get; set; }

	/// <summary>The state of the linked automated investigation, with the same values as <c>Investigation.State</c>.</summary>
	[JsonPropertyName("investigationState")]
	public string? InvestigationState { get; set; }

	/// <summary>Which Defender component detected the activity.</summary>
	[JsonPropertyName("detectionSource")]
	public string? DetectionSource { get; set; }

	/// <summary>The identifier of the detector that raised the alert.</summary>
	[JsonPropertyName("detectorId")]
	public string? DetectorId { get; set; }

	/// <summary>The alert category, such as <c>Execution</c> or <c>Exploit</c>.</summary>
	[JsonPropertyName("category")]
	public string? Category { get; set; }

	/// <summary>The threat family, when known.</summary>
	[JsonPropertyName("threatFamilyName")]
	public string? ThreatFamilyName { get; set; }

	/// <summary>The threat name, when known.</summary>
	[JsonPropertyName("threatName")]
	public string? ThreatName { get; set; }

	/// <summary>The alert title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The alert description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>When the alert was created (UTC).</summary>
	[JsonPropertyName("alertCreationTime")]
	public DateTimeOffset? AlertCreationTime { get; set; }

	/// <summary>When the triggering activity was first seen on the device.</summary>
	[JsonPropertyName("firstEventTime")]
	public DateTimeOffset? FirstEventTime { get; set; }

	/// <summary>When the triggering activity was last seen on the device.</summary>
	[JsonPropertyName("lastEventTime")]
	public DateTimeOffset? LastEventTime { get; set; }

	/// <summary>When the alert was last changed.</summary>
	[JsonPropertyName("lastUpdateTime")]
	public DateTimeOffset? LastUpdateTime { get; set; }

	/// <summary>When the alert was resolved, if it was.</summary>
	[JsonPropertyName("resolvedTime")]
	public DateTimeOffset? ResolvedTime { get; set; }

	/// <summary>The device the alert concerns.</summary>
	[JsonPropertyName("machineId")]
	public string? MachineId { get; set; }

	/// <summary>The device fully qualified name.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>The device group the device belongs to.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }

	/// <summary>The Entra tenant id.</summary>
	[JsonPropertyName("aadTenantId")]
	public string? AadTenantId { get; set; }

	/// <summary>MITRE ATT&amp;CK technique ids associated with the alert.</summary>
	[JsonPropertyName("mitreTechniques")]
	public IReadOnlyList<string> MitreTechniques { get; set; } = [];

	/// <summary>The user the alert relates to, if any.</summary>
	[JsonPropertyName("relatedUser")]
	public AlertRelatedUser? RelatedUser { get; set; }

	/// <summary>Analyst comments on the alert.</summary>
	[JsonPropertyName("comments")]
	public IReadOnlyList<AlertComment> Comments { get; set; } = [];

	/// <summary>The evidence behind the alert. Filled for single-alert reads and when a list asked for <c>$expand=evidence</c>.</summary>
	[JsonPropertyName("evidence")]
	public IReadOnlyList<AlertEvidence> Evidence { get; set; } = [];
}
