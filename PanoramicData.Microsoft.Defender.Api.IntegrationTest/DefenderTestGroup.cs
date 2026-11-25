namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest;

/// <summary>Shares one <see cref="DefenderFixture"/> (and so one token) across every integration test class.</summary>
[CollectionDefinition(Name)]
public sealed class DefenderTestGroup : ICollectionFixture<DefenderFixture>
{
	internal const string Name = "Defender";
}
