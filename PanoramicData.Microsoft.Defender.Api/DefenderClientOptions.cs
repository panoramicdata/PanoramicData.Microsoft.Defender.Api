using Azure.Core;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>
/// Configuration for <see cref="DefenderClient"/>. The values are read once when the client is constructed; changing this
/// object afterwards does not affect an existing client.
/// </summary>
/// <remarks>
/// The client sends a bearer token obtained from <see cref="Credential"/> for <see cref="Scope"/>, caches it until shortly
/// before it expires, and asks for a new one once when the API answers 401. Tokens are never logged or included in
/// exception messages or <see cref="ToString"/>.
/// </remarks>
public class DefenderClientOptions
{
	private static readonly TimeSpan MaxTimerDuration = TimeSpan.FromMilliseconds(int.MaxValue);

	/// <summary>
	/// Where tokens come from, for example <c>ClientSecretCredential</c> from <c>Azure.Identity</c>. Required. The client does
	/// not dispose it.
	/// </summary>
	public TokenCredential? Credential { get; set; }

	/// <summary>
	/// The token scope. Defaults to <see cref="DefenderEndpoints.DefaultScope"/>, which suits the commercial clouds. The
	/// US Government clouds need the scope for their own audience; set it when changing <see cref="BaseUri"/> to one of them.
	/// </summary>
	public string Scope { get; set; } = DefenderEndpoints.DefaultScope;

	/// <summary>
	/// The API's base address, with the <c>/api</c> prefix; every operation path is appended to it. Defaults to the global
	/// host. It must be an absolute <c>https</c> URL without credentials, query string or fragment. See
	/// <see cref="UseCloud(DefenderCloud)"/> for the regional and government hosts.
	/// </summary>
	public Uri BaseUri { get; set; } = DefenderEndpoints.BaseUri(DefenderCloud.Global);

	/// <summary>Sets <see cref="BaseUri"/> to the address of <paramref name="cloud"/>. The <see cref="Scope"/> is left alone.</summary>
	/// <param name="cloud">The cloud or region.</param>
	public void UseCloud(DefenderCloud cloud) => BaseUri = DefenderEndpoints.BaseUri(cloud);

	/// <summary>
	/// When <see langword="true"/>, the client refuses (with <see cref="DefenderReadOnlyException"/>, before anything is sent
	/// and before a token is requested) every request that could change Defender: any PATCH, PUT or DELETE, and any POST
	/// except the three that only read (<c>advancedqueries/run</c> and the two authenticated-scan history queries).
	/// Defaults to <see langword="false"/>.
	/// </summary>
	/// <remarks>
	/// This guards the REST surface. Use an Entra application that holds only read permissions for a truly read-only identity.
	/// </remarks>
	public bool IsReadOnly { get; set; }

	/// <summary>
	/// The SHA-256 thumbprint (hex, case and separators ignored) of a server certificate to trust even when it fails normal
	/// validation, for example behind an inspecting proxy. Other certificates are still validated normally. Ignored when
	/// <see cref="ServerCertificateValidationCallback"/> is set, and when a transport handler is supplied to the client.
	/// </summary>
	public string? TrustedServerCertificateThumbprint { get; set; }

	/// <summary>
	/// Full control over server certificate validation. Takes precedence over <see cref="TrustedServerCertificateThumbprint"/>.
	/// Ignored when a transport handler is supplied to the client.
	/// </summary>
	public Func<X509Certificate2?, X509Chain?, SslPolicyErrors, bool>? ServerCertificateValidationCallback { get; set; }

	/// <summary>
	/// HTTP timeout per attempt, covering sending the request and receiving the response headers. It does not include retry
	/// back-off, nor reading a streamed body. An attempt that exceeds it raises a <see cref="TimeoutException"/>; caller
	/// cancellation still raises <see cref="OperationCanceledException"/>. Must be greater than zero and at most
	/// <see cref="int.MaxValue"/> milliseconds.
	/// </summary>
	public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

