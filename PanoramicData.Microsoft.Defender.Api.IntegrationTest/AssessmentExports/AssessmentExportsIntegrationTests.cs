using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.AssessmentExports;

/// <summary>
/// Read-only checks of the assessment export operations against a live tenant (needs the <c>Vulnerability.Read.All</c>,
/// <c>Software.Read.All</c> and <c>SecurityBaselinesAssessment.Read.All</c> application permissions and onboarded devices).
/// The file exports allow only 5 calls a minute and 20 an hour, so only four of them are exercised, one call each. Live
/// verification is pending: see docs/TESTING.md.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class AssessmentExportsIntegrationTests(DefenderFixture fixture)
{
	private static readonly ExportByMachineOptions OneSmallPage = new() { PageSize = 2 };

	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task SecureConfigurationsByMachine_ReturnsDeviceConfigurationRows()
	{
		var page = await fixture.Client.AssessmentExports.ListSecureConfigurationsByMachineAsync(OneSmallPage, Ct);

		page.Value.Should().NotBeEmpty("the tenant needs onboarded devices");
		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.DeviceId) && !string.IsNullOrEmpty(r.ConfigurationId));
	}

	[Fact]
	public async Task SoftwareInventoryByMachine_ReturnsDeviceSoftwareRows()
	{
		var page = await fixture.Client.AssessmentExports.ListSoftwareInventoryByMachineAsync(OneSmallPage, Ct);

		page.Value.Should().NotBeEmpty();
		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.DeviceId) && !string.IsNullOrEmpty(r.SoftwareName));
	}

	[Fact]
	public async Task SoftwareVulnerabilitiesByMachine_ReturnsDeviceVulnerabilityRows()
	{
		var page = await fixture.Client.AssessmentExports.ListSoftwareVulnerabilitiesByMachineAsync(OneSmallPage, Ct);

		page.Value.Should().NotBeEmpty("the tenant needs at least one exposed vulnerability");
		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.DeviceId) && !string.IsNullOrEmpty(r.CveId));
	}

	[Fact]
	public async Task SoftwareVulnerabilityChangesByMachine_AcceptsASinceTime()
	{
		var options = new DeltaExportOptions { SinceTime = DateTimeOffset.UtcNow.AddDays(-2), PageSize = 5 };

		var page = await fixture.Client.AssessmentExports.ListSoftwareVulnerabilityChangesByMachineAsync(options, Ct);

		page.Value.Should().OnlyContain(r => r.Status == "New" || r.Status == "Fixed" || r.Status == "Updated");
	}

	[Fact]
	public async Task NonCpeSoftwareInventoryByMachine_ReturnsAPage()
	{
		var page = await fixture.Client.AssessmentExports.ListNonCpeSoftwareInventoryByMachineAsync(OneSmallPage, Ct);

		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.DeviceId));
	}

	[Fact]
	public async Task BrowserExtensionsByMachine_ReturnsAPage()
	{
		var page = await fixture.Client.AssessmentExports.ListBrowserExtensionsByMachineAsync(OneSmallPage, Ct);

		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.DeviceId) && !string.IsNullOrEmpty(r.ExtensionId));
	}

	[Fact]
	public async Task CertificatesByMachine_ReturnsAPage()
	{
		var page = await fixture.Client.AssessmentExports.ListCertificatesByMachineAsync(OneSmallPage, Ct);

		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.DeviceId) && !string.IsNullOrEmpty(r.Thumbprint));
	}

	[Fact]
	public async Task BaselineComplianceByMachine_ReturnsAPage()
	{
		var page = await fixture.Client.AssessmentExports.ListBaselineComplianceByMachineAsync(OneSmallPage, Ct);

		page.Value.Should().OnlyContain(r => !string.IsNullOrEmpty(r.DeviceId) && !string.IsNullOrEmpty(r.ConfigurationId));
	}

	[Fact]
	public async Task GetAllSoftwareInventoryAssessments_FollowsTheNextLink()
	{
		var seen = 0;

		await foreach (var record in fixture.Client.GetAllSoftwareInventoryAssessmentsAsync(new ExportByMachineOptions { PageSize = 2 }, Ct))
		{
			record.DeviceId.Should().NotBeNullOrEmpty();
			if (++seen == 5)
			{
				break;
			}
		}

		seen.Should().BeGreaterThan(2, "more than one page of two was read, so the next link was followed");
	}

	[Fact]
	public async Task SoftwareInventoryExport_ListsFilesThatStream()
	{
		var export = await fixture.Client.AssessmentExports.ExportSoftwareInventoryAsync(null, Ct);

		export.ExportFiles.Should().NotBeEmpty();
		export.ExportFiles.Should().OnlyContain(l => l.StartsWith("https://", StringComparison.Ordinal));
		await AssertFirstRecordAsync(fixture.Client.StreamSoftwareInventoryAssessmentsAsync(export, Ct), r => r.SoftwareName);
	}

	[Fact]
	public async Task SoftwareVulnerabilitiesExport_ListsFilesThatStream()
	{
		var export = await fixture.Client.AssessmentExports.ExportSoftwareVulnerabilitiesAsync(new ExportViaFilesOptions { SasValidHours = 2 }, Ct);

		export.ExportFiles.Should().NotBeEmpty();
		await AssertFirstRecordAsync(fixture.Client.StreamSoftwareVulnerabilityAssessmentsAsync(export, Ct), r => r.CveId);
	}

	[Fact]
	public async Task BrowserExtensionsExport_ListsFiles()
	{
		var export = await fixture.Client.AssessmentExports.ExportBrowserExtensionsAsync(null, Ct);

		export.ExportFiles.Should().NotBeNull();
		export.GeneratedTime.Should().NotBeNull();
	}

	[Fact]
	public async Task InfoGatheringExport_ListsFiles()
	{
		var export = await fixture.Client.AssessmentExports.ExportInfoGatheringAsync(null, Ct);

		export.ExportFiles.Should().NotBeNull();
		export.GeneratedTime.Should().NotBeNull();
	}

	private static async Task AssertFirstRecordAsync<T>(IAsyncEnumerable<T> records, Func<T, string?> key)
	{
		await foreach (var record in records)
		{
			key(record).Should().NotBeNullOrEmpty();
			return;
		}

		throw new InvalidOperationException("The export listed files but they held no records. Use a tenant with onboarded devices.");
	}
}
