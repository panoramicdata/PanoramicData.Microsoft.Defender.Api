using PanoramicData.MicrosoftDefender.Api.Models.Library;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using Refit;
using System.Net;
using System.Text;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The live response library: listing, uploading (multipart) and deleting.</summary>
public class LibraryTests
{
	private const string TextType = "text/plain; charset=utf-8";

	private const string JsonType = "application/json";

	private const string LibraryFileJson = """
        {
            "fileName": "script1.ps1",
            "sha256": "6e212a0db618507c44e4ec8ee7499dfef7e5767e5f8d31144df3b96fd1145caf",
            "description": "A made-up script",
            "creationTime": "2026-01-02T03:04:05Z",
            "lastUpdatedTime": "2026-01-03T03:04:05Z",
            "createdBy": "admin",
            "hasParameters": true,
            "parametersDescription": "takes one argument"
        }
        """;

	private static string Envelope(string items)
		=> $$"""{"@odata.context":"https://defender.test/api/$metadata#LibraryFiles","value":[{{items}}]}""";

	private static string Failure(string code, string message)
		=> $$$"""{"error":{"code":"{{{code}}}","message":"{{{message}}}","target":"corr-1"}}""";

	private static StreamPart Content(string text = "Write-Host hi", string fileName = "script1.ps1")
		=> new(new MemoryStream(Encoding.UTF8.GetBytes(text)), fileName, "text/plain");

