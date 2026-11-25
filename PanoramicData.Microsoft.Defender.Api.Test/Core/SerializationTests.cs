using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using PanoramicData.MicrosoftDefender.Api.Serialization;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using Refit;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

/// <summary>The enum and date converters, the content serializer, the URL formatter and the shared models' defaults.</summary>
public class SerializationTests
{
	public enum Colour
	{
		Unknown = 0,

		[JsonStringEnumMemberName("light-red")]
		Red = 1,

		Green = 2,

#pragma warning disable CA1069 // A deliberate alias: WireNames writes the first member's name and reads both.
		[JsonStringEnumMemberName("verde")]
		Verde = 2
#pragma warning restore CA1069
	}

	public enum Small : byte
	{
		Unknown = 0,
		One = 1
	}

	public enum Big : long
	{
		Unknown = 0,
		Huge = 1L << 40
	}

	private sealed class Named
	{
		[JsonPropertyName("wire_name")]
		public string? Renamed { get; init; }

		public string? Plain { get; init; }
	}

	private sealed class Lists
	{
		[JsonPropertyName("list")]
		public IReadOnlyList<string> List { get; init; } = [];

		[JsonPropertyName("array")]
		public string[] Array { get; init; } = [];

		[JsonPropertyName("map")]
		public IReadOnlyDictionary<string, string> Map { get; init; } = new Dictionary<string, string>();

		[JsonPropertyName("unset")]
		public IReadOnlyList<string>? Unset { get; init; }

		[JsonPropertyName("text")]
		public string? Text { get; init; } = "initial";
	}

	private static T ReadEnum<T>(string json) where T : struct, Enum
		=> JsonSerializer.Deserialize<T>(json, DefenderJson.Options);

	[Theory]
	[InlineData("\"light-red\"", Colour.Red)]
	[InlineData("\"LIGHT-RED\"", Colour.Red)]
	[InlineData("\"Green\"", Colour.Green)]
	[InlineData("\"verde\"", Colour.Green)]
	[InlineData("\"Red\"", Colour.Unknown)]
	[InlineData("\"purple\"", Colour.Unknown)]
	[InlineData("1", Colour.Red)]
	[InlineData("7", Colour.Unknown)]
	[InlineData("1.5", Colour.Unknown)]
	[InlineData("null", Colour.Unknown)]
	public void Enum_ReadsWireNamesAndNumbers(string json, Colour expected)
		=> ReadEnum<Colour>(json).Should().Be(expected);

	[Theory]
	[InlineData("1", Small.One)]
	[InlineData("257", Small.Unknown)]
	[InlineData("-1", Small.Unknown)]
	public void Enum_ReadsNumbersForAByteEnum(string json, Small expected)
		=> ReadEnum<Small>(json).Should().Be(expected);

	[Fact]
	public void Enum_ReadsNumbersForALongEnum()
		=> ReadEnum<Big>("1099511627776").Should().Be(Big.Huge);

	[Fact]
	public void Enum_ReadsNullableEnums()
	{
		JsonSerializer.Deserialize<Colour?>("null", DefenderJson.Options).Should().BeNull();
		JsonSerializer.Deserialize<VulnerabilitySeverity?>("\"High\"", DefenderJson.Options).Should().Be(VulnerabilitySeverity.High);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("{}")]
	public void Enum_OtherTokens_Throw(string json)
	{
		var act = () => ReadEnum<Colour>(json);

		act.Should().Throw<JsonException>().WithMessage("*Cannot read Colour from a JSON*");
	}

	[Theory]
	[InlineData(Colour.Red, "\"light-red\"")]
	[InlineData(Colour.Green, "\"Green\"")]
	[InlineData((Colour)9, "\"9\"")]
	public void Enum_WritesTheWireName(Colour value, string json)
		=> JsonSerializer.Serialize(value, DefenderJson.Options).Should().Be(json);

	[Fact]
	public void WireNames_UndefinedValuesAreNumbers_AndUnknownNamesTheDefault()
	{
		WireNames.Of((Small)200).Should().Be("200");
		WireNames.Of(VulnerabilitySeverity.Critical).Should().Be("Critical");
		WireNames.Parse<VulnerabilitySeverity>("MEDIUM").Should().Be(VulnerabilitySeverity.Medium);
		WireNames.Parse<VulnerabilitySeverity>("nope").Should().Be(VulnerabilitySeverity.Unknown);
	}

	[Theory]
	[InlineData("\"2024-07-30T00:00:00Z\"", 2024, 7, 30, 0)]
	[InlineData("\"2024-07-31T01:55:47Z\"", 2024, 7, 31, 1)]
	[InlineData("\"2024-07-31T03:55:47+02:00\"", 2024, 7, 31, 1)]
	[InlineData("\"2024-07-31T01:55:47\"", 2024, 7, 31, 1)]
	public void Date_ReadsIsoText_AsUtc(string json, int year, int month, int day, int hour)
	{
		var expected = new DateTimeOffset(year, month, day, hour, 0, 0, TimeSpan.Zero);

		Json.Read(new TolerantNullableDateTimeOffsetConverter(), json)!.Value.UtcDateTime.Date.Should().Be(expected.UtcDateTime.Date);
		Json.Read(new TolerantDateTimeOffsetConverter(), json).UtcDateTime.Hour.Should().Be(hour);
		Json.Read(new TolerantDateTimeOffsetConverter(), json).Offset.Should().Be(TimeSpan.Zero);
	}

