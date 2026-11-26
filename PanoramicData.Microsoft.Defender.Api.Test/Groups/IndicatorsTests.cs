using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Indicators;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Indicators: listing, creating, importing and deleting.</summary>
public class IndicatorsTests
{
	private const string IndicatorJson = """
        {
            "id": "994",
            "indicatorValue": "220e7d15b011d7fac48f2bd61114db1022197f7f",
            "indicatorType": "FileSha1",
            "action": "BlockAndRemediate",
            "application": "demo-app",
            "externalID": "ext-1",
            "source": "user@example.test",
            "sourceType": "AadApp",
            "createdBySource": "Demo app",
            "createdBy": "00000000-0000-0000-0000-000000000009",
            "lastUpdatedBy": "other@example.test",
            "creationTimeDateTimeUtc": "2026-01-02T03:04:05Z",
            "expirationTime": "2027-01-02T03:04:05Z",
            "lastUpdateTime": "2026-01-03T03:04:05Z",
            "severity": "Informational",
            "title": "Made-up indicator",
            "description": "A made-up indicator used by the tests.",
            "recommendedActions": "nothing",
            "rbacGroupNames": ["team1"],
            "rbacGroupIds": ["7"],
            "generateAlert": true,
            "educateUrl": "https://help.example.test/blocked"
        }
        """;

	private static readonly string FullSubmissionBody = """{"indicatorValue":"220e7d15b011d7fac48f2bd61114db1022197f7f","indicatorType":"FileSha1","action":"AlertAndBlock","title":"demo","description":"demo description","application":"demo-app","expirationTime":"2030-12-12T00:00:00.0000000+00:00","severity":"Informational","recommendedActions":"nothing","rbacGroupNames":["group1","group2"],"educateUrl":"https://help.example.test/x","generateAlert":true}""".Replace("+", WriteOperationChecks.EscapedPlus);

	private const string MinimalSubmissionBody = """{"indicatorValue":"example.test","indicatorType":"DomainName","action":"Allowed","title":"t","description":"d"}""";

	private static IndicatorSubmission FullSubmission() => new()
	{
		IndicatorValue = "220e7d15b011d7fac48f2bd61114db1022197f7f",
		IndicatorType = IndicatorType.FileSha1,
		Action = IndicatorAction.AlertAndBlock,
		Title = "demo",
		Description = "demo description",
		Application = "demo-app",
		ExpirationTime = new DateTimeOffset(2030, 12, 12, 0, 0, 0, TimeSpan.Zero),
		Severity = IndicatorSeverity.Informational,
		RecommendedActions = "nothing",
		RbacGroupNames = ["group1", "group2"],
		EducateUrl = "https://help.example.test/x",
		GenerateAlert = true
	};

	private static IndicatorSubmission MinimalSubmission() => new()
	{
		IndicatorValue = "example.test",
		IndicatorType = IndicatorType.DomainName,
		Action = IndicatorAction.Allowed,
		Title = "t",
		Description = "d"
	};

	private static string Envelope(string items)
		=> $$"""{"@odata.context":"https://defender.test/api/$metadata#Indicators","value":[{{items}}]}""";

	private static string Failure(string code, string message)
		=> $$$"""{"error":{"code":"{{{code}}}","message":"{{{message}}}","target":"corr-1"}}""";

	private static IReadOnlyList<IndicatorSubmission> Submissions(int count)
		=> [.. Enumerable.Range(0, count).Select(_ => MinimalSubmission())];

	[Fact]
	public async Task List_SendsGetIndicators_WithNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.ListAsync(null, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/indicators");
	}

	[Fact]
	public async Task List_SendsTheODataOptions_WithALiteralDollar()
	{
		var options = new ODataListOptions { Filter = "action eq 'Block' and title eq 'it''s'", Top = 50, Skip = 100 };

		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.ListAsync(options, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/indicators", "?$filter=action%20eq%20%27Block%27%20and%20title%20eq%20%27it%27%27s%27&$top=50&$skip=100");
	}

	[Fact]
	public async Task List_MapsEveryField_AndTheEnvelope()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Indicators.ListAsync(null, t), Envelope(IndicatorJson));

