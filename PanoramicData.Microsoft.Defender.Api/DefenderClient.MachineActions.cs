using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.MachineActions;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Actions requested on devices, and following them (<c>machineactions</c>).</summary>
	public IMachineActionsApi MachineActions => field ??= For<IMachineActionsApi>();

	/// <summary>Enumerates every machine action, following the page links (<c>GET machineactions</c>).</summary>
	/// <param name="options">OData options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every machine action.</returns>
	public IAsyncEnumerable<MachineAction> GetAllMachineActionsAsync(ODataListOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => MachineActions.ListAsync(options, token), cancellationToken);
}
