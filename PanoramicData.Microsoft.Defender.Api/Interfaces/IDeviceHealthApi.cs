using PanoramicData.MicrosoftDefender.Api.Exports;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.DeviceHealth;
using Refit;

namespace PanoramicData.MicrosoftDefender.Api.Interfaces;

/// <summary>
/// Microsoft Defender Antivirus health per device (<c>deviceavinfo</c>). Needs the <c>Machine.Read.All</c> application
/// permission, and a token for the <c>api.securitycenter.microsoft.com</c> audience (the client default scope). The "via
/// files" flavour of this report shares its path with the information-gathering export, which is
/// <see cref="IAssessmentExportsApi.ExportInfoGatheringAsync(ExportViaFilesOptions?, CancellationToken)"/>.
/// </summary>
/// <remarks>
/// Doubt: the 2022 release notes name this report <c>public/avdeviceshealth</c>, while its own page documents
/// <c>deviceavinfo</c>. The path of the page is used. Unverified.
/// </remarks>
public interface IDeviceHealthApi
{
	/// <summary>Lists the antivirus health of each device, one page at a time (<c>GET deviceavinfo</c>). Limited to 30 calls a minute and 1,000 an hour.</summary>
	/// <remarks>
	/// <c>$filter</c> may name machineId, computerDnsName, osKind, osPlatform, osVersion, avMode, avSignatureVersion,
	/// avEngineVersion, avPlatformVersion, quickScanResult, quickScanError, fullScanResult, fullScanError,
	/// avIsSignatureUpToDate, avIsEngineUpToDate, avIsPlatformUpToDate and rbacGroupId, but not rbacGroupName or id. The
	/// default page holds 20 records. The documentation gives a maximum page of 200,000 and a <c>$top</c> maximum of 10,000.
	/// </remarks>
	/// <param name="options">OData options, or <see langword="null"/> for none.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A page of device health records.</returns>
	[Get("deviceavinfo")]
	Task<ODataPage<DeviceAntivirusHealth>> ListAntivirusHealthAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);
}
