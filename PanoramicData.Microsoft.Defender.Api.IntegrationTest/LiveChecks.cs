using System.Net;
using PanoramicData.MicrosoftDefender.Api.Models;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest;

/// <summary>Assertions and lookups the live tests of every group make in the same way.</summary>
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

	/// <summary>Checks that the call is refused by the read-only client before anything is sent.</summary>
	/// <param name="call">The write call, aimed at a made-up identifier so that nothing real could be touched even if the guard failed.</param>
	public static async Task ShouldBeRefusedAsReadOnlyAsync(Func<Task> call)
		=> await call.Should().ThrowAsync<DefenderReadOnlyException>();

	/// <summary>Finds the first item of a page that satisfies a condition, failing loudly when the tenant has none.</summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="page">The page to search.</param>
	/// <param name="match">The condition the item must satisfy.</param>
	/// <param name="whenMissing">The failure message, which says what the tenant lacks.</param>
	/// <returns>The first matching item.</returns>
	public static T FirstOrThrow<T>(ODataPage<T> page, Func<T, bool> match, string whenMissing)
		where T : class
		=> page.Value.FirstOrDefault(match) ?? throw new InvalidOperationException(whenMissing);

	/// <summary>Checks that a page of at most <paramref name="top"/> items is not empty and that every item is complete.</summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="page">The page that was requested with <c>$top</c>.</param>
	/// <param name="top">The <c>$top</c> that was requested.</param>
	/// <param name="isComplete">Whether an item carries the properties every item must have.</param>
	public static void ShouldBeACompletePage<T>(ODataPage<T> page, int top, Func<T, bool> isComplete)
	{
		page.Value.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(top);
		page.Value.Should().OnlyContain(item => isComplete(item));
	}

	/// <summary>Reads a per-entity list and checks that every item it returns has an identifier.</summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="list">The list operation.</param>
	/// <param name="parentId">The identifier of the entity the list belongs to.</param>
	/// <param name="identifier">Selects the item's identifying value.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <param name="requireItems">Whether an empty list is a failure, for lists the tenant is expected to have content for.</param>
	/// <returns>A task that completes when the check has passed.</returns>
	public static async Task ShouldListIdentifiedItemsAsync<T>(Func<string, CancellationToken, Task<ODataPage<T>>> list, string parentId, Func<T, string?> identifier, CancellationToken cancellationToken, bool requireItems = false)
	{
		var page = await list(parentId, cancellationToken);

		if (requireItems)
		{
			page.Value.Should().NotBeEmpty();
		}

		page.Value.Should().OnlyContain(item => !string.IsNullOrEmpty(identifier(item)));
	}
}
