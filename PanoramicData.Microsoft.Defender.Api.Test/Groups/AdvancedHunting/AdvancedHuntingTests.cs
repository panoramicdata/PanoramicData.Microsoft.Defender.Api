using PanoramicData.MicrosoftDefender.Api.Models.AdvancedHunting;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;
using System.Text.Json;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups.AdvancedHunting;

#pragma warning disable CS0618 // Testing an obsolete operation on purpose.

/// <summary>
/// The advanced hunting group. The operation is marked obsolete (Microsoft is retiring it), so this file, and only this
/// file, suppresses CS0618; TreatWarningsAsErrors stays on for everything else.
/// </summary>
public class AdvancedHuntingTests
{
	private const string Query = "DeviceProcessEvents | where FileName =~ 'cmd.exe' | take 2";

	// The default JSON encoder writes each apostrophe as a six-character unicode escape, which the service decodes to the same query.
	private static readonly string ApostropheEscape = new string((char)92, 1) + "u0027";

	private static readonly string ExpectedBody = "{\"Query\":\"DeviceProcessEvents | where FileName =~ " + ApostropheEscape + "cmd.exe" + ApostropheEscape + " | take 2\"}";

	// Shaped like the service's response: PascalCase Schema and Results, rows keyed by column name.
	private const string ResultJson = """
		{
			"Schema": [
				{"Name": "Timestamp", "Type": "DateTime"},
				{"Name": "FileName", "Type": "String"},
				{"Name": "Count", "Type": "Long"},
				{"Name": "IsAdmin", "Type": "Boolean"},
				{"Name": "Extra", "Type": "Dynamic"}
			],
			"Results": [
				{"Timestamp": "2020-02-05T01:10:26.2648757Z", "FileName": "cmd.exe", "Count": 7, "IsAdmin": true, "Extra": {"a": [1, 2]}},
				{"Timestamp": "2020-02-05T01:10:27Z", "FileName": null, "Count": 0, "IsAdmin": false, "Extra": null}
			]
		}
		""";

	[Fact]
	public async Task Run_SendsPostAdvancedQueriesRun_WithTheQueryBody()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.AdvancedHunting.RunAsync(new AdvancedHuntingRequest { Query = Query }, t), ResultJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/advancedqueries/run", body: ExpectedBody);
		call.Authorization.Should().Be("Bearer token-1");
	}

	[Fact]
	public async Task TheClientHelper_SendsTheSameRequest()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.RunAdvancedHuntingQueryAsync(Query, t), ResultJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/advancedqueries/run", body: ExpectedBody);
	}

	[Fact]
	public async Task Run_MapsTheSchemaAndTheRows_AsRawJsonValues()
	{
		var result = await TestClient.ReadAsync((c, t) => c.RunAdvancedHuntingQueryAsync(Query, t), ResultJson);

		result.Schema.Select(s => (s.Name, s.Type)).Should().Equal(
			("Timestamp", "DateTime"), ("FileName", "String"), ("Count", "Long"), ("IsAdmin", "Boolean"), ("Extra", "Dynamic"));
		result.Results.Should().HaveCount(2);
		var first = result.Results[0];
		first["Timestamp"].GetDateTimeOffset().Should().Be(new DateTimeOffset(2020, 2, 5, 1, 10, 26, TimeSpan.Zero).AddTicks(2648757));
		first["FileName"].GetString().Should().Be("cmd.exe");
		first["Count"].GetInt64().Should().Be(7);
		first["IsAdmin"].GetBoolean().Should().BeTrue();
		first["Extra"].GetProperty("a").EnumerateArray().Select(e => e.GetInt32()).Should().Equal(1, 2);
		var second = result.Results[1];
		second["FileName"].ValueKind.Should().Be(JsonValueKind.Null);
		second["Extra"].ValueKind.Should().Be(JsonValueKind.Null);
	}

	[Fact]
	public async Task Run_ToleratesAnEmptyOrPartialResponse()
	{
		var empty = await TestClient.ReadAsync((c, t) => c.RunAdvancedHuntingQueryAsync(Query, t), """{"Schema":null,"Results":null,"Other":1}""");
		var partial = await TestClient.ReadAsync((c, t) => c.RunAdvancedHuntingQueryAsync(Query, t), "{}");

		empty.Schema.Should().BeEmpty();
		empty.Results.Should().BeEmpty();
		partial.Schema.Should().BeEmpty();
		partial.Results.Should().BeEmpty();
	}

	[Fact]
	public Task Run_AnInvalidQueryIsMappedAsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.RunAdvancedHuntingQueryAsync("not kql", t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"BadRequest","message":"Failed to run the query","target":"corr-1"}}""", "Failed to run the query");

	[Fact]
	public Task Run_AnExceededQuotaIsMappedAsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.RunAdvancedHuntingQueryAsync(Query, t), HttpStatusCode.TooManyRequests,
			"""{"error":{"code":"TooManyRequests","message":"Quota exceeded","target":"corr-1"}}""", "Quota exceeded");

	[Fact]
	public async Task Run_IsAllowedByTheReadOnlyGuard_BecauseItOnlyReads()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, ResultJson);
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		var result = await client.RunAdvancedHuntingQueryAsync(Query, TestContext.Current.CancellationToken);

		result.Results.Should().HaveCount(2);
		stub.Calls.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Post);
	}

	[Fact]
	public async Task Run_ResendsTheSameBodyAfterATokenRefresh()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Unauthorized);
		stub.Enqueue(HttpStatusCode.OK, ResultJson);
		using var client = TestClient.Create(stub);

		await client.RunAdvancedHuntingQueryAsync(Query, TestContext.Current.CancellationToken);

		stub.Calls.Select(c => c.Body).Should().OnlyContain(b => b == ExpectedBody);
		stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
	}

	[Fact]
	public void BothTheInterfaceMethodAndTheClientHelper_AreMarkedObsolete()
	{
		var interfaceMethod = typeof(Interfaces.IAdvancedHuntingApi).GetMethod(nameof(Interfaces.IAdvancedHuntingApi.RunAsync))!;
		var helper = typeof(DefenderClient).GetMethod(nameof(DefenderClient.RunAdvancedHuntingQueryAsync))!;

		foreach (var method in new[] { interfaceMethod, helper })
		{
			var obsolete = method.GetCustomAttributes(typeof(ObsoleteAttribute), false).Should().ContainSingle().Subject.Should().BeOfType<ObsoleteAttribute>().Subject;
			obsolete.Message.Should().Contain("runHuntingQuery");
		}
	}
}
#pragma warning restore CS0618
