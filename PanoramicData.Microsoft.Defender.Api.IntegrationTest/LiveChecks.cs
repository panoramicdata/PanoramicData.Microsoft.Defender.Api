using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest;

/// <summary>Assertions the live tests of every group make in the same way: following a next link, and asking for something that does not exist.</summary>
internal static class LiveChecks
{
	/// <summary>
	/// Reads items from a paged sequence until <paramref name="limit"/> have been read or it ends, checking that each one has an
	/// identifier. Used to show that the next link is followed: the caller asks for pages smaller than the limit.
	/// </summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="items">The sequence to read.</param>
	/// <param name="limit">How many items to read at most.</param>
	/// <param name="identifier">Selects the item's identifying value.</param>
	/// <returns>How many items were read.</returns>
	public static async Task<int> CountAsync<T>(IAsyncEnumerable<T> items, int limit, Func<T, string?> identifier)
	{
		var seen = 0;

		await foreach (var item in items)
		{
			identifier(item).Should().NotBeNullOrEmpty();
			if (++seen == limit)
			{
				break;
			}
		}

		return seen;
	}

	/// <summary>Checks that the call is raised as an API exception carrying HTTP 404.</summary>
	/// <param name="call">The call, which asks for an entity that does not exist.</param>
	public static async Task ShouldBeNotFoundAsync(Func<Task> call)
		=> (await call.Should().ThrowAsync<DefenderApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
