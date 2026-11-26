using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Serialization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>
	/// Downloads the files of a "via files" export one after another and yields their records as they are read.
	/// </summary>
	/// <remarks>
	/// The links are signed Azure Storage URLs, so they are fetched with a separate HTTP client that never sends the bearer
	/// token, over <c>https</c> only, without following redirects. A signature is a credential: it is never logged and never
	/// appears in an exception message (only the link's host and path do). Each file is gzip-compressed multi-line JSON.
	/// </remarks>
	/// <typeparam name="T">The record type of the export.</typeparam>
	/// <param name="export">The response of a "via files" export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	/// <exception cref="DefenderLinkException">A link is not an <c>https</c> URL; nothing is downloaded in that case.</exception>
	/// <exception cref="DefenderApiException">A download did not succeed.</exception>
	public async IAsyncEnumerable<T> StreamExportAsync<T>(
		ExportFilesResponse export,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(export);
		// Check every link before downloading any, so a bad one fails the call up front.
		var links = export.ExportFiles.Select(ValidateExportLink).ToList();
		foreach (var link in links)
		{
			using var response = await _downloadClient.GetAsync(link, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
			if (await DefenderErrorMapper.CreateAsync(response, _timeProvider, cancellationToken).ConfigureAwait(false) is { } error)
			{
				throw error;
			}

			using var body = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
			using var gzip = new GZipStream(body, CompressionMode.Decompress);
			await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<T>(gzip, topLevelValues: true, DefenderJson.Options, cancellationToken).ConfigureAwait(false))
			{
				if (record is not null)
				{
					yield return record;
				}
			}
		}
	}

	private static Uri ValidateExportLink(string link)
	{
		// Only the scheme and host are ever put in a message: the query string is the signature.
		if (!Uri.TryCreate(link, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
		{
			throw new DefenderLinkException("An export file link is not an absolute https URL.");
		}

		// Signed links carry the signature in the query; user info is never legitimate, and would be logged with the path.
		return uri.UserInfo.Length > 0
			? throw new DefenderLinkException("An export file link contains credentials.")
			: uri;
	}
}
