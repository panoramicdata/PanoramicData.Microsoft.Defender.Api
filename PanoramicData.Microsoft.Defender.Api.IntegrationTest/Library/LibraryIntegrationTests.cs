using Refit;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest.Library;

/// <summary>
/// Read-only checks of the live response library against a live tenant (needs <c>Library.Manage</c>). Live verification is
/// pending: see docs/TESTING.md. Uploading and deleting library files is unit-verified only and never run here.
/// </summary>
[Collection(DefenderTestGroup.Name)]
public class LibraryIntegrationTests(DefenderFixture fixture)
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	[Fact]
	public async Task ListAsync_ReturnsTheLibraryFiles()
	{
		var page = await fixture.Client.Library.ListAsync(Ct);

		page.Value.Should().OnlyContain(f => !string.IsNullOrEmpty(f.FileName) && !string.IsNullOrEmpty(f.Sha256));
	}

	[Fact]
	public async Task WritesAreRefused_ByTheReadOnlyClient_BeforeAnythingIsSent()
	{
		using var content = new MemoryStream([1, 2, 3]);
		var upload = () => fixture.Client.Library.UploadAsync(new StreamPart(content, "never-sent.txt", "text/plain"), null, null, null, Ct);
		var delete = () => fixture.Client.Library.DeleteAsync("never-sent.txt", Ct);

		await upload.Should().ThrowAsync<DefenderReadOnlyException>();
		await delete.Should().ThrowAsync<DefenderReadOnlyException>();
	}
}
