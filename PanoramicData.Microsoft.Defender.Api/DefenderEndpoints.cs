namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>The well-known base addresses and token audience of the Defender for Endpoint API.</summary>
public static class DefenderEndpoints
{
	/// <summary>
	/// The default token scope. Some of the API's operations still expect a token issued for this older audience and answer
	/// 403 for a token issued for another, even on the <c>api.security.microsoft.com</c> hosts.
	/// </summary>
	public static string DefaultScope => "https://api.securitycenter.microsoft.com/.default";

	/// <summary>The base address of the API in <paramref name="cloud"/>, with the <c>/api</c> prefix every operation path follows.</summary>
	/// <param name="cloud">The cloud or region.</param>
	/// <returns>An absolute <c>https</c> address ending in <c>/api</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="cloud"/> is not a defined value.</exception>
	public static Uri BaseUri(DefenderCloud cloud) => new(cloud switch
	{
		DefenderCloud.Global => "https://api.security.microsoft.com/api",
		DefenderCloud.UnitedStates => "https://us.api.security.microsoft.com/api",
		DefenderCloud.Europe => "https://eu.api.security.microsoft.com/api",
		DefenderCloud.UnitedKingdom => "https://uk.api.security.microsoft.com/api",
		DefenderCloud.Australia => "https://au.api.security.microsoft.com/api",
		DefenderCloud.Sweden => "https://swa.api.security.microsoft.com/api",
		DefenderCloud.India => "https://ina.api.security.microsoft.com/api",
		DefenderCloud.UnitedArabEmirates => "https://aea.api.security.microsoft.com/api",
		DefenderCloud.UsGovernmentGcc => "https://api-gcc.securitycenter.microsoft.us/api",
		DefenderCloud.UsGovernmentGccHigh => "https://api-gov.securitycenter.microsoft.us/api",
		_ => throw new ArgumentOutOfRangeException(nameof(cloud), cloud, "Unknown Defender cloud.")
	});
}
