using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups.Files;

/// <summary>The files group: a file's profile, related alerts and devices, and statistics, all addressed by hash.</summary>
public class FilesTests
{
	private const string Sha1 = "4388963aaa83afe2042a46a3c017ad50bdcdafb3";

	// Shaped like the service's responses (field names and types), with neutral values.
	private const string ProfileJson = """
		{
			"@odata.context": "https://defender.test/api/$metadata#Files/$entity",
			"sha1": "4388963aaa83afe2042a46a3c017ad50bdcdafb3",
			"sha256": "413c58c8267d2c8648d8f6384bacc2ae9c929b2b96578b6860b5087cd1bd6462",
			"globalPrevalence": 180022,
			"globalFirstObserved": "2017-09-19T03:51:27.6785431Z",
			"globalLastObserved": "2020-01-06T03:59:21Z",
			"size": 22139496,
			"fileType": "APP",
			"isPeFile": true,
			"filePublisher": "Example Publisher Ltd",
			"fileProductName": "Example Product",
			"signer": "Example Signer",
			"issuer": "Example CA",
			"signerHash": "6c3245d4a9bc0244d99dff27af259cbbae2e2d16",
			"isValidCertificate": false,
			"determinationType": "Pua",
			"determinationValue": "PUA:Win32/Example"
		}
		""";

	private const string StatisticsJson = """
		{
			"@odata.context": "https://defender.test/api/$metadata#microsoft.windowsDefenderATP.api.InOrgFileStats",
			"sha1": "0991a395da64e1c5fbe8732ed11e6be064081d9f",
			"organizationPrevalence": 14850,
			"orgFirstSeen": "2019-12-07T13:44:16Z",
			"orgLastSeen": "2020-01-06T13:39:36Z",
			"globallyPrevalence": 705012,
			"globalFirstObserved": "2015-03-19T12:20:07.3432441Z",
			"globalLastObserved": "2020-01-06T13:39:36Z",
			"topFileNames": ["MREC.exe", "mrec.bin"]
		}
		""";

