using Azure.Core;
using System.Net;
using System.Net.Http.Headers;

namespace PanoramicData.MicrosoftDefender.Api.Handlers;

/// <summary>
/// Sends each request with a bearer token from a <see cref="TokenCredential"/>. The token is cached until it is within the
/// refresh margin of expiring, and concurrent requests that need a token share one fetch. A 401 response invalidates the
/// token, and the request is sent once more with a fresh one when its body can be replayed (the credential may return the
/// same token again, in which case the second 401 is returned). Tokens are never logged or put in exception messages.
/// </summary>
internal sealed class AuthenticationHandler : DelegatingHandler
{
	private readonly TokenCredential _credential;
	private readonly TokenRequestContext _context;
	private readonly TimeSpan _refreshMargin;
	private readonly TimeProvider _timeProvider;
	private readonly SemaphoreSlim _lock = new(1, 1);
	private volatile CachedToken? _cached;

	public AuthenticationHandler(DefenderClientOptions options)
	{
		_credential = options.Credential!;
		_context = new TokenRequestContext([options.Scope]);
		_refreshMargin = options.TokenRefreshMargin;
		_timeProvider = options.TimeProvider;
	}

	/// <inheritdoc />
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var token = await GetTokenAsync(null, cancellationToken).ConfigureAwait(false);
		var response = await SendWithTokenAsync(request, token, cancellationToken).ConfigureAwait(false);
		if (response.StatusCode != HttpStatusCode.Unauthorized || !RetryHandler.IsReplayable(request.Content))
		{
			return response;
		}

		// The token expired early or was revoked: fetch another once, then resend.
		response.Dispose();
		token = await GetTokenAsync(token, cancellationToken).ConfigureAwait(false);
		return await SendWithTokenAsync(request, token, cancellationToken).ConfigureAwait(false);
	}

	private Task<HttpResponseMessage> SendWithTokenAsync(HttpRequestMessage request, string token, CancellationToken cancellationToken)
	{
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		return base.SendAsync(request, cancellationToken);
	}

	/// <summary>
	/// Returns a usable token, fetching one when there is none, the cached one is near expiry, or the cached one is
	/// <paramref name="staleToken"/> (concurrent requests that all saw the same 401 fetch only once).
	/// </summary>
	private async Task<string> GetTokenAsync(string? staleToken, CancellationToken cancellationToken)
	{
		if (Usable(_cached, staleToken) is { } cached)
		{
			return cached;
		}

		await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			if (Usable(_cached, staleToken) is { } fetchedMeanwhile)
			{
				return fetchedMeanwhile;
			}

			var fresh = await _credential.GetTokenAsync(_context, cancellationToken).ConfigureAwait(false);
			_cached = new CachedToken(fresh.Token, fresh.ExpiresOn);
			return fresh.Token;
		}
		finally
		{
			_lock.Release();
		}
	}

	private string? Usable(CachedToken? cached, string? staleToken)
		=> cached is not null
			&& cached.Value != staleToken
			&& cached.ExpiresOn - _refreshMargin > _timeProvider.GetUtcNow()
			? cached.Value
			: null;

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_lock.Dispose();
		}

		base.Dispose(disposing);
	}

	/// <summary>A token and when it expires. It has no <see cref="object.ToString"/> override, so the token text never reaches a log.</summary>
	private sealed class CachedToken(string value, DateTimeOffset expiresOn)
	{
		public string Value { get; } = value;

		public DateTimeOffset ExpiresOn { get; } = expiresOn;
	}
}
