using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

public partial class AlertsTests
{
	[Fact]
	public async Task ListDomains_SendsGetAlertsIdDomains()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.ListDomainsAsync("a1", t), Envelope("Domains", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts/a1/domains");
	}

	[Fact]
	public async Task ListDomains_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Alerts.ListDomainsAsync("a1", t), Envelope("Domains", """{"host":"www.example.test"}"""));

		page.Value.Should().ContainSingle().Which.Host.Should().Be("www.example.test");
	}

	[Fact]
	public Task ListDomains_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.ListDomainsAsync("a1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No domains"), "No domains");

	[Fact]
	public async Task ListFiles_SendsGetAlertsIdFiles()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.ListFilesAsync("a1", t), Envelope("Files", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts/a1/files");
	}

	[Fact]
	public async Task ListFiles_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Alerts.ListFilesAsync("a1", t), Envelope("Files", """
            {"sha1":"s1","sha256":"s256","md5":"m5","globalPrevalence":29,"globalFirstObserved":"2026-01-01T00:00:00Z","globalLastObserved":"2026-01-02T00:00:00Z","size":113984,"fileType":"Exe","isPeFile":true,"filePublisher":"Pub","fileProductName":"Prod","signer":"Signer","issuer":"Issuer","signerHash":"sh","isValidCertificate":"true","determinationType":"Unknown","determinationValue":null}
            """));

		var f = page.Value.Should().ContainSingle().Subject;
		(f.Sha1, f.Sha256, f.Md5, f.GlobalPrevalence, f.Size, f.FileType, f.IsPeFile).Should().Be(("s1", "s256", "m5", 29L, 113984L, "Exe", true));
		f.GlobalFirstObserved.Should().Be(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
		f.GlobalLastObserved.Should().Be(new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero));
		(f.FilePublisher, f.FileProductName, f.Signer, f.Issuer, f.SignerHash, f.IsValidCertificate, f.DeterminationType, f.DeterminationValue)
			.Should().Be(("Pub", "Prod", "Signer", "Issuer", "sh", true, "Unknown", null));
	}

	[Fact]
	public Task ListFiles_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.ListFilesAsync("a1", t), HttpStatusCode.Forbidden, Failure("Forbidden", "No File.Read.All"), "No File.Read.All");

	[Fact]
	public async Task ListIps_SendsGetAlertsIdIps()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.ListIpsAsync("a1", t), Envelope("Ips", ""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts/a1/ips");
	}

	[Fact]
	public async Task ListIps_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Alerts.ListIpsAsync("a1", t), Envelope("Ips", """{"id":"192.0.2.7"},{"id":"198.51.100.2"}"""));

		page.Value.Select(i => i.Id).Should().Equal("192.0.2.7", "198.51.100.2");
	}

	[Fact]
	public Task ListIps_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.ListIpsAsync("a1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No IPs"), "No IPs");

	[Fact]
	public async Task GetMachine_SendsGetAlertsIdMachine()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.GetMachineAsync("a1", t), "{}");

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts/a1/machine");
	}

	[Fact]
	public async Task GetMachine_MapsEveryField()
	{
		var m = await TestClient.ReadAsync((c, t) => c.Alerts.GetMachineAsync("a1", t), """
            {"id":"m1","computerDnsName":"pc-01.example.test","firstSeen":"2026-01-01T00:00:00Z","lastSeen":"2026-01-02T00:00:00Z","osPlatform":"Windows11","osProcessor":"x64","version":"24H2","lastIpAddress":"10.0.0.2","lastExternalIpAddress":"203.0.113.9","osBuild":"26100","healthStatus":"Active","deviceValue":"Normal","rbacGroupName":"GroupA","riskScore":"Low","exposureLevel":"Medium","aadDeviceId":"aad-1","machineTags":["t1","t2"],"ipAddresses":[{"ipAddress":"10.0.0.3","macAddress":"AABBCCDDEEFF","operationalStatus":"Up"}]}
            """);

		(m.Id, m.ComputerDnsName, m.OsPlatform, m.OsProcessor, m.Version, m.LastIpAddress, m.LastExternalIpAddress, m.OsBuild)
			.Should().Be(("m1", "pc-01.example.test", "Windows11", "x64", "24H2", "10.0.0.2", "203.0.113.9", 26100L));
		(m.HealthStatus, m.DeviceValue, m.RbacGroupName, m.RiskScore, m.ExposureLevel, m.AadDeviceId).Should().Be(("Active", "Normal", "GroupA", "Low", "Medium", "aad-1"));
		m.FirstSeen.Should().Be(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
		m.LastSeen.Should().Be(new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero));
		m.MachineTags.Should().Equal("t1", "t2");
		var ip = m.IpAddresses.Should().ContainSingle().Subject;
		(ip.IpAddress, ip.MacAddress, ip.OperationalStatus).Should().Be(("10.0.0.3", "AABBCCDDEEFF", "Up"));
	}

	[Fact]
	public Task GetMachine_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.GetMachineAsync("a1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No machine"), "No machine");

	[Fact]
	public async Task GetUser_SendsGetAlertsIdUser()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Alerts.GetUserAsync("a1", t), "{}");

		call.ShouldBeRequest(HttpMethod.Get, "/api/alerts/a1/user");
	}

	[Fact]
	public async Task GetUser_MapsEveryField()
	{
		var u = await TestClient.ReadAsync((c, t) => c.Alerts.GetUserAsync("a1", t), """
            {"id":"example/user1","accountName":"user1","accountDomain":"example","accountSid":"S-1-5-21-2","firstSeen":"2026-01-01T00:00:00Z","lastSeen":"2026-01-02T00:00:00Z","mostPrevalentMachineId":"m1","leastPrevalentMachineId":"m2","logonTypes":"Network","logOnMachinesCount":"3","isDomainAdmin":false,"isOnlyNetworkUser":"true"}
            """);

		(u.Id, u.AccountName, u.AccountDomain, u.AccountSid, u.MostPrevalentMachineId, u.LeastPrevalentMachineId, u.LogonTypes)
			.Should().Be(("example/user1", "user1", "example", "S-1-5-21-2", "m1", "m2", "Network"));
		(u.LogOnMachinesCount, u.IsDomainAdmin, u.IsOnlyNetworkUser).Should().Be((3, false, true));
		u.FirstSeen.Should().Be(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
		u.LastSeen.Should().Be(new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero));
	}

	[Fact]
	public Task GetUser_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Alerts.GetUserAsync("a1", t), HttpStatusCode.NotFound, Failure("ResourceNotFound", "No user"), "No user");
}
