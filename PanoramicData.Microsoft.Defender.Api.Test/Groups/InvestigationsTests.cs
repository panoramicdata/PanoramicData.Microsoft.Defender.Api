using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Investigations;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Automated investigations: listing, reading and starting one.</summary>
public class InvestigationsTests
{
	private const string InvestigationJson = """
        {
            "id": "63017",
            "startTime": "2026-01-06T14:11:34Z",
            "endTime": "2026-01-06T15:00:00Z",
            "cancelledBy": "analyst@example.test",
            "state": "PendingApproval",
            "statusDetails": "waiting",
            "machineId": "1111111111111111111111111111111111111111",
            "computerDnsName": "pc-01.example.test",
            "triggeringAlertId": "da637000000000000000_1000000001"
        }
        """;

	private const string StartBody = """{"Comment":"Triage requested"}""";

	private static string Envelope(string items)
		=> $$"""{"@odata.context":"https://defender.test/api/$metadata#Investigations","value":[{{items}}]}""";

	private static string Failure(string code, string message)
		=> $$$"""{"error":{"code":"{{{code}}}","message":"{{{message}}}","target":"corr-1"}}""";

	private static StartInvestigationRequest Start() => new() { Comment = "Triage requested" };

	private static void AssertInvestigation(Investigation i)
	{
		(i.Id, i.State, i.StatusDetails, i.CancelledBy).Should().Be(("63017", "PendingApproval", "waiting", "analyst@example.test"));
		i.StartTime.Should().Be(new DateTimeOffset(2026, 1, 6, 14, 11, 34, TimeSpan.Zero));
		i.EndTime.Should().Be(new DateTimeOffset(2026, 1, 6, 15, 0, 0, TimeSpan.Zero));
		(i.MachineId, i.ComputerDnsName, i.TriggeringAlertId).Should().Be(("1111111111111111111111111111111111111111", "pc-01.example.test", "da637000000000000000_1000000001"));
	}

	[Fact]
	public async Task List_SendsGetInvestigations_WithTheODataOptions()
	{
		var options = new ODataListOptions { Filter = "state eq 'Running'", Top = 10000, Skip = 3 };

		var call = await TestClient.CaptureAsync((c, t) => c.Investigations.ListAsync(options, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/investigations", "?$filter=state%20eq%20%27Running%27&$top=10000&$skip=3");
	}

	[Fact]
	public async Task List_WithNoOptions_SendsNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Investigations.ListAsync(null, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/investigations");
	}

	[Fact]
	public async Task List_MapsEveryField()
		=> AssertInvestigation((await TestClient.ReadAsync((c, t) => c.Investigations.ListAsync(null, t), Envelope(InvestigationJson))).Value.Should().ContainSingle().Subject);

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Investigations.ListAsync(null, t), Envelope("""{"id":63017,"endTime":null,"startTime":"","extra":true}"""));

		var i = page.Value.Should().ContainSingle().Subject;
		i.Id.Should().Be("63017");
		i.EndTime.Should().BeNull();
		i.StartTime.Should().BeNull();
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Investigations.ListAsync(null, t), HttpStatusCode.Forbidden, Failure("Forbidden", "Missing permission"), "Missing permission");

	[Fact]
	public async Task GetAllInvestigations_FollowsTheNextLink()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"value":[{"id":"1"}],"@odata.nextLink":"https://defender.test/api/investigations?$skiptoken=2"}""");
		stub.Enqueue(HttpStatusCode.OK, Envelope("""{"id":"2"}"""));
		using var client = TestClient.Create(stub);

		var ids = new List<string?>();
		await foreach (var investigation in client.GetAllInvestigationsAsync(null, TestContext.Current.CancellationToken))
		{
			ids.Add(investigation.Id);
		}

		ids.Should().Equal("1", "2");
	}

	[Fact]
	public async Task Get_SendsGetInvestigationsId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Investigations.GetAsync("63017", t), InvestigationJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/investigations/63017");
	}

	[Fact]
	public async Task Get_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Investigations.GetAsync("a/b c", t), InvestigationJson);

		call.Uri.AbsolutePath.Should().Be("/api/investigations/a%2Fb%20c");
	}

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertInvestigation(await TestClient.ReadAsync((c, t) => c.Investigations.GetAsync("63017", t), InvestigationJson));

	[Fact]
	public Task Get_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Investigations.GetAsync("1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No such investigation"), "No such investigation");

	[Fact]
	public async Task Start_SendsPostMachinesIdStartInvestigation_WithACapitalCommentProperty()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Investigations.StartAsync("m1", Start(), t), InvestigationJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/machines/m1/startInvestigation", string.Empty, StartBody);
	}

	[Fact]
	public async Task Start_EscapesTheMachineIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Investigations.StartAsync("a/b", Start(), t), InvestigationJson);

		call.Uri.AbsolutePath.Should().Be("/api/machines/a%2Fb/startInvestigation");
	}

	[Fact]
	public async Task Start_MapsTheInvestigationFrom201()
	{
		using var client = TestClient.Create(TestClient.Stub(InvestigationJson, HttpStatusCode.Created));

		AssertInvestigation(await client.Investigations.StartAsync("m1", Start(), TestContext.Current.CancellationToken));
	}

	[Fact]
	public Task Start_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Investigations.StartAsync("m1", Start(), t), HttpStatusCode.BadRequest, Failure("OsPlatformNotSupported", "Unsupported OS"), "Unsupported OS");

	[Fact]
	public Task Start_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Investigations.StartAsync("m1", Start(), t), HttpMethod.Post, "/api/machines/m1/startInvestigation", StartBody);

	[Fact]
	public Task Start_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Investigations.StartAsync("m1", Start(), t));
}
