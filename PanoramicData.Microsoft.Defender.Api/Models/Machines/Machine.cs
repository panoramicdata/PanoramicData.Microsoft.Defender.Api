using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>
/// A Defender device. This is a minimal stand-in written by the files, domains and users group so that its list
/// operations compile; the machines group owns this type and replaces it with the full entity.
/// </summary>
public class Machine
{
	/// <summary>The device id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The device's fully qualified DNS name.</summary>
	[JsonPropertyName("computerDnsName")]
	public string? ComputerDnsName { get; set; }

	/// <summary>The operating system platform.</summary>
	[JsonPropertyName("osPlatform")]
	public string? OsPlatform { get; set; }
}
