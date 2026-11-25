using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using Refit;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

/// <summary>Pins how Refit and the Defender serializer encode query strings and request bodies, as sent on the wire.</summary>
public class RequestEncodingTests
{
	public interface IProbe
	{
		[Get("probe")]
		Task<ODataPage<string>> ListAsync([Query] ODataListOptions? options, CancellationToken cancellationToken);

		[Get("probe")]
		Task<ODataPage<string>> ByMachineAsync([Query] ExportByMachineOptions? options, CancellationToken cancellationToken);

		[Get("probe")]
		Task<ODataPage<string>> ViaFilesAsync([Query] ExportViaFilesOptions? options, CancellationToken cancellationToken);

		[Get("probe/{name}")]
		Task<ODataPage<string>> NamedAsync(string name, CancellationToken cancellationToken);

		[Patch("probe")]
		Task UpdateAsync([Body] Body body, CancellationToken cancellationToken);
	}

	public sealed class Body
	{
		[System.Text.Json.Serialization.JsonPropertyName("Comment")]
		public string? Comment { get; set; }

		[System.Text.Json.Serialization.JsonPropertyName("Flag")]
		public bool? Flag { get; set; }

		[System.Text.Json.Serialization.JsonPropertyName("Skipped")]
		public string? Skipped { get; set; }
	}

	private static (IProbe Probe, StubHandler Stub) Create()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"value":[]}""");
		var client = new HttpClient(new Handlers.ODataQueryHandler { InnerHandler = stub }) { BaseAddress = new Uri(TestClient.BaseUrl) };
		return (RestService.For<IProbe>(client, DefenderClient.CreateSettings(TimeProvider.System)), stub);
	}

	[Fact]
	public async Task ODataOptions_KeepTheDollarLiteralInTheQueryName()
	{
		var (probe, stub) = Create();

		await probe.ListAsync(new ODataListOptions { Filter = "severity eq 'High'", Top = 5, Skip = 10 }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().StartWith("?$filter=").And.Contain("&$top=5&$skip=10").And.NotContain("%24");
	}

	[Fact]
	public async Task Filter_EscapesQuotesSpacesAndDollarsInTheValue()
	{
		var (probe, stub) = Create();

		await probe.ListAsync(new ODataListOptions { Filter = "name eq 'O''Neil $x&y'" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?$filter=name%20eq%20%27O%27%27Neil%20%24x%26y%27");
	}

	[Fact]
	public async Task NullOptions_SendNoQuery()
	{
		var (probe, stub) = Create();

		await probe.ListAsync(null, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().BeEmpty();
	}

	[Fact]
	public async Task UnsetOptionProperties_AreOmitted()
	{
		var (probe, stub) = Create();

		await probe.ListAsync(new ODataListOptions { Top = 1 }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?$top=1");
	}

	[Fact]
	public async Task ExportByMachineOptions_UseTheirWireNames_AndFormatTimesAsUtcIso()
	{
		var (probe, stub) = Create();

		await probe.ByMachineAsync(
			new ExportByMachineOptions { PageSize = 50000, Top = 3, SinceTime = new DateTimeOffset(2026, 1, 2, 5, 4, 3, TimeSpan.FromHours(2)) },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?pageSize=50000&$top=3&sinceTime=2026-01-02T03%3A04%3A03Z");
	}

	[Fact]
	public async Task ExportViaFilesOptions_UseTheirWireName()
	{
		var (probe, stub) = Create();

		await probe.ViaFilesAsync(new ExportViaFilesOptions { SasValidHours = 6 }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?sasValidHours=6");
	}

	[Fact]
	public async Task PathParameters_AreEscapedAsOneSegment()
	{
		var (probe, stub) = Create();

		await probe.NamedAsync("a/b c", TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/probe/a%2Fb%20c");
	}

	[Fact]
	public async Task Body_IsSentAsJson_WithExplicitCaseSensitiveNames_AndNullsOmitted()
	{
		var (probe, stub) = Create();

		await probe.UpdateAsync(new Body { Comment = "x", Flag = true }, TestContext.Current.CancellationToken);

		stub.Calls[0].ShouldBeRequest(HttpMethod.Patch, "/api/probe", body: """{"Comment":"x","Flag":true}""");
	}

	[Theory]
	[InlineData("2026-01-02T03:04:05.1230000+00:00", "2026-01-02T03:04:05.123Z")]
	[InlineData("2026-01-02T03:04:05+00:00", "2026-01-02T03:04:05Z")]
	public void FormatDate_DropsEmptyFractionsOnly(string input, string expected)
		=> Serialization.DefenderUrlParameterFormatter.FormatDate(DateTimeOffset.Parse(input, System.Globalization.CultureInfo.InvariantCulture)).Should().Be(expected);
}
