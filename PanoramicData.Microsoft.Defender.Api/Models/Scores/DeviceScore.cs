using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Scores;

/// <summary>A point-in-time score: the exposure score or the secure score for devices, for the whole tenant or one device group.</summary>
public class DeviceScore
{
	/// <summary>The current score.</summary>
	[JsonPropertyName("score")]
	public double? Score { get; set; }

	/// <summary>When the score was calculated.</summary>
	[JsonPropertyName("time")]
	public DateTimeOffset? Time { get; set; }

	/// <summary>The device group the score is for; absent for the tenant-wide scores.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }

	/// <summary>The device group's id, when present.</summary>
	[JsonPropertyName("rbacGroupId")]
	public string? RbacGroupId { get; set; }
}
