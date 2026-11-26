using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Indicators;

/// <summary>A batch of indicators to create or update (<c>POST indicators/import</c>): one to <see cref="MaxBatchSize"/> items.</summary>
public class IndicatorImportRequest
{
	/// <summary>The most indicators the service accepts in one call.</summary>
	public const int MaxBatchSize = 500;

	/// <summary>The indicators (the property is named <c>Indicators</c> on the wire, capital I).</summary>
	/// <exception cref="ArgumentException">The list is empty.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The list holds more than <see cref="MaxBatchSize"/> items.</exception>
	[JsonPropertyName("Indicators")]
	public required IReadOnlyList<IndicatorSubmission> Indicators
	{
		get;
		init => field = BatchGuard.Check(value, MaxBatchSize, nameof(Indicators));
	}
}
