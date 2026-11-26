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
			// URI parsing collapses a "." or ".." path segment, which would send the call to a different path (the parent
			// collection, for an id of ".."). Nothing legitimate is spelled this way, in a path or a query.
			"." or ".." => throw new ArgumentException("A value of '.' or '..' cannot be sent: it would change the request path.", nameof(parameterValue)),
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
