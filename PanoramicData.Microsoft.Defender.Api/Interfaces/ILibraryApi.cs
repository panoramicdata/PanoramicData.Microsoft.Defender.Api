using PanoramicData.MicrosoftDefender.Api.Models.Library;
using PanoramicData.MicrosoftDefender.Api.Models;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// The live response library of scripts and tools (<c>libraryfiles</c>). Needs <c>Library.Manage</c>. The documentation
/// marks these operations as prerelease, so their behaviour may change. Upload and delete change tenant data and are
/// refused by a read-only client.
/// </summary>
public interface ILibraryApi
{
	/// <summary>Lists the files in the library (<c>GET libraryfiles</c>).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of library files.</returns>
	[Get("libraryfiles")]
	Task<ODataPage<LibraryFile>> ListAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Uploads a file (<c>POST libraryfiles</c>, <c>multipart/form-data</c>). Changes tenant data; the service limits files
	/// to 20 MB. The form fields are named <c>File</c>, <c>Description</c>, <c>ParametersDescription</c> and
	/// <c>OverrideIfExists</c>; the last three are omitted when <see langword="null"/>.
	/// </summary>
	/// <param name="file">The file content and its name. Create it with a <see cref="StreamPart"/> or <see cref="ByteArrayPart"/>.</param>
	/// <param name="description">A description of the file.</param>
	/// <param name="parametersDescription">What parameters a script takes.</param>
	/// <param name="overrideIfExists">Whether to replace a file of the same name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The stored library entry.</returns>
	/// <remarks>
	/// A multipart body is not replayed: the client does not retry the upload on 429, 503 or a dropped connection, and does
	/// not resend it after a 401 (the caller sees the failure and can upload again with fresh content).
	/// </remarks>
	[Multipart]
	[Post("libraryfiles")]
	Task<LibraryFile> UploadAsync([AliasAs("File")] StreamPart file, [AliasAs("Description")] string? description, [AliasAs("ParametersDescription")] string? parametersDescription, [AliasAs("OverrideIfExists")] bool? overrideIfExists, CancellationToken cancellationToken);

	/// <summary>Deletes a library file by name (<c>DELETE libraryfiles/{fileName}</c>). Destructive. Answers 204, or 404 when it does not exist.</summary>
	/// <param name="fileName">The file name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that completes when the file is deleted.</returns>
	[Delete("libraryfiles/{fileName}")]
	Task DeleteAsync(string fileName, CancellationToken cancellationToken);
}
