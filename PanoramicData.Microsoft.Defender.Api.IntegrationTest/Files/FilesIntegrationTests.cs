using PanoramicData.MicrosoftDefender.Api.IntegrationTest.Entities;
using PanoramicData.MicrosoftDefender.Api.Models;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Files;

/// <summary>
/// Read-only checks of the files operations against a live tenant. Needs <c>File.Read.All</c>, <c>Alert.ReadWrite.All</c> and
/// <c>Machine.ReadWrite.All</c> application permissions and the setting <c>Defender:Test:FileSha1</c> (a SHA-1 the tenant has
/// seen). Live verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class FilesIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private string Sha1 => TestSetting.Required(fixture, "Defender:Test:FileSha1");

	[Fact]
	public async Task GetAsync_ReturnsTheFileProfile()
	{
		var file = await fixture.Client.Files.GetAsync(Sha1, Ct);

		file.Sha1.Should().BeEquivalentTo(Sha1);
		file.Sha256.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task GetStatisticsAsync_ReturnsPrevalence_ForALookBackWindow()
	{
		var stats = await fixture.Client.Files.GetStatisticsAsync(Sha1, new EntityStatisticsOptions { LookBackHours = 48 }, Ct);

		stats.Sha1.Should().BeEquivalentTo(Sha1);
		stats.OrganizationPrevalence.Should().BeGreaterThanOrEqualTo(0);
	}

	[Fact]
	public async Task ListAlertsAndMachines_AnswerPages()
	{
		var alerts = await fixture.Client.Files.ListAlertsAsync(Sha1, Ct);
		var machines = await fixture.Client.Files.ListMachinesAsync(Sha1, Ct);

		alerts.Value.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Id));
		machines.Value.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Id));
	}

	[Fact]
	public async Task GetAllFileAlertsAsync_EnumeratesThePages()
	{
		var count = 0;

		await foreach (var alert in fixture.Client.GetAllFileAlertsAsync(Sha1, Ct))
		{
			alert.Id.Should().NotBeNullOrEmpty();
			count++;
		}

		count.Should().BeGreaterThanOrEqualTo(0);
	}

	[Fact]
	public async Task AnUnknownHash_IsRaisedAsAnApiException()
	{
		await LiveChecks.ShouldBeNotFoundAsync(() => fixture.Client.Files.GetAsync("0000000000000000000000000000000000000000", Ct));
	}
}
