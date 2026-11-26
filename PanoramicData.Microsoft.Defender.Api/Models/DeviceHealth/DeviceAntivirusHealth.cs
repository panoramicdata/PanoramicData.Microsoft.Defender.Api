using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.DeviceHealth;

/// <summary>The Microsoft Defender Antivirus health of one device. Unverified against a live tenant: the documentation names two different paths for this report.</summary>
public class DeviceAntivirusHealth
{
	/// <summary>The record's id (the device GUID).</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The device id.</summary>
	[JsonPropertyName("machineId")]
	public string? MachineId { get; set; }

	/// <summary>The device's DNS name.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>The operating system kind: windows, mac or linux.</summary>
	[JsonPropertyName("osKind")]
	public string? OsKind { get; set; }

	/// <summary>The operating system platform.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }

	/// <summary>The operating system version.</summary>
	[JsonPropertyName("osVersion")]
	public string? OsVersion { get; set; }

	/// <summary>The antivirus mode as a string-typed number: empty or 3 is other, 0 active, 1 passive, 2 disabled, 4 EDR blocked, 5 passive audit.</summary>
	[JsonPropertyName("avMode")]
	public string? AvMode { get; set; }

	/// <summary>The security intelligence version.</summary>
	[JsonPropertyName("avSignatureVersion")]
	public string? AvSignatureVersion { get; set; }

	/// <summary>The antivirus engine version.</summary>
	[JsonPropertyName("avEngineVersion")]
	public string? AvEngineVersion { get; set; }

	/// <summary>The antivirus platform version.</summary>
	[JsonPropertyName("avPlatformVersion")]
	public string? AvPlatformVersion { get; set; }

	/// <summary>Whether the signatures are current: True, False or Unknown.</summary>
	[JsonPropertyName("avIsSignatureUpToDate")]
	public string? AvIsSignatureUpToDate { get; set; }

	/// <summary>Whether the engine is current: True, False or Unknown.</summary>
	[JsonPropertyName("avIsEngineUpToDate")]
	public string? AvIsEngineUpToDate { get; set; }

	/// <summary>Whether the platform is current: True, False or Unknown.</summary>
	[JsonPropertyName("avIsPlatformUpToDate")]
	public string? AvIsPlatformUpToDate { get; set; }

	/// <summary>When the security intelligence build was released.</summary>
	[JsonPropertyName("avSignaturePublishTime")]
	public DateTimeOffset? AvSignaturePublishTime { get; set; }

	/// <summary>When the device last updated its security intelligence.</summary>
	[JsonPropertyName("avSignatureUpdateTime")]
	public DateTimeOffset? AvSignatureUpdateTime { get; set; }

	/// <summary>When the engine was last updated on the device.</summary>
	[JsonPropertyName("avEngineUpdateTime")]
	public DateTimeOffset? AvEngineUpdateTime { get; set; }

	/// <summary>When the platform was last updated on the device.</summary>
	[JsonPropertyName("avPlatformUpdateTime")]
	public DateTimeOffset? AvPlatformUpdateTime { get; set; }

	/// <summary>When the device was last seen.</summary>
	[JsonPropertyName("lastSeenTime")]
	public DateTimeOffset? LastSeenTime { get; set; }

	/// <summary>When the report data was refreshed.</summary>
	[JsonPropertyName("dataRefreshTimestamp")]
	public DateTimeOffset? DataRefreshTimestamp { get; set; }

	/// <summary>The last quick scan's result: Completed, Canceled or Failed.</summary>
	[JsonPropertyName("quickScanResult")]
	public string? QuickScanResult { get; set; }

	/// <summary>The last quick scan's error code.</summary>
	[JsonPropertyName("quickScanError")]
	public string? QuickScanError { get; set; }

	/// <summary>When the last quick scan finished.</summary>
	[JsonPropertyName("quickScanTime")]
	public DateTimeOffset? QuickScanTime { get; set; }

	/// <summary>The last full scan's result: Completed, Canceled or Failed.</summary>
	[JsonPropertyName("fullScanResult")]
	public string? FullScanResult { get; set; }

	/// <summary>The last full scan's error code.</summary>
	[JsonPropertyName("fullScanError")]
	public string? FullScanError { get; set; }

	/// <summary>When the last full scan finished.</summary>
	[JsonPropertyName("fullScanTime")]
	public DateTimeOffset? FullScanTime { get; set; }

	/// <summary>The device group's name.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }

	/// <summary>The device group's id.</summary>
	[JsonPropertyName("rbacGroupId")]
	public string? RbacGroupId { get; set; }
}
