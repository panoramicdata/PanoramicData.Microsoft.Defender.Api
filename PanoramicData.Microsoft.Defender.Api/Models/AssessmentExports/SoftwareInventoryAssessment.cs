using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>One software product installed on one device (a row per device, vendor, name and version).</summary>
public class SoftwareInventoryAssessment : AssessmentSoftwareRecord
{
	/// <summary>How many weaknesses the product has on this device.</summary>
	[JsonPropertyName("numberOfWeaknesses")]
	public long? NumberOfWeaknesses { get; set; }

	/// <summary>The end-of-support state, such as None, EOS Version, Upcoming EOS Version, EOS Software or Upcoming EOS Software.</summary>
	[JsonPropertyName("endOfSupportStatus")]
	public string? EndOfSupportStatus { get; set; }

	/// <summary>When support for the product ended or will end.</summary>
	[JsonPropertyName("endOfSupportDate")]
	public DateTimeOffset? EndOfSupportDate { get; set; }

	/// <summary>When the product was first seen on the device.</summary>
	[JsonPropertyName("softwareFirstSeenTimestamp")]
	public DateTimeOffset? SoftwareFirstSeenTimestamp { get; set; }
}
