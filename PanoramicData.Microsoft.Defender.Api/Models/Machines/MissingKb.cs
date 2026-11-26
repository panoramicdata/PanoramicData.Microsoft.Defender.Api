using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>A security update (KB) a device is missing.</summary>
public class MissingKb : MissingKbBase
{
	/// <summary>The operating system build the update applies to.</summary>
	[JsonPropertyName("osBuild")]
	public long? OsBuild { get; set; }
}
