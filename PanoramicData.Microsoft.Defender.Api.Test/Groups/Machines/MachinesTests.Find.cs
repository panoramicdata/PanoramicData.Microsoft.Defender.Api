using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Finding devices by internal IP, by IP and by tag: the non-standard path and query forms.</summary>
public partial class MachinesTests
{
	private static readonly DateTimeOffset Moment = new(2024, 5, 6, 7, 8, 9, TimeSpan.Zero);

	[Fact]
	public async Task Find_SendsTheParenthesisedPath_WithTheTimestampAndKey()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.FindAsync(Moment, TestAddresses.Device, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, $"/api/machines/find(timestamp=2024-05-06T07%3A08%3A09Z,key={TestAddresses.Device})");
	}

	[Fact]
	public async Task Find_EscapesTheKey_AndConvertsTheTimestampToUtc()
	{
		var local = new DateTimeOffset(2024, 5, 6, 9, 8, 9, TimeSpan.FromHours(2));

		var call = await TestClient.CaptureAsync((c, t) => c.Machines.FindAsync(local, "fe80::1/x y", t), Envelope(""));

		call.Uri.AbsolutePath.Should().Be("/api/machines/find(timestamp=2024-05-06T07%3A08%3A09Z,key=fe80%3A%3A1%2Fx%20y)");
		call.Uri.Query.Should().BeEmpty();
	}

	[Fact]
	public async Task Find_MapsTheDevices()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.FindAsync(Moment, TestAddresses.Device, t), Envelope(MachineJson));

		AssertMachine(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public Task Find_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.FindAsync(Moment, TestAddresses.Device, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task FindByIp_SendsTheQuotedIpAndTheTimestampInThePath()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.FindByIpAsync(TestAddresses.Device, Moment, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, $"/api/machines/findbyip(ip='{TestAddresses.Device}',timestamp=2024-05-06T07%3A08%3A09Z)");
	}

	[Fact]
	public async Task FindByIp_EscapesAnApostropheInTheIp_SoTheQuotesCannotBeBroken()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.FindByIpAsync("1'2/3", Moment, t), Envelope(""));

		call.Uri.AbsolutePath.Should().Be("/api/machines/findbyip(ip='1%272%2F3',timestamp=2024-05-06T07%3A08%3A09Z)");
	}

	[Fact]
	public async Task FindByIp_MapsTheDevices()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.FindByIpAsync(TestAddresses.Device, Moment, t), Envelope(MachineJson));

		AssertMachine(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public Task FindByIp_MapsAnError_ForATimestampOlderThan30Days()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.FindByIpAsync(TestAddresses.Device, Moment, t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"BadRequest","message":"Timestamp too old","target":"c"}}""", "Timestamp too old");

	[Fact]
	public async Task FindByTag_SendsTheTagAsAQueryParameter()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.FindByTagAsync("tag a&b", null, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/machines/findbytag", "?tag=tag%20a%26b");
	}

	[Fact]
	public async Task FindByTag_SendsTheStartsWithSwitch()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.FindByTagAsync("pre", new FindByTagOptions { UseStartsWithFilter = true }, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/machines/findbytag", "?tag=pre&useStartsWithFilter=true");
	}

	[Fact]
	public async Task FindByTag_OmitsTheSwitchWhenUnset()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.FindByTagAsync("pre", new FindByTagOptions(), t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/machines/findbytag", "?tag=pre");
	}

	[Fact]
	public async Task FindByTag_MapsTheDevices()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Machines.FindByTagAsync("tag-a", null, t), Envelope(MachineJson));

		AssertMachine(page.Value.Should().ContainSingle().Subject);
	}

	[Fact]
	public Task FindByTag_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.FindByTagAsync("x", null, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task GetAllMachinesByTag_EnumeratesThePages()
	{
		using var client = TestClient.Create(TestClient.Stub(Envelope("""{"id":"a"},{"id":"b"}""")));

		var ids = await client.GetAllMachinesByTagAsync("t", null, TestContext.Current.CancellationToken).Select(m => m.Id).ToListAsync(TestContext.Current.CancellationToken);

		ids.Should().Equal("a", "b");
	}
}
