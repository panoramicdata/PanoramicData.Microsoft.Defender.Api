using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Serialization;

/// <summary>The shared parsing rule behind the tolerant date converters.</summary>
internal static class TolerantDates
{
	/// <summary>
	/// Reads an ISO 8601 timestamp from a JSON string (a value with no offset is taken as UTC). JSON <c>null</c>, an empty
	/// string and text that is not a date all read as <see langword="null"/>; any other token is an error.
	/// </summary>
	public static DateTimeOffset? Read(ref Utf8JsonReader reader)
		=> reader.TokenType switch
		{
			JsonTokenType.Null => null,
			JsonTokenType.String => Parse(reader.GetString()),
			_ => throw new JsonException($"Cannot read a date from a JSON {reader.TokenType}.")
		};

	private static DateTimeOffset? Parse(string? text)
		=> DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var value)
			? value
			: null;

	/// <summary>Writes a timestamp as round-trip ISO 8601 text.</summary>
	public static void Write(Utf8JsonWriter writer, DateTimeOffset value)
		=> writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
}

/// <summary>
/// Reads a <see cref="DateTimeOffset"/> tolerantly (see <see cref="TolerantDates"/>); a missing or unreadable value reads
/// as the default value. Writes ISO 8601 text.
/// </summary>
internal sealed class TolerantDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	/// <inheritdoc />
	public override bool HandleNull => true;

	/// <inheritdoc />
	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> TolerantDates.Read(ref reader) ?? default;

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
		=> TolerantDates.Write(writer, value);
}

/// <summary>Reads a nullable <see cref="DateTimeOffset"/> tolerantly (see <see cref="TolerantDates"/>).</summary>
internal sealed class TolerantNullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
	/// <inheritdoc />
	public override bool HandleNull => true;

	/// <inheritdoc />
	public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> TolerantDates.Read(ref reader);

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
	{
		if (value is { } date)
		{
			TolerantDates.Write(writer, date);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}