	[Theory]
	[InlineData("null")]
	[InlineData("\"\"")]
	[InlineData("\"not a date\"")]
	public void Date_NullEmptyOrUnreadableText_IsDefault_OrNullWhenNullable(string json)
	{
		Json.Read(new TolerantDateTimeOffsetConverter(), json).Should().Be(default);
		Json.Read(new TolerantNullableDateTimeOffsetConverter(), json).Should().BeNull();
	}

	[Theory]
	[InlineData("12")]
	[InlineData("true")]
	[InlineData("{}")]
	public void Date_OtherTokens_Throw(string json)
	{
		var act = () => Json.Read(new TolerantNullableDateTimeOffsetConverter(), json);

		act.Should().Throw<JsonException>().WithMessage("Cannot read a date from a JSON*");
	}

	[Fact]
	public void Date_WritesRoundTripText_OrNull()
	{
		var date = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
		// The serializer escapes the plus sign of the offset as a six-character escape sequence.
		var plus = new string([(char)92, 'u', '0', '0', '2', 'B']);
		var expected = $"\"2026-01-02T03:04:05.0000000{plus}00:00\"";

		Json.Write(new TolerantDateTimeOffsetConverter(), date).Should().Be(expected);
		Json.Write<DateTimeOffset?>(new TolerantNullableDateTimeOffsetConverter(), date).Should().Be(expected);
		Json.Write<DateTimeOffset?>(new TolerantNullableDateTimeOffsetConverter(), null).Should().Be("null");
	}

	[Fact]
	public async Task ToHttpContent_IsBufferedJson()
	{
		using var content = new DefenderContentSerializer().ToHttpContent(new Named { Renamed = "x", Plain = "y" });

		content.Should().BeOfType<StringContent>();
		content.Headers.ContentType!.ToString().Should().Be("application/json");
		(await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("""{"wire_name":"x","Plain":"y"}""");
	}

	[Fact]
	public void ToHttpContent_RequiresABody()
	{
		var act = () => new DefenderContentSerializer().ToHttpContent<object?>(null);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public async Task FromHttpContent_ReadsWithDefenderJsonOptions()
	{
		using var content = new StringContent("""{"WIRE_NAME":"x","plain":5}""", Encoding.UTF8, "application/json");

		var value = await new DefenderContentSerializer().FromHttpContentAsync<Named>(content, TestContext.Current.CancellationToken);

		value!.Renamed.Should().Be("x");
		value.Plain.Should().Be("5");
	}

	[Theory]
	[InlineData(nameof(Named.Renamed), "wire_name")]
	[InlineData(nameof(Named.Plain), null)]
	public void GetFieldNameForProperty_UsesJsonPropertyName(string property, string? expected)
		=> new DefenderContentSerializer().GetFieldNameForProperty(typeof(Named).GetProperty(property)!).Should().Be(expected);

	public static TheoryData<object?, string?> UrlValues => new()
	{
		{ null, null },
		{ true, "true" },
		{ false, "false" },
		{ VulnerabilitySeverity.High, "High" },
		{ new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(1)), "2026-01-02T02:04:05Z" },
		{ new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), "2026-01-02T03:04:05Z" },
		{ 1.5, "1.5" },
		{ 12, "12" },
		{ "text", "text" }
	};

	[Theory]
	[MemberData(nameof(UrlValues))]
	public void UrlParameterFormatter_UsesInvariantWireFormats(object? value, string? expected)
	{
		var previous = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
		try
		{
			new DefenderUrlParameterFormatter().Format(value, GeneratedParameterAttributeProvider.Empty, value?.GetType() ?? typeof(object)).Should().Be(expected);
		}
		finally
		{
			CultureInfo.CurrentCulture = previous;
		}
	}

	[Fact]
	public void NullCollections_KeepTheInitialiser()
	{
		var lists = JsonSerializer.Deserialize<Lists>("""{"list":null,"array":null,"map":null,"unset":null,"text":null}""", DefenderJson.Options)!;

		lists.List.Should().BeEmpty();
		lists.Array.Should().BeEmpty();
		lists.Map.Should().BeEmpty();
		lists.Unset.Should().BeNull();
		lists.Text.Should().BeNull();
	}

	[Fact]
	public void CollectionValues_AreStillRead()
	{
		var lists = JsonSerializer.Deserialize<Lists>("""{"list":["a"],"array":["x"],"map":{"k":"v"},"unset":["u"]}""", DefenderJson.Options)!;

		lists.List.Should().Equal("a");
		lists.Array.Should().Equal("x");
		lists.Map.Should().Equal(new Dictionary<string, string> { ["k"] = "v" });
		lists.Unset.Should().Equal("u");
	}

	[Fact]
	public void SharedModels_KeepTheirDefaults()
	{
		var page = JsonSerializer.Deserialize<ODataPage<Vulnerability>>(
			"""{"@odata.context":null,"value":[{"id":"CVE-1","exploitTypes":null,"exploitUris":null,"tags":null}]}""",
			DefenderJson.Options)!;

		page.Value.Should().ContainSingle().Which.ExploitTypes.Should().BeEmpty();
		page.Value[0].Tags.Should().BeEmpty();
		JsonSerializer.Deserialize<ODataPage<Vulnerability>>("""{"value":null}""", DefenderJson.Options)!.Value.Should().BeEmpty();
	}
}
