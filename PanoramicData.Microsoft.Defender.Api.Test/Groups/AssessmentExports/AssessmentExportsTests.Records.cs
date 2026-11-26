using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.AssessmentExports;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>What each export record type maps from a realistic record, and how it tolerates loose JSON.</summary>
public partial class AssessmentExportsTests
{
	private const string DeviceJson = """ "deviceId":"d1","deviceName":"pc-01.example.test","rbacGroupName":"GroupOne","rbacGroupId":86 """;

	private const string LooseInventoryRecord = """,{"deviceId":"d2","DiskPaths":null,"EndOfSupportDate":"","NumberOfWeaknesses":null,"extra":{"x":1}}""";

	private static string Rec(string extra) => "{" + DeviceJson + "," + extra + "}";

	private static readonly DateTimeOffset Stamp = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

	private static void AssertDevice(AssessmentDeviceRecord record)
		=> (record.DeviceId, record.DeviceName, record.RbacGroupName, record.RbacGroupId).Should().Be(("d1", "pc-01.example.test", "GroupOne", "86"));

	[Fact]
	public async Task SecureConfigurations_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListSecureConfigurationsByMachineAsync(null, t),
			Envelope(Rec(""" "configurationId":"scid-1","configurationName":"Disable telnet","configurationCategory":"Network","configurationSubcategory":"Sub","configurationImpact":5,"isApplicable":true,"isCompliant":false,"isExpectedUserImpact":false,"osPlatform":"Windows11","osVersion":"10.0","recommendationReference":"sca-_-scid-1","timestamp":"2026-01-02 03:04:05.000" """),
				"https://defender.test/api/machines/SecureConfigurationsAssessmentByMachine?$skiptoken=1"));

		page.NextLink.Should().Be("https://defender.test/api/machines/SecureConfigurationsAssessmentByMachine?$skiptoken=1");
		var r = page.Value.Should().ContainSingle().Subject;
		AssertDevice(r);
		(r.ConfigurationId, r.ConfigurationName, r.ConfigurationCategory, r.ConfigurationSubcategory, r.ConfigurationImpact)
			.Should().Be(("scid-1", "Disable telnet", "Network", "Sub", 5.0));
		(r.IsApplicable, r.IsCompliant, r.IsExpectedUserImpact, r.OsPlatform, r.OsVersion, r.RecommendationReference, r.Timestamp)
			.Should().Be((true, false, false, "Windows11", "10.0", "sca-_-scid-1", Stamp));
	}

	[Fact]
	public async Task SoftwareInventory_MapsEveryField_AndToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListSoftwareInventoryByMachineAsync(null, t),
			Envelope(Rec(""" "SoftwareVendor":"vendor","SoftwareName":"product","SoftwareVersion":"1.0","OSPlatform":"Windows11","DiskPaths":["C:\\app"],"RegistryPaths":["HKLM\\app"],"NumberOfWeaknesses":"3","EndOfSupportStatus":"EOS Version","EndOfSupportDate":"2026-01-02T03:04:05Z","SoftwareFirstSeenTimestamp":"2026-01-02T03:04:05Z" """) + LooseInventoryRecord));


		var r = page.Value[0];
		AssertDevice(r);
		(r.SoftwareVendor, r.SoftwareName, r.SoftwareVersion, r.OsPlatform, r.NumberOfWeaknesses, r.EndOfSupportStatus, r.EndOfSupportDate, r.SoftwareFirstSeenTimestamp)
			.Should().Be(("vendor", "product", "1.0", "Windows11", 3L, "EOS Version", Stamp, Stamp));
		r.DiskPaths.Should().Equal("C:\\app");
		r.RegistryPaths.Should().Equal("HKLM\\app");
		var loose = page.Value[1];
		loose.DiskPaths.Should().BeEmpty();
		loose.EndOfSupportDate.Should().BeNull();
		loose.NumberOfWeaknesses.Should().BeNull();
	}

	[Fact]
	public async Task NonCpeSoftwareInventory_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListNonCpeSoftwareInventoryByMachineAsync(null, t),
			Envelope(Rec(""" "SoftwareVendor":"vendor","SoftwareName":"product","SoftwareVersion":"2.0","OSPlatform":"Windows10","SoftwareLastSeenTimestamp":"2026-01-02T03:04:05Z" """)));

		var r = page.Value.Should().ContainSingle().Subject;
		AssertDevice(r);
		(r.SoftwareVendor, r.SoftwareName, r.SoftwareVersion, r.OsPlatform, r.SoftwareLastSeenTimestamp).Should().Be(("vendor", "product", "2.0", "Windows10", Stamp));
	}

	private const string VulnerabilityRecordJson = """
		"Id":"rec-1","CveId":"CVE-2024-0001","CvssScore":"8.8","VulnerabilitySeverityLevel":"High","ExploitabilityLevel":"ExploitIsPublic","SoftwareVendor":"vendor","SoftwareName":"product","SoftwareVersion":"1.0","OSPlatform":"Windows11","DiskPaths":["C:\\app"],"RegistryPaths":["HKLM\\app"],"FirstSeenTimestamp":"2026-01-02T03:04:05Z","LastSeenTimestamp":"2026-01-02T03:04:05Z","RecommendationReference":"va-_-vendor-_-product","RecommendedSecurityUpdate":"Update product","RecommendedSecurityUpdateId":"KB1"
		""";

	private static void AssertVulnerabilityRecord(SoftwareVulnerabilityAssessment r)
	{
		AssertDevice(r);
		(r.Id, r.CveId, r.CvssScore, r.VulnerabilitySeverityLevel, r.ExploitabilityLevel).Should().Be(("rec-1", "CVE-2024-0001", 8.8, "High", "ExploitIsPublic"));
		(r.SoftwareVendor, r.SoftwareName, r.SoftwareVersion, r.OsPlatform).Should().Be(("vendor", "product", "1.0", "Windows11"));
		r.DiskPaths.Should().Equal("C:\\app");
		r.RegistryPaths.Should().Equal("HKLM\\app");
		(r.FirstSeenTimestamp, r.LastSeenTimestamp).Should().Be((Stamp, Stamp));
		(r.RecommendationReference, r.RecommendedSecurityUpdate, r.RecommendedSecurityUpdateId).Should().Be(("va-_-vendor-_-product", "Update product", "KB1"));
	}

	[Fact]
	public async Task SoftwareVulnerabilities_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListSoftwareVulnerabilitiesByMachineAsync(null, t),
			Envelope(Rec(VulnerabilityRecordJson + ""","SecurityUpdateAvailable":"true" """)));

		var r = page.Value.Should().ContainSingle().Subject;
		AssertVulnerabilityRecord(r);
		r.SecurityUpdateAvailable.Should().BeTrue();
	}

	[Fact]
	public async Task SoftwareVulnerabilityChanges_MapsTheDeltaColumnsToo()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListSoftwareVulnerabilityChangesByMachineAsync(new DeltaExportOptions { SinceTime = Stamp }, t),
			Envelope(Rec(VulnerabilityRecordJson + ""","EventTimestamp":"2026-01-02T03:04:05Z","Status":"Fixed" """)));

		var r = page.Value.Should().ContainSingle().Subject;
		AssertVulnerabilityRecord(r);
		(r.EventTimestamp, r.Status).Should().Be((Stamp, "Fixed"));
	}

	[Fact]
	public async Task BrowserExtensions_MapsEveryField_IncludingPermissions()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListBrowserExtensionsByMachineAsync(null, t),
			Envelope(Rec(""" "BrowserName":"chrome","ExtensionId":"ext-1","ExtensionName":"Ext","ExtensionDescription":"Does things","ExtensionVersion":"1.2","ExtensionRisk":"High","IsActivated":true,"InstallationTime":"2026-01-02T03:04:05.000Z","Permissions":[{"Id":"tabs","IsRequired":true,"Risk":"High"},{"Id":"https://*/*","IsRequired":false,"Risk":"Low"}] """)));

		var r = page.Value.Should().ContainSingle().Subject;
		AssertDevice(r);
		(r.BrowserName, r.ExtensionId, r.ExtensionName, r.ExtensionDescription, r.ExtensionVersion, r.ExtensionRisk, r.IsActivated, r.InstallationTime)
			.Should().Be(("chrome", "ext-1", "Ext", "Does things", "1.2", "High", true, Stamp));
		r.Permissions.Should().HaveCount(2);
		(r.Permissions[0].Id, r.Permissions[0].IsRequired, r.Permissions[0].Risk).Should().Be(("tabs", true, "High"));
		(r.Permissions[1].Id, r.Permissions[1].IsRequired, r.Permissions[1].Risk).Should().Be(("https://*/*", false, "Low"));
	}

	[Fact]
	public async Task Certificates_MapsBothSpellingsOfTheDocumentedColumns()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListCertificatesByMachineAsync(null, t),
			Envelope(Rec(""" "thumbprint":"AA11","path":"LocalMachine\\Root\\AA11","signatureAlgorithm":"sha384ECDSA","serialNumber":"6086","subjectType":"CA","keyLength":256,"keySize":"2048","notAfter":"2026-01-02T03:04:05Z","notBefore":"2025-01-02T03:04:05Z","expirationDate":"2027-01-02T03:04:05Z","issueDate":"2024-01-02T03:04:05Z","isSelfSigned":true,"subjectObject":{"cn":"x"},"issuerObject":null,"keyUsageArray":["a","b"],"extendedKeyUsageArray":[] """)));

		var r = page.Value.Should().ContainSingle().Subject;
		AssertDevice(r);
		(r.Thumbprint, r.Path, r.SignatureAlgorithm, r.SerialNumber, r.SubjectType, r.KeyLength, r.KeySize, r.IsSelfSigned)
			.Should().Be(("AA11", "LocalMachine\\Root\\AA11", "sha384ECDSA", "6086", "CA", 256L, "2048", true));
		(r.NotAfter, r.NotBefore).Should().Be((Stamp, Stamp.AddYears(-1)));
		(r.ExpirationDate, r.IssueDate).Should().Be((Stamp.AddYears(1), Stamp.AddYears(-2)));
		r.SubjectObject!.Value.GetProperty("cn").GetString().Should().Be("x");
		r.IssuerObject.Should().BeNull();
		r.KeyUsageArray!.Value.GetArrayLength().Should().Be(2);
		r.ExtendedKeyUsageArray!.Value.GetArrayLength().Should().Be(0);
	}

	[Fact]
	public async Task BaselineCompliance_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.AssessmentExports.ListBaselineComplianceByMachineAsync(null, t),
			Envelope(Rec(""" "id":"d1_p1_1.1.1","profileId":"p1","configurationId":"1.1.1","isApplicable":true,"isCompliant":false,"osPlatform":"WindowsServer2019","osVersion":"10.0.17763","dataCollectionTimeOffset":"2026-01-02T03:04:05Z","complianceCalculationTimeOffset":"2026-01-02T03:04:05Z","recommendedValue":["Greater than or equal '24'"],"currentValue":["24"],"source":["password_hist_len"] """)));

		var r = page.Value.Should().ContainSingle().Subject;
		AssertDevice(r);
		(r.Id, r.ProfileId, r.ConfigurationId, r.IsApplicable, r.IsCompliant, r.OsPlatform, r.OsVersion)
			.Should().Be(("d1_p1_1.1.1", "p1", "1.1.1", true, false, "WindowsServer2019", "10.0.17763"));
		(r.DataCollectionTimeOffset, r.ComplianceCalculationTimeOffset).Should().Be((Stamp, Stamp));
		r.RecommendedValue.Should().Equal("Greater than or equal '24'");
		r.CurrentValue.Should().Equal("24");
		r.Source.Should().Equal("password_hist_len");
	}

	[Fact]
	public async Task EveryByMachineOperation_ReadsAnEmptyOrAbsentValueList()
	{
		foreach (var op in ByMachineOps)
		{
			using var client = TestClient.Create(TestClient.Stub("{\"@odata.context\":\"x\"}"));

			await op.Call(client, null, TestContext.Current.CancellationToken);
		}
	}

	[Fact]
	public async Task GetAllHelpers_WireTheirOperationAndOptions()
	{
		var options = new ExportByMachineOptions { PageSize = 5 };
		var delta = new DeltaExportOptions { SinceTime = Stamp };
		const string q = "?pageSize=5";

		await ShouldEnumerateAsync((c, t) => c.GetAllSecureConfigurationAssessmentsAsync(options, t), "/api/machines/SecureConfigurationsAssessmentByMachine", q);
		await ShouldEnumerateAsync((c, t) => c.GetAllSoftwareInventoryAssessmentsAsync(options, t), "/api/machines/SoftwareInventoryByMachine", q);
		await ShouldEnumerateAsync((c, t) => c.GetAllSoftwareVulnerabilityAssessmentsAsync(options, t), "/api/machines/SoftwareVulnerabilitiesByMachine", q);
		await ShouldEnumerateAsync((c, t) => c.GetAllSoftwareVulnerabilityChangesAsync(delta, t), "/api/machines/SoftwareVulnerabilityChangesByMachine", "?sinceTime=2026-01-02T03%3A04%3A05Z");
		await ShouldEnumerateAsync((c, t) => c.GetAllNonCpeSoftwareInventoryAssessmentsAsync(options, t), "/api/machines/SoftwareInventoryNoProductCodeByMachine", q);
		await ShouldEnumerateAsync((c, t) => c.GetAllBrowserExtensionAssessmentsAsync(options, t), "/api/Machines/BrowserExtensionsInventoryByMachine", q);
		await ShouldEnumerateAsync((c, t) => c.GetAllCertificateAssessmentsAsync(options, t), "/api/machines/certificateAssessmentByMachine", q);
		await ShouldEnumerateAsync((c, t) => c.GetAllBaselineComplianceAssessmentsAsync(null, t), "/api/machines/baselineComplianceAssessmentByMachine");
	}

	[Fact]
	public async Task GetAll_FollowsTheNextLinkOfAnExport()
	{
		var stub = new StubHandler();
		stub.Enqueue(System.Net.HttpStatusCode.OK, Envelope("{\"deviceId\":\"d1\"}", "https://defender.test/api/machines/SoftwareInventoryByMachine?pageSize=1&$skiptoken=abc"));
		stub.Enqueue(System.Net.HttpStatusCode.OK, Envelope("{\"deviceId\":\"d2\"}"));
		using var client = TestClient.Create(stub);

		var records = await client.GetAllSoftwareInventoryAssessmentsAsync(new ExportByMachineOptions { PageSize = 1 }, TestContext.Current.CancellationToken)
			.ToListAsync(TestContext.Current.CancellationToken);

		records.Select(r => r.DeviceId).Should().Equal("d1", "d2");
		stub.Calls.Select(c => c.Uri.Query).Should().Equal("?pageSize=1", "?pageSize=1&$skiptoken=abc");
	}
}
