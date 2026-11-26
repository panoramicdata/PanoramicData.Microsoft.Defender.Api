using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models.Machines;

/// <summary>The optional query parameters of the find-by-tag operation.</summary>
public class FindByTagOptions
{
	/// <summary>When <see langword="true"/>, matches tags that start with the given text instead of tags equal to it.</summary>
	[AliasAs("useStartsWithFilter")]
	public bool? UseStartsWithFilter { get; set; }
}
