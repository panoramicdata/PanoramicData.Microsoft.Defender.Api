using PanoramicData.MicrosoftDefender.Api.Test.Support;
using Refit;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

/// <summary>
/// Request bodies built by Refit must be replayable through the real client, or write operations are never retried and never
/// resent after a token refresh.
/// </summary>
public class ReplayTests
{
	private const string ExpectedBody = """{"Comment":"hello","Ids":["1","2"]}""";

	public interface IWriteProbe
	{
		[Post("probe")]
		Task PostAsync([Body] Payload body, CancellationToken cancellationToken);

		[Patch("probe")]
		Task PatchAsync([Body] Payload body, CancellationToken cancellationToken);
	}

	public sealed class Payload
	{
		[System.Text.Json.Serialization.JsonPropertyName("Comment")]
		public string? Comment { get; set; }

		[System.Text.Json.Serialization.JsonPropertyName("Ids")]
		public IReadOnlyList<string> Ids { get; set; } = [];
	}

	private static Payload Body() => new() { Comment = "hello", Ids = ["1", "2"] };

	[Fact]
	public async Task JsonPost_IsRetriedOn503_WithAnIdenticalBody()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		stub.Enqueue(HttpStatusCode.Created);
		using var client = TestClient.Create(stub, o => (o.MaxRetries, o.RetryBaseDelay) = (1, TimeSpan.Zero));

		await client.For<IWriteProbe>().PostAsync(Body(), TestContext.Current.CancellationToken);

		stub.Calls.Should().HaveCount(2);
		stub.Calls.Select(c => c.Body).Should().OnlyContain(b => b == ExpectedBody);
		stub.Calls.Select(c => c.ContentType).Should().OnlyContain(t => t == "application/json");
	}

	[Fact]
	public async Task JsonPatch_IsResentOnceAfterATokenRefreshOn401_WithAnIdenticalBody()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Unauthorized);
		stub.Enqueue(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);

		await client.For<IWriteProbe>().PatchAsync(Body(), TestContext.Current.CancellationToken);

		stub.Calls.Select(c => c.Method).Should().OnlyContain(m => m == HttpMethod.Patch);
		stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
		stub.Calls.Select(c => c.Body).Should().OnlyContain(b => b == ExpectedBody);
	}

	[Fact]
	public async Task JsonPatch_IsNotRetriedOn500()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.InternalServerError);
		using var client = TestClient.Create(stub, o => (o.MaxRetries, o.RetryBaseDelay) = (3, TimeSpan.Zero));

		var act = () => client.For<IWriteProbe>().PatchAsync(Body(), TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<DefenderApiException>();
		stub.Calls.Should().ContainSingle("a 500 may follow a partial write, so PATCH is never retried on it");
	}
}
