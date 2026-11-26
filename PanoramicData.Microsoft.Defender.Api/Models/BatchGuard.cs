namespace PanoramicData.MicrosoftDefender.Api.Models;

/// <summary>Checks the size of a batch before it is sent, so an oversized or empty batch fails with a clear message instead of a 400 from the service.</summary>
internal static class BatchGuard
{
	/// <summary>Returns <paramref name="items"/> when it holds between one and <paramref name="maxItems"/> items; otherwise throws.</summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="items">The batch.</param>
	/// <param name="maxItems">The documented limit, or <see langword="null"/> when the service documents none.</param>
	/// <param name="paramName">The name of the property being set.</param>
	/// <returns>The same batch.</returns>
	public static IReadOnlyList<T> Check<T>(IReadOnlyList<T> items, int? maxItems, string paramName)
	{
		ArgumentNullException.ThrowIfNull(items, paramName);
		if (items.Count == 0)
		{
			throw new ArgumentException($"{paramName} must contain at least one item.", paramName);
		}

		if (items.Count > maxItems)
		{
			throw new ArgumentOutOfRangeException(paramName, items.Count, $"{paramName} holds {items.Count} items but the service accepts at most {maxItems} per call.");
		}

		return items;
	}
}
