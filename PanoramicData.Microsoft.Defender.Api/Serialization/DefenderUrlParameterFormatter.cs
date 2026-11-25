using Refit;
using System.Globalization;

namespace PanoramicData.MicrosoftDefender.Api.Serialization;

/// <summary>
/// Formats query and path parameters: booleans as lowercase <c>true</c>/<c>false</c>, enums by their wire name, and dates as
/// UTC ISO 8601 text, all in the invariant culture.
/// </summary>
internal sealed class DefenderUrlParameterFormatter : DefaultUrlParameterFormatter
{
	/// <inheritdoc />
	public override string? Format(object? parameterValue, IParameterAttributeProvider attributeProvider, Type type)
		=> parameterValue switch
		{
			null => null,
			bool flag => flag ? "true" : "false",
			Enum value => WireNames.Of(value),
			DateTimeOffset date => FormatDate(date),
			DateTime date => FormatDate(new DateTimeOffset(date.ToUniversalTime())),
			_ => base.Format(parameterValue, attributeProvider, type)
		};

	/// <summary>A UTC timestamp with seconds, and fractional seconds only when present.</summary>
	internal static string FormatDate(DateTimeOffset date)
		=> date.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture);
}
