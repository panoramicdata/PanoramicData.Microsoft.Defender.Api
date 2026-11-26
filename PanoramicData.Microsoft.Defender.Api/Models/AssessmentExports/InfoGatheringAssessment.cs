using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>
/// One record of the information-gathering export. Its columns are not documented, so only the device id is modelled and every
/// other column is kept in <see cref="AdditionalProperties"/>. Unverified against a live tenant.
/// </summary>
public class InfoGatheringAssessment
{
	/// <summary>The device's identifier.</summary>
	[JsonPropertyName("deviceId")]
	public string? DeviceId { get; set; }

	/// <summary>Every other column of the record, as raw JSON.</summary>
	[JsonExtensionData]
	public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
