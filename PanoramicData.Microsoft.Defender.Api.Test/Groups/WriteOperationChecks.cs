using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Checks shared by the tests of operations that change tenant data: replay after a 401, and refusal by a read-only client.</summary>
internal static class WriteOperationChecks
{
	/// <summary>The JSON escape of a plus sign (a backslash, u, 002B) that the serializer writes in an offset such as +00:00.</summary>
	public static readonly string EscapedPlus = $"{(char)92}u002B";

	/// <summary>
	/// A 401 makes the client fetch a new token and send the request once more; the second request must be the same request
	/// (method, path, query and body) with the new token.
	/// </summary>
	public static async Task ShouldReplayAfter401Async(Func<DefenderClient, CancellationToken, Task> call, HttpMethod method, string path, string? body)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Unauthorized, """{"error":{"code":"Unauthorized","message":"expired","target":"c"}}""");
		stub.Enqueue(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		await call(client, TestContext.Current.CancellationToken);

		stub.Calls.Should().HaveCount(2);
		stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
		foreach (var recorded in stub.Calls)
		{
			recorded.ShouldBeRequest(method, path, string.Empty, body);
		}
	}

	/// <summary>A read-only client refuses the operation before sending anything or fetching a token.</summary>
	public static async Task ShouldBeRefusedWhenReadOnlyAsync(Func<DefenderClient, CancellationToken, Task> call)
	{
		var stub = new StubHandler();
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		var act = () => call(client, TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<DefenderReadOnlyException>();
		stub.Calls.Should().BeEmpty();
	}
}
