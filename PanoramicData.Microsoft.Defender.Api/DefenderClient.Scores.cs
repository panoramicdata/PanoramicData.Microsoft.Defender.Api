using PanoramicData.MicrosoftDefender.Api.Interfaces;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>The exposure score and the secure score for devices (<c>exposureScore</c>, <c>configurationScore</c>).</summary>
	public IScoresApi Scores => field ??= For<IScoresApi>();
}
