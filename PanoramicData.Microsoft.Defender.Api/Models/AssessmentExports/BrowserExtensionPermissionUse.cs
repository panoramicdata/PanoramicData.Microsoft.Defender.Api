using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One permission requested by an installed browser extension.</summary>
public class BrowserExtensionPermissionUse
{
	/// <summary>The permission's identifier, such as tabs or a URL pattern.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>Whether the extension requires the permission.</summary>
	[JsonPropertyName("isRequired")]
	public bool? IsRequired { get; set; }

	/// <summary>The risk level of the permission.</summary>
	[JsonPropertyName("risk")]
	public string? Risk { get; set; }
}
