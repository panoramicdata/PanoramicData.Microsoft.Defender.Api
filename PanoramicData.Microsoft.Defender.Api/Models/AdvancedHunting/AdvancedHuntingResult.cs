using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AdvancedHunting;

/// <summary>The answer to an advanced hunting query: the column schema and the rows.</summary>
public class AdvancedHuntingResult
{
	/// <summary>The columns of the result, in query order.</summary>
	[JsonPropertyName("Schema")]
	public IReadOnlyList<HuntingColumn> Schema { get; set; } = [];

	/// <summary>
	/// The result rows. Each row maps a column name to its value as raw JSON, because the columns (and so the value types)
	/// depend on the query; use the <see cref="Schema"/> to interpret them.
	/// </summary>
	[JsonPropertyName("Results")]
	public IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> Results { get; set; } = [];
}