	private static void AssertLibraryFile(LibraryFile f)
	{
		(f.FileName, f.Sha256, f.Description, f.CreatedBy, f.HasParameters, f.ParametersDescription)
			.Should().Be(("script1.ps1", "6e212a0db618507c44e4ec8ee7499dfef7e5767e5f8d31144df3b96fd1145caf", "A made-up script", "admin", true, "takes one argument"));
		f.CreationTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero));
		f.LastUpdatedTime.Should().Be(new DateTimeOffset(2026, 1, 3, 3, 4, 5, TimeSpan.Zero));
	}

	/// <summary>One part of a multipart body, with the boundary taken out so tests do not depend on it.</summary>
	private sealed record Part(string Name, string? FileName, string? ContentType, string Value);

	private static List<Part> ParseMultipart(string body)
	{
		var newline = "\r\n";
		var boundary = body[..body.IndexOf(newline, StringComparison.Ordinal)];
		boundary.Should().StartWith("--");
		var parts = new List<Part>();
		foreach (var raw in body.Split(boundary).Skip(1).Where(p => p.StartsWith(newline, StringComparison.Ordinal)))
		{
			var split = raw.IndexOf(newline + newline, StringComparison.Ordinal);
			var headers = raw[newline.Length..split].Split(newline);
			var value = raw[(split + (2 * newline.Length))..].TrimEnd('\r', '\n');
			var disposition = headers.Single(h => h.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase));
			var type = headers.SingleOrDefault(h => h.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase));
			parts.Add(new Part(Attribute(disposition, "name")!, Attribute(disposition, "filename"), type?["Content-Type:".Length..].Trim(), value));
		}

		return parts;
	}

	private static string? Attribute(string disposition, string name)
	{
		var marker = $"; {name}=";
		var start = disposition.IndexOf(marker, StringComparison.Ordinal);
		if (start < 0)
		{
			return null;
		}

		var rest = disposition[(start + marker.Length)..];
		var end = rest.IndexOf(';', StringComparison.Ordinal);
		return (end < 0 ? rest : rest[..end]).Trim('"');
	}

	[Fact]
	public async Task List_SendsGetLibraryFiles()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Library.ListAsync(t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/libraryfiles");
	}

	[Fact]
	public async Task List_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Library.ListAsync(t), Envelope(LibraryFileJson));

		AssertLibraryFile(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Library.ListAsync(t), Envelope("""{"fileName":"a","hasParameters":"false","creationTime":"","description":null,"extra":1}"""));

		var f = page.Value.Should().ContainSingle().Subject;
		(f.HasParameters, f.CreationTime, f.Description).Should().Be((false, null, null));
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Library.ListAsync(t), HttpStatusCode.Forbidden, Failure("Forbidden", "Missing Library.Manage"), "Missing Library.Manage");

	[Fact]
	public async Task Upload_SendsPostLibraryFiles_AsMultipartFormData_WithEveryField()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Library.UploadAsync(Content(), "A made-up script", "takes one argument", true, t), LibraryFileJson);

		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/libraryfiles");
		call.Uri.Query.Should().BeEmpty();
		call.ContentType.Should().Be("multipart/form-data");
		call.Authorization.Should().Be("Bearer token-1");
		ParseMultipart(call.Body!).Should().Equal(
		[
			new Part("File", "script1.ps1", "text/plain", "Write-Host hi"),
			new Part("Description", null, TextType, "A made-up script"),
			new Part("ParametersDescription", null, TextType, "takes one argument"),
			new Part("OverrideIfExists", null, JsonType, "true")
		]);
	}

	[Fact]
	public async Task Upload_OmitsTheOptionalFieldsThatAreNull()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Library.UploadAsync(Content(), null, null, null, t), LibraryFileJson);

		ParseMultipart(call.Body!).Should().Equal([new Part("File", "script1.ps1", "text/plain", "Write-Host hi")]);
	}

	[Fact]
	public async Task Upload_WritesFalseAsLowercaseFalse()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Library.UploadAsync(Content(), null, null, false, t), LibraryFileJson);

		ParseMultipart(call.Body!).Should().Contain(new Part("OverrideIfExists", null, JsonType, "false"));
	}

	[Fact]
	public async Task Upload_MapsTheStoredFile()
		=> AssertLibraryFile(await TestClient.ReadAsync((c, t) => c.Library.UploadAsync(Content(), null, null, null, t), LibraryFileJson));

	[Fact]
	public Task Upload_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Library.UploadAsync(Content(), null, null, null, t), HttpStatusCode.BadRequest, Failure("InvalidInput", "File too large"), "File too large");

	[Fact]
	public Task Upload_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Library.UploadAsync(Content(), null, null, null, t));

	// The retry handler only replays buffered bodies, and a multipart body is not one of them (it may wrap a read-once
	// stream). So an upload is sent exactly once: no retry on 429/503 or a connection failure, and no resend after a 401.
	[Theory]
	[InlineData(HttpStatusCode.ServiceUnavailable)]
	[InlineData(HttpStatusCode.TooManyRequests)]
	public async Task Upload_IsNotRetried_BecauseAMultipartBodyCannotBeReplayed(HttpStatusCode status)
	{
		var stub = new StubHandler();
		stub.Enqueue(status, Failure("TooManyRequests", "Slow down"));
		stub.Enqueue(HttpStatusCode.OK, LibraryFileJson);
		using var client = TestClient.Create(stub, o => (o.MaxRetries, o.RetryBaseDelay) = (3, TimeSpan.Zero));

		var act = () => client.Library.UploadAsync(Content(), null, null, null, TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<DefenderApiException>();
		stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task Upload_IsNotResentAfter401_BecauseAMultipartBodyCannotBeReplayed()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Unauthorized, Failure("Unauthorized", "expired"));
		stub.Enqueue(HttpStatusCode.OK, LibraryFileJson);
		using var client = TestClient.Create(stub);

		var act = () => client.Library.UploadAsync(Content(), null, null, null, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task Delete_SendsDeleteLibraryFilesFileName()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Library.DeleteAsync("script1.ps1", t), string.Empty);

		call.ShouldBeRequest(HttpMethod.Delete, "/api/libraryfiles/script1.ps1");
	}

	[Fact]
	public async Task Delete_EscapesTheFileNameAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Library.DeleteAsync("a/b c.ps1", t), string.Empty);

		call.Uri.AbsolutePath.Should().Be("/api/libraryfiles/a%2Fb%20c.ps1");
	}

	[Fact]
	public async Task Delete_AcceptsA204WithNoBody()
	{
		using var client = TestClient.Create(TestClient.Stub(string.Empty, HttpStatusCode.NoContent));

		await client.Library.DeleteAsync("script1.ps1", TestContext.Current.CancellationToken);
	}

	[Fact]
	public Task Delete_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Library.DeleteAsync("x", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No such file"), "No such file");

	[Fact]
	public Task Delete_IsReplayedAfter401()
		=> WriteOperationChecks.ShouldReplayAfter401Async((c, t) => c.Library.DeleteAsync("script1.ps1", t), HttpMethod.Delete, "/api/libraryfiles/script1.ps1", null);

	[Fact]
	public Task Delete_IsRefusedWhenReadOnly()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync((c, t) => c.Library.DeleteAsync("script1.ps1", t));
}
