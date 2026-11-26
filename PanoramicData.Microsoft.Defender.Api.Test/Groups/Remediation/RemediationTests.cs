using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Remediation;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using static PanoramicData.MicrosoftDefender.Api.Test.Support.GroupHelpers;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>The remediation activity operations: each pins its request and maps a realistic response.</summary>
public class RemediationTests
{
	private const string TaskJson = """
		{"id":"task-1","title":"Update product","createdOn":"2021-02-10T13:20:36.4718166Z","requesterId":"req-id","requesterEmail":"user1@example.test","status":"Active","statusLastModifiedOn":"2021-02-11T00:00:00Z","description":"Update it.","relatedComponent":"Product","targetDevices":18511,"rbacGroupNames":["GroupA","GroupB"],"fixedDevices":2866,"requesterNotes":"note","dueOn":"2021-02-12T00:00:00Z","category":"Software","productivityImpactRemediationType":"AllExposedAssets","priority":"Medium","completionMethod":"Manual","completerId":"comp-id","completerEmail":"c@example.test","scid":"scid-1","type":"Update","productId":"vendor-_-product","vendorId":"vendor","nameId":"product","recommendedVersion":"2.0","recommendedVendor":"rv","recommendedProgram":"rp"}
		""";

	private static void AssertTask(RemediationTask r)
	{
		(r.Id, r.Title, r.Description, r.Category, r.Status, r.Type, r.Priority).Should().Be(("task-1", "Update product", "Update it.", "Software", "Active", "Update", "Medium"));
		(r.CreatedOn, r.DueOn, r.StatusLastModifiedOn).Should().Be((new DateTimeOffset(2021, 2, 10, 13, 20, 36, TimeSpan.Zero).AddTicks(4718166), new DateTimeOffset(2021, 2, 12, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2021, 2, 11, 0, 0, 0, TimeSpan.Zero)));
		(r.CompletionMethod, r.CompleterEmail, r.CompleterId, r.RequesterEmail, r.RequesterId, r.RequesterNotes)
			.Should().Be(("Manual", "c@example.test", "comp-id", "user1@example.test", "req-id", "note"));
		(r.FixedDevices, r.TargetDevices, r.NameId, r.ProductId, r.VendorId).Should().Be((2866L, 18511L, "product", "vendor-_-product", "vendor"));
		r.RbacGroupNames.Should().Equal("GroupA", "GroupB");
		(r.RecommendedVersion, r.RecommendedVendor, r.RecommendedProgram, r.RelatedComponent, r.Scid, r.ProductivityImpactRemediationType)
			.Should().Be(("2.0", "rv", "rp", "Product", "scid-1", "AllExposedAssets"));
	}

	[Fact]
	public Task List_SendsGetRemediationTasks()
		=> ShouldGetAsync((c, t) => c.Remediation.ListAsync(null, t), Envelope(""), "/api/remediationtasks");

	[Fact]
	public Task List_SendsTheODataOptions()
		=> ShouldGetAsync((c, t) => c.Remediation.ListAsync(new ODataListOptions { Filter = "status eq 'Active'", Top = 5, Skip = 1 }, t), Envelope(""),
			"/api/remediationtasks", "?$filter=status%20eq%20%27Active%27&$top=5&$skip=1");

	[Fact]
	public async Task List_MapsEveryField_AndToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Remediation.ListAsync(null, t),
			Envelope(TaskJson + ",{\"id\":\"t2\",\"rbacGroupNames\":null,\"targetDevices\":\"4\",\"dueOn\":\"\",\"completerEmail\":null,\"scid\":null}"));

		AssertTask(page.Value[0]);
		var loose = page.Value[1];
		loose.RbacGroupNames.Should().BeEmpty();
		(loose.TargetDevices, loose.DueOn, loose.CompleterEmail, loose.Scid).Should().Be((4L, (DateTimeOffset?)null, (string?)null, (string?)null));
	}

	[Fact]
	public Task List_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Remediation.ListAsync(null, t));

	[Fact]
	public Task Get_SendsGetRemediationTasksId()
		=> ShouldGetAsync((c, t) => c.Remediation.GetAsync("task-1", t), TaskJson, "/api/remediationtasks/task-1");

	[Fact]
	public async Task Get_MapsEveryField()
		=> AssertTask(await TestClient.ReadAsync((c, t) => c.Remediation.GetAsync("task-1", t), TaskJson));

	[Fact]
	public Task Get_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Remediation.GetAsync("x", t));

	[Fact]
	public Task ListMachineReferences_SendsGetRemediationTasksIdMachineReferences()
		=> ShouldGetAsync((c, t) => c.Remediation.ListMachineReferencesAsync("task-1", t), Envelope(""), "/api/remediationtasks/task-1/machineReferences");

	[Fact]
	public async Task ListMachineReferences_MapsEveryField()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Remediation.ListMachineReferencesAsync("task-1", t),
			Envelope("""{"id":"m1","computerDnsName":"srv.example.test","osPlatform":"WindowsServer2016","rbacGroupName":"UnassignedGroup"}"""));

		var m = page.Value.Should().ContainSingle().Subject;
		(m.Id, m.ComputerDnsName, m.OsPlatform, m.RbacGroupName).Should().Be(("m1", "srv.example.test", "WindowsServer2016", "UnassignedGroup"));
	}

	[Fact]
	public Task ListMachineReferences_MapsAnError()
		=> ShouldFailWith403Async((c, t) => c.Remediation.ListMachineReferencesAsync("x", t));

	[Fact]
	public async Task GetAllHelpers_WireTheirOperations()
	{
		await ShouldEnumerateAsync((c, t) => c.GetAllRemediationTasksAsync(new ODataListOptions { Top = 2 }, t), "/api/remediationtasks", "?$top=2");
		await ShouldEnumerateAsync((c, t) => c.GetAllRemediationTaskMachineReferencesAsync("t1", t), "/api/remediationtasks/t1/machineReferences");
	}
}
