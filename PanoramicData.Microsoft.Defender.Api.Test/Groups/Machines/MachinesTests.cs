using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Reading devices: list, get, find and the per-device sub-collections.</summary>
public partial class MachinesTests
{
	// Shaped like the service's response (field names and types), with neutral values.
	internal const string MachineJson = """
		{
			"id": "0123456789abcdef0123456789abcdef01234567",
			"computerDnsName": "pc-01.example.test",
			"firstSeen": "2024-01-02T03:04:05Z",
			"lastSeen": "2024-02-03T04:05:06Z",
			"osPlatform": "Windows11",
			"onboardingStatus": "Onboarded",
			"osProcessor": "x64",
			"version": "22H2",
			"osBuild": 22621,
			"lastIpAddress": "192.0.2.5",
			"lastExternalIpAddress": "203.0.113.9",
			"healthStatus": "Active",
			"rbacGroupName": "GroupOne",
			"rbacGroupId": 7,
			"riskScore": "Low",
			"aadDeviceId": "aad-1",
			"machineTags": ["tag-a", "tag-b"],
			"exposureLevel": "Medium",
			"deviceValue": "High",
			"ipAddresses": [{"ipAddress": "192.0.2.5", "macAddress": "AABBCCDDEEFF", "type": "Ethernet", "operationalStatus": "Up"}],
			"osArchitecture": "64-bit",
			"vmMetadata": {"vmId": "vm-1", "cloudProvider": "Azure", "resourceId": "res-1", "subscriptionId": "sub-1"}
		}
		""";

	internal const string ErrorJson = """{"error":{"code":"Forbidden","message":"Missing permission","target":"corr-1"}}""";

	private const string Id = "0123456789abcdef0123456789abcdef01234567";

	internal static string Envelope(string items) => $$"""{"@odata.context":"https://defender.test/api/$metadata#Machines","value":[{{items}}]}""";

	private static void AssertMachine(Machine m)
	{
		m.Id.Should().Be(Id);
		m.ComputerDnsName.Should().Be("pc-01.example.test");
		m.FirstSeen.Should().Be(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero));
		m.LastSeen.Should().Be(new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero));
		m.OsPlatform.Should().Be("Windows11");
		m.OnboardingStatus.Should().Be("Onboarded");
		m.OsProcessor.Should().Be("x64");
		m.Version.Should().Be("22H2");
		m.OsBuild.Should().Be(22621);
		m.LastIpAddress.Should().Be(TestAddresses.Device);
		m.LastExternalIpAddress.Should().Be("203.0.113.9");
		m.HealthStatus.Should().Be("Active");
		m.RbacGroupName.Should().Be("GroupOne");
		m.RbacGroupId.Should().Be(7);
		m.RiskScore.Should().Be("Low");
		m.AadDeviceId.Should().Be("aad-1");
		m.MachineTags.Should().Equal("tag-a", "tag-b");
		m.ExposureLevel.Should().Be("Medium");
		m.DeviceValue.Should().Be(MachineDeviceValue.High);
		m.OsArchitecture.Should().Be("64-bit");
		var ip = m.IpAddresses.Should().ContainSingle().Subject;
		(ip.IpAddress, ip.MacAddress, ip.Type, ip.OperationalStatus).Should().Be((TestAddresses.Device, "AABBCCDDEEFF", "Ethernet", "Up"));
		var vm = m.VmMetadata.Should().NotBeNull().And.Subject.As<MachineVmMetadata>();
		(vm.VmId, vm.CloudProvider, vm.ResourceId, vm.SubscriptionId).Should().Be(("vm-1", "Azure", "res-1", "sub-1"));
	}

	[Fact]
	public async Task List_SendsGetMachines_WithTheODataOptions()
	{
		var options = new ODataListOptions { Filter = "computerDnsName eq 'pc 01' and riskScore eq 'High'", Top = 10000, Skip = 20000 };

		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListAsync(options, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/machines", "?$filter=computerDnsName%20eq%20%27pc%2001%27%20and%20riskScore%20eq%20%27High%27&$top=10000&$skip=20000");
		call.Authorization.Should().Be("Bearer token-1");
	}

	[Fact]
	public async Task List_WithNoOptions_SendsNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.ListAsync(null, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/machines");
	}

	[Fact]
	public async Task List_MapsEveryField_AndTheEnvelope()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListAsync(null, t),
			$$"""{"@odata.context":"https://defender.test/api/$metadata#Machines","value":[{{MachineJson}}],"@odata.nextLink":"https://defender.test/api/machines?$skip=1"}""");

		page.Context.Should().Be("https://defender.test/api/$metadata#Machines");
		page.NextLink.Should().Be("https://defender.test/api/machines?$skip=1");
		AssertMachine(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.ListAsync(null, t),
			Envelope("""{"id":"m1","osBuild":"19045","rbacGroupId":"3","deviceValue":"Gigantic","machineTags":null,"ipAddresses":null,"lastSeen":"","vmMetadata":null,"extra":{"x":1}}"""));

		var item = page.Value.Should().ContainSingle().Subject;
		item.OsBuild.Should().Be(19045);
		item.RbacGroupId.Should().Be(3);
		item.DeviceValue.Should().Be(MachineDeviceValue.Unknown);
		item.MachineTags.Should().BeEmpty();
		item.IpAddresses.Should().BeEmpty();
		item.LastSeen.Should().BeNull();
		item.VmMetadata.Should().BeNull();
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.ListAsync(null, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task List_AnEmptyListMayBeA404()
	{
		using var client = TestClient.Create(TestClient.Stub("""{"error":{"code":"ResourceNotFound","message":"No machines","target":"c"}}""", HttpStatusCode.NotFound));

		var act = () => client.Machines.ListAsync(null, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<DefenderApiException>()).Which.Code.Should().Be("ResourceNotFound");
	}

	[Fact]
	public async Task GetAllMachines_FollowsTheNextLink()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"value":[{"id":"a"}],"@odata.nextLink":"https://defender.test/api/machines?$skip=1"}""");
		stub.Enqueue(HttpStatusCode.OK, Envelope("""{"id":"b"}"""));
		using var client = TestClient.Create(stub);
		var ids = new List<string?>();

		await foreach (var machine in client.GetAllMachinesAsync(new ODataListOptions { Top = 1 }, TestContext.Current.CancellationToken))
		{
			ids.Add(machine.Id);
		}

		ids.Should().Equal("a", "b");
		stub.Calls[0].Uri.Query.Should().Be("?$top=1");
		stub.Calls[1].Uri.ToString().Should().Be("https://defender.test/api/machines?$skip=1");
	}

	[Fact]
	public async Task Get_SendsGetMachinesId()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.GetAsync(Id, t), MachineJson);

		call.ShouldBeRequest(HttpMethod.Get, "/api/machines/" + Id);
	}

	[Fact]
	public async Task Get_EscapesTheIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.GetAsync("host/name 1", t), MachineJson);

		call.Uri.AbsolutePath.Should().Be("/api/machines/host%2Fname%201");
	}

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertMachine(await TestClient.ReadAsync((c, t) => c.Machines.GetAsync(Id, t), MachineJson));

	[Fact]
	public Task Get_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.GetAsync("x", t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"Machine x was not found","target":"c"}}""", "Machine x was not found");
}
