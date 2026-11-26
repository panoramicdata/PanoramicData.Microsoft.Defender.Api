using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public partial class AuthenticationHandlerTests
{
	[Fact]
	public async Task On401_FetchesANewToken_AndReplaysTheRequestWithItsBody()
	{
		using var harness = new Harness().Respond(HttpStatusCode.Unauthorized, HttpStatusCode.OK);

		using var response = await harness.SendAsync(HttpMethod.Patch, new StringContent("""{"status":"Resolved"}""", System.Text.Encoding.UTF8, "application/json"));

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls.Select(c => c.Authorization).Should().Equal("Bearer token-1", "Bearer token-2");
		harness.Stub.Calls.Select(c => c.Body).Should().AllBe("""{"status":"Resolved"}""");
		harness.Credential.Issued.Should().Be(2);
	}

	[Fact]
	public async Task On401_WhenTheCredentialReturnsTheSameToken_TheSecond401IsReturned()
	{
		using var harness = new Harness(configure: c => c.FixedToken = "cached").Respond(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized);

		using var response = await harness.SendAsync();

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		harness.Stub.Calls.Should().HaveCount(2, "the request is replayed once, never more");
		harness.Credential.Issued.Should().Be(2, "the stale token is never served from the cache");
	}

	[Fact]
	public async Task On401_WithAStreamBody_IsNotReplayed()
	{
		using var harness = new Harness().Respond(HttpStatusCode.Unauthorized);

		using var response = await harness.SendAsync(HttpMethod.Post, new StreamContent(new MemoryStream("x"u8.ToArray())));

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		harness.Stub.Calls.Should().ContainSingle();
		harness.Credential.Issued.Should().Be(1);
	}

	[Theory]
	[InlineData(HttpStatusCode.Forbidden)]
	[InlineData(HttpStatusCode.OK)]
	public async Task OtherStatuses_AreReturnedUntouched(HttpStatusCode status)
	{
		using var harness = new Harness().Respond(status);

		using var response = await harness.SendAsync();

		response.StatusCode.Should().Be(status);
		harness.Stub.Calls.Should().ContainSingle();
	}
}
