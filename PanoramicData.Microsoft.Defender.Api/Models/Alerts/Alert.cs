using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>
/// An alert raised by Defender. This is a minimal stand-in written by the machines group so that
/// <c>machines/{id}/alerts</c> has a result type; the alerts group owns this type and may replace it.
/// </summary>
public class Alert
{
	/// <summary>The alert id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The id of the incident the alert belongs to.</summary>
	[JsonPropertyName("incidentId")]
	public long? IncidentId { get; set; }

	/// <summary>The id of the investigation started for the alert.</summary>
	[JsonPropertyName("investigationId")]
	public long? InvestigationId { get; set; }

	/// <summary>Who the alert is assigned to.</summary>
	[JsonPropertyName("assignedTo")]
	public string? AssignedTo { get; set; }

	/// <summary>The severity, such as <c>High</c>.</summary>
	[JsonPropertyName("severity")]
	public string? Severity { get; set; }

	/// <summary>The status, such as <c>New</c>.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The classification, such as <c>TruePositive</c>.</summary>
	[JsonPropertyName("classification")]
	public string? Classification { get; set; }

	/// <summary>The determination.</summary>
	[JsonPropertyName("determination")]
	public string? Determination { get; set; }

	/// <summary>The state of the automated investigation.</summary>
	[JsonPropertyName("investigationState")]
	public string? InvestigationState { get; set; }

	/// <summary>The detection source.</summary>
	[JsonPropertyName("detectionSource")]
	public string? DetectionSource { get; set; }

	/// <summary>The alert category.</summary>
	[JsonPropertyName("category")]
	public string? Category { get; set; }

	/// <summary>The threat family name.</summary>
	[JsonPropertyName("threatFamilyName")]
	public string? ThreatFamilyName { get; set; }

	/// <summary>The alert title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The alert description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>When the alert was created.</summary>
	[JsonPropertyName("alertCreationTime")]
	public DateTimeOffset? AlertCreationTime { get; set; }

	/// <summary>When the first related event happened.</summary>
	[JsonPropertyName("firstEventTime")]
	public DateTimeOffset? FirstEventTime { get; set; }

	/// <summary>When the last related event happened.</summary>
	[JsonPropertyName("lastEventTime")]
	public DateTimeOffset? LastEventTime { get; set; }

	/// <summary>When the alert was last updated.</summary>
	[JsonPropertyName("lastUpdateTime")]
	public DateTimeOffset? LastUpdateTime { get; set; }

	/// <summary>When the alert was resolved.</summary>
	[JsonPropertyName("resolvedTime")]
	public DateTimeOffset? ResolvedTime { get; set; }

	/// <summary>The id of the device the alert concerns.</summary>
	[JsonPropertyName("machineId")]
	public string? MachineId { get; set; }

	/// <summary>The DNS name of the device the alert concerns.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>The name of the device's group.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }

	/// <summary>The MITRE techniques the alert maps to.</summary>
	[JsonPropertyName("mitreTechniques")]
	public IReadOnlyList<string> MitreTechniques { get; set; } = [];
}
