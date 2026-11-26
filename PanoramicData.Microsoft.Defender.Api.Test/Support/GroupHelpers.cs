using System.IO.Compression;
using System.Net;
using System.Text;

namespace PanoramicData.MicrosoftDefender.Api.Test.Support;

/// <summary>Helpers shared by the tests of the vulnerability-management endpoint groups.</summary>
internal static class GroupHelpers
{
	/// <summary>An OData collection envelope holding <paramref name="items"/> (raw JSON, comma separated), with an optional next link.</summary>
	public static string Envelope(string items, string? nextLink = null)
		=> "{\"@odata.context\":\"https://defender.test/api/$metadata#x\",\"value\":[" + items + "]"
			+ (nextLink is null ? "" : ",\"@odata.nextLink\":\"" + nextLink + "\"") + "}";

	/// <summary>The gzip bytes of <paramref name="text"/>, as the export files are served.</summary>
	public static byte[] Gzip(string text)
	{
		using var buffer = new MemoryStream();
		using (var gzip = new GZipStream(buffer, CompressionLevel.Fastest))
		{
			gzip.Write(Encoding.UTF8.GetBytes(text));
		}

		return buffer.ToArray();
	}

	/// <summary>Asserts that <paramref name="call"/> sends one GET to <paramref name="path"/> with <paramref name="query"/> and that the call carried a bearer token.</summary>
	public static async Task ShouldGetAsync(Func<DefenderClient, CancellationToken, Task> call, string response, string path, string query = "")
	{
		var recorded = await TestClient.CaptureAsync(call, response);

		recorded.ShouldBeRequest(HttpMethod.Get, path, query);
		recorded.Authorization.Should().Be("Bearer token-1");
	}

	/// <summary>Asserts that <paramref name="call"/> raises the mapped API exception for a 403 answer.</summary>
	public static Task ShouldFailWith403Async(Func<DefenderClient, CancellationToken, Task> call)
		=> TestClient.ShouldFailAsync(call, HttpStatusCode.Forbidden,
			"""{"error":{"code":"Forbidden","message":"Missing permission","target":"corr-1"}}""", "Missing permission");

	/// <summary>
	/// Enumerates a high-level helper against one empty page and asserts the single request it made, so the helper's own
	/// wiring (path and options) is pinned.
	/// </summary>
	public static async Task ShouldEnumerateAsync<T>(Func<DefenderClient, CancellationToken, IAsyncEnumerable<T>> enumerate, string path, string query = "")
	{
		var stub = TestClient.Stub(Envelope(""));
		using var client = TestClient.Create(stub);

		var items = await enumerate(client, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

		items.Should().BeEmpty();
		stub.Calls.Should().ContainSingle().Which.ShouldBeRequest(HttpMethod.Get, path, query);
	}
}
