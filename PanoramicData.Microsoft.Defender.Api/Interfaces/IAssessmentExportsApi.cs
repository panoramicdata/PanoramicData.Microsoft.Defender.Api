using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Bulk snapshots of Defender Vulnerability Management data, one record per device (<c>machines/...</c>). Each dataset comes
/// in up to two flavours. The <c>...ByMachine</c> operations return a paged JSON collection (30 calls a minute, 1,000 an
/// hour); read it with the matching <c>GetAll...Async</c> helper on <see cref="DefenderClient"/>. The <c>...Export</c>
/// operations return download links to gzip files of JSON lines (5 calls a minute, 20 an hour): keep the response and read it
/// with the matching <c>Stream...Async</c> helper or <see cref="DefenderClient.StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>.
/// The data is a snapshot of now, with no history. Which application permission is needed is stated on each operation.
/// </summary>
/// <remarks>
/// The documentation says some extra, undocumented columns may appear and be dropped later; the record models read only the
/// documented ones. The record models are shared by the JSON and the file flavour of a dataset, which is an assumption that no
/// live test has confirmed yet.
/// </remarks>
public interface IAssessmentExportsApi
{
	/// <summary>
	/// Lists the secure-configuration assessment, one record per device and configuration, as paged JSON
	/// (<c>GET machines/SecureConfigurationsAssessmentByMachine</c>). Needs <c>Vulnerability.Read.All</c>.
	/// </summary>
	/// <param name="options">Paging options, or <see langword="null"/> for the service defaults.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of records.</returns>
	[Get("machines/SecureConfigurationsAssessmentByMachine")]
	Task<ODataPage<SecureConfigurationAssessment>> ListSecureConfigurationsByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the secure-configuration assessment (<c>GET machines/SecureConfigurationsAssessmentExport</c>).
	/// Needs <c>Vulnerability.Read.All</c>. Limited to 5 calls a minute and 20 an hour.
	/// </summary>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the default of one hour.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("machines/SecureConfigurationsAssessmentExport")]
	Task<ExportFilesResponse> ExportSecureConfigurationsAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the installed-software assessment, one record per device, vendor, product and version, as paged JSON
	/// (<c>GET machines/SoftwareInventoryByMachine</c>). Needs <c>Software.Read.All</c>.
	/// </summary>
	/// <remarks>The documentation's example also passes <c>sinceTime</c>, which its parameter list does not mention; it is sent when set and unverified.</remarks>
	/// <param name="options">Paging options, or <see langword="null"/> for the service defaults.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of records.</returns>
	[Get("machines/SoftwareInventoryByMachine")]
	Task<ODataPage<SoftwareInventoryAssessment>> ListSoftwareInventoryByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the installed-software assessment (<c>GET machines/SoftwareInventoryExport</c>). Needs
	/// <c>Software.Read.All</c>. Limited to 5 calls a minute and 20 an hour.
	/// </summary>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the default of one hour.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("machines/SoftwareInventoryExport")]
	Task<ExportFilesResponse> ExportSoftwareInventoryAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the software-vulnerability assessment, one record per device, product and CVE, as paged JSON
	/// (<c>GET machines/SoftwareVulnerabilitiesByMachine</c>). Needs <c>Vulnerability.Read.All</c>. A record is about 1 KB, so
	/// size the page accordingly.
	/// </summary>
	/// <param name="options">Paging options, or <see langword="null"/> for the service defaults.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of records.</returns>
	[Get("machines/SoftwareVulnerabilitiesByMachine")]
	Task<ODataPage<SoftwareVulnerabilityAssessment>> ListSoftwareVulnerabilitiesByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the software-vulnerability assessment (<c>GET machines/SoftwareVulnerabilitiesExport</c>).
	/// Needs <c>Vulnerability.Read.All</c>. Limited to 5 calls a minute and 20 an hour.
	/// </summary>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the default of one hour.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("machines/SoftwareVulnerabilitiesExport")]
	Task<ExportFilesResponse> ExportSoftwareVulnerabilitiesAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the changes to software vulnerabilities since a point in time (new, fixed or updated), as paged JSON
	/// (<c>GET machines/SoftwareVulnerabilityChangesByMachine</c>). Needs <c>Vulnerability.Read.All</c>. This is a delta feed,
	/// not a full export.
	/// </summary>
	/// <param name="options">The required <c>sinceTime</c> (the service looks back at most 14 days) and optional paging.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of change records.</returns>
	[Get("machines/SoftwareVulnerabilityChangesByMachine")]
	Task<ODataPage<SoftwareVulnerabilityChange>> ListSoftwareVulnerabilityChangesByMachineAsync([Query] DeltaExportOptions options, CancellationToken cancellationToken);

