using PanoramicData.MicrosoftDefender.Api.Models.Machines;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The write operations: they are unit-verified only (never run live) and refused by a read-only client.</summary>
public partial class MachinesTests
{
	private static readonly (string Name, Func<DefenderClient, CancellationToken, Task> Call)[] Writes =
	[
		("Update", (c, t) => c.Machines.UpdateAsync(Id, new UpdateMachineRequest { MachineTags = ["a"] }, t)),
		("AddOrRemoveTag", (c, t) => c.Machines.AddOrRemoveTagAsync(Id, new MachineTagRequest { Value = "v", Action = MachineTagAction.Add }, t)),
		("AddOrRemoveTagForMultipleMachines", (c, t) => c.Machines.AddOrRemoveTagForMultipleMachinesAsync(new MultiMachineTagRequest { Value = "v", Action = MachineTagAction.Remove, MachineIds = ["m1"] }, t)),
		("SetDeviceValue", (c, t) => c.Machines.SetDeviceValueAsync(Id, new SetDeviceValueRequest { DeviceValue = MachineDeviceValue.Low }, t))
	];

	[Fact]
	public async Task Update_SendsPatchMachinesId_WithBothProperties()
	{
		var request = new UpdateMachineRequest { MachineTags = ["tag-a", "tag b"], DeviceValue = MachineDeviceValue.High };

		var call = await TestClient.CaptureAsync((c, t) => c.Machines.UpdateAsync(Id, request, t), MachineJson);

		call.ShouldBeRequest(HttpMethod.Patch, "/api/machines/" + Id, "", """{"machineTags":["tag-a","tag b"],"deviceValue":"High"}""");
	}

	[Fact]
	public async Task Update_OmitsPropertiesLeftNull()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.UpdateAsync("a/b", new UpdateMachineRequest { DeviceValue = MachineDeviceValue.Normal }, t), MachineJson);

		call.ShouldBeRequest(HttpMethod.Patch, "/api/machines/a%2Fb", "", """{"deviceValue":"Normal"}""");
	}

	[Fact]
	public async Task Update_MapsTheUpdatedMachine()
		=> AssertMachine(await TestClient.ReadAsync((c, t) => c.Machines.UpdateAsync(Id, new UpdateMachineRequest(), t), MachineJson));

	[Fact]
	public Task Update_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.UpdateAsync(Id, new UpdateMachineRequest(), t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"InvalidRequestBody","message":"Bad body","target":"c"}}""", "Bad body");

	[Fact]
	public async Task AddOrRemoveTag_SendsPostMachinesIdTags_WithCaseSensitiveProperties()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.AddOrRemoveTagAsync(Id, new MachineTagRequest { Value = "test Tag 2", Action = MachineTagAction.Add }, t), MachineJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/machines/" + Id + "/tags", "", """{"Value":"test Tag 2","Action":"Add"}""");
	}

	[Fact]
	public async Task AddOrRemoveTag_SendsRemove()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.AddOrRemoveTagAsync(Id, new MachineTagRequest { Value = "x", Action = MachineTagAction.Remove }, t), MachineJson);

		call.Body.Should().Be("""{"Value":"x","Action":"Remove"}""");
	}

	[Fact]
	public async Task AddOrRemoveTag_MapsTheUpdatedMachine()
		=> AssertMachine(await TestClient.ReadAsync((c, t) => c.Machines.AddOrRemoveTagAsync(Id, new MachineTagRequest { Value = "x", Action = MachineTagAction.Add }, t), MachineJson));

	[Fact]
	public Task AddOrRemoveTag_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.AddOrRemoveTagAsync(Id, new MachineTagRequest { Value = "x", Action = MachineTagAction.Add }, t), HttpStatusCode.NotFound,
			"""{"error":{"code":"ResourceNotFound","message":"No such device","target":"c"}}""", "No such device");

	[Fact]
	public async Task AddOrRemoveTagForMultipleMachines_SendsPostWithTheMachineIds()
	{
		var request = new MultiMachineTagRequest { Value = "Tag", Action = MachineTagAction.Add, MachineIds = ["m1", "m2"] };

		var call = await TestClient.CaptureAsync((c, t) => c.Machines.AddOrRemoveTagForMultipleMachinesAsync(request, t), "{}");

		call.ShouldBeRequest(HttpMethod.Post, "/api/machines/AddOrRemoveTagForMultipleMachines", "", """{"Value":"Tag","Action":"Add","MachineIds":["m1","m2"]}""");
	}

	[Fact]
	public async Task AddOrRemoveTagForMultipleMachines_IgnoresAnyResponseBody()
	{
		using var client = TestClient.Create(TestClient.Stub("""[1,2,3]"""));

		await client.Machines.AddOrRemoveTagForMultipleMachinesAsync(new MultiMachineTagRequest { Value = "T", Action = MachineTagAction.Add, MachineIds = [] }, TestContext.Current.CancellationToken);
	}

	[Fact]
	public Task AddOrRemoveTagForMultipleMachines_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.AddOrRemoveTagForMultipleMachinesAsync(new MultiMachineTagRequest { Value = "T", Action = MachineTagAction.Add, MachineIds = [] }, t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"MaximumBatchSizeExceeded","message":"Too many devices","target":"c"}}""", "Too many devices");

	[Fact]
	public async Task SetDeviceValue_SendsPostMachinesIdSetDeviceValue()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Machines.SetDeviceValueAsync(Id, new SetDeviceValueRequest { DeviceValue = MachineDeviceValue.High }, t), MachineJson);

		call.ShouldBeRequest(HttpMethod.Post, "/api/machines/" + Id + "/setDeviceValue", "", """{"DeviceValue":"High"}""");
	}

	[Fact]
	public async Task SetDeviceValue_MapsTheUpdatedMachine()
		=> AssertMachine(await TestClient.ReadAsync((c, t) => c.Machines.SetDeviceValueAsync(Id, new SetDeviceValueRequest { DeviceValue = MachineDeviceValue.High }, t), MachineJson));

	[Fact]
	public Task SetDeviceValue_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Machines.SetDeviceValueAsync(Id, new SetDeviceValueRequest { DeviceValue = MachineDeviceValue.High }, t), HttpStatusCode.Forbidden, ErrorJson, "Missing permission");

	[Fact]
	public async Task EveryWrite_IsRefusedByAReadOnlyClient_BeforeAnythingIsSent()
	{
		foreach (var (name, call) in Writes)
		{
			var stub = new StubHandler();
			using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

			var act = () => call(client, TestContext.Current.CancellationToken);

			await act.Should().ThrowAsync<DefenderReadOnlyException>($"{name} changes state");
			stub.Calls.Should().BeEmpty(name);
		}
	}

	[Fact]
	public async Task EveryWrite_ResendsAnIdenticalBodyAfterA401()
	{
		foreach (var (name, call) in Writes)
		{
			var stub = new StubHandler();
			stub.Enqueue(HttpStatusCode.Unauthorized);
			stub.Enqueue(HttpStatusCode.OK, MachineJson);
			using var client = TestClient.Create(stub);

			await call(client, TestContext.Current.CancellationToken);

			stub.Calls.Should().HaveCount(2, name);
			stub.Calls[1].Body.Should().NotBeNullOrEmpty(name).And.Be(stub.Calls[0].Body, name);
			stub.Calls[1].Authorization.Should().Be("Bearer token-2", name);
		}
	}
}
