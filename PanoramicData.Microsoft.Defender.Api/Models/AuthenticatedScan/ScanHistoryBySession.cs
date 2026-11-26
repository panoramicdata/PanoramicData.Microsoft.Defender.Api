using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>One scan session and how each of its targets fared, from a history query by session.</summary>
public class ScanHistoryBySession
{
	/// <summary>The organisation id.</summary>
	[JsonPropertyName("orgId")]
	public string? OrgId { get; set; }

	/// <summary>The id of the definition the session ran (the service names this member in the plural but sends one id).</summary>
	[JsonPropertyName("ScanDefinitionIds")]
	public string? ScanDefinitionIds { get; set; }

	/// <summary>The session id (the service names this member in the plural but sends one id).</summary>
	[JsonPropertyName("SessionIds")]
	public string? SessionIds { get; set; }

	/// <summary>How many targets were scanned successfully.</summary>
	[JsonPropertyName("NumberOfSuccessfullyScannedTargets")]
	public int? NumberOfSuccessfullyScannedTargets { get; set; }

	/// <summary>How many targets the session had.</summary>
	[JsonPropertyName("NumberOfTargets")]
	public int? NumberOfTargets { get; set; }

	/// <summary>The session's outcome label. An open set, so kept as text.</summary>
	[JsonPropertyName("ScanStatus")]
	public string? ScanStatus { get; set; }

	/// <summary>When the session last scanned.</summary>
	[JsonPropertyName("LastScanned")]
	public DateTimeOffset? LastScanned { get; set; }

	/// <summary>
	/// The per-target results. The documentation's example is malformed (it shows an object holding several), so a single
	/// object and an array both read as a list.
	/// </summary>
	[JsonPropertyName("ListScannedTargets")]
	[JsonConverter(typeof(ScannedTargetListConverter))]
	public IReadOnlyList<ScannedTarget> ListScannedTargets { get; set; } = [];
}
