using System.Text.RegularExpressions;

namespace PanoramicData.MicrosoftDefender.Api.Handlers;

/// <summary>
/// Refuses, before anything is sent, every request that could change Defender: any method other than GET or HEAD, except
/// POSTs to the three read-only operations in <see cref="ReadOnlyPosts"/>.
/// </summary>
internal sealed partial class ReadOnlyHandler(Uri baseAddress) : DelegatingHandler
{
	/// <summary>
	/// POST endpoints that only read: running an advanced hunting query, and the two authenticated-scan history queries.
	/// Paths are relative to the base address and matched case-insensitively.
	/// </summary>
	[GeneratedRegex(@"^(advancedqueries/run|DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId|DeviceAuthenticatedScanDefinitions/GetScanHistoryBySessionId)/?$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
	private static partial Regex ReadOnlyPosts();

	/// <inheritdoc />
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (!IsAllowed(request))
		{
			throw new DefenderReadOnlyException(request.Method.Method, request.RequestUri!.GetLeftPart(UriPartial.Path));
		}

		return base.SendAsync(request, cancellationToken);
	}

	private bool IsAllowed(HttpRequestMessage request)
	{
		if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head)
		{
			return true;
		}

		return request.Method == HttpMethod.Post
			&& RelativePath(request.RequestUri!) is { } relative
			&& ReadOnlyPosts().IsMatch(relative);
	}

	/// <summary>The request path relative to the base address, or <see langword="null"/> when the request is not below it.</summary>
	private string? RelativePath(Uri requestUri)
	{
		var basePath = baseAddress.AbsolutePath;
		var path = requestUri.AbsolutePath;
		return path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? path[basePath.Length..] : null;
	}
}
