using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Models.Vulnerabilities;
using PanoramicData.MicrosoftDefender.Api.Serialization;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

/// <summary>"Via files" exports: the response model, and downloading signed links without the bearer token.</summary>
public class ExportTests
{
	private const string FileOne = "https://store.blob.test/exports/one.json.gz?sv=2024&sig=SECRET-ONE";
	private const string FileTwo = "https://store.blob.test/exports/two.json.gz?sv=2024&sig=SECRET-TWO";

	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private static byte[] Gzip(string text)
	{
		using var buffer = new MemoryStream();
		using (var gzip = new GZipStream(buffer, CompressionLevel.Fastest))
		{
			gzip.Write(Encoding.UTF8.GetBytes(text));
		}

		return buffer.ToArray();
	}

	private static (DefenderClient Client, StubHandler Api, StubHandler Downloads, FakeCredential Credential, CapturingLogger Logger) Create(int retries = 0)
	{
		var clock = TestClient.NewClock();
		var credential = new FakeCredential(clock);
		var logger = new CapturingLogger();
		var api = new StubHandler();
		var downloads = new StubHandler();
		var client = new DefenderClient(TestClient.Options(clock, o =>
		{
			o.Credential = credential;
			o.Logger = logger;
			o.MaxRetries = retries;
			o.RetryBaseDelay = TimeSpan.Zero;
		}), api, downloads);
		return (client, api, downloads, credential, logger);
	}

	private static ExportFilesResponse Export(params string[] files) => new() { ExportFiles = files };

