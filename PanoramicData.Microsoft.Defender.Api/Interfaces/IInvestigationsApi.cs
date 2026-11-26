using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Investigations;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Automated investigations (<c>investigations</c>). Reading needs the <c>Alert.ReadWrite.All</c> application permission
/// (the documented one).
/// </summary>
public interface IInvestigationsApi
{
	/// <summary>Lists investigations, one page at a time (<c>GET investigations</c>).</summary>
	/// <param name="options">OData options, or <see langword="null"/> for none. <c>$filter</c> may name startTime, id, state, machineId and triggeringAlertId; <c>$top</c> is limited to 10,000 (the page spells it <c>$stop</c>, a typo).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of investigations.</returns>
	[Get("investigations")]
	Task<ODataPage<Investigation>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

	/// <summary>Gets one investigation by its id, or by the id of the alert that triggered it (<c>GET investigations/{id}</c>).</summary>
	/// <param name="id">The investigation id.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The investigation.</returns>
	[Get("investigations/{id}")]
	Task<Investigation> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Starts an automated investigation on a device (<c>POST machines/{id}/startInvestigation</c>). Changes tenant state;
	/// needs the <c>Alert.ReadWrite.All</c> permission and, as a user, the <c>Active remediation actions</c> role. Limited to
	/// 50 calls an hour. The service answers 201 with the new investigation.
	/// </summary>
	/// <param name="id">The device id.</param>
	/// <param name="request">The reason for the investigation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The investigation.</returns>
	[Post("machines/{id}/startInvestigation")]
	Task<Investigation> StartAsync(string id, [Body] StartInvestigationRequest request, CancellationToken cancellationToken);
}
