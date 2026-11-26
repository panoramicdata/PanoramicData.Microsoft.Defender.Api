using PanoramicData.MicrosoftDefender.Api.Interfaces;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>The live response library (<c>libraryfiles</c>).</summary>
	public ILibraryApi Library => field ??= For<ILibraryApi>();
}
