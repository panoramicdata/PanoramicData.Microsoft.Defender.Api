using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Indicators;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Custom threat-intelligence indicators (<c>indicators</c>): values Defender should block, allow, audit or alert on.
/// Needs <c>Ti.ReadWrite.All</c> (without <c>.All</c>, only the caller's own indicators are visible). A tenant holds at
/// most 15,000 active indicators. The create, import and delete operations change tenant data and are refused by a
/// read-only client.
/// </summary>
public interface IIndicatorsApi
{
	/// <summary>Lists the active indicators, one page at a time (<c>GET indicators</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name application, createdByDisplayName, expirationTime, generateAlert, title, rbacGroupNames, rbacGroupIds, indicatorValue, indicatorType, creationTimeDateTimeUtc, createdBy, action and severity.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of indicators.</returns>
	[Get("indicators")]
	Task<ODataPage<Indicator>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Creates or updates one indicator (<c>POST indicators</c>). Changes tenant data; CIDR ranges are not supported. The
	/// service answers 200 with the stored indicator.
	/// </summary>
	/// <param name="indicator">The indicator.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The stored indicator.</returns>
	[Post("indicators")]
	Task<Indicator> CreateAsync([Body] IndicatorSubmission indicator, CancellationToken cancellationToken);

	/// <summary>
	/// Creates or updates up to 500 indicators in one call (<c>POST indicators/import</c>). Changes tenant data; limited to
	/// 30 calls a minute. One result per indicator says whether it was accepted.
	/// </summary>
	/// <param name="request">The batch (1 to 500 indicators; the request type enforces this).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The per-indicator results.</returns>
	[Post("indicators/import")]
	Task<ODataPage<IndicatorImportResult>> ImportAsync([Body] IndicatorImportRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes one indicator (<c>DELETE indicators/{id}</c>). Destructive. Answers 204, or 404 when it does not exist.</summary>
	/// <param name="id">The indicator id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that completes when the indicator is deleted.</returns>
	[Delete("indicators/{id}")]
	Task DeleteAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes up to 500 indicators in one call (<c>POST indicators/BatchDelete</c>). Destructive; limited to 30 calls a
	/// minute and 1,500 an hour. Answers 204; 400 for an empty, oversized or invalid list, 403 when the caller cannot see an
	/// indicator's device groups, and 404 when any id does not exist.
	/// </summary>
	/// <param name="request">The ids (1 to 500; the request type enforces this).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that completes when the indicators are deleted.</returns>
	[Post("indicators/BatchDelete")]
	Task BatchDeleteAsync([Body] IndicatorBatchDeleteRequest request, CancellationToken cancellationToken);
}