	/// <summary>
	/// Maximum retries of a transient failure. Any verb is retried on 429 and 503; other 5xx responses are retried only for
	/// idempotent verbs (GET, HEAD, PUT, DELETE), never POST or PATCH. A connection that could not be established (refused,
	/// reset during the TLS handshake, or a name that did not resolve) is retried for any verb, since nothing was sent.
	/// Requests with a stream body are never retried. Must not be negative.
	/// </summary>
	public int MaxRetries { get; set; } = 3;

	/// <summary>Initial back-off, doubled on each retry (up to <see cref="MaxRetryDelay"/>). Must not be negative.</summary>
	public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

	/// <summary>
	/// The longest single wait before a retry. A server-supplied <c>Retry-After</c> longer than this is not waited out: the
	/// response is returned (as a <see cref="DefenderApiException"/> carrying <see cref="DefenderApiException.RetryAfter"/>)
	/// instead of retrying sooner than the server asked. Must be greater than zero and at most
	/// <see cref="int.MaxValue"/> milliseconds.
	/// </summary>
	public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>How long before a token expires the client stops using it and asks for a new one. Must not be negative.</summary>
	public TimeSpan TokenRefreshMargin { get; set; } = TimeSpan.FromMinutes(5);

	/// <summary>The clock behind token expiry, retry waits and timeouts. Defaults to <see cref="TimeProvider.System"/>.</summary>
	public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

	/// <summary>Optional logger. Tokens, query strings and download signatures are never logged; only the method and path are.</summary>
	public ILogger? Logger { get; set; }

	internal void Validate()
	{
		if (Credential is null)
		{
			throw new ArgumentException("Credential is required: set it to a TokenCredential such as ClientSecretCredential.", nameof(Credential));
		}

		ArgumentException.ThrowIfNullOrWhiteSpace(Scope);
		ValidateBaseUri();
		ArgumentNullException.ThrowIfNull(TimeProvider);
		_ = DefenderClient.NormalizeThumbprint(TrustedServerCertificateThumbprint);
		ArgumentOutOfRangeException.ThrowIfNegative(MaxRetries);
		// The upper bounds are those of timers and Task.Delay, which would otherwise throw on the first request rather than here.
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Timeout, TimeSpan.Zero);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(Timeout, MaxTimerDuration);
		ArgumentOutOfRangeException.ThrowIfLessThan(RetryBaseDelay, TimeSpan.Zero);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(MaxRetryDelay, TimeSpan.Zero);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(MaxRetryDelay, MaxTimerDuration);
		ArgumentOutOfRangeException.ThrowIfLessThan(TokenRefreshMargin, TimeSpan.Zero);
	}

	private void ValidateBaseUri()
	{
		ArgumentNullException.ThrowIfNull(BaseUri);
		if (!BaseUri.IsAbsoluteUri || BaseUri.Scheme != Uri.UriSchemeHttps)
		{
			throw new ArgumentException("BaseUri must be an absolute https URL: the bearer token is sent to it.", nameof(BaseUri));
		}

		// HttpClient never sends user info, and it would appear in logs and exception messages that show the URL.
		if (BaseUri.UserInfo.Length > 0)
		{
			throw new ArgumentException("BaseUri must not contain credentials.", nameof(BaseUri));
		}

		// Operation paths are appended to BaseUri, which a query string or fragment would swallow.
		if (BaseUri.Query.Length > 0 || BaseUri.Fragment.Length > 0)
		{
			throw new ArgumentException("BaseUri must not have a query string or fragment.", nameof(BaseUri));
		}
	}

	/// <inheritdoc />
	public override string ToString()
		=> $"DefenderClientOptions {{ BaseUri = {BaseUri}, Scope = {Scope}, Credential = {(Credential is null ? "(none)" : Credential.GetType().Name)}, ReadOnly = {IsReadOnly} }}";
}
