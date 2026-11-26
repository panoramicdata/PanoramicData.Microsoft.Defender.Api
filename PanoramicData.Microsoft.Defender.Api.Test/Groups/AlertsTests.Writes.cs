using PanoramicData.MicrosoftDefender.Api.Models.Alerts;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

public partial class AlertsTests
{
	private const string FullUpdateBody = """{"status":"Resolved","assignedTo":"secop@example.test","classification":"FalsePositive","determination":"NotMalicious","comment":"checked"}""";

	private static AlertUpdate FullUpdate() => new()
	{
		Status = AlertStatus.Resolved,
		AssignedTo = "secop@example.test",
		Classification = AlertClassification.FalsePositive,
		Determination = "NotMalicious",
		Comment = "checked"
	};

	private static AlertBatchUpdate BatchUpdate() => new()
	{
		AlertIds = ["a1", "a2"],
		Status = AlertStatus.Resolved,
		AssignedTo = "secop@example.test",
		Classification = AlertClassification.InformationalExpectedActivity,
		Determination = "SecurityTesting",
		Comment = "checked"
	};

	private const string BatchBody = """{"alertIds":["a1","a2"],"status":"Resolved","assignedTo":"secop@example.test","classification":"InformationalExpectedActivity","determination":"SecurityTesting","comment":"checked"}""";

	private static CreateAlertRequest CreateRequest() => new()
	{
		EventTime = new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero).AddTicks(7115183),
		ReportId = "20776",
		MachineId = "m1",
		Severity = AlertSeverity.Low,
		Title = "example",
		Description = "example alert",
		RecommendedAction = "nothing",
		Category = "Exploit"
	};

	private static readonly string CreateBody = """{"eventTime":"2026-02-03T04:05:06.7115183+00:00","reportId":"20776","machineId":"m1","severity":"Low","title":"example","description":"example alert","recommendedAction":"nothing","category":"Exploit"}""".Replace("+", WriteOperationChecks.EscapedPlus);

	[Fact]
	public async Task Update_SendsPatchAlertsId_WithEveryProperty()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.UpdateAsync("a1", FullUpdate(), t), AlertJson);

		call.ShouldBeRequest(HttpMethod.Patch, "/api/alerts/a1", string.Empty, FullUpdateBody);
	}

	[Fact]
	public async Task Update_OmitsPropertiesThatAreNull()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.UpdateAsync("a1", new AlertUpdate { Comment = "just a note" }, t), AlertJson);

		call.ShouldBeRequest(HttpMethod.Patch, "/api/alerts/a1", string.Empty, """{"comment":"just a note"}""");
	}

	[Fact]
	public async Task Update_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.UpdateAsync("a/b", new AlertUpdate(), t), AlertJson);

		call.Uri.AbsolutePath.Should().Be("/api/alerts/a%2Fb");
		call.Body.Should().Be("{}");
	}

	[Fact]
	public async Task Update_MapsTheUpdatedAlert()
		=> AssertAlert(await TestClient.ReadAsync((c, t) => c.Alerts.UpdateAsync("a1", FullUpdate(), t), AlertJson));

	[Fact]
	public Task Update_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.UpdateAsync("a1", FullUpdate(), t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No such alert"), "No such alert");

	[Fact]
	public Task Update_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Alerts.UpdateAsync("a1", FullUpdate(), t), HttpMethod.Patch, "/api/alerts/a1", FullUpdateBody);

	[Fact]
	public Task Update_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Alerts.UpdateAsync("a1", FullUpdate(), t));

	[Fact]
	public async Task BatchUpdate_SendsPostAlertsBatchUpdate()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.BatchUpdateAsync(BatchUpdate(), t), string.Empty);

		call.ShouldBeRequest(HttpMethod.Post, "/api/alerts/batchUpdate", string.Empty, BatchBody);
	}

	[Fact]
	public async Task BatchUpdate_OmitsPropertiesThatAreNull()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.BatchUpdateAsync(new AlertBatchUpdate { AlertIds = ["a1"], Comment = "note" }, t), string.Empty);

		call.ShouldBeRequest(HttpMethod.Post, "/api/alerts/batchUpdate", string.Empty, """{"alertIds":["a1"],"comment":"note"}""");
	}

	[Fact]
	public void BatchUpdate_RejectsAnEmptyList()
	{
		var act = () => new AlertBatchUpdate { AlertIds = [] };

		act.Should().Throw<ArgumentException>().WithMessage("AlertIds must contain at least one item.*").Which.ParamName.Should().Be("AlertIds");
	}

	[Fact]
	public void BatchUpdate_RejectsANullList()
	{
		var act = () => new AlertBatchUpdate { AlertIds = null! };

		act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("AlertIds");
	}

	[Fact]
	public void BatchUpdate_EnforcesNoMaximum_BecauseNoneIsDocumented()
	{
		var ids = Enumerable.Range(0, 5000).Select(i => $"a{i}").ToList();

		new AlertBatchUpdate { AlertIds = ids }.AlertIds.Should().HaveCount(5000);
	}

	[Fact]
	public Task BatchUpdate_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.BatchUpdateAsync(BatchUpdate(), t), HttpStatusCode.BadRequest, Failure("InvalidRequestBody", "Bad ids"), "Bad ids");

	[Fact]
	public Task BatchUpdate_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Alerts.BatchUpdateAsync(BatchUpdate(), t), HttpMethod.Post, "/api/alerts/batchUpdate", BatchBody);

	[Fact]
	public Task BatchUpdate_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Alerts.BatchUpdateAsync(BatchUpdate(), t));

	[Fact]
	public async Task CreateByReference_SendsPostAlertsCreateAlertByReference()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.CreateByReferenceAsync(CreateRequest(), t), AlertJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/alerts/CreateAlertByReference", string.Empty, CreateBody);
	}

	[Fact]
	public async Task CreateByReference_MapsTheNewAlert()
		=> AssertAlert(await TestClient.ReadAsync((c, t) => c.Alerts.CreateByReferenceAsync(CreateRequest(), t), AlertJson));

	[Fact]
	public Task CreateByReference_MapsAnErrorWhenTheEventIsNotFound()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.CreateByReferenceAsync(CreateRequest(), t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No such event"), "No such event");

	[Fact]
	public Task CreateByReference_IsReplayedIdenticallyAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Alerts.CreateByReferenceAsync(CreateRequest(), t), HttpMethod.Post, "/api/alerts/CreateAlertByReference", CreateBody);

	[Fact]
	public Task CreateByReference_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Alerts.CreateByReferenceAsync(CreateRequest(), t));
}
