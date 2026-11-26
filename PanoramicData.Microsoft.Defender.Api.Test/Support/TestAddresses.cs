namespace PanoramicData.MicrosoftDefender.Api.Test.Support;

/// <summary>
/// The IP addresses the tests send and expect, each defined once. All come from the ranges reserved for documentation
/// (RFC 5737), so none can address a real host.
/// </summary>
internal static class TestAddresses
{
	/// <summary>A device's internal address, as sent to the find-by-IP operations.</summary>
	public const string Device = "192.0.2.5";

	/// <summary>The address whose alerts and statistics the IPs group reads.</summary>
	public const string Statistics = "198.51.100.77";
}
