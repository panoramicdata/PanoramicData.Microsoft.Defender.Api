namespace PanoramicData.MicrosoftDefender.Api.Handlers;

/// <summary>Reads how long a response's <c>Retry-After</c> header asks the caller to wait.</summary>
internal static class RetryAfterHeader
{
	/// <summary>
	/// The wait the header asks for, whether given as a number of seconds or as an HTTP date (an earlier date reads as zero),
	/// or <see langword="null"/> without the header.
	/// </summary>
	public static TimeSpan? Read(HttpResponseMessage response, TimeProvider timeProvider)
	{
		var header = response.Headers.RetryAfter;
		if (header?.Delta is { } delta)
		{
			return delta;
		}

		if (header?.Date is { } date)
		{
			var wait = date - timeProvider.GetUtcNow();
			return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
		}

		return null;
	}
}
