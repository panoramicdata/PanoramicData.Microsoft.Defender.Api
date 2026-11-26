using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>An action requested on a device, such as an isolation or a scan, and how far it has got.</summary>
public class MachineAction
{
	/// <summary>The action id (a GUID).</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The kind of action, such as <c>Isolate</c> or <c>RunAntiVirusScan</c>. Kept as text because the set is open.</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>The scope of the action, such as the isolation type.</summary>
	[JsonPropertyName("scope")]
	public string? Scope { get; set; }

	/// <summary>Who requested the action.</summary>
	[JsonPropertyName("requestor")]
	public string? Requestor { get; set; }

	/// <summary>An external id attached to the action.</summary>
	[JsonPropertyName("externalID")]
	public string? ExternalId { get; set; }

	/// <summary>The source the request came from.</summary>
	[JsonPropertyName("requestSource")]
	public string? RequestSource { get; set; }

	/// <summary>The commands of a live response session.</summary>
	[JsonPropertyName("commands")]
	public IReadOnlyList<string> Commands { get; set; } = [];

	/// <summary>Who requested the cancellation, if it was cancelled.</summary>
	[JsonPropertyName("cancellationRequestor")]
	public string? CancellationRequestor { get; set; }

	/// <summary>The comment given when the action was requested.</summary>
	[JsonPropertyName("requestorComment")]
	public string? RequestorComment { get; set; }

	/// <summary>The comment given when the action was cancelled.</summary>
	[JsonPropertyName("cancellationComment")]
	public string? CancellationComment { get; set; }

	/// <summary>The action's state.</summary>
	[JsonPropertyName("status")]
	public MachineActionStatus Status { get; set; }

	/// <summary>The id of the device the action targets.</summary>
	[JsonPropertyName("machineId")]
	public string? MachineId { get; set; }

	/// <summary>The DNS name of the device the action targets.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>When the action was created.</summary>
	[JsonPropertyName("creationDateTimeUtc")]
	public DateTimeOffset? CreationDateTimeUtc { get; set; }

	/// <summary>When the cancellation was requested.</summary>
	[JsonPropertyName("cancellationDateTimeUtc")]
	public DateTimeOffset? CancellationDateTimeUtc { get; set; }

	/// <summary>When the action was last updated.</summary>
	[JsonPropertyName("lastUpdateDateTimeUtc")]
	public DateTimeOffset? LastUpdateDateTimeUtc { get; set; }

	/// <summary>The action's title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The file the action concerns, for file actions.</summary>
	[JsonPropertyName("relatedFileInfo")]
	public MachineActionFileInfo? RelatedFileInfo { get; set; }
}
