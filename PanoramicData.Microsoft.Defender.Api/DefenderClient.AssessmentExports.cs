using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Interfaces;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;

namespace PanoramicData.MicrosoftDefender.Api;

public sealed partial class DefenderClient
{
	/// <summary>Bulk per-device exports of vulnerability-management data (<c>machines/...</c>).</summary>
	public IAssessmentExportsApi AssessmentExports => field ??= For<IAssessmentExportsApi>();

	/// <summary>Enumerates every secure-configuration assessment record, following the page links (<c>GET machines/SecureConfigurationsAssessmentByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">Paging options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<SecureConfigurationAssessment> GetAllSecureConfigurationAssessmentsAsync(ExportByMachineOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListSecureConfigurationsByMachineAsync(options, token), cancellationToken);

	/// <summary>Enumerates every software-inventory record, following the page links (<c>GET machines/SoftwareInventoryByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">Paging options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<SoftwareInventoryAssessment> GetAllSoftwareInventoryAssessmentsAsync(ExportByMachineOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListSoftwareInventoryByMachineAsync(options, token), cancellationToken);

	/// <summary>Enumerates every software-vulnerability record, following the page links (<c>GET machines/SoftwareVulnerabilitiesByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">Paging options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<SoftwareVulnerabilityAssessment> GetAllSoftwareVulnerabilityAssessmentsAsync(ExportByMachineOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListSoftwareVulnerabilitiesByMachineAsync(options, token), cancellationToken);

	/// <summary>Enumerates every software-vulnerability change record, following the page links (<c>GET machines/SoftwareVulnerabilityChangesByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">The required <c>sinceTime</c> and optional paging for the first page.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<SoftwareVulnerabilityChange> GetAllSoftwareVulnerabilityChangesAsync(DeltaExportOptions options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListSoftwareVulnerabilityChangesByMachineAsync(options, token), cancellationToken);

	/// <summary>Enumerates every non-CPE software-inventory record, following the page links (<c>GET machines/SoftwareInventoryNoProductCodeByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">Paging options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<NonCpeSoftwareInventoryAssessment> GetAllNonCpeSoftwareInventoryAssessmentsAsync(ExportByMachineOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListNonCpeSoftwareInventoryByMachineAsync(options, token), cancellationToken);

	/// <summary>Enumerates every browser-extension record, following the page links (<c>GET Machines/BrowserExtensionsInventoryByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">Paging options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<BrowserExtensionAssessment> GetAllBrowserExtensionAssessmentsAsync(ExportByMachineOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListBrowserExtensionsByMachineAsync(options, token), cancellationToken);

	/// <summary>Enumerates every certificate record, following the page links (<c>GET machines/certificateAssessmentByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">Paging options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<CertificateAssessment> GetAllCertificateAssessmentsAsync(ExportByMachineOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListCertificatesByMachineAsync(options, token), cancellationToken);

	/// <summary>Enumerates every baseline-compliance record, following the page links (<c>GET machines/baselineComplianceAssessmentByMachine</c>). Mind the rate limit of 30 calls a minute.</summary>
	/// <param name="options">Paging options for the first page, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Every record.</returns>
	public IAsyncEnumerable<BaselineComplianceAssessment> GetAllBaselineComplianceAssessmentsAsync(ExportByMachineOptions? options, CancellationToken cancellationToken)
		=> GetAllAsync(token => AssessmentExports.ListBaselineComplianceByMachineAsync(options, token), cancellationToken);

	/// <summary>Downloads the files of a secure-configuration assessment export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<SecureConfigurationAssessment> StreamSecureConfigurationAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<SecureConfigurationAssessment>(export, cancellationToken);

	/// <summary>Downloads the files of a software-inventory export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<SoftwareInventoryAssessment> StreamSoftwareInventoryAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<SoftwareInventoryAssessment>(export, cancellationToken);

	/// <summary>Downloads the files of a software-vulnerability export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<SoftwareVulnerabilityAssessment> StreamSoftwareVulnerabilityAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<SoftwareVulnerabilityAssessment>(export, cancellationToken);

	/// <summary>Downloads the files of a non-CPE software-inventory export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<NonCpeSoftwareInventoryAssessment> StreamNonCpeSoftwareInventoryAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<NonCpeSoftwareInventoryAssessment>(export, cancellationToken);

	/// <summary>Downloads the files of a browser-extension export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<BrowserExtensionAssessment> StreamBrowserExtensionAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<BrowserExtensionAssessment>(export, cancellationToken);

	/// <summary>Downloads the files of a information-gathering export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<InfoGatheringAssessment> StreamInfoGatheringAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<InfoGatheringAssessment>(export, cancellationToken);

	/// <summary>Downloads the files of a certificate export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<CertificateAssessment> StreamCertificateAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<CertificateAssessment>(export, cancellationToken);

	/// <summary>Downloads the files of a baseline-compliance export and yields its records (see <see cref="StreamExportAsync{T}(ExportFilesResponse, CancellationToken)"/>).</summary>
	/// <param name="export">The response of the matching export operation.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The records of every file, in order.</returns>
	public IAsyncEnumerable<BaselineComplianceAssessment> StreamBaselineComplianceAssessmentsAsync(ExportFilesResponse export, CancellationToken cancellationToken)
		=> StreamExportAsync<BaselineComplianceAssessment>(export, cancellationToken);
}
