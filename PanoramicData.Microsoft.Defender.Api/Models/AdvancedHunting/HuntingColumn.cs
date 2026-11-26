using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.AdvancedHunting;

/// <summary>One column of an advanced hunting result: its name and the KQL type the service reports for it.</summary>
public class HuntingColumn
{
	/// <summary>The column name, which is also the key of the value in each result row.</summary>
	[JsonPropertyName("Name")]
	public string? Name { get; set; }

	/// <summary>The KQL type name, for example <c>DateTime</c> or <c>String</c>. An open set, so kept as text.</summary>
	[JsonPropertyName("Type")]
	public string? Type { get; set; }
}
