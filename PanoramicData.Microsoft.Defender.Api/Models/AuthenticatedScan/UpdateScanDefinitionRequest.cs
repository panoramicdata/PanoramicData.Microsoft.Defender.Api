using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>
/// The body that updates a scan definition. Every member is optional and omitted when <see langword="null"/>. The
/// documentation shows only a full example, so whether a partial body leaves the other fields alone is unverified. The
/// request may carry credentials: never log it.
/// </summary>
public class UpdateScanDefinitionRequest
{
	/// <summary>The kind of scan.</summary>
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

	/// <summary>The hours between runs.</summary>
	[JsonPropertyName("intervalInHours")]
	public int? IntervalInHours { get; set; }

	/// <summary>What <see cref="Target"/> holds.</summary>
	[JsonPropertyName("targetType")]
	public string? TargetType { get; set; }

	/// <summary>The device that runs the scan.</summary>
	[JsonPropertyName("scannerAgent")]
	public ScanAgentReference? ScannerAgent { get; set; }

	/// <summary>New credentials; optional when updating.</summary>
	[JsonPropertyName("scanAuthenticationParams")]
	public SnmpAuthParams? ScanAuthenticationParams { get; set; }
}
