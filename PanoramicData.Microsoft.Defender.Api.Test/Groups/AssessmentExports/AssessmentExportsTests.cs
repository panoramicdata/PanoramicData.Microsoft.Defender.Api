using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Requests of the assessment export operations: the "by machine" JSON flavour and the "via files" flavour.</summary>
public partial class AssessmentExportsTests
{
	private const string FilesResponse = """{"exportFiles":["https://store.blob.test/exports/a.json.gz?sv=1&sig=SECRET-A"],"generatedTime":"2026-01-02T03:04:05Z"}""";

	internal sealed record ByMachineOp(string Path, Func<DefenderClient, ExportByMachineOptions?, CancellationToken, Task> Call);

	internal sealed record ViaFilesOp(string Path, Func<DefenderClient, ExportViaFilesOptions?, CancellationToken, Task<ExportFilesResponse>> Call);

	private static readonly ByMachineOp[] ByMachineOps =
	[
		new("/api/machines/SecureConfigurationsAssessmentByMachine", (c, o, t) => c.AssessmentExports.ListSecureConfigurationsByMachineAsync(o, t)),
		new("/api/machines/SoftwareInventoryByMachine", (c, o, t) => c.AssessmentExports.ListSoftwareInventoryByMachineAsync(o, t)),
		new("/api/machines/SoftwareVulnerabilitiesByMachine", (c, o, t) => c.AssessmentExports.ListSoftwareVulnerabilitiesByMachineAsync(o, t)),
		new("/api/machines/SoftwareInventoryNoProductCodeByMachine", (c, o, t) => c.AssessmentExports.ListNonCpeSoftwareInventoryByMachineAsync(o, t)),
		new("/api/Machines/BrowserExtensionsInventoryByMachine", (c, o, t) => c.AssessmentExports.ListBrowserExtensionsByMachineAsync(o, t)),
		new("/api/machines/certificateAssessmentByMachine", (c, o, t) => c.AssessmentExports.ListCertificatesByMachineAsync(o, t)),
		new("/api/machines/baselineComplianceAssessmentByMachine", (c, o, t) => c.AssessmentExports.ListBaselineComplianceByMachineAsync(o, t)),
	];

	private static readonly ViaFilesOp[] ViaFilesOps =
	[
		new("/api/machines/SecureConfigurationsAssessmentExport", (c, o, t) => c.AssessmentExports.ExportSecureConfigurationsAsync(o, t)),
		new("/api/machines/SoftwareInventoryExport", (c, o, t) => c.AssessmentExports.ExportSoftwareInventoryAsync(o, t)),
		new("/api/machines/SoftwareVulnerabilitiesExport", (c, o, t) => c.AssessmentExports.ExportSoftwareVulnerabilitiesAsync(o, t)),
		new("/api/machines/SoftwareInventoryNonCpeExport", (c, o, t) => c.AssessmentExports.ExportNonCpeSoftwareInventoryAsync(o, t)),
		new("/api/machines/browserextensionsinventoryExport", (c, o, t) => c.AssessmentExports.ExportBrowserExtensionsAsync(o, t)),
		new("/api/Machines/InfoGatheringExport", (c, o, t) => c.AssessmentExports.ExportInfoGatheringAsync(o, t)),
		new("/api/machines/certificateAssessmentExport", (c, o, t) => c.AssessmentExports.ExportCertificatesAsync(o, t)),
		new("/api/machines/BaselineComplianceAssessmentExport", (c, o, t) => c.AssessmentExports.ExportBaselineComplianceAsync(o, t)),
	];

	private static Task ShouldGetByMachine(string path)
		=> ShouldGetAsync((c, t) => ByMachineOps.Single(o => o.Path == path).Call(c, null, t), Envelope(""), path);

	private static Task ShouldGetViaFiles(string path)
		=> ShouldGetAsync((c, t) => ViaFilesOps.Single(o => o.Path == path).Call(c, null, t), FilesResponse, path);

	[Fact]
	public Task SecureConfigurationsByMachine_SendsGetSecureConfigurationsAssessmentByMachine()
		=> ShouldGetByMachine("/api/machines/SecureConfigurationsAssessmentByMachine");

	[Fact]
	public Task SecureConfigurationsExport_SendsGetSecureConfigurationsAssessmentExport()
		=> ShouldGetViaFiles("/api/machines/SecureConfigurationsAssessmentExport");

	[Fact]
	public Task SoftwareInventoryByMachine_SendsGetSoftwareInventoryByMachine()
		=> ShouldGetByMachine("/api/machines/SoftwareInventoryByMachine");

	[Fact]
	public Task SoftwareInventoryExport_SendsGetSoftwareInventoryExport()
		=> ShouldGetViaFiles("/api/machines/SoftwareInventoryExport");

	[Fact]
	public Task SoftwareVulnerabilitiesByMachine_SendsGetSoftwareVulnerabilitiesByMachine()
		=> ShouldGetByMachine("/api/machines/SoftwareVulnerabilitiesByMachine");

	[Fact]
	public Task SoftwareVulnerabilitiesExport_SendsGetSoftwareVulnerabilitiesExport()
		=> ShouldGetViaFiles("/api/machines/SoftwareVulnerabilitiesExport");

	[Fact]
	public Task NonCpeSoftwareInventoryByMachine_SendsGetSoftwareInventoryNoProductCodeByMachine()
		=> ShouldGetByMachine("/api/machines/SoftwareInventoryNoProductCodeByMachine");