		page.Context.Should().Be("https://defender.test/api/$metadata#Indicators");
		var i = page.Value.Should().ContainSingle().Subject;
		(i.Id, i.IndicatorValue, i.IndicatorType, i.Action).Should().Be(("994", "220e7d15b011d7fac48f2bd61114db1022197f7f", IndicatorType.FileSha1, IndicatorAction.BlockAndRemediate));
		(i.Application, i.ExternalId, i.Source, i.SourceType, i.CreatedBySource, i.CreatedBy, i.LastUpdatedBy)
			.Should().Be(("demo-app", "ext-1", "user@example.test", "AadApp", "Demo app", "00000000-0000-0000-0000-000000000009", "other@example.test"));
		i.CreationTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero));
		i.ExpirationTime.Should().Be(new DateTimeOffset(2027, 1, 2, 3, 4, 5, TimeSpan.Zero));
		i.LastUpdateTime.Should().Be(new DateTimeOffset(2026, 1, 3, 3, 4, 5, TimeSpan.Zero));
		(i.Severity, i.Title, i.Description, i.RecommendedActions).Should().Be((IndicatorSeverity.Informational, "Made-up indicator", "A made-up indicator used by the tests.", "nothing"));
		i.RbacGroupNames.Should().Equal("team1");
		i.RbacGroupIds.Should().Equal("7");
		(i.GenerateAlert, i.EducateUrl).Should().Be((true, "https://help.example.test/blocked"));
	}

	[Theory]
	[InlineData("Alert", IndicatorAction.Alert)]
	[InlineData("AlertAndBlock", IndicatorAction.AlertAndBlock)]
	[InlineData("Warn", IndicatorAction.Warn)]
	[InlineData("Block", IndicatorAction.Block)]
	[InlineData("Audit", IndicatorAction.Audit)]
	[InlineData("Allowed", IndicatorAction.Allowed)]
	[InlineData("SomethingNew", IndicatorAction.Unknown)]
	public async Task List_ReadsEveryAction_IncludingTheLegacyOnes(string wire, IndicatorAction expected)
	{
		var page = await TestClient.ReadAsync((c, t) => c.Indicators.ListAsync(null, t), Envelope($$"""{"id":"1","action":"{{wire}}"}"""));

		page.Value.Should().ContainSingle().Which.Action.Should().Be(expected);
	}

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Indicators.ListAsync(null, t),
			Envelope("""{"id":"1","indicatorType":"Hologram","severity":"Cosmic","generateAlert":"true","expirationTime":"","rbacGroupNames":null,"rbacGroupIds":null,"extra":[1]}"""));

		var i = page.Value.Should().ContainSingle().Subject;
		i.IndicatorType.Should().Be(IndicatorType.Unknown);
		i.Severity.Should().Be(IndicatorSeverity.Unknown);
		i.GenerateAlert.Should().BeTrue();
		i.ExpirationTime.Should().BeNull();
		i.RbacGroupNames.Should().BeEmpty();
		i.RbacGroupIds.Should().BeEmpty();
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.ListAsync(null, t), HttpStatusCode.Forbidden, Failure("Forbidden", "Missing Ti.ReadWrite.All"), "Missing Ti.ReadWrite.All");

	[Fact]
	public async Task GetAllIndicators_FollowsTheNextLink()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"value":[{"id":"1"}],"@odata.nextLink":"https://defender.test/api/indicators?$skiptoken=2"}""");
		stub.Enqueue(HttpStatusCode.OK, Envelope("""{"id":"2"}"""));
		using var client = TestClient.Create(stub);

		var ids = new List<string?>();
		await foreach (var indicator in client.GetAllIndicatorsAsync(new ODataListOptions { Top = 1 }, TestContext.Current.CancellationToken))
		{
			ids.Add(indicator.Id);
		}

		ids.Should().Equal("1", "2");
		stub.Calls.Select(c => c.Uri.Query).Should().Equal("?$top=1", "?$skiptoken=2");
	}

	[Fact]
	public async Task Create_SendsPostIndicators_WithEveryProperty()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.CreateAsync(FullSubmission(), t), IndicatorJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/indicators", string.Empty, FullSubmissionBody);
	}

	[Fact]
	public async Task Create_OmitsTheOptionalProperties()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.CreateAsync(MinimalSubmission(), t), IndicatorJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/indicators", string.Empty, MinimalSubmissionBody);
	}

	[Theory]
	[InlineData(IndicatorType.FileSha256, "FileSha256")]
	[InlineData(IndicatorType.FileMd5, "FileMd5")]
	[InlineData(IndicatorType.CertificateThumbprint, "CertificateThumbprint")]
	[InlineData(IndicatorType.IpAddress, "IpAddress")]
	[InlineData(IndicatorType.Url, "Url")]
	public async Task Create_WritesTheIndicatorTypeWireName(IndicatorType type, string wire)
	{
		var submission = MinimalSubmission();
		submission.IndicatorType = type;

		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.CreateAsync(submission, t), IndicatorJson);

		call.Body.Should().Contain($"\"indicatorType\":\"{wire}\"");
	}

	[Theory]
	[InlineData(IndicatorAction.Alert, "Alert")]
	[InlineData(IndicatorAction.Block, "Block")]
	[InlineData(IndicatorAction.Audit, "Audit")]
	[InlineData(IndicatorAction.Warn, "Warn")]
	[InlineData(IndicatorAction.BlockAndRemediate, "BlockAndRemediate")]
	public async Task Create_WritesTheActionWireName(IndicatorAction action, string wire)
	{
		var submission = MinimalSubmission();
		submission.Action = action;

		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.CreateAsync(submission, t), IndicatorJson);

		call.Body.Should().Contain($"\"action\":\"{wire}\"");
	}

	[Fact]
	public async Task Create_MapsTheStoredIndicator()
	{
		var i = await TestClient.ReadAsync((c, t) => c.Indicators.CreateAsync(FullSubmission(), t), IndicatorJson);

		(i.Id, i.Action).Should().Be(("994", IndicatorAction.BlockAndRemediate));
	}

	[Fact]
	public Task Create_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.CreateAsync(FullSubmission(), t), HttpStatusCode.BadRequest, Failure("InvalidRequestBody", "Audit needs generateAlert"), "Audit needs generateAlert");

	[Fact]
	public Task Create_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Indicators.CreateAsync(FullSubmission(), t), HttpMethod.Post, "/api/indicators", FullSubmissionBody);

	[Fact]
	public Task Create_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Indicators.CreateAsync(FullSubmission(), t));

	[Fact]
	public async Task Import_SendsPostIndicatorsImport_WithACapitalIndicatorsProperty()
	{
		var request = new IndicatorImportRequest { Indicators = [MinimalSubmission(), MinimalSubmission()] };

		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.ImportAsync(request, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Post, "/api/indicators/import", string.Empty, $$"""{"Indicators":[{{MinimalSubmissionBody}},{{MinimalSubmissionBody}}]}""");
	}

	[Fact]
	public async Task Import_MapsEveryResultField()
	{
		var request = new IndicatorImportRequest { Indicators = Submissions(2) };

		var page = await TestClient.ReadAsync((c, t) => c.Indicators.ImportAsync(request, t),
			"""{"value":[{"id":"2841","indicator":"example.test","isFailed":false,"failureReason":null},{"id":null,"indicator":"bad","isFailed":"true","failureReason":"Invalid value"}]}""");

		page.Value.Should().HaveCount(2);
		var ok = page.Value[0];
		(ok.Id, ok.Indicator, ok.IsFailed, ok.FailureReason).Should().Be(("2841", "example.test", false, null));
		var failed = page.Value[1];
		(failed.Id, failed.Indicator, failed.IsFailed, failed.FailureReason).Should().Be((null, "bad", true, "Invalid value"));
	}

	[Fact]
	public async Task Import_AcceptsExactlyTheMaximumBatch()
	{
		var request = new IndicatorImportRequest { Indicators = Submissions(IndicatorImportRequest.MaxBatchSize) };

		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.ImportAsync(request, t), Envelope(""));

		call.Body.Should().StartWith("""{"Indicators":[{"indicatorValue":""");
		IndicatorImportRequest.MaxBatchSize.Should().Be(500);
	}

	[Fact]
	public void Import_RejectsMoreThanTheMaximumBatch_BeforeAnythingIsSent()
	{
		var act = () => new IndicatorImportRequest { Indicators = Submissions(501) };

		var thrown = act.Should().Throw<ArgumentOutOfRangeException>().Which;
		thrown.ParamName.Should().Be("Indicators");
		thrown.ActualValue.Should().Be(501);
		thrown.Message.Should().Contain("Indicators holds 501 items but the service accepts at most 500 per call.");
	}

	[Fact]
	public void Import_RejectsAnEmptyBatch()
	{
		var act = () => new IndicatorImportRequest { Indicators = [] };

		act.Should().Throw<ArgumentException>().WithMessage("Indicators must contain at least one item.*");
	}

	[Fact]
	public Task Import_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.ImportAsync(new IndicatorImportRequest { Indicators = Submissions(1) }, t), HttpStatusCode.BadRequest, Failure("MaximumBatchSizeExceeded", "Too many"), "Too many");

	[Fact]
	public Task Import_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Indicators.ImportAsync(new IndicatorImportRequest { Indicators = Submissions(1) }, t), HttpMethod.Post, "/api/indicators/import", $$"""{"Indicators":[{{MinimalSubmissionBody}}]}""");

	[Fact]
	public Task Import_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Indicators.ImportAsync(new IndicatorImportRequest { Indicators = Submissions(1) }, t));

	[Fact]
	public async Task Delete_SendsDeleteIndicatorsId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.DeleteAsync("994", t), string.Empty);

		call.ShouldBeRequest(HttpMethod.Delete, "/api/indicators/994");
	}

	[Fact]
	public async Task Delete_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.DeleteAsync("a/b c", t), string.Empty);

		call.Uri.AbsolutePath.Should().Be("/api/indicators/a%2Fb%20c");
	}

	[Fact]
	public async Task Delete_AcceptsA204WithNoBody()
	{
		using var client = TestClient.Create(TestClient.Stub(string.Empty, HttpStatusCode.NoContent));

		await client.Indicators.DeleteAsync("1", TestContext.Current.CancellationToken);
	}

	[Fact]
	public Task Delete_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.DeleteAsync("1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No such indicator"), "No such indicator");

	[Fact]
	public Task Delete_IsReplayedAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Indicators.DeleteAsync("1", t), HttpMethod.Delete, "/api/indicators/1", null);

	[Fact]
	public Task Delete_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Indicators.DeleteAsync("1", t));

	[Fact]
	public async Task BatchDelete_SendsPostIndicatorsBatchDelete_WithACapitalIndicatorIdsProperty()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["1", "2", "5"] }, t), string.Empty);

		call.ShouldBeRequest(HttpMethod.Post, "/api/indicators/BatchDelete", string.Empty, """{"IndicatorIds":["1","2","5"]}""");
	}

	[Fact]
	public void BatchDelete_AcceptsTheMaximum_AndRejectsMore()
	{
		var ids = Enumerable.Range(0, 501).Select(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList();

		new IndicatorBatchDeleteRequest { IndicatorIds = [.. ids.Take(500)] }.IndicatorIds.Should().HaveCount(500);
		var act = () => new IndicatorBatchDeleteRequest { IndicatorIds = ids };
		act.Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("IndicatorIds");
	}

	[Fact]
	public void BatchDelete_RejectsAnEmptyList()
	{
		var act = () => new IndicatorBatchDeleteRequest { IndicatorIds = [] };

		act.Should().Throw<ArgumentException>().WithMessage("IndicatorIds must contain at least one item.*");
	}

	[Fact]
	public Task BatchDelete_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["9"] }, t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "Indicator 9 not found"), "Indicator 9 not found");

	[Fact]
	public Task BatchDelete_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["1"] }, t), HttpMethod.Post, "/api/indicators/BatchDelete", """{"IndicatorIds":["1"]}""");

	[Fact]
	public Task BatchDelete_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Indicators.BatchDeleteAsync(new IndicatorBatchDeleteRequest { IndicatorIds = ["1"] }, t));
}
