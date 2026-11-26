using PanoramicData.MicrosoftDefender.Api.IntegrationTest.Entities;
using PanoramicData.MicrosoftDefender.Api.Models.AuthenticatedScan;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.AuthenticatedScan;

/// <summary>
/// Read-only checks of the authenticated scan operations against a live tenant (needs <c>Machine.Read.All</c> and at least
/// one scan definition and agent). The history queries are POSTs that only read, so the read-only client allows them. The
/// create, update and delete operations are NEVER run live: they are unit-tested only, and here only the read-only guard's
/// refusal is checked. Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class AuthenticatedScanIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private async Task<ScanDefinition> FirstDefinitionAsync()
	{
		var page = await fixture.Client.AuthenticatedScan.ListDefinitionsAsync(Ct);
		return page.Value.FirstOrDefault(d => !string.IsNullOrEmpty(d.Id))
			?? throw new InvalidOperationException("The tenant has no authenticated scan definition, so the scan operations cannot be checked. Create one in the Defender portal first.");
	}

	[Fact]
	public async Task ListDefinitionsAsync_ReturnsDefinitionsWithoutSecrets()
	{
		var definition = await FirstDefinitionAsync();

		definition.ScanName.Should().NotBeNullOrEmpty();
		definition.ScanAuthenticationParams?.AuthPassword.Should().BeNull("the service does not echo credentials");
	}

	[Fact]
	public async Task ListAgentsAsync_ReturnsAgents_AndGetAgentAsyncReadsOne()
	{
		var agents = await fixture.Client.AuthenticatedScan.ListAgentsAsync(Ct);
		var first = agents.Value.FirstOrDefault(a => !string.IsNullOrEmpty(a.Id))
			?? throw new InvalidOperationException("The tenant has no scan agent, so the agent lookup cannot be checked.");

		var details = await fixture.Client.AuthenticatedScan.GetAgentAsync(first.Id!, Ct);

		(details.Id ?? (details.Value.Count > 0 ? details.Value[0].Id : null)).Should().Be(first.Id);
	}

	[Fact]
	public async Task GetAllScanDefinitionsAsync_EnumeratesThePages()
	{
		var ids = new List<string?>();

		await foreach (var definition in fixture.Client.GetAllScanDefinitionsAsync(Ct))
		{
			ids.Add(definition.Id);
		}

		ids.Should().NotBeEmpty().And.OnlyContain(id => !string.IsNullOrEmpty(id));
	}

	[Fact]
	public async Task GetScanHistoryByDefinitionAsync_ReturnsRows_AndHonoursTop()
	{
		var definition = await FirstDefinitionAsync();

		var page = await fixture.Client.AuthenticatedScan.GetScanHistoryByDefinitionAsync(
			new ScanHistoryByDefinitionRequest { ScanDefinitionIds = [definition.Id!] }, new ScanHistoryOptions { Top = 1 }, Ct);

		page.Value.Should().HaveCountLessThanOrEqualTo(1);
	}

	[Fact]
	public async Task GetScanHistoryBySessionAsync_ReturnsTheSession()
	{
		var sessionId = TestSetting.Required(fixture, "Defender:Test:ScanSessionId");

		var page = await fixture.Client.AuthenticatedScan.GetScanHistoryBySessionAsync(new ScanHistoryBySessionRequest { SessionIds = [sessionId] }, Ct);

		page.Value.Should().OnlyContain(s => s.SessionIds == sessionId);
	}

	[Fact]
	public async Task TheWrites_AreRefusedByTheReadOnlyClient_AndNeverSent()
	{
		var act = () => fixture.Client.AuthenticatedScan.DeleteDefinitionsAsync(new DeleteScanDefinitionsRequest { ScanDefinitionIds = ["00000000-0000-0000-0000-000000000000"] }, Ct);

		await act.Should().ThrowAsync<DefenderReadOnlyException>();
	}
}