	[Theory]
	[InlineData("exportFiles")]
	[InlineData("ExportFiles")]
	[InlineData("Export files")]
	public void Response_ReadsEitherSpellingOfTheFileList(string key)
	{
		var json = $$"""{"{{key}}":["{{FileOne}}","{{FileTwo}}"],"generatedTime":"2026-11-25T10:00:00Z"}""";

		var response = JsonSerializer.Deserialize<ExportFilesResponse>(json, DefenderJson.Options)!;

		response.ExportFiles.Should().Equal(FileOne, FileTwo);
		response.GeneratedTime.Should().Be(new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero));
	}

	[Theory]
	[InlineData("""{"generatedTime":null}""")]
	[InlineData("""{"Export files":null,"exportFiles":null}""")]
	public void Response_WithoutFiles_HasAnEmptyList(string json)
		=> JsonSerializer.Deserialize<ExportFilesResponse>(json, DefenderJson.Options)!.ExportFiles.Should().BeEmpty();

	[Fact]
	public void Response_WritesTheCanonicalSpellingOnly()
	{
		var json = JsonSerializer.Serialize(new ExportFilesResponse { ExportFiles = [FileOne] }, DefenderJson.Options);

		json.Should().StartWith("""{"exportFiles":["https://store.blob.test/""").And.NotContain("Export files");
	}

	[Fact]
	public void Response_ToString_NeverRevealsALink()
	{
		var text = new ExportFilesResponse { ExportFiles = [FileOne, FileTwo], GeneratedTime = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero) }.ToString();

		text.Should().Be("ExportFilesResponse { Files = 2, GeneratedTime = 2026-01-02T03:04:05.0000000+00:00 }");
		text.Should().NotContain("SECRET").And.NotContain("blob.test");
	}

	[Fact]
	public async Task Stream_ReadsEveryFileInOrder_SkippingBlankAndNullLines()
	{
		var (client, api, downloads, credential, _) = Create();
		using var _c = client;
		downloads.EnqueueBytes(Gzip("{\"id\":\"CVE-1\",\"severity\":\"High\"}\n\n{\"id\":\"CVE-2\"}\nnull\n"), "application/octet-stream");
		downloads.EnqueueBytes(Gzip("{\"id\":\"CVE-3\"}"), "application/octet-stream");

		var records = await client.StreamExportAsync<Vulnerability>(Export(FileOne, FileTwo), Ct).ToListAsync(Ct);

		records.Select(r => r.Id).Should().Equal("CVE-1", "CVE-2", "CVE-3");
		records[0].Severity.Should().Be(VulnerabilitySeverity.High);
		downloads.Calls.Select(c => c.Uri.AbsoluteUri).Should().Equal(FileOne, FileTwo);
		downloads.Calls.Should().OnlyContain(c => c.Method == HttpMethod.Get);
		api.Calls.Should().BeEmpty();
		credential.Issued.Should().Be(0, "downloads never ask for a token");
	}

	[Fact]
	public async Task Stream_NeverSendsAnAuthorizationHeader_ToTheStorageHost()
	{
		var (client, _, downloads, _, _) = Create();
		using var _c = client;
		downloads.EnqueueBytes(Gzip("{}"), "application/octet-stream");

		await client.StreamExportAsync<Vulnerability>(Export(FileOne), Ct).ToListAsync(Ct);

		downloads.Calls.Should().ContainSingle();
		downloads.Calls[0].Authorization.Should().BeNull();
		downloads.Calls[0].Headers.Authorization.Should().BeNull();
		downloads.Calls[0].Headers.Contains("Authorization").Should().BeFalse();
	}

	[Fact]
	public async Task Stream_WithNoFiles_YieldsNothing()
	{
		var (client, _, downloads, _, _) = Create();
		using var _c = client;

		(await client.StreamExportAsync<Vulnerability>(Export(), Ct).ToListAsync(Ct)).Should().BeEmpty();
		downloads.Calls.Should().BeEmpty();
	}

	[Fact]
	public async Task Stream_RequiresAResponse()
	{
		var (client, _, _, _, _) = Create();
		using var _c = client;

		await ((Func<Task>)(() => client.StreamExportAsync<Vulnerability>(null!, Ct).ToListAsync(Ct).AsTask())).Should().ThrowAsync<ArgumentNullException>();
	}

	[Theory]
	[InlineData("http://store.blob.test/one.json.gz?sig=SECRET-HTTP")]
	[InlineData("/relative/one.json.gz?sig=SECRET-REL")]
	[InlineData("file:///etc/passwd?sig=SECRET-FILE")]
	public async Task Stream_RefusesALinkThatIsNotHttps_BeforeDownloadingAnything(string bad)
	{
		var (client, _, downloads, _, _) = Create();
		using var _c = client;

		var act = () => client.StreamExportAsync<Vulnerability>(Export(FileOne, bad), Ct).ToListAsync(Ct).AsTask();

		var thrown = await act.Should().ThrowAsync<DefenderLinkException>();
		thrown.Which.Message.Should().Be("An export file link is not an absolute https URL.");
		downloads.Calls.Should().BeEmpty("every link is checked before the first download");
	}

	[Fact]
	public async Task Stream_RefusesALinkWithCredentials_BeforeDownloadingOrLoggingAnything()
	{
		var (client, _, downloads, _, logger) = Create();
		using var _c = client;

		var act = () => client.StreamExportAsync<Vulnerability>(Export("https://user:hunter2@store.blob.test/one.json.gz?sig=SECRET"), Ct).ToListAsync(Ct).AsTask();

		var thrown = await act.Should().ThrowAsync<DefenderLinkException>();
		thrown.Which.Message.Should().Be("An export file link contains credentials.");
		downloads.Calls.Should().BeEmpty();
		logger.Messages.Should().NotContain(m => m.Contains("hunter2", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData(HttpStatusCode.Forbidden, "<Error><Code>AuthenticationFailed</Code><Message>sig=SECRET-ONE</Message></Error>", "HTTP 403 (Forbidden)")]
	[InlineData(HttpStatusCode.Found, "", "HTTP 302 (Found)")]
	[InlineData(HttpStatusCode.NotFound, "not json", "HTTP 404 (Not Found)")]
	public async Task Stream_ADownloadFailure_RaisesAnExceptionThatNeverRevealsTheSignature(HttpStatusCode status, string body, string message)
	{
		var (client, _, downloads, _, _) = Create();
		using var _c = client;
		downloads.Enqueue(status, body);

		var act = () => client.StreamExportAsync<Vulnerability>(Export(FileOne), Ct).ToListAsync(Ct).AsTask();

		var thrown = (await act.Should().ThrowAsync<DefenderApiException>()).Which;
		thrown.StatusCode.Should().Be(status);
		thrown.Message.Should().Be(message);
		thrown.ToString().Should().NotContain("SECRET");
	}

	[Fact]
	public async Task Stream_LogsOnlyThePath_NeverTheSignature()
	{
		var (client, _, downloads, _, logger) = Create(retries: 1);
		using var _c = client;
		downloads.Enqueue(HttpStatusCode.ServiceUnavailable);
		downloads.EnqueueBytes(Gzip("{}"), "application/octet-stream");

		await client.StreamExportAsync<Vulnerability>(Export(FileOne), Ct).ToListAsync(Ct);

		logger.Messages.Should().NotBeEmpty().And.OnlyContain(m => !m.Contains("SECRET") && !m.Contains("sig=") && !m.Contains('?'));
		logger.Messages.Should().Contain(m => m.Contains("https://store.blob.test/exports/one.json.gz", StringComparison.Ordinal));
	}

	[Fact]
	public async Task Stream_CancellationStopsTheEnumeration()
	{
		var (client, _, downloads, _, _) = Create();
		using var _c = client;
		downloads.EnqueueBytes(Gzip("{\"id\":\"1\"}\n{\"id\":\"2\"}"), "application/octet-stream");
		using var cts = new CancellationTokenSource();

		var act = async () =>
		{
			await foreach (var _ in client.StreamExportAsync<Vulnerability>(Export(FileOne), cts.Token))
			{
				await cts.CancelAsync();
			}
		};

		await act.Should().ThrowAsync<OperationCanceledException>();
	}
}
