using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Ips;

/// <summary>How often devices in the organisation connected to an IP address over the look-back window.</summary>
public class IpStatistics
{
	/// <summary>The IP address the statistics describe.</summary>
	[JsonPropertyName("ipAddress")]
	public string? IpAddress { get; set; }

	/// <summary>How many distinct devices connected to the address; zero when it is valid but unseen.</summary>
	[JsonPropertyName("organizationPrevalence")]
	public long? OrganizationPrevalence { get; set; }

	/// <summary>When the organisation first connected to the address.</summary>
	[JsonPropertyName("orgFirstSeen")]
	public DateTimeOffset? OrgFirstSeen { get; set; }

	/// <summary>When the organisation last connected to the address.</summary>
	[JsonPropertyName("orgLastSeen")]
	public DateTimeOffset? OrgLastSeen { get; set; }
}
