using PanoramicData.MicrosoftDefender.Api.Models.MachineActions;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>
/// Requesting actions on a device. All of these have a real effect on it, so they are unit-verified only (never run live)
/// and refused by a read-only client.
/// </summary>
public partial class MachineActionsTests
{
	private const string DeviceId = "0123456789abcdef0123456789abcdef01234567";

	private static readonly (string Name, Func<DefenderClient, CancellationToken, Task> Call)[] Requests =
	[
		("Isolate", (c, t) => c.MachineActions.IsolateAsync(DeviceId, new IsolateRequest { Comment = "x" }, t)),
		("Unisolate", (c, t) => c.MachineActions.UnisolateAsync(DeviceId, new CommentRequest { Comment = "x" }, t)),
		("RestrictCodeExecution", (c, t) => c.MachineActions.RestrictCodeExecutionAsync(DeviceId, new CommentRequest { Comment = "x" }, t)),
		("UnrestrictCodeExecution", (c, t) => c.MachineActions.UnrestrictCodeExecutionAsync(DeviceId, new CommentRequest { Comment = "x" }, t)),
		("RunAntiVirusScan", (c, t) => c.MachineActions.RunAntiVirusScanAsync(DeviceId, new RunAntiVirusScanRequest { Comment = "x", ScanType = AntiVirusScanType.Full }, t)),
		("StopAndQuarantineFile", (c, t) => c.MachineActions.StopAndQuarantineFileAsync(DeviceId, new StopAndQuarantineFileRequest { Comment = "x", Sha1 = "abc" }, t)),
		("CollectInvestigationPackage", (c, t) => c.MachineActions.CollectInvestigationPackageAsync(DeviceId, new CommentRequest { Comment = "x" }, t)),
		("Offboard", (c, t) => c.MachineActions.OffboardAsync(DeviceId, new CommentRequest { Comment = "x" }, t)),
		("RunLiveResponse", (c, t) => c.MachineActions.RunLiveResponseAsync(DeviceId, new RunLiveResponseRequest { Commands = [LiveResponseCommand.GetFile("C:\\x")] }, t)),
		("Cancel", (c, t) => c.MachineActions.CancelAsync(ActionId, new CancelMachineActionRequest(), t))
	];

	private static string DevicePath(string verb) => $"/api/machines/{DeviceId}/{verb}";

	private static async Task AssertPostAsync(Func<DefenderClient, CancellationToken, Task> call, string verb, string body)
	{
		var recorded = await TestClient.CaptureAsync(call, ActionJson);

		recorded.ShouldBeRequest(HttpMethod.Post, DevicePath(verb), "", body);
	}

	[Fact]
	public Task Isolate_SendsPostMachinesIdIsolate_WithTheCommentAndType()
		=> AssertPostAsync((c, t) => c.MachineActions.IsolateAsync(DeviceId, new IsolateRequest { Comment = "contain it", IsolationType = IsolationType.Selective }, t),
			"isolate", """{"Comment":"contain it","IsolationType":"Selective"}""");

	[Fact]
	public Task Isolate_OmitsTheTypeWhenUnset()
		=> AssertPostAsync((c, t) => c.MachineActions.IsolateAsync(DeviceId, new IsolateRequest { Comment = "c" }, t), "isolate", """{"Comment":"c"}""");

	[Theory]
	[InlineData(IsolationType.Full, "Full")]
	[InlineData(IsolationType.Selective, "Selective")]
	[InlineData(IsolationType.UnManagedDevice, "UnManagedDevice")]
	public Task Isolate_SendsEveryIsolationTypeByItsWireName(IsolationType type, string wire)
		=> AssertPostAsync((c, t) => c.MachineActions.IsolateAsync(DeviceId, new IsolateRequest { Comment = "c", IsolationType = type }, t), "isolate", $$"""{"Comment":"c","IsolationType":"{{wire}}"}""");