	[Fact]
	public Task NonCpeSoftwareInventoryExport_SendsGetSoftwareInventoryNonCpeExport()
		=> ShouldGetViaFiles("/api/machines/SoftwareInventoryNonCpeExport");

	[Fact]
	public Task BrowserExtensionsByMachine_SendsGetBrowserExtensionsInventoryByMachine()
		=> ShouldGetByMachine("/api/Machines/BrowserExtensionsInventoryByMachine");

	[Fact]
	public Task BrowserExtensionsExport_SendsGetBrowserExtensionsInventoryExport()
		=> ShouldGetViaFiles("/api/machines/browserextensionsinventoryExport");

	[Fact]
	public Task InfoGatheringExport_SendsGetInfoGatheringExport()
		=> ShouldGetViaFiles("/api/Machines/InfoGatheringExport");

	[Fact]
	public Task CertificatesByMachine_SendsGetCertificateAssessmentByMachine()
		=> ShouldGetByMachine("/api/machines/certificateAssessmentByMachine");

	[Fact]
	public Task CertificatesExport_SendsGetCertificateAssessmentExport()
		=> ShouldGetViaFiles("/api/machines/certificateAssessmentExport");

	[Fact]
	public Task BaselineComplianceByMachine_SendsGetBaselineComplianceAssessmentByMachine()
		=> ShouldGetByMachine("/api/machines/baselineComplianceAssessmentByMachine");

	[Fact]
	public Task BaselineComplianceExport_SendsGetBaselineComplianceAssessmentExport()
		=> ShouldGetViaFiles("/api/machines/BaselineComplianceAssessmentExport");

	[Fact]
	public async Task SoftwareVulnerabilityChangesByMachine_SendsTheRequiredSinceTime()
	{
		var options = new DeltaExportOptions { SinceTime = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)) };

		await ShouldGetAsync((c, t) => c.AssessmentExports.ListSoftwareVulnerabilityChangesByMachineAsync(options, t), Envelope(""),
			"/api/machines/SoftwareVulnerabilityChangesByMachine", "?sinceTime=2026-01-02T01%3A04%3A05Z");
	}

	[Fact]
	public async Task SoftwareVulnerabilityChangesByMachine_SendsEveryOption()
	{
		var options = new DeltaExportOptions { SinceTime = new DateTimeOffset(2026, 1, 2, 3, 4, 5, 250, TimeSpan.Zero), PageSize = 500, Top = 20 };

		await ShouldGetAsync((c, t) => c.AssessmentExports.ListSoftwareVulnerabilityChangesByMachineAsync(options, t), Envelope(""),
			"/api/machines/SoftwareVulnerabilityChangesByMachine", "?sinceTime=2026-01-02T03%3A04%3A05.25Z&pageSize=500&$top=20");
	}

	[Fact]
	public async Task ByMachineOperations_SendEveryOption_WithTheirWireNames()
	{
		var options = new ExportByMachineOptions { PageSize = 200000, Top = 7, SinceTime = new DateTimeOffset(2026, 5, 19, 18, 35, 49, TimeSpan.Zero) };

		foreach (var op in ByMachineOps)
		{
			await ShouldGetAsync((c, t) => op.Call(c, options, t), Envelope(""), op.Path, "?pageSize=200000&$top=7&sinceTime=2026-05-19T18%3A35%3A49Z");
		}
	}

	[Fact]
	public async Task ViaFilesOperations_SendTheLinkLifetime()
	{
		foreach (var op in ViaFilesOps)
		{
			await ShouldGetAsync((c, t) => op.Call(c, new ExportViaFilesOptions { SasValidHours = 24 }, t), FilesResponse, op.Path, "?sasValidHours=24");
		}
	}

	[Fact]
	public async Task ViaFilesOperations_DoNotValidateTheLinkLifetime()
	{
		// The documentation gives a maximum of 6 hours for most exports and 24 for others, so the service decides.
		await ShouldGetAsync((c, t) => c.AssessmentExports.ExportSoftwareInventoryAsync(new ExportViaFilesOptions { SasValidHours = 99 }, t),
			FilesResponse, "/api/machines/SoftwareInventoryExport", "?sasValidHours=99");
	}

	[Fact]
	public async Task EveryOperation_MapsAnError()
	{
		foreach (var op in ByMachineOps)
		{
			await ShouldFailWith403Async((c, t) => op.Call(c, null, t));
		}

		foreach (var op in ViaFilesOps)
		{
			await ShouldFailWith403Async((c, t) => op.Call(c, null, t));
		}

		await ShouldFailWith403Async((c, t) => c.AssessmentExports.ListSoftwareVulnerabilityChangesByMachineAsync(new DeltaExportOptions { SinceTime = DateTimeOffset.UnixEpoch }, t));
	}

	[Fact]
	public async Task ViaFilesOperations_ReadTheLinksAndTheGenerationTime()
	{
		foreach (var op in ViaFilesOps)
		{
			using var client = TestClient.Create(TestClient.Stub(FilesResponse));
			var response = await op.Call(client, null, TestContext.Current.CancellationToken);

			response.ExportFiles.Should().Equal("https://store.blob.test/exports/a.json.gz?sv=1&sig=SECRET-A");
			response.GeneratedTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero));
			response.ToString().Should().NotContain("SECRET");
		}
	}

}
