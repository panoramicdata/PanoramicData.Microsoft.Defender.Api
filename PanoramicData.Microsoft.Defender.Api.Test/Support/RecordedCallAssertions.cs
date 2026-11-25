namespace PanoramicData.MicrosoftDefender.Api.Test.Support;

/// <summary>Assertions on a request recorded by <see cref="StubHandler"/>, shared by every endpoint group's tests.</summary>
internal static class RecordedCallAssertions
{
	/// <summary>
	/// Asserts the method, escaped path, raw escaped query and body of a recorded request. The path and query are compared as
	/// sent on the wire (not unescaped), so a literal <c>$</c> in a query name and the escaping of values are pinned. A body
	/// must be JSON.
	/// </summary>
	public static void ShouldBeRequest(this RecordedCall call, HttpMethod method, string path, string query = "", string? body = null)
	{
		call.Method.Should().Be(method);
		call.Uri.AbsolutePath.Should().Be(path);
		call.Uri.Query.Should().Be(query);
		call.Body.Should().Be(body);
		if (body is not null)
		{
			call.ContentType.Should().Be("application/json");
		}
	}
}
