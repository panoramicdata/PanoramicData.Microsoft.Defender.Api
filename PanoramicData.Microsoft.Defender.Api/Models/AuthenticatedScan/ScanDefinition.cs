using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>An authenticated network scan: what to scan, how often, with which credentials and which agent.</summary>
public class ScanDefinition
{
	/// <summary>The definition id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The kind of scan; <c>Network</c> is the only documented value.</summary>
	[JsonPropertyName("scanType")]
	public string? ScanType { get; set; }

	/// <summary>The scan's name.</summary>
	[JsonPropertyName("scanName")]
	public string? ScanName { get; set; }

	/// <summary>Whether the scan runs on its schedule.</summary>
	[JsonPropertyName("isActive")]
	public bool? IsActive { get; set; }

	/// <summary>The comma-separated IP addresses or host names to scan.</summary>
	[JsonPropertyName("target")]
	public string? Target { get; set; }

	/// <summary>The organisation id.</summary>
	[JsonPropertyName("orgId")]
	public string? OrgId { get; set; }

	/// <summary>The hours between runs.</summary>
	[JsonPropertyName("intervalInHours")]
	public int? IntervalInHours { get; set; }

	/// <summary>The identity that created the definition.</summary>
	[JsonPropertyName("createdBy")]
	public string? CreatedBy { get; set; }

	/// <summary>
	/// What <see cref="Target"/> holds. The documentation says <c>IP Address</c> or <c>Hostname</c> but its examples show
	/// <c>Ip</c>, so this is kept as text.
	/// </summary>
	[JsonPropertyName("targetType")]
	public string? TargetType { get; set; }

	/// <summary>The credentials; normally <see langword="null"/> on read.</summary>
	[JsonPropertyName("scanAuthenticationParams")]
	public SnmpAuthParams? ScanAuthenticationParams { get; set; }

	/// <summary>The agent that runs the scan.</summary>
	[JsonPropertyName("scannerAgent")]
	public ScanAgent? ScannerAgent { get; set; }

	/// <summary>The most recent run.</summary>
	[JsonPropertyName("latestScan")]
	public LatestScan? LatestScan { get; set; }

	/// <summary>Further configuration the service returns that the documentation does not describe, as raw JSON; <see langword="null"/> when absent.</summary>
	[JsonPropertyName("advancedActiveConfiguration")]
	public JsonElement? AdvancedActiveConfiguration { get; set; }
}
