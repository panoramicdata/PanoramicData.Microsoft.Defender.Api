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
		var stub = await SendAfter401Async(call, HttpStatusCode.OK, "{}");

		stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
		foreach (var recorded in stub.Calls)
		{
			recorded.ShouldBeRequest(method, path, string.Empty, body);
		}
	}

	/// <summary>A read-only client refuses the operation before sending anything or fetching a token.</summary>
	public static async Task ShouldBeRefusedWhenReadOnlyAsync(Func<DefenderClient, CancellationToken, Task> call, string name = "the operation")
	{
		var stub = new StubHandler();
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		var act = () => call(client, TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<DefenderReadOnlyException>($"{name} changes state");
		stub.Calls.Should().BeEmpty(name);
	}

	/// <summary>Every named operation is refused by a read-only client before anything is sent.</summary>
	public static async Task ShouldBeRefusedWhenReadOnlyAsync(IEnumerable<(string Name, Func<DefenderClient, CancellationToken, Task> Call)> operations)
	{
		foreach (var (name, call) in operations)
		{
			await ShouldBeRefusedWhenReadOnlyAsync(call, name);
		}
	}

	/// <summary>Every named operation, after a 401, sends the same non-empty body again with the new token.</summary>
	public static async Task ShouldResendIdenticalBodiesAfter401Async(IEnumerable<(string Name, Func<DefenderClient, CancellationToken, Task> Call)> operations, HttpStatusCode success, string responseBody)
	{
		foreach (var (name, call) in operations)
		{
			var stub = await SendAfter401Async(call, success, responseBody);

			stub.Calls[1].Body.Should().NotBeNullOrEmpty(name).And.Be(stub.Calls[0].Body, name);
			stub.Calls[1].Authorization.Should().Be("Bearer token-2", name);
		}
	}

	private static async Task<StubHandler> SendAfter401Async(Func<DefenderClient, CancellationToken, Task> call, HttpStatusCode success, string responseBody)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Unauthorized, """{"error":{"code":"Unauthorized","message":"expired","target":"c"}}""");
		stub.Enqueue(success, responseBody);
		using var client = TestClient.Create(stub);

		await call(client, TestContext.Current.CancellationToken);

		stub.Calls.Should().HaveCount(2);
		return stub;
	}
}
