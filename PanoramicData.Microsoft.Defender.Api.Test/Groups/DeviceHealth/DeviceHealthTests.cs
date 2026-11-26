using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The antivirus device health operation: it pins its request and maps a realistic response.</summary>
public class DeviceHealthTests
{
	private const string HealthJson = """
		{"id":"guid-1","machineId":"machine-1","computerDnsName":"pc-01","osKind":"windows","osPlatform":"Windows10","osVersion":"10.0.19044.1865","avMode":"0","avSignatureVersion":"1.371.1279.0","avEngineVersion":"1.1.19428.0","avPlatformVersion":"4.18.2206.108","lastSeenTime":"2022-08-02T19:40:45Z","quickScanResult":"Completed","quickScanError":"","quickScanTime":"2022-08-02T18:40:15.882Z","fullScanResult":"Failed","fullScanError":"0x80508023","fullScanTime":null,"dataRefreshTimestamp":"2022-08-02T21:16:23Z","avEngineUpdateTime":"2022-08-02T00:03:39Z","avSignatureUpdateTime":"2022-08-02T00:03:40Z","avPlatformUpdateTime":"2022-06-20T16:59:35Z","avIsSignatureUpToDate":"True","avIsEngineUpToDate":"False","avIsPlatformUpToDate":"Unknown","avSignaturePublishTime":"2022-08-02T00:03:41Z","rbacGroupName":"TVM1","rbacGroupId":4415}
		""";

	private static DateTimeOffset Utc(int month, int day, int hour, int minute, int second) => new(2022, month, day, hour, minute, second, TimeSpan.Zero);

	[Fact]
	public Task ListAntivirusHealth_SendsGetDeviceAvInfo()
		=> ShouldGetAsync((c, t) => c.DeviceHealth.ListAntivirusHealthAsync(null, t), Envelope(""), "/api/deviceavinfo");

	[Fact]
	public Task ListAntivirusHealth_SendsTheODataOptions()
		=> ShouldGetAsync((c, t) => c.DeviceHealth.ListAntivirusHealthAsync(new ODataListOptions { Filter = "avMode eq '0'", Top = 10000, Skip = 20 }, t), Envelope(""),
			"/api/deviceavinfo", "?$filter=avMode%20eq%20%270%27&$top=10000&$skip=20");

	[Fact]
	public async Task ListAntivirusHealth_MapsEveryField_AndToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.DeviceHealth.ListAntivirusHealthAsync(null, t),
			Envelope(HealthJson + ",{\"id\":\"g2\",\"avMode\":2,\"rbacGroupId\":\"9\",\"avIsEngineUpToDate\":true,\"quickScanTime\":\"\"}"));

		var h = page.Value[0];
		(h.Id, h.MachineId, h.ComputerDnsName, h.OsKind, h.OsPlatform, h.OsVersion).Should().Be(("guid-1", "machine-1", "pc-01", "windows", "Windows10", "10.0.19044.1865"));
		(h.AvMode, h.AvSignatureVersion, h.AvEngineVersion, h.AvPlatformVersion).Should().Be(("0", "1.371.1279.0", "1.1.19428.0", "4.18.2206.108"));
		(h.AvIsSignatureUpToDate, h.AvIsEngineUpToDate, h.AvIsPlatformUpToDate).Should().Be(("True", "False", "Unknown"));
		(h.QuickScanResult, h.QuickScanError, h.FullScanResult, h.FullScanError, h.FullScanTime).Should().Be(("Completed", "", "Failed", "0x80508023", (DateTimeOffset?)null));
		h.QuickScanTime.Should().Be(Utc(8, 2, 18, 40, 15).AddMilliseconds(882));
		(h.LastSeenTime, h.DataRefreshTimestamp).Should().Be((Utc(8, 2, 19, 40, 45), Utc(8, 2, 21, 16, 23)));
		(h.AvEngineUpdateTime, h.AvSignatureUpdateTime, h.AvPlatformUpdateTime, h.AvSignaturePublishTime)
			.Should().Be((Utc(8, 2, 0, 3, 39), Utc(8, 2, 0, 3, 40), Utc(6, 20, 16, 59, 35), Utc(8, 2, 0, 3, 41)));
		(h.RbacGroupName, h.RbacGroupId).Should().Be(("TVM1", "4415"));
		var loose = page.Value[1];
		(loose.AvMode, loose.RbacGroupId, loose.AvIsEngineUpToDate, loose.QuickScanTime).Should().Be(("2", "9", "true", (DateTimeOffset?)null));
	}

	[Fact]
	public Task ListAntivirusHealth_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.DeviceHealth.ListAntivirusHealthAsync(null, t));

	[Fact]
	public Task GetAllAntivirusHealth_WiresTheOperation()
		=> ShouldEnumerateAsync((c, t) => c.GetAllAntivirusHealthAsync(new ODataListOptions { Top = 6 }, t), "/api/deviceavinfo", "?$top=6");
}
