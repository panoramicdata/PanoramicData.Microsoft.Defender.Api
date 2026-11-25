using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Serialization;
using Refit;
using System.Security.Cryptography;

namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>
/// Client for the Microsoft Defender for Endpoint REST API (v1.0). Each group of endpoints is a property (for example
/// <see cref="Vulnerabilities"/>); the groups are created on first use. This is an unofficial client, not affiliated with or
/// endorsed by Microsoft.
/// </summary>
/// <remarks>
/// <para>
/// Every request carries a bearer token from <see cref="DefenderClientOptions.Credential"/> and transient failures are
/// retried (see <see cref="DefenderClientOptions"/>). Non-success responses raise <see cref="DefenderApiException"/>; a failure
/// to send raises the transport's own exception (such as <see cref="HttpRequestException"/>, or <see cref="TimeoutException"/>
/// when an attempt exceeds <see cref="DefenderClientOptions.Timeout"/>).
/// </para>
/// <para>
/// List operations return one page (<see cref="Models.ODataPage{T}"/>); <see cref="GetNextPageAsync{T}(Models.ODataPage{T}, CancellationToken)"/>
/// and the <c>GetAll...</c> methods follow the page links. Exports "via files" are read with
/// <see cref="StreamExportAsync{T}(Exports.ExportFilesResponse, CancellationToken)"/>.
/// </para>
/// <para>A client is thread-safe and intended to be long-lived: create one per tenant and identity, and dispose it when done.</para>
/// </remarks>
public sealed partial class DefenderClient : IDisposable
{
	/// <summary>
	/// How long an idle pooled connection is kept. Reusing a connection the service has already closed fails mid-request
	/// with "connection forcibly closed", so connections are dropped well before a typical server keep-alive timeout.
	/// </summary>
	internal static readonly TimeSpan PooledConnectionIdleTimeout = TimeSpan.FromSeconds(5);

	/// <summary>How long any pooled connection is kept, so DNS changes (for example a load balancer move) are picked up.</summary>
	internal static readonly TimeSpan PooledConnectionLifetime = TimeSpan.FromMinutes(5);

	private readonly HttpMessageHandler _pipeline;
	private readonly HttpMessageHandler _downloadPipeline;
	private readonly HttpClient _httpClient;
	private readonly HttpClient _downloadClient;
	private readonly TimeProvider _timeProvider;

	/// <summary>Creates a client.</summary>
	/// <param name="options">Connection options.</param>
	public DefenderClient(DefenderClientOptions options)
		: this(options, CreateTransport(Validated(options)), CreateDownloadTransport())
	{
	}

	/// <summary>
	/// Creates a client that sends API requests through <paramref name="innerHandler"/> (for example a proxy-aware or
	/// instrumented handler). The client takes ownership of the handler and disposes it. Export file downloads use a separate
	/// default transport. <see cref="DefenderClientOptions.TrustedServerCertificateThumbprint"/> and
	/// <see cref="DefenderClientOptions.ServerCertificateValidationCallback"/> are ignored: configure certificate validation
	/// on the handler instead.
	/// </summary>
	/// <param name="options">Connection options.</param>
	/// <param name="innerHandler">The handler that sends API requests to the network.</param>
	public DefenderClient(DefenderClientOptions options, HttpMessageHandler innerHandler)
		: this(options, innerHandler, CreateDownloadTransport())
	{
	}

	/// <summary>
	/// Creates a client with a handler for API requests and a different one for export file downloads. The download handler
	/// never sees the bearer token: download links are signed Azure Storage URLs. The client takes ownership of both handlers.
	/// </summary>
	/// <param name="options">Connection options.</param>
	/// <param name="innerHandler">The handler that sends API requests to the network.</param>
	/// <param name="downloadHandler">The handler that sends export file downloads to the network.</param>
	public DefenderClient(DefenderClientOptions options, HttpMessageHandler innerHandler, HttpMessageHandler downloadHandler)
	{
		ArgumentNullException.ThrowIfNull(innerHandler);
		ArgumentNullException.ThrowIfNull(downloadHandler);
		Validated(options);
		BaseAddress = CreateBaseAddress(options.BaseUri);
		_timeProvider = options.TimeProvider;
		_pipeline = CreatePipeline(options, BaseAddress, innerHandler);
		_downloadPipeline = new RetryHandler(options) { InnerHandler = downloadHandler };
		_httpClient = CreateHttpClient(_pipeline, BaseAddress);
		_downloadClient = CreateHttpClient(_downloadPipeline, null);
		Settings = CreateSettings(_timeProvider);
	}

	/// <summary>The base address every operation path is appended to, always ending in <c>/</c>.</summary>
	public Uri BaseAddress { get; }

	/// <summary>The Refit settings shared by every endpoint group of this client.</summary>
	internal RefitSettings Settings { get; }

	/// <summary>Creates Refit settings that serialise as Defender expects and map errors with <paramref name="timeProvider"/>.</summary>
	internal static RefitSettings CreateSettings(TimeProvider timeProvider) => new()
	{
		ContentSerializer = new DefenderContentSerializer(),
		// Interface paths are relative (no leading slash) so they append to the "/api" prefix of the base address.
		UrlResolution = UrlResolutionMode.Rfc3986,
		UrlParameterFormatter = new DefenderUrlParameterFormatter(),
		// Unbuffered, Refit sends a serialized body as a read-once push stream, which can be neither retried nor resent
		// after a 401.
		Buffered = true,
		ExceptionFactory = response => new ValueTask<Exception?>(DefenderErrorMapper.CreateAsync(response, timeProvider)),
		// Refit would wrap every exception thrown while sending in its ApiRequestException; surface them as themselves
		// (TimeoutException, HttpRequestException, ObjectDisposedException...), as documented.
		TransportExceptionFactory = static (_, exception, _) => exception
	};

