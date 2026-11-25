using Microsoft.Extensions.Time.Testing;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Support;

internal static class TestClient
{
	public const string BaseUrl = "https://defender.test/api/";

	/// <summary>The fixed clock every test client uses unless a test supplies its own.</summary>
	public static FakeTimeProvider NewClock() => new(new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero));

	public static DefenderClientOptions Options(TimeProvider clock, Action<DefenderClientOptions>? tweak = null)
	{
		var options = new DefenderClientOptions
		{
			Credential = new FakeCredential(clock),
			BaseUri = new Uri(BaseUrl),
			TimeProvider = clock,
			MaxRetries = 0
		};
		tweak?.Invoke(options);
		return options;
	}

	public static DefenderClient Create(StubHandler stub, Action<DefenderClientOptions>? tweak = null)
		=> new(Options(NewClock(), tweak), stub);

	/// <summary>A stub that answers one request with <paramref name="json"/>.</summary>
	public static StubHandler Stub(string json, HttpStatusCode status = HttpStatusCode.OK)
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	/// <summary>
	/// Sends <paramref name="call"/> through a client whose transport answers <paramref name="response"/>, and returns the
	/// one request it made.
	/// </summary>
	public static async Task<RecordedCall> CaptureAsync(Func<DefenderClient, CancellationToken, Task> call, string response)
	{
		var stub = Stub(response);
		using var client = Create(stub);
		await call(client, TestContext.Current.CancellationToken);
		return stub.Calls.Should().ContainSingle().Subject;
	}

	/// <summary>Sends <paramref name="call"/> through a client whose transport answers <paramref name="response"/>, and returns what it read.</summary>
	public static async Task<T> ReadAsync<T>(Func<DefenderClient, CancellationToken, Task<T>> call, string response)
	{
		using var client = Create(Stub(response));
		return await call(client, TestContext.Current.CancellationToken);
	}

	/// <summary>Asserts that <paramref name="call"/> raises <see cref="DefenderApiException"/> when Defender answers <paramref name="status"/> and <paramref name="response"/>.</summary>
	public static async Task ShouldFailAsync(Func<DefenderClient, CancellationToken, Task> call, HttpStatusCode status, string response, string message)
	{
		using var client = Create(Stub(response, status));
		var thrown = await ((Func<Task>)(() => call(client, TestContext.Current.CancellationToken))).Should().ThrowAsync<DefenderApiException>();
		thrown.Which.StatusCode.Should().Be(status);
		thrown.Which.Message.Should().Be(message);
	}
}
