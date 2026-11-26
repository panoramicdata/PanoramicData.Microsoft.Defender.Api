using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>The body that creates a scan definition. The request carries credentials: never log it.</summary>
public class CreateScanDefinitionRequest
{
	/// <summary>The kind of scan; <c>Network</c> is the only documented value.</summary>
	[JsonPropertyName("scanType")]
	public string ScanType { get; set; } = "Network";

	/// <summary>The scan's name.</summary>
	[JsonPropertyName("scanName")]
	public required string ScanName { get; set; }

	/// <summary>Whether the scan runs on its schedule.</summary>
	[JsonPropertyName("isActive")]
	public required bool IsActive { get; set; }

	/// <summary>The comma-separated IP addresses or host names to scan.</summary>
	[JsonPropertyName("target")]
	public required string Target { get; set; }

	/// <summary>The hours between runs.</summary>
	[JsonPropertyName("intervalInHours")]
	public required int IntervalInHours { get; set; }

	/// <summary>
	/// What <see cref="Target"/> holds. The documentation lists <c>IP Address</c> and <c>Hostname</c> but its examples send
	/// <c>Ip</c> (unverified which the service accepts).
	/// </summary>
	[JsonPropertyName("targetType")]
	public required string TargetType { get; set; }

	/// <summary>The device that runs the scan.</summary>
	[JsonPropertyName("scannerAgent")]
	public required ScanAgentReference ScannerAgent { get; set; }

	/// <summary>The credentials the scan authenticates with. Required when creating.</summary>
	[JsonPropertyName("scanAuthenticationParams")]
	public required SnmpAuthParams ScanAuthenticationParams { get; set; }
}
