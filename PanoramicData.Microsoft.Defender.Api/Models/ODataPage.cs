using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models;

/// <summary>
/// One page of a collection: the OData envelope Defender wraps list responses in. <see cref="NextLink"/> is an absolute URL
/// when more pages follow; pass the page to <see cref="DefenderClient.GetNextPageAsync{T}(ODataPage{T}, CancellationToken)"/>
/// to fetch it, or use <see cref="DefenderClient.GetAllAsync{T}(Func{CancellationToken, Task{ODataPage{T}}}, CancellationToken)"/>
/// to enumerate every item.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class ODataPage<T>
{
	/// <summary>The OData context URL describing the collection.</summary>
	[JsonPropertyName("@odata.context")]
	public string? Context { get; set; }

	/// <summary>The items on this page; empty when the response held none.</summary>
	[JsonPropertyName("value")]
	public IReadOnlyList<T> Value { get; set; } = [];

	/// <summary>The absolute URL of the next page, or <see langword="null"/> on the last page.</summary>
	[JsonPropertyName("@odata.nextLink")]
	public string? NextLink { get; set; }
}
