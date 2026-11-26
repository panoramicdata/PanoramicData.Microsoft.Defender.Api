using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Domains;

/// <summary>How often devices in the organisation contacted a domain over the look-back window.</summary>
public class DomainStatistics
{
	/// <summary>The domain the statistics describe.</summary>
	[JsonPropertyName("host")]
	public string? Host { get; set; }

	/// <summary>How many devices contacted the domain; zero when the domain is unknown.</summary>
	[JsonPropertyName("organizationPrevalence")]
	public long? OrganizationPrevalence { get; set; }

	/// <summary>When the organisation first contacted the domain.</summary>
	[JsonPropertyName("orgFirstSeen")]
	public DateTimeOffset? OrgFirstSeen { get; set; }

	/// <summary>When the organisation last contacted the domain.</summary>
	[JsonPropertyName("orgLastSeen")]
	public DateTimeOffset? OrgLastSeen { get; set; }
}
