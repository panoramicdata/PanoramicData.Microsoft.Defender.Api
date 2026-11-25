using PanoramicData.MicrosoftDefender.Api.Handlers;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public class ODataQueryHandlerTests
{
	private static async Task<Uri> SendAsync(string uri)
	{
		using var harness = new HandlerHarness(new ODataQueryHandler());
		harness.Stub.Enqueue(HttpStatusCode.OK);
		using var response = await harness.SendAsync(HttpMethod.Get, uri);
		return harness.Stub.Calls.Should().ContainSingle().Subject.Uri;
	}

	[Theory]
	[InlineData("https://h.test/api/x?%24filter=a%20eq%20%27b%27&%24top=5", "https://h.test/api/x?$filter=a%20eq%20%27b%27&$top=5")]
	[InlineData("https://h.test/api/x?%24Skip=1", "https://h.test/api/x?$Skip=1")]
	[InlineData("https://h.test/api/x?pageSize=3&%24top=5", "https://h.test/api/x?pageSize=3&$top=5")]
	public async Task EscapedDollarsInQueryNames_AreWrittenLiterally(string sent, string expected)
		=> (await SendAsync(sent)).AbsoluteUri.Should().Be(expected);

	[Theory]
	[InlineData("https://h.test/api/x")]
	[InlineData("https://h.test/api/x?pageSize=3")]
	[InlineData("https://h.test/api/x?$filter=a")]
	public async Task OtherQueries_AreLeftAlone(string sent)
		=> (await SendAsync(sent)).AbsoluteUri.Should().Be(sent);

	[Fact]
	public async Task EscapedDollarsInValues_AndPercentSigns_AreNeverTouched()
	{
		var uri = await SendAsync("https://h.test/api/a%2Fb?%24filter=cost%20eq%20%2450%20and%20x%3D100%25&k=%24v");

		uri.AbsoluteUri.Should().Be("https://h.test/api/a%2Fb?$filter=cost%20eq%20%2450%20and%20x%3D100%25&k=%24v");
	}
}
