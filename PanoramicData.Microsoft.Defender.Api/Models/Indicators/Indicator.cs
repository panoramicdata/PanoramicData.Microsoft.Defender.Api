using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>A custom threat-intelligence indicator, as returned by the indicator operations.</summary>
public class Indicator
{
	/// <summary>The indicator id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The value matched (a hash, address, domain or URL).</summary>
	[JsonPropertyName("indicatorValue")]
	public string? IndicatorValue { get; set; }

	/// <summary>What kind of value it is.</summary>
	[JsonPropertyName("indicatorType")]
	public IndicatorType IndicatorType { get; set; }

	/// <summary>What Defender does on a match.</summary>
	[JsonPropertyName("action")]
	public IndicatorAction Action { get; set; }

	/// <summary>A friendly name shown in block notifications.</summary>
	[JsonPropertyName("application")]
	public string? Application { get; set; }

	/// <summary>A caller-supplied correlation id.</summary>
	[JsonPropertyName("externalID")]
	public string? ExternalId { get; set; }

	/// <summary>The user or application that submitted it.</summary>
	[JsonPropertyName("source")]
	public string? Source { get; set; }

	/// <summary><c>User</c> for portal-created indicators, <c>AadApp</c> for API-created ones.</summary>
	[JsonPropertyName("sourceType")]
	public string? SourceType { get; set; }

	/// <summary>The name of the submitter.</summary>
	[JsonPropertyName("createdBySource")]
	public string? CreatedBySource { get; set; }

	/// <summary>The identity of the submitter.</summary>
	[JsonPropertyName("createdBy")]
	public string? CreatedBy { get; set; }

	/// <summary>Who last changed it.</summary>
	[JsonPropertyName("lastUpdatedBy")]
	public string? LastUpdatedBy { get; set; }

	/// <summary>When it was created.</summary>
	[JsonPropertyName("creationTimeDateTimeUtc")]
	public DateTimeOffset? CreationTime { get; set; }

	/// <summary>When it expires, if it does.</summary>
	[JsonPropertyName("expirationTime")]
	public DateTimeOffset? ExpirationTime { get; set; }

	/// <summary>When it was last changed.</summary>
	[JsonPropertyName("lastUpdateTime")]
	public DateTimeOffset? LastUpdateTime { get; set; }

	/// <summary>The indicator severity.</summary>
	[JsonPropertyName("severity")]
	public IndicatorSeverity Severity { get; set; }

	/// <summary>The title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>Recommended follow-up text.</summary>
	[JsonPropertyName("recommendedActions")]
	public string? RecommendedActions { get; set; }

	/// <summary>Device groups it applies to; empty means all devices.</summary>
	[JsonPropertyName("rbacGroupNames")]
	public IReadOnlyList<string> RbacGroupNames { get; set; } = [];

	/// <summary>Device group ids it applies to; empty means all devices.</summary>
	[JsonPropertyName("rbacGroupIds")]
	public IReadOnlyList<string> RbacGroupIds { get; set; } = [];

	/// <summary>Whether a match raises an alert.</summary>
	[JsonPropertyName("generateAlert")]
	public bool? GenerateAlert { get; set; }

	/// <summary>The custom notification link, for block and warn actions on URLs.</summary>
	[JsonPropertyName("educateUrl")]
	public string? EducateUrl { get; set; }
}
