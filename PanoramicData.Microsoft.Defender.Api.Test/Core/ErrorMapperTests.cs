using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;
using System.Net.Http.Headers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public class ErrorMapperTests
{
	private static readonly DateTimeOffset Now = new(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);

	private static async Task<DefenderApiException> MapAsync(HttpStatusCode status, HttpContent content, string? reason = null, Action<HttpResponseMessage>? configure = null)
	{
		using var response = new HttpResponseMessage(status) { Content = content, ReasonPhrase = reason };
		configure?.Invoke(response);
		var exception = await DefenderErrorMapper.CreateAsync(response, TestClient.NewClock());
		return exception.Should().BeOfType<DefenderApiException>().Subject;
	}

	private static Task<DefenderApiException> MapAsync(string body, HttpStatusCode status = HttpStatusCode.BadRequest)
		=> MapAsync(status, new StringContent(body));

	[Fact]
	public async Task Success_IsNotAnError()
	{
		using var response = new HttpResponseMessage(HttpStatusCode.NoContent);

		(await DefenderErrorMapper.CreateAsync(response, TimeProvider.System)).Should().BeNull();
	}

	[Fact]
	public async Task ErrorObject_ProvidesCodeMessageAndCorrelationId()
	{
		var exception = await MapAsync(
			"""{"error":{"code":"Forbidden","message":"Missing permission","target":"3a1f-77"}}""",
			HttpStatusCode.Forbidden);

		exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		exception.Code.Should().Be("Forbidden");
		exception.Message.Should().Be("Missing permission");
		exception.CorrelationId.Should().Be("3a1f-77");
		exception.RetryAfter.Should().BeNull();
	}

	[Fact]
	public async Task ErrorText_IsTheMessage()
	{
		var exception = await MapAsync("""{"error":"plain text"}""");

		exception.Message.Should().Be("plain text");
		exception.Code.Should().BeNull();
	}

	[Fact]
	public async Task PartialErrorObject_KeepsWhatIsThere()
	{
		var exception = await MapAsync("""{"error":{"code":"ODataError","message":7}}""");

		exception.Code.Should().Be("ODataError");
		exception.Message.Should().Be("HTTP 400 (Bad Request)");
		exception.CorrelationId.Should().BeNull();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("Internal error")]
	[InlineData("[1,2]")]
	[InlineData("{not json")]
	[InlineData("{}")]
	[InlineData("""{"error":null}""")]
	[InlineData("""{"error":[]}""")]
	[InlineData("<html><body>Bad gateway<br></body></html>")]
	public async Task NoReadableError_FallsBackToTheStatus(string body)
	{
		var exception = await MapAsync(body, HttpStatusCode.BadGateway);

		exception.Message.Should().Be("HTTP 502 (Bad Gateway)");
		exception.Code.Should().BeNull();
		exception.CorrelationId.Should().BeNull();
	}

	[Theory]
	[InlineData(null, "HTTP 599 (599)")]
	[InlineData("Custom", "HTTP 599 (Custom)")]
	public async Task Fallback_NamesTheReasonPhraseOrStatus(string? reason, string expected)
		=> (await MapAsync((HttpStatusCode)599, new StringContent(""), reason)).Message.Should().Be(expected);

	[Fact]
	public async Task RetryAfter_IsReadAsSeconds_OrAsADate()
	{
		var seconds = await MapAsync(HttpStatusCode.TooManyRequests, new StringContent(""), configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(42)));
		var date = await MapAsync(HttpStatusCode.TooManyRequests, new StringContent(""), configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(Now.AddSeconds(90)));
		var past = await MapAsync(HttpStatusCode.TooManyRequests, new StringContent(""), configure: r => r.Headers.RetryAfter = new RetryConditionHeaderValue(Now.AddSeconds(-90)));

		seconds.RetryAfter.Should().Be(TimeSpan.FromSeconds(42));
		date.RetryAfter.Should().Be(TimeSpan.FromSeconds(90));
		past.RetryAfter.Should().Be(TimeSpan.Zero);
	}

	/// <summary>A body that cannot be read at all (a lost connection) still maps to an exception naming the status.</summary>
	[Fact]
	public async Task UnreadableBody_FallsBackToTheStatus()
	{
		var exception = await MapAsync(HttpStatusCode.InternalServerError, new ThrowingContent());

		exception.Message.Should().Be("HTTP 500 (Internal Server Error)");
	}

	private sealed class ThrowingContent : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => throw new HttpRequestException("lost");

		protected override bool TryComputeLength(out long length)
		{
			length = 0;
			return false;
		}

		protected override Task<Stream> CreateContentReadStreamAsync() => throw new HttpRequestException("lost");
	}
}
