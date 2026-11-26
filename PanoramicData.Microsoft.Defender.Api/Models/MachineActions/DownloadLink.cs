using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>A short-lived signed download link. The link is a credential: do not log it.</summary>
public class DownloadLink
{
	/// <summary>The signed URL.</summary>
	[JsonPropertyName("value")]
	public string? Value { get; set; }

	/// <summary>Describes the link without its signature.</summary>
	/// <returns>A fixed description.</returns>
	public override string ToString() => "DownloadLink";
}
