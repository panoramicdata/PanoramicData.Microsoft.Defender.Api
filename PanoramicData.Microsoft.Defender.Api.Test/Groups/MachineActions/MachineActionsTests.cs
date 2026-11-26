using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.MachineActions;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Following machine actions: list, get, cancel and the download links.</summary>
public partial class MachineActionsTests
{
	// Shaped like the service's response (field names and types), with neutral values.
	internal const string ActionJson = """
		{
			"id": "11111111-2222-3333-4444-555555555555",
			"type": "StopAndQuarantineFile",
			"scope": "Selective",
			"requestor": "analyst@example.test",
			"externalID": "ext-1",
			"requestSource": "PublicApi",
			"commands": ["RunScript", "GetFile"],
			"cancellationRequestor": "boss@example.test",
			"requestorComment": "because",
			"cancellationComment": "changed my mind",
			"status": "InProgress",
			"machineId": "0123456789abcdef0123456789abcdef01234567",
			"computerDnsName": "pc-01.example.test",
			"creationDateTimeUtc": "2024-03-01T01:02:03Z",
			"cancellationDateTimeUtc": "2024-03-01T01:03:03Z",
			"lastUpdateDateTimeUtc": "2024-03-01T01:04:03Z",
			"title": "A title",
			"relatedFileInfo": {"fileIdentifier": "0123456789abcdef0123456789abcdef01234567", "fileIdentifierType": "Sha1"}
		}
		""";

	internal const string ErrorJson = """{"error":{"code":"Forbidden","message":"Missing permission","target":"corr-1"}}""";

	private const string ActionId = "11111111-2222-3333-4444-555555555555";

	private static string Envelope(string items) => $$"""{"@odata.context":"https://defender.test/api/$metadata#MachineActions","value":[{{items}}]}""";

	private static void AssertAction(MachineAction a)
	{
		(a.Id, a.Type, a.Scope, a.Requestor, a.ExternalId, a.RequestSource).Should().Be((ActionId, "StopAndQuarantineFile", "Selective", "analyst@example.test", "ext-1", "PublicApi"));
		a.Commands.Should().Equal("RunScript", "GetFile");
		(a.CancellationRequestor, a.RequestorComment, a.CancellationComment).Should().Be(("boss@example.test", "because", "changed my mind"));
		a.Status.Should().Be(MachineActionStatus.InProgress);
		(a.MachineId, a.ComputerDnsName, a.Title).Should().Be(("0123456789abcdef0123456789abcdef01234567", "pc-01.example.test", "A title"));
		a.CreationDateTimeUtc.Should().Be(new DateTimeOffset(2024, 3, 1, 1, 2, 3, TimeSpan.Zero));
		a.CancellationDateTimeUtc.Should().Be(new DateTimeOffset(2024, 3, 1, 1, 3, 3, TimeSpan.Zero));
		a.LastUpdateDateTimeUtc.Should().Be(new DateTimeOffset(2024, 3, 1, 1, 4, 3, TimeSpan.Zero));
		var file = a.RelatedFileInfo.Should().NotBeNull().And.Subject.As<MachineActionFileInfo>();
		(file.FileIdentifier, file.FileIdentifierType).Should().Be(("0123456789abcdef0123456789abcdef01234567", "Sha1"));
	}

