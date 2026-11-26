using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

/// <summary>
/// Reads a list of scanned targets that the documentation shows both as an array and as a single object (its example is
/// malformed), so either shape reads as a list. Any other value reads as an empty list.
/// </summary>
internal sealed class ScannedTargetListConverter : JsonConverter<IReadOnlyList<ScannedTarget>>
{
	public override IReadOnlyList<ScannedTarget> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.StartArray:
				// An array token never deserializes to null.
				return JsonSerializer.Deserialize<List<ScannedTarget>>(ref reader, options)!;
			case JsonTokenType.StartObject:
				// An object token never deserializes to null.
				return [JsonSerializer.Deserialize<ScannedTarget>(ref reader, options)!];
			default:
				reader.Skip();
				return [];
		}
	}

	public override void Write(Utf8JsonWriter writer, IReadOnlyList<ScannedTarget> value, JsonSerializerOptions options)
		=> JsonSerializer.Serialize(writer, value.ToList(), options);
}
