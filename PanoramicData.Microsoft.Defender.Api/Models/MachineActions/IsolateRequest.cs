using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

/// <summary>The body that isolates a device. Property names are case-sensitive on the server.</summary>
public class IsolateRequest
{
	/// <summary>Why the device is isolated.</summary>
	[JsonPropertyName("Comment")]
	public required string Comment { get; set; }

	/// <summary>The kind of isolation; omitted (so the service decides) when <see langword="null"/>.</summary>
	[JsonPropertyName("IsolationType")]
	public IsolationType? IsolationType { get; set; }
}