	/// <summary>
	/// Lists installed software that has no CPE product code, one record per device, vendor, product and version, as paged
	/// JSON (<c>GET machines/SoftwareInventoryNoProductCodeByMachine</c>). Needs <c>Software.Read.All</c>.
	/// </summary>
	/// <param name="options">Paging options, or <see langword="null"/> for the service defaults.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of records.</returns>
	[Get("machines/SoftwareInventoryNoProductCodeByMachine")]
	Task<ODataPage<NonCpeSoftwareInventoryAssessment>> ListNonCpeSoftwareInventoryByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the software that has no CPE product code (<c>GET machines/SoftwareInventoryNonCpeExport</c>).
	/// Needs <c>Software.Read.All</c>. Limited to 5 calls a minute and 20 an hour.
	/// </summary>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the default of one hour.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("machines/SoftwareInventoryNonCpeExport")]
	Task<ExportFilesResponse> ExportNonCpeSoftwareInventoryAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Lists installed browser extensions, one record per device, browser and extension, as paged JSON
	/// (<c>GET Machines/BrowserExtensionsInventoryByMachine</c>). Needs <c>Software.Read.All</c>. A record is about 0.5 KB.
	/// </summary>
	/// <param name="options">Paging options, or <see langword="null"/> for the service defaults.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of records.</returns>
	[Get("Machines/BrowserExtensionsInventoryByMachine")]
	Task<ODataPage<BrowserExtensionAssessment>> ListBrowserExtensionsByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the browser-extension assessment (<c>GET machines/browserextensionsinventoryExport</c>).
	/// Needs <c>Software.Read.All</c>. Limited to 5 calls a minute and 20 an hour.
	/// </summary>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the default of one hour.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("machines/browserextensionsinventoryExport")]
	Task<ExportFilesResponse> ExportBrowserExtensionsAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the information-gathering assessment, one record per device
	/// (<c>GET Machines/InfoGatheringExport</c>). Needs <c>Vulnerability.Read.All</c>. Limited to 5 calls a minute and 20 an hour.
	/// </summary>
	/// <remarks>
	/// The documentation lists this path twice, as this assessment and as the "via files" flavour of the device-health report,
	/// with differing limits on <c>sasValidHours</c> (6 and 24 hours). It is one operation here, and the lifetime is not
	/// validated client-side: the service decides. Unverified.
	/// </remarks>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the default of one hour.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("Machines/InfoGatheringExport")]
	Task<ExportFilesResponse> ExportInfoGatheringAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the certificate assessment, one record per device, thumbprint and path, as paged JSON
	/// (<c>GET machines/certificateAssessmentByMachine</c>). Needs <c>Vulnerability.Read.All</c>. A record is about 1 KB.
	/// </summary>
	/// <param name="options">Paging options, or <see langword="null"/> for the service defaults.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of records.</returns>
	[Get("machines/certificateAssessmentByMachine")]
	Task<ODataPage<CertificateAssessment>> ListCertificatesByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the certificate assessment (<c>GET machines/certificateAssessmentExport</c>). Needs
	/// <c>Vulnerability.Read.All</c>. Limited to 5 calls a minute and 20 an hour.
	/// </summary>
	/// <remarks>This page documents a maximum link lifetime of 24 hours and a default of 3, unlike the others (6 and 1); the value is not validated here.</remarks>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the service default.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("machines/certificateAssessmentExport")]
	Task<ExportFilesResponse> ExportCertificatesAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Lists the security-baseline compliance assessment, one record per device, profile and configuration, as paged JSON
	/// (<c>GET machines/baselineComplianceAssessmentByMachine</c>). Needs <c>SecurityBaselinesAssessment.Read.All</c>.
	/// </summary>
	/// <param name="options">Paging options, or <see langword="null"/> for the service defaults.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of records.</returns>
	[Get("machines/baselineComplianceAssessmentByMachine")]
	Task<ODataPage<BaselineComplianceAssessment>> ListBaselineComplianceByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

	/// <summary>
	/// Requests download links for the security-baseline compliance assessment
	/// (<c>GET machines/BaselineComplianceAssessmentExport</c>). Needs <c>SecurityBaselinesAssessment.Read.All</c>. Limited to 5
	/// calls a minute and 20 an hour.
	/// </summary>
	/// <param name="options">Link lifetime options, or <see langword="null"/> for the default of one hour.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed download links. Treat them as secrets.</returns>
	[Get("machines/BaselineComplianceAssessmentExport")]
	Task<ExportFilesResponse> ExportBaselineComplianceAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);
}