	[Fact]
	public async Task List_SendsGetMachineactions_WithTheODataOptions()
	{
		var options = new ODataListOptions { Filter = "status eq 'Pending' and machineId eq 'a b'", Top = 10000, Skip = 5 };

		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.ListAsync(options, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/machineactions", "?$filter=status%20eq%20%27Pending%27%20and%20machineId%20eq%20%27a%20b%27&$top=10000&$skip=5");
	}

	[Fact]
	public async Task List_WithNoOptions_SendsNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.ListAsync(null, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/machineactions");
	}

	[Fact]
	public async Task List_MapsEveryField_AndTheEnvelope()
	{
		var page = await TestClient.ReadAsync((c, t) => c.MachineActions.ListAsync(null, t),
			$$"""{"@odata.context":"c","value":[{{ActionJson}}],"@odata.nextLink":"https://defender.test/api/machineactions?$skip=1"}""");

		page.NextLink.Should().Be("https://defender.test/api/machineactions?$skip=1");
		AssertAction(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.MachineActions.ListAsync(null, t),
			Envelope("""{"id":"x","status":"Exploded","commands":null,"creationDateTimeUtc":"","relatedFileInfo":null,"extra":1}"""));

		var item = page.Value.Should().ContainSingle().Subject;
		item.Status.Should().Be(MachineActionStatus.Unknown);
		item.Commands.Should().BeEmpty();
		item.CreationDateTimeUtc.Should().BeNull();
		item.RelatedFileInfo.Should().BeNull();
	}

	[Theory]
	[InlineData("Pending", MachineActionStatus.Pending)]
	[InlineData("InProgress", MachineActionStatus.InProgress)]
	[InlineData("Succeeded", MachineActionStatus.Succeeded)]
	[InlineData("Failed", MachineActionStatus.Failed)]
	[InlineData("TimeOut", MachineActionStatus.TimeOut)]
	[InlineData("Cancelled", MachineActionStatus.Cancelled)]
	public async Task List_MapsEveryStatus(string wire, MachineActionStatus expected)
	{
		var page = await TestClient.ReadAsync((c, t) => c.MachineActions.ListAsync(null, t), Envelope($$"""{"status":"{{wire}}"}"""));

		page.Value.Should().ContainSingle().Which.Status.Should().Be(expected);
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.MachineActions.ListAsync(null, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task GetAllMachineActions_FollowsTheNextLink()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"value":[{"id":"a"}],"@odata.nextLink":"https://defender.test/api/machineactions?$skip=1"}""");
		stub.Enqueue(HttpStatusCode.OK, Envelope("""{"id":"b"}"""));
		using var client = TestClient.Create(stub);

		var ids = await client.GetAllMachineActionsAsync(null, TestContext.Current.CancellationToken).Select(a => a.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("a", "b");
	}

	[Fact]
	public async Task Get_SendsGetMachineactionsId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.GetAsync(ActionId, t), ActionJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/machineactions/" + ActionId);
	}

	[Fact]
	public async Task Get_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.GetAsync("a/b c", t), ActionJson);

		call.Uri.AbsolutePath.Should().Be("/api/machineactions/a%2Fb%20c");
	}

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertAction(await TestClient.ReadAsync((c, t) => c.MachineActions.GetAsync(ActionId, t), ActionJson));

	[Fact]
	public Task Get_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.MachineActions.GetAsync("x", t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"No such action","target":"c"}}""", "No such action");

	[Fact]
	public async Task Cancel_SendsPostMachineactionsIdCancel_WithTheComment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.CancelAsync(ActionId, new CancelMachineActionRequest { Comment = "no longer needed" }, t), ActionJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/machineactions/" + ActionId + "/cancel", "", """{"Comment":"no longer needed"}""");
	}

	[Fact]
	public async Task Cancel_WithoutAComment_SendsAnEmptyObject()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.CancelAsync(ActionId, new CancelMachineActionRequest(), t), ActionJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/machineactions/" + ActionId + "/cancel", "", "{}");
	}

	[Fact]
	public async Task Cancel_MapsTheUpdatedAction()
		=> AssertAction(await TestClient.ReadAsync((c, t) => c.MachineActions.CancelAsync(ActionId, new CancelMachineActionRequest(), t), ActionJson));

	[Fact]
	public Task Cancel_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.MachineActions.CancelAsync(ActionId, new CancelMachineActionRequest(), t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"BadRequest","message":"Already finished","target":"c"}}""", "Already finished");

	[Fact]
	public async Task GetPackageUri_SendsGetMachineactionsIdGetPackageUri()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.GetPackageUriAsync(ActionId, t), """{"value":"https://files.test/pkg?sig=secret"}""");

		call.ShouldBeRequest(HttpMethod.Get, "/api/machineactions/" + ActionId + "/getPackageUri");
	}

	[Fact]
	public async Task GetPackageUri_MapsTheLink_WithoutLeakingItInToString()
	{
		var link = await TestClient.ReadAsync((c, t) => c.MachineActions.GetPackageUriAsync(ActionId, t), """{"@odata.context":"c","value":"https://files.test/pkg?sig=secret"}""");

		link.Value.Should().Be("https://files.test/pkg?sig=secret");
		link.ToString().Should().NotContain("secret");
	}

	[Fact]
	public Task GetPackageUri_MapsAnErrorWhileThePackageIsNotReady()
		=> TestClient.ShouldFailAsync((c, t) => c.MachineActions.GetPackageUriAsync(ActionId, t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"Not ready","target":"c"}}""", "Not ready");

	[Fact]
	public async Task GetLiveResponseResultDownloadLink_SendsTheIndexInTheParenthesisedPath()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.GetLiveResponseResultDownloadLinkAsync(ActionId, 2, t), """{"value":"https://files.test/r"}""");

		call.ShouldBeRequest(HttpMethod.Get, "/api/machineactions/" + ActionId + "/GetLiveResponseResultDownloadLink(index=2)");
	}

	[Fact]
	public async Task GetLiveResponseResultDownloadLink_EscapesTheId_AndWritesIndexZero()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.GetLiveResponseResultDownloadLinkAsync("a/b", 0, t), """{"value":"https://files.test/r"}""");

		call.Uri.AbsolutePath.Should().Be("/api/machineactions/a%2Fb/GetLiveResponseResultDownloadLink(index=0)");
	}

	[Fact]
	public async Task GetLiveResponseResultDownloadLink_MapsTheLink()
	{
		var link = await TestClient.ReadAsync((c, t) => c.MachineActions.GetLiveResponseResultDownloadLinkAsync(ActionId, 0, t), """{"value":"https://files.test/r?sig=secret"}""");

		link.Value.Should().Be("https://files.test/r?sig=secret");
	}

	[Fact]
	public Task GetLiveResponseResultDownloadLink_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.MachineActions.GetLiveResponseResultDownloadLinkAsync(ActionId, 9, t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"No such command","target":"c"}}""", "No such command");
}
