using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>The indicators to delete in one call (<c>POST indicators/BatchDelete</c>): one to <see cref="MaxBatchSize"/> ids.</summary>
public class IndicatorBatchDeleteRequest
{
	/// <summary>The most ids the service accepts in one call.</summary>
	public static int MaxBatchSize => 500;

	/// <summary>The ids of the indicators to delete (the property is named <c>IndicatorIds</c> on the wire).</summary>
	/// <exception cref="ArgumentException">The list is empty.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The list holds more than <see cref="MaxBatchSize"/> items.</exception>
	[JsonPropertyName("IndicatorIds")]
	public required IReadOnlyList<string> IndicatorIds
	{
		get;
		init => field = BatchGuard.Check(value, MaxBatchSize, nameof(IndicatorIds));
	}
}
