using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>
/// Shared checks for the operations that list the alerts or devices related to a file, domain, IP address or user: they all
/// answer the same two stand-in entities, so each group's tests pin the request and delegate the rest here.
/// </summary>
internal static class RelatedEntityChecks
{
	public const string AlertJson = """{"id":"da637-1","title":"Made-up alert","severity":"High","machineId":"m1"}""";

	public const string MachineJson = """{"id":"m1","computerDnsName":"pc-01.example.test","osPlatform":"Windows11"}""";

	public const string NextLink = "https://defender.test/api/next?$skiptoken=1";

	public static string Page(string items, string? next = null)
	{
		var link = next is null ? string.Empty : ",\"@odata.nextLink\":\"" + next + "\"";
		return $$"""{"@odata.context":"https://defender.test/api/$metadata#x","value":[{{items}}]{{link}}}""";
	}

	/// <summary>Sends <paramref name="call"/> and asserts it made exactly one GET to <paramref name="path"/> with <paramref name="query"/>.</summary>
	public static async Task AssertGetAsync(Func<DefenderClient, CancellationToken, Task> call, string path, string query = "")
	{
		var recorded = await TestClient.CaptureAsync(call, Page(""));

		recorded.ShouldBeRequest(HttpMethod.Get, path, query);
		recorded.Authorization.Should().Be("Bearer token-1");
	}

	public static async Task AssertAlertsAsync(Func<DefenderClient, CancellationToken, Task<ODataPage<Alert>>> call)
	{
		var page = await TestClient.ReadAsync(call, Page(AlertJson, NextLink));

		page.NextLink.Should().Be(NextLink);
		var alert = page.Value.Should().ContainSingle().Subject;
		(alert.Id, alert.Title, alert.Severity, alert.MachineId).Should().Be(("da637-1", "Made-up alert", AlertSeverity.High, "m1"));
	}

	public static async Task AssertMachinesAsync(Func<DefenderClient, CancellationToken, Task<ODataPage<Machine>>> call)
	{
		var page = await TestClient.ReadAsync(call, Page(MachineJson));

		var machine = page.Value.Should().ContainSingle().Subject;
		(machine.Id, machine.ComputerDnsName, machine.OsPlatform).Should().Be(("m1", "pc-01.example.test", "Windows11"));
	}

	public static Task AssertErrorAsync(Func<DefenderClient, CancellationToken, Task> call, HttpStatusCode status = HttpStatusCode.Forbidden)
		=> TestClient.ShouldFailAsync(call, status,
			"""{"error":{"code":"Forbidden","message":"Not allowed for this token","target":"corr-1"}}""", "Not allowed for this token");

	/// <summary>Enumerates a high-level helper over two pages and asserts both requests and the merged items.</summary>
	public static async Task AssertPagesAsync<T>(Func<DefenderClient, CancellationToken, IAsyncEnumerable<T>> helper, string path, string firstItem, string secondItem, Func<T, string?> idOf)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page(firstItem, NextLink));
		stub.Enqueue(HttpStatusCode.OK, Page(secondItem));
		using var client = TestClient.Create(stub);

		var items = await helper(client, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

		items.Select(idOf).Should().Equal("a", "b");
		stub.Calls.Should().HaveCount(2);
		stub.Calls[0].ShouldBeRequest(HttpMethod.Get, path);
		stub.Calls[1].Uri.ToString().Should().Be(NextLink);
	}
}
