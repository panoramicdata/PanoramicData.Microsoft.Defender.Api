using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Models;

/// <summary>
/// The OData query options most list operations accept. Pass <see langword="null"/> (or leave a property unset) to omit one.
/// Operations that accept further options derive from this class. Which properties <see cref="Filter"/> may name, and the
/// largest <see cref="Top"/>, differ per operation: see the operation's documentation.
/// </summary>
public class ODataListOptions
{
	/// <summary>
	/// An OData <c>$filter</c> expression, for example <c>severity eq 'High'</c>. Sent as given: quote string literals
	/// with single apostrophes and double any apostrophe inside one.
	/// </summary>
	[AliasAs("$filter")]
	public string? Filter { get; set; }

	/// <summary>The most items to return (<c>$top</c>).</summary>
	[AliasAs("$top")]
	public int? Top { get; set; }

	/// <summary>How many items to skip (<c>$skip</c>).</summary>
	[AliasAs("$skip")]
	public int? Skip { get; set; }
}
