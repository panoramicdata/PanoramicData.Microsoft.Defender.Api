using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>
/// An indicator to create or update (<c>POST indicators</c>, and each item of <c>POST indicators/import</c>). The
/// service matches on <see cref="IndicatorValue"/> and <see cref="IndicatorType"/>, so submitting an existing pair updates it.
/// </summary>
public class IndicatorSubmission
{
	/// <summary>The value to match.</summary>
	[JsonPropertyName("indicatorValue")]
	public required string IndicatorValue { get; set; }

	/// <summary>What kind of value it is.</summary>
	[JsonPropertyName("indicatorType")]
	public required IndicatorType IndicatorType { get; set; }

	/// <summary>What to do on a match. <see cref="IndicatorAction.Audit"/> requires <see cref="GenerateAlert"/> to be <see langword="true"/>.</summary>
	[JsonPropertyName("action")]
	public required IndicatorAction Action { get; set; }

	/// <summary>The alert title.</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The indicator description.</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>A friendly name shown in block notifications instead of the file or domain. Only honoured for new indicators.</summary>
	[JsonPropertyName("application")]
	public string? Application { get; set; }

	/// <summary>When the indicator expires; none means it never does.</summary>
	[JsonPropertyName("expirationTime")]
	public DateTimeOffset? ExpirationTime { get; set; }

	/// <summary>The severity.</summary>
	[JsonPropertyName("severity")]
	public IndicatorSeverity? Severity { get; set; }

	/// <summary>Recommended follow-up text for the alert.</summary>
	[JsonPropertyName("recommendedActions")]
	public string? RecommendedActions { get; set; }

	/// <summary>The device groups the indicator applies to; none means all devices.</summary>
	[JsonPropertyName("rbacGroupNames")]
	public IReadOnlyList<string>? RbacGroupNames { get; set; }

	/// <summary>A custom notification link, for block and warn actions on URL indicators.</summary>
	[JsonPropertyName("educateUrl")]
	public string? EducateUrl { get; set; }

	/// <summary>Whether a match raises an alert.</summary>
	[JsonPropertyName("generateAlert")]
	public bool? GenerateAlert { get; set; }
}
