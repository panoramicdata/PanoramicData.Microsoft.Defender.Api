using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;
using System.Net.Http.Headers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

/// <summary>Following <c>@odata.nextLink</c>: authenticated, and never to another host.</summary>
public class PagingTests
{
	private const string Next = "https://defender.test/api/vulnerabilities?$skiptoken=abc";

	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private static string Page(string ids, string? nextLink = null)
		=> $$"""{"@odata.context":"c","value":[{{ids}}]{{(nextLink is null ? "" : $$""","@odata.nextLink":"{{nextLink}}" """)}}}""";

	[Fact]
	public async Task GetNextPage_ReturnsNullOnTheLastPage()
	{
		using var client = TestClient.Create(new StubHandler());

		(await client.GetNextPageAsync(new ODataPage<Vulnerability>(), Ct)).Should().BeNull();
		(await client.GetNextPageAsync(new ODataPage<Vulnerability> { NextLink = "" }, Ct)).Should().BeNull();
	}

	[Fact]
	public async Task GetNextPage_RequiresAPage()
	{
		using var client = TestClient.Create(new StubHandler());

		await ((Func<Task>)(() => client.GetNextPageAsync<Vulnerability>(null!, Ct))).Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task GetNextPage_SendsAnAuthenticatedGet_ToTheLinkAsGiven()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"CVE-2"}"""));
		using var client = TestClient.Create(stub);

		var page = await client.GetNextPageAsync(new ODataPage<Vulnerability> { NextLink = Next }, Ct);

		page!.Value.Should().ContainSingle().Which.Id.Should().Be("CVE-2");
		stub.Calls.Should().ContainSingle().Which.ShouldBeRequest(HttpMethod.Get, "/api/vulnerabilities", "?$skiptoken=abc");
		stub.Calls[0].Authorization.Should().Be("Bearer token-1");
	}

	[Fact]
	public async Task GetNextPage_KeepsEscapesInTheLinkUntouched()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page(""));
		using var client = TestClient.Create(stub);

		await client.GetNextPageAsync(new ODataPage<Vulnerability> { NextLink = "https://DEFENDER.test:443/api/vulnerabilities?$filter=a%20eq%20%27b%27&$skip=5" }, Ct);

		stub.Calls[0].Uri.Query.Should().Be("?$filter=a%20eq%20%27b%27&$skip=5");
	}

	[Theory]
	[InlineData("https://evil.test/api/vulnerabilities?$skiptoken=abc", "evil.test")]
	[InlineData("https://defender.test.evil.test/api/vulnerabilities", "defender.test.evil.test")]
	[InlineData("http://defender.test/api/vulnerabilities", "http://defender.test")]
	[InlineData("https://defender.test:8443/api/vulnerabilities", "8443")]
	[InlineData("https://defender.test@evil.test/api", "evil.test")]
	public async Task GetNextPage_RefusesALinkToAnotherHostSchemeOrPort_BeforeSendingAnything(string link, string mentioned)
	{
		var stub = new StubHandler();
		using var client = TestClient.Create(stub);

		var act = () => client.GetNextPageAsync(new ODataPage<Vulnerability> { NextLink = link + "&sig=SECRET" }, Ct);

		var thrown = await act.Should().ThrowAsync<DefenderLinkException>();
		thrown.Which.Message.Should().Contain(mentioned).And.NotContain("SECRET").And.NotContain("sig=");
		stub.Calls.Should().BeEmpty("the bearer token must never leave the configured host");
	}

	[Theory]
	[InlineData("/api/vulnerabilities?$skiptoken=abc")]
	[InlineData("not a url")]
	public async Task GetNextPage_RefusesARelativeLink(string link)
	{
		var stub = new StubHandler();
		using var client = TestClient.Create(stub);

		var act = () => client.GetNextPageAsync(new ODataPage<Vulnerability> { NextLink = link }, Ct);

		(await act.Should().ThrowAsync<DefenderLinkException>()).Which.Message.Should().Be("The next link is not an absolute URL.");
		stub.Calls.Should().BeEmpty();
	}

	[Fact]
	public async Task GetNextPage_MapsErrors()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.TooManyRequests, """{"error":{"code":"TooManyRequests","message":"slow down","target":"id-1"}}""", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)));
		using var client = TestClient.Create(stub);

		var act = () => client.GetNextPageAsync(new ODataPage<Vulnerability> { NextLink = Next }, Ct);

		var thrown = (await act.Should().ThrowAsync<DefenderApiException>()).Which;
		(thrown.StatusCode, thrown.Code, thrown.CorrelationId, thrown.RetryAfter).Should().Be((HttpStatusCode.TooManyRequests, "TooManyRequests", "id-1", (TimeSpan?)TimeSpan.FromSeconds(60)));
	}

	[Fact]
	public async Task GetNextPage_ANullBody_IsAnEmptyPage()
	{
		using var client = TestClient.Create(TestClient.Stub("null"));

		var page = await client.GetNextPageAsync(new ODataPage<Vulnerability> { NextLink = Next }, Ct);

		page!.Value.Should().BeEmpty();
		page.NextLink.Should().BeNull();
	}

	[Fact]
	public async Task GetAll_FollowsEveryPage_InOrder()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"1"},{"id":"2"}""", Next));
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"3"}""", "https://defender.test/api/vulnerabilities?$skiptoken=def"));
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"4"}"""));
		using var client = TestClient.Create(stub);

		var ids = new List<string?>();
		await foreach (var item in client.GetAllVulnerabilitiesAsync(new ODataListOptions { Top = 2 }, Ct))
		{
			ids.Add(item.Id);
		}

		ids.Should().Equal("1", "2", "3", "4");
		stub.Calls.Select(c => c.Uri.Query).Should().Equal("?$top=2", "?$skiptoken=abc", "?$skiptoken=def");
		stub.Calls.Select(c => c.Authorization).Should().AllBe("Bearer token-1");
	}

	[Fact]
	public async Task GetAll_WithAnEmptyCollection_YieldsNothing()
	{
		using var client = TestClient.Create(TestClient.Stub(Page("")));

		var items = await client.GetAllVulnerabilitiesAsync(null, Ct).ToListAsync(Ct);

		items.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAll_AForeignNextLink_ThrowsAfterTheFirstPageIsYielded()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"1"}""", "https://evil.test/steal?token=1"));
		using var client = TestClient.Create(stub);
		var seen = new List<string?>();

		var act = async () =>
		{
			await foreach (var item in client.GetAllVulnerabilitiesAsync(null, Ct))
			{
				seen.Add(item.Id);
			}
		};

		await act.Should().ThrowAsync<DefenderLinkException>();
		seen.Should().Equal("1");
		stub.Calls.Should().ContainSingle("the foreign link was never requested");
	}

	[Fact]
	public async Task GetAll_RequiresAFirstPage()
	{
		using var client = TestClient.Create(new StubHandler());

		var act = async () =>
		{
			await foreach (var _ in client.GetAllAsync<Vulnerability>(null!, Ct))
			{
				// Never reached.
			}
		};

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task TheOtherHighLevelHelpers_PageToo()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"m1"}""", Next));
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"m2"}"""));
		stub.Enqueue(HttpStatusCode.OK, Page("""{"id":"v1"}"""));
		using var client = TestClient.Create(stub);

		var machines = await client.GetAllMachineReferencesAsync("CVE-1", Ct).ToListAsync(Ct);
		var rows = await client.GetAllMachineVulnerabilitiesAsync(new ODataListOptions { Filter = "severity eq 'High'" }, Ct).ToListAsync(Ct);

		machines.Select(m => m.Id).Should().Equal("m1", "m2");
		rows.Select(r => r.Id).Should().Equal("v1");
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/vulnerabilities/CVE-1/machineReferences");
		stub.Calls[2].Uri.Query.Should().Be("?$filter=severity%20eq%20%27High%27");
	}
}
