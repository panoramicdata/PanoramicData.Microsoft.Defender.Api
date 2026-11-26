using PanoramicData.MicrosoftDefender.Api.Models.Indicators;
using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

public partial class IndicatorsTests
{
	[Fact]
	public async Task List_SendsGetIndicators_WithNoQuery()
	{
		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.ListAsync(null, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/indicators");
	}

	[Fact]
	public async Task List_SendsTheODataOptions_WithALiteralDollar()
	{
		var options = new ODataListOptions { Filter = "action eq 'Block' and title eq 'it''s'", Top = 50, Skip = 100 };

		var call = await TestClient.CaptureAsync((c, t) => c.Indicators.ListAsync(options, t), Envelope(""));

		call.ShouldBeRequest(HttpMethod.Get, "/api/indicators", "?$filter=action%20eq%20%27Block%27%20and%20title%20eq%20%27it%27%27s%27&$top=50&$skip=100");
	}

	[Fact]
	public async Task List_MapsEveryField_AndTheEnvelope()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Indicators.ListAsync(null, t), Envelope(IndicatorJson));

		page.Context.Should().Be("https://defender.test/api/$metadata#Indicators");
		var i = page.Value.Should().ContainSingle().Subject;
		(i.Id, i.IndicatorValue, i.IndicatorType, i.Action).Should().Be(("994", "220e7d15b011d7fac48f2bd61114db1022197f7f", IndicatorType.FileSha1, IndicatorAction.BlockAndRemediate));
		(i.Application, i.ExternalId, i.Source, i.SourceType, i.CreatedBySource, i.CreatedBy, i.LastUpdatedBy)
			.Should().Be(("demo-app", "ext-1", "user@example.test", "AadApp", "Demo app", "00000000-0000-0000-0000-000000000009", "other@example.test"));
		i.CreationTime.Should().Be(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero));
		i.ExpirationTime.Should().Be(new DateTimeOffset(2027, 1, 2, 3, 4, 5, TimeSpan.Zero));
		i.LastUpdateTime.Should().Be(new DateTimeOffset(2026, 1, 3, 3, 4, 5, TimeSpan.Zero));
		(i.Severity, i.Title, i.Description, i.RecommendedActions).Should().Be((IndicatorSeverity.Informational, "Made-up indicator", "A made-up indicator used by the tests.", "nothing"));
		i.RbacGroupNames.Should().Equal("team1");
		i.RbacGroupIds.Should().Equal("7");
		(i.GenerateAlert, i.EducateUrl).Should().Be((true, "https://help.example.test/blocked"));
	}

	[Theory]
	[InlineData("Alert", IndicatorAction.Alert)]
	[InlineData("AlertAndBlock", IndicatorAction.AlertAndBlock)]
	[InlineData("Warn", IndicatorAction.Warn)]
	[InlineData("Block", IndicatorAction.Block)]
	[InlineData("Audit", IndicatorAction.Audit)]
	[InlineData("Allowed", IndicatorAction.Allowed)]
	[InlineData("SomethingNew", IndicatorAction.Unknown)]
	public async Task List_ReadsEveryAction_IncludingTheLegacyOnes(string wire, IndicatorAction expected)
	{
		var page = await TestClient.ReadAsync((c, t) => c.Indicators.ListAsync(null, t), Envelope($$"""{"id":"1","action":"{{wire}}"}"""));

		page.Value.Should().ContainSingle().Which.Action.Should().Be(expected);
	}

	[Fact]
	public async Task List_ToleratesLooseJson()
	{
		var page = await TestClient.ReadAsync((c, t) => c.Indicators.ListAsync(null, t),
			Envelope("""{"id":"1","indicatorType":"Hologram","severity":"Cosmic","generateAlert":"true","expirationTime":"","rbacGroupNames":null,"rbacGroupIds":null,"extra":[1]}"""));

		var i = page.Value.Should().ContainSingle().Subject;
		i.IndicatorType.Should().Be(IndicatorType.Unknown);
		i.Severity.Should().Be(IndicatorSeverity.Unknown);
		i.GenerateAlert.Should().BeTrue();
		i.ExpirationTime.Should().BeNull();
		i.RbacGroupNames.Should().BeEmpty();
		i.RbacGroupIds.Should().BeEmpty();
	}

	[Fact]
	public Task List_MapsAnError()
		=> TestClient.ShouldFailAsync((c, t) => c.Indicators.ListAsync(null, t), HttpStatusCode.Forbidden, Failure("Forbidden", "Missing Ti.ReadWrite.All"), "Missing Ti.ReadWrite.All");

	[Fact]
	public async Task GetAllIndicators_FollowsTheNextLink()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"value":[{"id":"1"}],"@odata.nextLink":"https://defender.test/api/indicators?$skiptoken=2"}""");
		stub.Enqueue(HttpStatusCode.OK, Envelope("""{"id":"2"}"""));
		using var client = TestClient.Create(stub);

		var ids = new List<string?>();
		await foreach (var indicator in client.GetAllIndicatorsAsync(new ODataListOptions { Top = 1 }, TestContext.Current.CancellationToken))
		{
			ids.Add(indicator.Id);
		}

		ids.Should().Equal("1", "2");
		stub.Calls.Select(c => c.Uri.Query).Should().Equal("?$top=1", "?$skiptoken=2");
	}
}