	[Fact]
	public async Task Isolate_EscapesTheDeviceIdAsOneSegment()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.MachineActions.IsolateAsync("a/b c", new IsolateRequest { Comment = "c" }, t), ActionJson);

		call.Uri.AbsolutePath.Should().Be("/api/machines/a%2Fb%20c/isolate");
	}

	[Fact]
	public async Task Isolate_MapsTheCreatedAction()
		=> AssertAction(await TestClient.ReadAsync((c, t) => c.MachineActions.IsolateAsync(DeviceId, new IsolateRequest { Comment = "c" }, t), ActionJson));

	[Fact]
	public Task Isolate_MapsAnActionAlreadyInProgress()
		=> TestClient.ShouldFailAsync((c, t) => c.MachineActions.IsolateAsync(DeviceId, new IsolateRequest { Comment = "c" }, t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"ActiveRequestAlreadyExists","message":"Action is already in progress","target":"c"}}""", "Action is already in progress");

	[Fact]
	public Task Unisolate_SendsPostMachinesIdUnisolate()
		=> AssertPostAsync((c, t) => c.MachineActions.UnisolateAsync(DeviceId, new CommentRequest { Comment = "release" }, t), "unisolate", """{"Comment":"release"}""");

	[Fact]
	public Task RestrictCodeExecution_SendsPostMachinesIdRestrictCodeExecution()
		=> AssertPostAsync((c, t) => c.MachineActions.RestrictCodeExecutionAsync(DeviceId, new CommentRequest { Comment = "restrict" }, t), "restrictCodeExecution", """{"Comment":"restrict"}""");

	[Fact]
	public Task UnrestrictCodeExecution_SendsPostMachinesIdUnrestrictCodeExecution()
		=> AssertPostAsync((c, t) => c.MachineActions.UnrestrictCodeExecutionAsync(DeviceId, new CommentRequest { Comment = "lift" }, t), "unrestrictCodeExecution", """{"Comment":"lift"}""");

	[Theory]
	[InlineData(AntiVirusScanType.Quick, "Quick")]
	[InlineData(AntiVirusScanType.Full, "Full")]
	public Task RunAntiVirusScan_SendsPostMachinesIdRunAntiVirusScan_WithTheScanType(AntiVirusScanType type, string wire)
		=> AssertPostAsync((c, t) => c.MachineActions.RunAntiVirusScanAsync(DeviceId, new RunAntiVirusScanRequest { Comment = "scan", ScanType = type }, t), "runAntiVirusScan", $$"""{"Comment":"scan","ScanType":"{{wire}}"}""");

	[Fact]
	public Task StopAndQuarantineFile_SendsPostMachinesIdStopAndQuarantineFile()
		=> AssertPostAsync((c, t) => c.MachineActions.StopAndQuarantineFileAsync(DeviceId, new StopAndQuarantineFileRequest { Comment = "bad file", Sha1 = "0123456789abcdef0123456789abcdef01234567" }, t),
			"StopAndQuarantineFile", """{"Comment":"bad file","Sha1":"0123456789abcdef0123456789abcdef01234567"}""");

	[Fact]
	public Task CollectInvestigationPackage_SendsPostMachinesIdCollectInvestigationPackage()
		=> AssertPostAsync((c, t) => c.MachineActions.CollectInvestigationPackageAsync(DeviceId, new CommentRequest { Comment = "collect" }, t), "collectInvestigationPackage", """{"Comment":"collect"}""");

	[Fact]
	public Task Offboard_SendsPostMachinesIdOffboard()
		=> AssertPostAsync((c, t) => c.MachineActions.OffboardAsync(DeviceId, new CommentRequest { Comment = "retire" }, t), "offboard", """{"Comment":"retire"}""");

	[Fact]
	public Task RunLiveResponse_SendsPostMachinesIdRunliveresponse_WithTheCommands()
		=> AssertPostAsync((c, t) => c.MachineActions.RunLiveResponseAsync(DeviceId, new RunLiveResponseRequest
		{
			Comment = "investigate",
			Commands = [LiveResponseCommand.PutFile("tool.exe"), LiveResponseCommand.RunScript("scan.ps1", "-All"), LiveResponseCommand.RunScript("noargs.ps1", null), LiveResponseCommand.GetFile("C:\\temp\\out.txt")]
		}, t), "runliveresponse",
			"""{"Commands":[{"type":"PutFile","params":[{"key":"FileName","value":"tool.exe"}]},{"type":"RunScript","params":[{"key":"ScriptName","value":"scan.ps1"},{"key":"Args","value":"-All"}]},{"type":"RunScript","params":[{"key":"ScriptName","value":"noargs.ps1"}]},{"type":"GetFile","params":[{"key":"Path","value":"C:\\temp\\out.txt"}]}],"Comment":"investigate"}""");

	[Fact]
	public Task RunLiveResponse_OmitsTheCommentWhenUnset()
		=> AssertPostAsync((c, t) => c.MachineActions.RunLiveResponseAsync(DeviceId, new RunLiveResponseRequest { Commands = [] }, t), "runliveresponse", """{"Commands":[]}""");

	[Fact]
	public Task RunLiveResponse_MapsAnErrorForAConcurrentSession()
		=> TestClient.ShouldFailAsync((c, t) => c.MachineActions.RunLiveResponseAsync(DeviceId, new RunLiveResponseRequest { Commands = [] }, t), HttpStatusCode.BadRequest,
			"""{"error":{"code":"ActiveRequestAlreadyExists","message":"A session is active","target":"c"}}""", "A session is active");

	[Fact]
	public Task EveryRequest_IsRefusedByAReadOnlyClient_BeforeAnythingIsSent()
		=> WriteOperationChecks.ShouldBeRefusedWhenReadOnlyAsync(Requests);

	[Fact]
	public Task EveryRequest_ResendsAnIdenticalBodyAfterA401()
		=> WriteOperationChecks.ShouldResendIdenticalBodiesAfter401Async(Requests, HttpStatusCode.Created, ActionJson);

	[Fact]
	public void Commands_BuildTheDocumentedParameterKeys()
	{
		LiveResponseCommand.RunScript("s", null).Params.Should().ContainSingle().Which.Key.Should().Be("ScriptName");
		LiveResponseCommand.GetFile("p").Type.Should().Be(LiveResponseCommandType.GetFile);
		LiveResponseCommand.PutFile("f").Type.Should().Be(LiveResponseCommandType.PutFile);
	}
}
