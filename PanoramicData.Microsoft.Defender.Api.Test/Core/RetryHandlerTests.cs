using Microsoft.Extensions.Time.Testing;
using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public partial class RetryHandlerTests
{
	private const string Url = "https://defender.test/api/vulnerabilities?$filter=secret%20filter";

	/// <summary>A retry handler over a stub, recording the delays it would wait instead of waiting.</summary>
	private sealed class Harness : IDisposable
	{
		private readonly HandlerHarness _harness;

		public Harness(Action<DefenderClientOptions>? tweak = null, HttpMessageHandler? inner = null)
		{
			var options = new DefenderClientOptions
			{
				MaxRetries = 3,
				RetryBaseDelay = TimeSpan.FromSeconds(1),
				MaxRetryDelay = TimeSpan.FromSeconds(30),
				Logger = Logger,
				TimeProvider = Clock
			};
			tweak?.Invoke(options);
			var handler = new RetryHandler(options)
			{
				Delay = (delay, _) =>
				{
					Delays.Add(delay);
					return Task.CompletedTask;
				}
			};
			_harness = new HandlerHarness(handler, inner);
		}

		public FakeTimeProvider Clock { get; } = TestClient.NewClock();

		public StubHandler Stub => _harness.Stub;

		public List<TimeSpan> Delays { get; } = [];

		public CapturingLogger Logger { get; } = new();

		public Harness Respond(params HttpStatusCode[] statuses)
		{
			foreach (var status in statuses)
			{
				Stub.Enqueue(status);
			}

			return this;
		}

		public Task<HttpResponseMessage> SendAsync(HttpMethod method, HttpContent? content = null)
			=> _harness.SendAsync(method, Url, content);

		public void Dispose() => _harness.Dispose();
	}

	[Theory]
	[InlineData("GET", HttpStatusCode.InternalServerError)]
	[InlineData("PUT", HttpStatusCode.BadGateway)]
	[InlineData("DELETE", HttpStatusCode.GatewayTimeout)]
	[InlineData("HEAD", HttpStatusCode.InternalServerError)]
	[InlineData("POST", HttpStatusCode.ServiceUnavailable)]
	[InlineData("POST", HttpStatusCode.TooManyRequests)]
	[InlineData("PATCH", HttpStatusCode.TooManyRequests)]
	public async Task RetriesTransientFailures_UntilSuccess(string method, HttpStatusCode status)
	{
		using var harness = new Harness().Respond(status, status, HttpStatusCode.OK);

		using var response = await harness.SendAsync(new HttpMethod(method));

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls.Should().HaveCount(3);
		harness.Delays.Should().Equal(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
	}

	[Theory]
	[InlineData("POST", HttpStatusCode.InternalServerError)]
	[InlineData("PATCH", HttpStatusCode.BadGateway)]
	[InlineData("GET", HttpStatusCode.NotFound)]
	[InlineData("GET", HttpStatusCode.Unauthorized)]
	[InlineData("GET", HttpStatusCode.OK)]
	public async Task DoesNotRetry_OtherResponses(string method, HttpStatusCode status)
	{
		using var harness = new Harness().Respond(status);

		using var response = await harness.SendAsync(new HttpMethod(method));

		response.StatusCode.Should().Be(status);
		harness.Stub.Calls.Should().ContainSingle();
		harness.Delays.Should().BeEmpty();
	}

	[Fact]
	public async Task StopsAfterMaxRetries_ReturningTheLastResponse()
	{
		using var harness = new Harness(o => o.MaxRetries = 2).Respond(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, HttpStatusCode.BadGateway);

		using var response = await harness.SendAsync(HttpMethod.Get);

		response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
		harness.Stub.Calls.Should().HaveCount(3);
	}

	[Fact]
	public async Task BackOff_DoublesUpToTheMaximum()
	{
		using var harness = new Harness(o => (o.MaxRetries, o.RetryBaseDelay, o.MaxRetryDelay) = (5, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10)))
			.Respond([.. Enumerable.Repeat(HttpStatusCode.ServiceUnavailable, 6)]);

		using var response = await harness.SendAsync(HttpMethod.Get);

		harness.Delays.Should().Equal(
			TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
	}

	[Fact]
	public async Task BackOff_ABaseAboveTheMaximumIsCapped()
	{
		using var harness = new Harness(o => (o.MaxRetries, o.RetryBaseDelay, o.MaxRetryDelay) = (1, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10)))
			.Respond(HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Get);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(10));
	}

	[Theory]
	[InlineData(7, 7)]
	[InlineData(0, 0)]
	[InlineData(30, 30)]
	public async Task RetryAfterSeconds_IsHonoured(int seconds, int expected)
	{
		using var harness = new Harness();
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Post);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Delays.Should().Equal(TimeSpan.FromSeconds(expected));
	}

	[Theory]
	[InlineData(20, 20)]
	[InlineData(-90, 0)]
	public async Task RetryAfterDate_IsMeasuredAgainstTheInjectedClock(int secondsFromNow, int expected)
	{
		using var harness = new Harness();
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(harness.Clock.GetUtcNow().AddSeconds(secondsFromNow)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Get);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(expected));
	}

	[Theory]
	[InlineData(3600, false)]
	[InlineData(86400, true)]
	public async Task RetryAfterBeyondTheCap_IsNotCutShort_TheResponseIsReturned(int seconds, bool asDate)
	{
		using var harness = new Harness();
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, configure: r => r.Headers.RetryAfter = asDate
			? new RetryConditionHeaderValue(harness.Clock.GetUtcNow().AddSeconds(seconds))
			: new RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds)));

		using var response = await harness.SendAsync(HttpMethod.Get);

		response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
		harness.Stub.Calls.Should().ContainSingle("retrying sooner than the server asked would only be throttled again");
		harness.Delays.Should().BeEmpty();
		harness.Logger.Messages.Should().ContainSingle(m => m.Contains("longer than the configured maximum", StringComparison.Ordinal));
	}

	[Fact]
	public async Task RetryAfterBeyondTheCap_IsNotCutShort_WithoutALogger()
	{
		using var harness = new Harness(o => o.Logger = null);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromHours(1)));

		using var response = await harness.SendAsync(HttpMethod.Get);

		response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
		harness.Stub.Calls.Should().ContainSingle();
	}

	public static TheoryData<string, Func<HttpContent>> ReplayableContent => new()
	{
		{ "a=1&b=%2A", () => new FormUrlEncodedContent([new("a", "1"), new("b", "*")]) },
		{ """{"x":1}""", () => new StringContent("""{"x":1}""", Encoding.UTF8, "application/json") },
		{ """{"x":1}""", () => System.Net.Http.Json.JsonContent.Create(new { x = 1 }) },
		{ "raw", () => new ByteArrayContent("raw"u8.ToArray()) },
		{ "memory", () => new ReadOnlyMemoryContent("memory"u8.ToArray()) }
	};

	[Theory]
	[MemberData(nameof(ReplayableContent))]
	public async Task BufferedBodies_AreReplayedIdentically(string body, Func<HttpContent> content)
	{
		using var harness = new Harness().Respond(HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Post, content());

		harness.Stub.Calls.Select(c => c.Body).Should().Equal(body, body);
	}

	[Fact]
	public async Task StreamBodies_AreNotRetried()
	{
		using var harness = new Harness().Respond(HttpStatusCode.ServiceUnavailable);

		using var response = await harness.SendAsync(HttpMethod.Post, new StreamContent(new MemoryStream("data"u8.ToArray())));

		response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Calls.Should().ContainSingle();
	}

	[Theory]
	[InlineData(null, true)]
	[InlineData(typeof(StringContent), true)]
	[InlineData(typeof(StreamContent), false)]
	public void IsReplayable_OnlyForBufferedContent(Type? type, bool expected)
	{
		using HttpContent? content = type == typeof(StringContent) ? new StringContent("x")
			: type == typeof(StreamContent) ? new StreamContent(Stream.Null)
			: null;

		RetryHandler.IsReplayable(content).Should().Be(expected);
	}
}
