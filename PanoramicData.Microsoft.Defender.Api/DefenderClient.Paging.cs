using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Serialization;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>
	/// Fetches the page after <paramref name="page"/>, authenticated like every other request.
	/// </summary>
	/// <remarks>
	/// The page's next link is an absolute URL, so it is checked before anything is sent: it must be <c>https</c> on the
	/// same host and port as <see cref="BaseAddress"/>, so the bearer token can never be sent anywhere else. Otherwise a
	/// <see cref="DefenderLinkException"/> is thrown.
	/// </remarks>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="page">A page returned by a list operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The next page, or <see langword="null"/> when <paramref name="page"/> is the last one.</returns>
	public async Task<ODataPage<T>?> GetNextPageAsync<T>(ODataPage<T> page, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(page);
		if (string.IsNullOrEmpty(page.NextLink))
		{
			return null;
		}

		var link = ValidateNextLink(page.NextLink);
		using var response = await _httpClient.GetAsync(link, cancellationToken).ConfigureAwait(false);
		if (await DefenderErrorMapper.CreateAsync(response, _timeProvider, cancellationToken).ConfigureAwait(false) is { } error)
		{
			throw error;
		}

		return await response.Content.ReadFromJsonAsync<ODataPage<T>>(DefenderJson.Options, cancellationToken).ConfigureAwait(false)
			?? new ODataPage<T>();
	}

	/// <summary>
	/// Enumerates every item of a collection, fetching pages as the enumeration proceeds (see
	/// <see cref="GetNextPageAsync{T}(ODataPage{T}, CancellationToken)"/> for how the links are checked).
	/// </summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="firstPage">Fetches the first page, for example <c>ct =&gt; client.Vulnerabilities.ListAsync(options, ct)</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The items of every page, in order.</returns>
	public async IAsyncEnumerable<T> GetAllAsync<T>(
		Func<CancellationToken, Task<ODataPage<T>>> firstPage,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(firstPage);
		var page = await firstPage(cancellationToken).ConfigureAwait(false);
		while (true)
		{
			foreach (var item in page.Value)
			{
				yield return item;
			}

			var next = await GetNextPageAsync(page, cancellationToken).ConfigureAwait(false);
			if (next is null)
			{
				yield break;
			}

			page = next;
		}
	}

	private Uri ValidateNextLink(string nextLink)
	{
		// On Linux and macOS a rooted path such as "/api/x" parses as an absolute file:// URI, so a file URI counts as not absolute.
		if (!Uri.TryCreate(nextLink, UriKind.Absolute, out var link) || link.IsFile)
		{
			throw new DefenderLinkException("The next link is not an absolute URL.");
		}

		if (link.Scheme != BaseAddress.Scheme
			|| !string.Equals(link.Host, BaseAddress.Host, StringComparison.OrdinalIgnoreCase)
			|| link.Port != BaseAddress.Port)
		{
			throw new DefenderLinkException($"The next link points to {link.Scheme}://{link.Host}:{link.Port}, not to {BaseAddress.Scheme}://{BaseAddress.Host}:{BaseAddress.Port}; it was not followed.");
		}

		return link;
	}
}
