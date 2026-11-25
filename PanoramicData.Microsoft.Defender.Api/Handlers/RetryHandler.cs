using Microsoft.Extensions.Logging;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Handlers;

/// <summary>
/// Applies the per-attempt timeout and retries transient failures. A request is retried (up to
/// <see cref="DefenderClientOptions.MaxRetries"/> times, honouring <c>Retry-After</c>, else exponential back-off) only when
/// its body is replayable and either the status is 429 or 503, the status is another 5xx and the verb is idempotent, or the
/// connection could not be established (refused, reset during the TLS handshake, name not resolved), when nothing was sent.
/// A <c>Retry-After</c> longer than the maximum delay is never cut short: the response is returned instead. Only the method
/// and path are logged, never the query string. The settings are copied at construction.
/// </summary>
internal sealed class RetryHandler : DelegatingHandler
{
	private static readonly HashSet<HttpMethod> IdempotentMethods =
		[HttpMethod.Get, HttpMethod.Head, HttpMethod.Put, HttpMethod.Delete, HttpMethod.Options, HttpMethod.Trace];

	private readonly ILogger? _logger;
	private readonly TimeProvider _timeProvider;
	private readonly TimeSpan _timeout;
	private readonly int _maxRetries;
	private readonly TimeSpan _retryBaseDelay;
	private readonly TimeSpan _maxRetryDelay;

	/// <summary>Creates a handler with the retry and timeout settings of <paramref name="options"/>.</summary>
	/// <param name="options">The client options.</param>
	public RetryHandler(DefenderClientOptions options)
	{
		_logger = options.Logger;
		_timeProvider = options.TimeProvider;
		_timeout = options.Timeout;
		_maxRetries = options.MaxRetries;
		_retryBaseDelay = options.RetryBaseDelay;
		_maxRetryDelay = options.MaxRetryDelay;
		Delay = (wait, cancellationToken) => Task.Delay(wait, _timeProvider, cancellationToken);
	}

	/// <summary>Waits before a retry; replaceable so tests never sleep.</summary>
	internal Func<TimeSpan, CancellationToken, Task> Delay { get; set; }

	/// <inheritdoc />
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Only the path is logged: query strings can carry filters, and download links carry a signature.
		var path = request.RequestUri!.GetLeftPart(UriPartial.Path);
		var retries = IsReplayable(request.Content) ? _maxRetries : 0;
		var backoff = _retryBaseDelay;
		var attempt = 0;
		for (; attempt < retries; attempt++)
		{
			TimeSpan wait;
			try
			{
				var response = await SendAttemptAsync(request, path, attempt, cancellationToken).ConfigureAwait(false);
				if (!IsRetryable(request.Method, response.StatusCode))
				{
					return response;
				}

				var requested = RetryAfterHeader.Read(response, _timeProvider);
				if (requested > _maxRetryDelay)
				{
					// The server wants a longer pause than we are willing to take; retrying sooner would only be throttled again.
					LogTooLong(response.StatusCode, request.Method, path, requested.Value);
					return response;
				}

				wait = requested ?? CapDelay(backoff);
				LogRetry(response.StatusCode, request.Method, path, wait);
				response.Dispose();
			}
			catch (HttpRequestException exception) when (IsConnectionFailure(exception))
			{
				wait = CapDelay(backoff);
				LogConnectionRetry(exception.HttpRequestError, request.Method, path, wait);
			}

			await Delay(wait, cancellationToken).ConfigureAwait(false);
			backoff = NextBackoff(backoff);
		}

		// The last attempt (or the only one, for a body that cannot be replayed) returns or throws whatever happens.
		return await SendAttemptAsync(request, path, attempt, cancellationToken).ConfigureAwait(false);
	}

	private void LogRetry(HttpStatusCode status, HttpMethod method, string path, TimeSpan wait)
	{
		if (_logger is not null)
		{
			Log.Retrying(_logger, (int)status, method, path, wait);
		}
	}

	private void LogTooLong(HttpStatusCode status, HttpMethod method, string path, TimeSpan requested)
	{
		if (_logger is not null)
		{
			Log.RetryAfterTooLong(_logger, (int)status, method, path, requested);
		}
	}

	private void LogConnectionRetry(HttpRequestError error, HttpMethod method, string path, TimeSpan wait)
	{
		if (_logger is not null)
		{
			Log.RetryingConnection(_logger, error, method, path, wait);
		}
	}

	/// <summary>
	/// Whether the request failed while the connection was being established (including the TLS handshake and name
	/// resolution), so nothing was sent and any verb can safely be retried. Failures after the request went out are not.
	/// </summary>
	internal static bool IsConnectionFailure(HttpRequestException exception)
		=> exception.HttpRequestError is HttpRequestError.ConnectionError
			or HttpRequestError.SecureConnectionError
			or HttpRequestError.NameResolutionError;

	private async Task<HttpResponseMessage> SendAttemptAsync(HttpRequestMessage request, string path, int attempt, CancellationToken cancellationToken)
	{
		if (_logger is not null)
		{
			Log.Sending(_logger, request.Method, path, attempt + 1);
		}

		using var timeout = new CancellationTokenSource(_timeout, _timeProvider);
		using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
		try
		{
			return await base.SendAsync(request, linked.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested && timeout.IsCancellationRequested)
		{
			throw new TimeoutException($"Defender did not respond within {_timeout}.", exception);
		}
	}

	private TimeSpan CapDelay(TimeSpan wait) => wait > _maxRetryDelay ? _maxRetryDelay : wait;

	// Doubling is capped so it can never overflow TimeSpan.
	private TimeSpan NextBackoff(TimeSpan backoff) => backoff > _maxRetryDelay / 2 ? _maxRetryDelay : backoff * 2;

	/// <summary>
	/// Whether the body can be sent again unchanged: none, or buffered content (byte arrays, strings and form fields), or
	/// JSON content, which serialises its value afresh on each send. Streams are not, since they may be read-once.
	/// </summary>
	internal static bool IsReplayable(HttpContent? content)
		=> content is null or ByteArrayContent or ReadOnlyMemoryContent or System.Net.Http.Json.JsonContent;

	/// <summary>
	/// 429 and 503 mean the request was not processed, so any verb is retried. Other 5xx may follow a partial or complete
	/// write, so only idempotent verbs are retried on them (never POST or PATCH).
	/// </summary>
	internal static bool IsRetryable(HttpMethod method, HttpStatusCode status)
		=> status is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable
			|| ((int)status >= 500 && IdempotentMethods.Contains(method));
}
