using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.BrowserExtensions;

/// <summary>A browser-extension permission and what it allows. The documented response nests a list of these inside an item's own value property, so that nesting is modelled too. Unverified.</summary>
public class BrowserExtensionPermissionInfo
{
	/// <summary>The permission's key, such as audioCapture.</summary>
	[JsonPropertyName("key")]
	public string? Key { get; set; }

	/// <summary>A short name for the permission.</summary>
	[JsonPropertyName("permissionName")]
	public string? PermissionName { get; set; }

	/// <summary>What the permission allows.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>Nested permissions, when the service wraps them in a further value list.</summary>
	[JsonPropertyName("value")]
	public IReadOnlyList<BrowserExtensionPermissionInfo> Value { get; set; } = [];
}
