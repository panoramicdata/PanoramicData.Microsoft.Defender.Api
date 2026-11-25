using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models;

/// <summary>
/// The query options of the "via files" export operations, which return an <see cref="Exports.ExportFilesResponse"/> listing
/// download links. These operations are limited to a handful of calls per hour: keep the response and download from it.
/// </summary>
public class ExportViaFilesOptions
{
	/// <summary>How many hours the download links stay valid (<c>sasValidHours</c>). The service documents a maximum of 6 hours for most exports.</summary>
	[AliasAs("sasValidHours")]
	public int? SasValidHours { get; set; }
}
