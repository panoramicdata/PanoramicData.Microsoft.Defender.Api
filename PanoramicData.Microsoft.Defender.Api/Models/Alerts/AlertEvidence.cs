using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>One piece of evidence (a file, process, user, address and so on) attached to an alert. Which properties are filled depends on <see cref="EntityType"/>.</summary>
public class AlertEvidence
{
	/// <summary>The kind of entity, such as <c>User</c>, <c>Process</c> or <c>File</c>.</summary>
	[JsonPropertyName("entityType")]
	public string? EntityType { get; set; }

	/// <summary>When the evidence was recorded.</summary>
	[JsonPropertyName("evidenceCreationTime")]
	public DateTimeOffset? EvidenceCreationTime { get; set; }

	/// <summary>The SHA-1 hash of the file.</summary>
	[JsonPropertyName("sha1")]
	public string? Sha1 { get; set; }

	/// <summary>The SHA-256 hash of the file.</summary>
	[JsonPropertyName("sha256")]
	public string? Sha256 { get; set; }

	/// <summary>The file name.</summary>
	[JsonPropertyName("fileName")]
	public string? FileName { get; set; }

	/// <summary>The folder holding the file.</summary>
	[JsonPropertyName("filePath")]
	public string? FilePath { get; set; }

	/// <summary>The process id.</summary>
	[JsonPropertyName("processId")]
	public long? ProcessId { get; set; }

	/// <summary>The process command line.</summary>
	[JsonPropertyName("processCommandLine")]
	public string? ProcessCommandLine { get; set; }

	/// <summary>When the process started.</summary>
	[JsonPropertyName("processCreationTime")]
	public DateTimeOffset? ProcessCreationTime { get; set; }

	/// <summary>The parent process id.</summary>
	[JsonPropertyName("parentProcessId")]
	public long? ParentProcessId { get; set; }

	/// <summary>When the parent process started.</summary>
	[JsonPropertyName("parentProcessCreationTime")]
	public DateTimeOffset? ParentProcessCreationTime { get; set; }

	/// <summary>The parent process file name.</summary>
	[JsonPropertyName("parentProcessFileName")]
	public string? ParentProcessFileName { get; set; }

	/// <summary>The folder holding the parent process file.</summary>
	[JsonPropertyName("parentProcessFilePath")]
	public string? ParentProcessFilePath { get; set; }

	/// <summary>An IP address.</summary>
	[JsonPropertyName("ipAddress")]
	public string? IpAddress { get; set; }

	/// <summary>A URL.</summary>
	[JsonPropertyName("url")]
	public string? Url { get; set; }

	/// <summary>A registry key.</summary>
	[JsonPropertyName("registryKey")]
	public string? RegistryKey { get; set; }

	/// <summary>A registry hive.</summary>
	[JsonPropertyName("registryHive")]
	public string? RegistryHive { get; set; }

	/// <summary>The type of a registry value.</summary>
	[JsonPropertyName("registryValueType")]
	public string? RegistryValueType { get; set; }

	/// <summary>A registry value.</summary>
	[JsonPropertyName("registryValue")]
	public string? RegistryValue { get; set; }

	/// <summary>An account name.</summary>
	[JsonPropertyName("accountName")]
	public string? AccountName { get; set; }

	/// <summary>An account domain.</summary>
	[JsonPropertyName("domainName")]
	public string? DomainName { get; set; }

	/// <summary>An account security identifier.</summary>
	[JsonPropertyName("userSid")]
	public string? UserSid { get; set; }

	/// <summary>An account Entra object id.</summary>
	[JsonPropertyName("aadUserId")]
	public string? AadUserId { get; set; }

	/// <summary>An account user principal name.</summary>
	[JsonPropertyName("userPrincipalName")]
	public string? UserPrincipalName { get; set; }

	/// <summary>What the detection did with the entity, such as <c>Detected</c>.</summary>
	[JsonPropertyName("detectionStatus")]
	public string? DetectionStatus { get; set; }
}
