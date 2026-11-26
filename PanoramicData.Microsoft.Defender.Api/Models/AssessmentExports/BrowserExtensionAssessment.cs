using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One browser extension installed on one device (a row per device, browser and extension).</summary>
public class BrowserExtensionAssessment : AssessmentDeviceRecord
{
	/// <summary>The browser the extension is installed in.</summary>
	[JsonPropertyName("browserName")]
	public string? BrowserName { get; set; }

	/// <summary>The extension's identifier.</summary>
	[JsonPropertyName("extensionId")]
	public string? ExtensionId { get; set; }

	/// <summary>The extension's name.</summary>
	[JsonPropertyName("extensionName")]
	public string? ExtensionName { get; set; }

	/// <summary>The extension's description.</summary>
	[JsonPropertyName("extensionDescription")]
	public string? ExtensionDescription { get; set; }

	/// <summary>The extension's version.</summary>
	[JsonPropertyName("extensionVersion")]
	public string? ExtensionVersion { get; set; }

	/// <summary>The highest risk level among its permissions: None, Low, Medium, High or Critical.</summary>
	[JsonPropertyName("extensionRisk")]
	public string? ExtensionRisk { get; set; }

	/// <summary>Whether the extension is active.</summary>
	[JsonPropertyName("isActivated")]
	public bool? IsActivated { get; set; }

	/// <summary>When the extension was installed.</summary>
	[JsonPropertyName("installationTime")]
	public DateTimeOffset? InstallationTime { get; set; }

	/// <summary>The permissions the extension requests.</summary>
	[JsonPropertyName("permissions")]
	public IReadOnlyList<BrowserExtensionPermissionUse> Permissions { get; set; } = [];
}
