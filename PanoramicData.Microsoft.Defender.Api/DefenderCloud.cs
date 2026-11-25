namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>The Defender for Endpoint service hosts a tenant can be served from.</summary>
/// <remarks>
/// A regional host is recommended for performance when the tenant is in that region. The US Government hosts need a matching
/// token audience: set <see cref="DefenderClientOptions.Scope"/> to the one for that cloud.
/// </remarks>
public enum DefenderCloud
{
	/// <summary>The global host, <c>api.security.microsoft.com</c>.</summary>
	Global = 0,

	/// <summary>The United States regional host, <c>us.api.security.microsoft.com</c>.</summary>
	UnitedStates,

	/// <summary>The European Union regional host, <c>eu.api.security.microsoft.com</c>.</summary>
	Europe,

	/// <summary>The United Kingdom regional host, <c>uk.api.security.microsoft.com</c>.</summary>
	UnitedKingdom,

	/// <summary>The Australia regional host, <c>au.api.security.microsoft.com</c>.</summary>
	Australia,

	/// <summary>The Sweden regional host, <c>swa.api.security.microsoft.com</c>.</summary>
	Sweden,

	/// <summary>The India regional host, <c>ina.api.security.microsoft.com</c>.</summary>
	India,

	/// <summary>The United Arab Emirates regional host, <c>aea.api.security.microsoft.com</c>.</summary>
	UnitedArabEmirates,

	/// <summary>The US Government (GCC) host, <c>api-gcc.securitycenter.microsoft.us</c>.</summary>
	UsGovernmentGcc,

	/// <summary>The US Government (GCC High) host, <c>api-gov.securitycenter.microsoft.us</c>.</summary>
	UsGovernmentGccHigh
}
