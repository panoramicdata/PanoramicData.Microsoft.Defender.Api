using System.Text.Json.Serialization;
using PanoramicData.MicrosoftDefender.Api.Models.Files;

namespace PanoramicData.MicrosoftDefender.Api.Models.Alerts;

/// <summary>A file related to an alert (<c>GET alerts/{id}/files</c>): the file's profile plus its MD5 hash.</summary>
public class AlertFile : FileProfile
{
	/// <summary>The MD5 hash.</summary>
	[JsonPropertyName("md5")]
	public string? Md5 { get; set; }
}
