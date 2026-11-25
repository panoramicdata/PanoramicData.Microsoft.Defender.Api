using System.Net;

namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>Raised when Defender returns a non-success response.</summary>
/// <remarks>
/// Many list operations answer 404 (code <c>ResourceNotFound</c>) when there is nothing to list, for example when no
/// machine has reported recently, so a 404 from a list call can mean "empty" rather than "wrong path".
/// </remarks>
/// <param name="statusCode">The HTTP status code.</param>
/// <param name="code">The error code Defender returned (such as <c>Forbidden</c>), or <see langword="null"/>.</param>
/// <param name="message">The exception message: Defender's error message, or a fallback naming the status.</param>
/// <param name="correlationId">The request correlation id Defender returned, to quote to Microsoft support, or <see langword="null"/>.</param>
/// <param name="retryAfter">How long the response asked the caller to wait, or <see langword="null"/>.</param>
public sealed class DefenderApiException(HttpStatusCode statusCode, string? code, string message, string? correlationId, TimeSpan? retryAfter)
	: Exception(message)
{
	/// <summary>The HTTP status code.</summary>
	public HttpStatusCode StatusCode { get; } = statusCode;

	/// <summary>The error code Defender returned, or <see langword="null"/> when the body held none.</summary>
	public string? Code { get; } = code;

	/// <summary>The request correlation id (the error's <c>target</c>), or <see langword="null"/> when the body held none.</summary>
	public string? CorrelationId { get; } = correlationId;

	/// <summary>How long the response's <c>Retry-After</c> header asked the caller to wait, or <see langword="null"/> without one.</summary>
	public TimeSpan? RetryAfter { get; } = retryAfter;
}
