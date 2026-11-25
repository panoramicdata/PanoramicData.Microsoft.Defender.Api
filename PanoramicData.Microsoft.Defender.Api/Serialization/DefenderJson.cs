using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PanoramicData.MicrosoftDefender.Api.Serialization;

/// <summary>Shared <see cref="JsonSerializerOptions"/> for Defender for Endpoint payloads.</summary>
/// <remarks>
/// Responses use camelCase names, while request body property names are case-sensitive on the server, so no naming policy
/// is applied: every model names each property explicitly with <see cref="JsonPropertyNameAttribute"/>. Reading is tolerant:
/// numbers and booleans may arrive as strings, enums with an unrecognised name read as their default (<c>Unknown</c>) member,
/// timestamps are read leniently, and a JSON <c>null</c> for a collection keeps the property's initial value.
/// </remarks>
public static class DefenderJson
{
	/// <summary>
	/// The options used by <see cref="DefenderClient"/>. They are read-only: copy them
	/// (<c>new JsonSerializerOptions(DefenderJson.Options)</c>) to customise.
	/// </summary>
	public static JsonSerializerOptions Options { get; } = Create();

	private static JsonSerializerOptions Create()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
			TypeInfoResolver = new DefaultJsonTypeInfoResolver
			{
				Modifiers = { CollectionDefaults.KeepOnNull }
			},
		};
		options.Converters.Add(new TolerantBooleanConverter());
		options.Converters.Add(new TolerantNullableBooleanConverter());
		options.Converters.Add(new TolerantStringConverter());
		options.Converters.Add(new TolerantInt32Converter());
		options.Converters.Add(new TolerantNullableInt32Converter());
		options.Converters.Add(new TolerantInt64Converter());
		options.Converters.Add(new TolerantNullableInt64Converter());
		options.Converters.Add(new TolerantDoubleConverter());
		options.Converters.Add(new TolerantNullableDoubleConverter());
		options.Converters.Add(new TolerantDateTimeOffsetConverter());
		options.Converters.Add(new TolerantNullableDateTimeOffsetConverter());
		options.Converters.Add(new TolerantEnumConverterFactory());
		// Shared and static: lock it so no caller can change serialization for every client.
		options.MakeReadOnly();
		return options;
	}
}