	internal T For<T>() => RestService.For<T>(_httpClient, Settings);

	/// <inheritdoc />
	public void Dispose()
	{
		_httpClient.Dispose();
		_downloadClient.Dispose();
		_pipeline.Dispose();
		_downloadPipeline.Dispose();
	}

	// Validation comes before the transport is created, so invalid options do not leave an undisposed network handler.
	private static DefenderClientOptions Validated(DefenderClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		options.Validate();
		return options;
	}

	private static Uri CreateBaseAddress(Uri baseUri)
	{
		var text = baseUri.AbsoluteUri;
		return new Uri(text.EndsWith('/') ? text : text + "/");
	}

	// Pipeline, outermost first: the read-only guard refuses before a token is fetched; authentication wraps retrying, so a
	// retried request keeps its token and a 401 is replayed through retrying again.
	private static ODataQueryHandler CreatePipeline(DefenderClientOptions options, Uri baseAddress, HttpMessageHandler innerHandler)
	{
		HttpMessageHandler handler = new RetryHandler(options) { InnerHandler = innerHandler };
		handler = new AuthenticationHandler(options) { InnerHandler = handler };
		if (options.IsReadOnly)
		{
			handler = new ReadOnlyHandler(baseAddress) { InnerHandler = handler };
		}

		return new ODataQueryHandler { InnerHandler = handler };
	}

	private static HttpClient CreateHttpClient(HttpMessageHandler pipeline, Uri? baseAddress)
		=> new(pipeline, disposeHandler: false)
		{
			BaseAddress = baseAddress,
			// The per-attempt timeout is applied inside RetryHandler so retries and Retry-After waits are not cut short.
			Timeout = System.Threading.Timeout.InfiniteTimeSpan
		};

	internal static SocketsHttpHandler CreateTransport(DefenderClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		return CreateTransport(options.ServerCertificateValidationCallback, options.TrustedServerCertificateThumbprint);
	}

	/// <summary>
	/// Creates the API's network handler: <paramref name="validationCallback"/> when set, else trust for the certificate with
	/// <paramref name="trustedThumbprint"/> (SHA-256) on top of normal validation. Redirects are never followed, so a bearer
	/// token cannot be carried to another host.
	/// </summary>
	internal static SocketsHttpHandler CreateTransport(
		Func<System.Security.Cryptography.X509Certificates.X509Certificate2?, System.Security.Cryptography.X509Certificates.X509Chain?, System.Net.Security.SslPolicyErrors, bool>? validationCallback,
		string? trustedThumbprint)
	{
		var handler = CreateDownloadTransport();
		var callback = validationCallback ?? PinnedCallback(NormalizeThumbprint(trustedThumbprint));
		if (callback is not null)
		{
			// The sender is the SslStream, not the request, so the callback is not given one.
			handler.SslOptions.RemoteCertificateValidationCallback = (_, certificate, chain, errors)
				=> callback(AsCertificate2(certificate), chain, errors);
		}

		return handler;
	}

	/// <summary>The network handler for downloads from signed storage links: normal certificate validation and no redirects.</summary>
	internal static SocketsHttpHandler CreateDownloadTransport()
		=> new()
		{
			AllowAutoRedirect = false,
			PooledConnectionIdleTimeout = PooledConnectionIdleTimeout,
			PooledConnectionLifetime = PooledConnectionLifetime
		};

	/// <summary>TLS hands over an <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2"/>; anything else is loaded as one.</summary>
	internal static System.Security.Cryptography.X509Certificates.X509Certificate2? AsCertificate2(System.Security.Cryptography.X509Certificates.X509Certificate? certificate)
		=> certificate switch
		{
			null => null,
			System.Security.Cryptography.X509Certificates.X509Certificate2 certificate2 => certificate2,
			_ => System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadCertificate(certificate.GetRawCertData())
		};

	private static Func<System.Security.Cryptography.X509Certificates.X509Certificate2?, System.Security.Cryptography.X509Certificates.X509Chain?, System.Net.Security.SslPolicyErrors, bool>? PinnedCallback(string? pinned)
		=> pinned is null
			? null
			: (certificate, _, errors) => errors == System.Net.Security.SslPolicyErrors.None
				|| (certificate is not null
					&& string.Equals(certificate.GetCertHashString(HashAlgorithmName.SHA256), pinned, StringComparison.OrdinalIgnoreCase));

	internal static string? NormalizeThumbprint(string? thumbprint)
	{
		if (string.IsNullOrWhiteSpace(thumbprint))
		{
			return null;
		}

		var hex = new string([.. thumbprint.Where(Uri.IsHexDigit)]);
		return hex.Length == 64
			? hex
			: throw new ArgumentException("TrustedServerCertificateThumbprint must be a SHA-256 thumbprint (64 hex digits).", nameof(thumbprint));
	}
}
