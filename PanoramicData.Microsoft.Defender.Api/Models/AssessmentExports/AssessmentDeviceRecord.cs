using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

/// <summary>The device columns every per-device vulnerability-management export record carries. Records are read case-insensitively, so the mixed spelling the service uses for these names (DeviceId or deviceId) does not matter.</summary>
public class AssessmentDeviceRecord
{
	/// <summary>The device's identifier.</summary>
	[JsonPropertyName("deviceId")]
	public string? DeviceId { get; set; }

	/// <summary>The device's fully qualified DNS name.</summary>
	[JsonPropertyName("deviceName")]
	public string? DeviceName { get; set; }

	/// <summary>The device group the device belongs to; the service reports Unassigned or None when there is no group.</summary>
	[JsonPropertyName("rbacGroupName")]
	public string? RbacGroupName { get; set; }

	/// <summary>The device group's identifier, kept as text because the service types it inconsistently.</summary>
	[JsonPropertyName("rbacGroupId")]
	public string? RbacGroupId { get; set; }
}