	[Fact]
	public async Task Get_SendsGetFilesId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Files.GetAsync(Sha1, t), ProfileJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/files/" + Sha1);
		call.Authorization.Should().Be("Bearer token-1");
	}

	[Fact]
	public async Task Get_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Files.GetAsync("a/b c", t), ProfileJson);

		call.Uri.AbsolutePath.Should().Be("/api/files/a%2Fb%20c");
	}

	[Fact]
	public async Task Get_MapsEveryField()
	{
		var file = await TestClient.ReadAsync((c, t) => c.Files.GetAsync(Sha1, t), ProfileJson);

		(file.Sha1, file.Sha256, file.GlobalPrevalence, file.Size, file.FileType, file.IsPeFile)
			.Should().Be((Sha1, "413c58c8267d2c8648d8f6384bacc2ae9c929b2b96578b6860b5087cd1bd6462", 180022L, 22139496L, "APP", true));
		(file.FilePublisher, file.FileProductName, file.Signer, file.Issuer, file.SignerHash, file.IsValidCertificate)
			.Should().Be(("Example Publisher Ltd", "Example Product", "Example Signer", "Example CA", "6c3245d4a9bc0244d99dff27af259cbbae2e2d16", false));
		(file.DeterminationType, file.DeterminationValue).Should().Be(("Pua", "PUA:Win32/Example"));
		file.GlobalFirstObserved.Should().Be(new DateTimeOffset(2017, 9, 19, 3, 51, 27, TimeSpan.Zero).AddTicks(6785431));
		file.GlobalLastObserved.Should().Be(new DateTimeOffset(2020, 1, 6, 3, 59, 21, TimeSpan.Zero));
	}

	[Fact]
	public async Task Get_ToleratesLooseJson()
	{
		var file = await TestClient.ReadAsync((c, t) => c.Files.GetAsync(Sha1, t),
			"""{"sha1":"x","globalPrevalence":"12","size":"7","isPeFile":"true","isValidCertificate":null,"globalLastObserved":"","fileType":null,"extra":{"unmodelled":1}}""");

		(file.GlobalPrevalence, file.Size, file.IsPeFile, file.IsValidCertificate, file.GlobalLastObserved, file.FileType)
			.Should().Be((12L, 7L, true, (bool?)null, (DateTimeOffset?)null, (string?)null));
	}

	[Fact]
	public Task Get_AnUnknownFileIsA404()
		=> TestClient.ShouldFailAsync((c, t) => c.Files.GetAsync(Sha1, t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"File not found","target":"corr-1"}}""", "File not found");

	[Fact]
	public Task ListAlerts_SendsGetFilesIdAlerts()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.Files.ListAlertsAsync(Sha1, t), "/api/files/" + Sha1 + "/alerts");

	[Fact]
	public Task ListAlerts_MapsTheAlerts()
		=> RelatedEntityChecks.AssertAlertsAsync((c, t) => c.Files.ListAlertsAsync(Sha1, t));

	[Fact]
	public Task ListAlerts_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Files.ListAlertsAsync(Sha1, t));

	[Fact]
	public Task ListMachines_SendsGetFilesIdMachines()
		=> RelatedEntityChecks.AssertGetAsync((c, t) => c.Files.ListMachinesAsync(Sha1, t), "/api/files/" + Sha1 + "/machines");

	[Fact]
	public Task ListMachines_MapsTheMachines()
		=> RelatedEntityChecks.AssertMachinesAsync((c, t) => c.Files.ListMachinesAsync(Sha1, t));

	[Fact]
	public Task ListMachines_MapsAnError()
		=> RelatedEntityChecks.AssertErrorAsync((c, t) => c.Files.ListMachinesAsync(Sha1, t));

	[Fact]
	public async Task GetStatistics_WithNoOptions_SendsNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Files.GetStatisticsAsync(Sha1, null, t), StatisticsJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/files/" + Sha1 + "/stats");
	}

	[Fact]
	public async Task GetStatistics_SendsGetFilesIdStats_WithTheLookBackHours()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Files.GetStatisticsAsync(Sha1, new EntityStatisticsOptions { LookBackHours = 48 }, t), StatisticsJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/files/" + Sha1 + "/stats", "?lookBackHours=48");
	}

	[Fact]
	public async Task GetStatistics_MapsEveryField()
	{
		var stats = await TestClient.ReadAsync((c, t) => c.Files.GetStatisticsAsync(Sha1, null, t), StatisticsJson);

		(stats.Sha1, stats.OrganizationPrevalence, stats.GloballyPrevalence).Should().Be(("0991a395da64e1c5fbe8732ed11e6be064081d9f", 14850L, 705012L));
		stats.OrgFirstSeen.Should().Be(new DateTimeOffset(2019, 12, 7, 13, 44, 16, TimeSpan.Zero));
		stats.OrgLastSeen.Should().Be(new DateTimeOffset(2020, 1, 6, 13, 39, 36, TimeSpan.Zero));
		stats.GlobalFirstObserved.Should().Be(new DateTimeOffset(2015, 3, 19, 12, 20, 7, TimeSpan.Zero).AddTicks(3432441));
		stats.GlobalLastObserved.Should().Be(new DateTimeOffset(2020, 1, 6, 13, 39, 36, TimeSpan.Zero));
		stats.TopFileNames.Should().Equal("MREC.exe", "mrec.bin");
	}

	[Fact]
	public async Task GetStatistics_ToleratesLooseJson()
	{
		var stats = await TestClient.ReadAsync((c, t) => c.Files.GetStatisticsAsync(Sha1, null, t),
			"""{"organizationPrevalence":"5","orgFirstSeen":"","topFileNames":null}""");

		(stats.OrganizationPrevalence, stats.OrgFirstSeen).Should().Be((5L, (DateTimeOffset?)null));
		stats.TopFileNames.Should().BeEmpty();
	}

	[Fact]
	public Task GetStatistics_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Files.GetStatisticsAsync(Sha1, null, t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"File not found","target":"corr-1"}}""", "File not found");

	[Fact]
	public Task GetAllFileAlerts_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllFileAlertsAsync(Sha1, t), "/api/files/" + Sha1 + "/alerts", """{"id":"a"}""", """{"id":"b"}""", a => a.Id);

	[Fact]
	public Task GetAllFileMachines_FollowsTheNextLink()
		=> RelatedEntityChecks.AssertPagesAsync((c, t) => c.GetAllFileMachinesAsync(Sha1, t), "/api/files/" + Sha1 + "/machines", """{"id":"a"}""", """{"id":"b"}""", m => m.Id);

	[Fact]
	public async Task EveryOperation_IsAllowedByTheReadOnlyGuard_BecauseAllAreGets()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, ProfileJson);
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		(await client.Files.GetAsync(Sha1, TestContext.Current.CancellationToken)).Sha1.Should().Be(Sha1);
	}
}
