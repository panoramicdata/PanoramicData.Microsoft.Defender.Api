using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Reading the records of the "via files" exports through the typed stream helpers, without sending the token to storage.</summary>
public partial class AssessmentExportsTests
{
	private const string FileLink = "https://store.blob.test/exports/a.json.gz?sv=1&sig=SECRET-A";

	private static async Task<IReadOnlyList<T>> StreamAsync<T>(Func<DefenderClient, ExportFilesResponse, CancellationToken, IAsyncEnumerable<T>> stream, string lines)
	{
		var clock = TestClient.NewClock();
		var downloads = new StubHandler();
		downloads.EnqueueBytes(Gzip(lines), "application/octet-stream");
		using var client = new DefenderClient(TestClient.Options(clock), new StubHandler(), downloads);

		var records = await stream(client, new ExportFilesResponse { ExportFiles = [FileLink] }, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

		downloads.Calls.Should().ContainSingle().Which.Authorization.Should().BeNull("the bearer token never goes to the storage host");
		return records;
	}

	[Fact]
	public async Task StreamHelpers_ReadTheRecordsOfEachExport()
	{
		const string line = "{\"deviceId\":\"d1\",\"extra\":1}\n";

		(await StreamAsync((c, e, t) => c.StreamSecureConfigurationAssessmentsAsync(e, t), line)).Should().ContainSingle().Which.DeviceId.Should().Be("d1");
		(await StreamAsync((c, e, t) => c.StreamSoftwareInventoryAssessmentsAsync(e, t), line)).Should().ContainSingle().Which.DeviceId.Should().Be("d1");
		(await StreamAsync((c, e, t) => c.StreamSoftwareVulnerabilityAssessmentsAsync(e, t), line)).Should().ContainSingle().Which.DeviceId.Should().Be("d1");
		(await StreamAsync((c, e, t) => c.StreamNonCpeSoftwareInventoryAssessmentsAsync(e, t), line)).Should().ContainSingle().Which.DeviceId.Should().Be("d1");
		(await StreamAsync((c, e, t) => c.StreamBrowserExtensionAssessmentsAsync(e, t), line)).Should().ContainSingle().Which.DeviceId.Should().Be("d1");
		(await StreamAsync((c, e, t) => c.StreamCertificateAssessmentsAsync(e, t), line)).Should().ContainSingle().Which.DeviceId.Should().Be("d1");
		(await StreamAsync((c, e, t) => c.StreamBaselineComplianceAssessmentsAsync(e, t), line)).Should().ContainSingle().Which.DeviceId.Should().Be("d1");
	}

	[Fact]
	public async Task StreamInfoGathering_KeepsTheUndocumentedColumns()
	{
		var records = await StreamAsync((c, e, t) => c.StreamInfoGatheringAssessmentsAsync(e, t), "{\"deviceId\":\"d1\",\"someColumn\":7,\"other\":\"x\"}\n{\"deviceId\":\"d2\"}");

		records.Select(r => r.DeviceId).Should().Equal("d1", "d2");
		records[0].AdditionalProperties!["someColumn"].GetInt32().Should().Be(7);
		records[0].AdditionalProperties!["other"].GetString().Should().Be("x");
		records[1].AdditionalProperties.Should().BeNullOrEmpty();
	}

	[Fact]
	public async Task ExportThenStream_NeverSendsTheTokenToStorage_AndNeverLogsTheSignature()
	{
		var clock = TestClient.NewClock();
		var api = new StubHandler();
		api.Enqueue(System.Net.HttpStatusCode.OK, FilesResponse);
		var downloads = new StubHandler();
		downloads.EnqueueBytes(Gzip("{\"deviceId\":\"d9\",\"thumbprint\":\"AA\"}"), "application/octet-stream");
		var logger = new CapturingLogger();
		using var client = new DefenderClient(TestClient.Options(clock, o => o.Logger = logger), api, downloads);
		var ct = TestContext.Current.CancellationToken;

		var export = await client.AssessmentExports.ExportCertificatesAsync(new ExportViaFilesOptions { SasValidHours = 3 }, ct);
		var certificates = await client.StreamCertificateAssessmentsAsync(export, ct).ToListAsync(ct);

		certificates.Should().ContainSingle().Which.Thumbprint.Should().Be("AA");
		api.Calls.Should().ContainSingle().Which.Authorization.Should().Be("Bearer token-1");
		downloads.Calls.Should().ContainSingle().Which.Authorization.Should().BeNull();
		logger.Messages.Should().OnlyContain(m => !m.Contains("SECRET", StringComparison.Ordinal));
	}
}
