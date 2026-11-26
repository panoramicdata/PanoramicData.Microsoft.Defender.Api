using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Investigations;

/// <summary>An automated investigation.</summary>
public class Investigation
{
	/// <summary>The investigation id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>When it started.</summary>
	[JsonPropertyName("startTime")]
	public DateTimeOffset? StartTime { get; set; }

	/// <summary>When it ended, if it has.</summary>
	[JsonPropertyName("endTime")]
	public DateTimeOffset? EndTime { get; set; }

	/// <summary>Who cancelled it, if anyone.</summary>
	[JsonPropertyName("cancelledBy")]
	public string? CancelledBy { get; set; }

	/// <summary>Its state, for example <c>Running</c>, <c>PendingApproval</c> or <c>SuccessfullyRemediated</c>. Roughly eighteen values exist and the set grows, so it is kept as text.</summary>
	[JsonPropertyName("state")]
	public string? State { get; set; }

	/// <summary>Extra detail about the state.</summary>
	[JsonPropertyName("statusDetails")]
	public string? StatusDetails { get; set; }

	/// <summary>The device investigated.</summary>
	[JsonPropertyName("machineId")]
	public string? MachineId { get; set; }

	/// <summary>The device fully qualified name.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>The alert that triggered the investigation.</summary>
	[JsonPropertyName("triggeringAlertId")]
	public string? TriggeringAlertId { get; set; }
}
