using PanoramicData.MicrosoftDefender.Api.Models.Indicators;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

public partial class IndicatorsTests
{
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
}
