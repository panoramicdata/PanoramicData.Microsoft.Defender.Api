using System.Net;
using System.Net.Http.Headers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Support;

internal sealed record RecordedCall(HttpMethod Method, Uri Uri, string? Body, HttpRequestHeaders Headers)
{
	/// <summary>The request Content-Type media type, or <see langword="null"/> without a body.</summary>
	public string? ContentType { get; init; }

	/// <summary>
	/// The Authorization header as it was when the request arrived. <see cref="Headers"/> is the live collection, which a
	/// handler that resends the same request (retry, token refresh) changes afterwards.
	/// </summary>
	public string? Authorization { get; init; }
}

/// <summary>A transport that records each request and answers from a queue.</summary>
internal sealed class StubHandler : HttpMessageHandler
{
	private readonly Queue<Func<HttpResponseMessage>> _responses = new();

	public List<RecordedCall> Calls { get; } = [];

	public void Enqueue(HttpStatusCode status, string json = "{}", Action<HttpResponseMessage>? configure = null)
		=> _responses.Enqueue(() =>
		{
			var response = new HttpResponseMessage(status)
			{
				Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
			};
			configure?.Invoke(response);
			return response;
		});

	/// <summary>Queues a binary body (for example a gzip file) with the given media type.</summary>
	public void EnqueueBytes(byte[] bytes, string contentType)
		=> _responses.Enqueue(() =>
		{
			var content = new ByteArrayContent(bytes);
			content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
		});

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Each body is read exactly once, as a real transport would.
		var content = request.Content;
		var body = content is null ? null : await content.ReadAsStringAsync(cancellationToken);
		Calls.Add(new RecordedCall(request.Method, request.RequestUri!, body, request.Headers)
		{
			ContentType = content?.Headers.ContentType?.MediaType,
			Authorization = request.Headers.Authorization?.ToString()
		});
		return _responses.Dequeue()();
	}
}
