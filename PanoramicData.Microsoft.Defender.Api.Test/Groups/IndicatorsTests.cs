using PanoramicData.MicrosoftDefender.Api.Models;
using PanoramicData.MicrosoftDefender.Api.Models.Indicators;
using PanoramicData.MicrosoftDefender.Api.Test.Support;
using System.Net;

namespace PanoramicData.MicrosoftDefender.Api.Test.Groups;

/// <summary>Indicators: listing, creating, importing and deleting. The tests are split by concern across the partial files; this one holds what they share.</summary>
public partial class IndicatorsTests
{
	private const string IndicatorJson = """
        {
            "id": "994",
            "indicatorValue": "220e7d15b011d7fac48f2bd61114db1022197f7f",
            "indicatorType": "FileSha1",
            "action": "BlockAndRemediate",
            "application": "demo-app",
            "externalID": "ext-1",
            "source": "user@example.test",
            "sourceType": "AadApp",
            "createdBySource": "Demo app",
            "createdBy": "00000000-0000-0000-0000-000000000009",
            "lastUpdatedBy": "other@example.test",
            "creationTimeDateTimeUtc": "2026-01-02T03:04:05Z",
            "expirationTime": "2027-01-02T03:04:05Z",
            "lastUpdateTime": "2026-01-03T03:04:05Z",
            "severity": "Informational",
            "title": "Made-up indicator",
            "description": "A made-up indicator used by the tests.",
            "recommendedActions": "nothing",
            "rbacGroupNames": ["team1"],
            "rbacGroupIds": ["7"],
            "generateAlert": true,
            "educateUrl": "https://help.example.test/blocked"
        }
        """;

	private static readonly string FullSubmissionBody = """{"indicatorValue":"220e7d15b011d7fac48f2bd61114db1022197f7f","indicatorType":"FileSha1","action":"AlertAndBlock","title":"demo","description":"demo description","application":"demo-app","expirationTime":"2030-12-12T00:00:00.0000000+00:00","severity":"Informational","recommendedActions":"nothing","rbacGroupNames":["group1","group2"],"educateUrl":"https://help.example.test/x","generateAlert":true}""".Replace("+", WriteOperationChecks.EscapedPlus);

	private const string MinimalSubmissionBody = """{"indicatorValue":"example.test","indicatorType":"DomainName","action":"Allowed","title":"t","description":"d"}""";

	private static IndicatorSubmission FullSubmission() => new()
	{
		IndicatorValue = "220e7d15b011d7fac48f2bd61114db1022197f7f",
		IndicatorType = IndicatorType.FileSha1,
		Action = IndicatorAction.AlertAndBlock,
		Title = "demo",
		Description = "demo description",
		Application = "demo-app",
		ExpirationTime = new DateTimeOffset(2030, 12, 12, 0, 0, 0, TimeSpan.Zero),
		Severity = IndicatorSeverity.Informational,
		RecommendedActions = "nothing",
		RbacGroupNames = ["group1", "group2"],
		EducateUrl = "https://help.example.test/x",
		GenerateAlert = true
	};

	private static IndicatorSubmission MinimalSubmission() => new()
	{
		IndicatorValue = "example.test",
		IndicatorType = IndicatorType.DomainName,
		Action = IndicatorAction.Allowed,
		Title = "t",
		Description = "d"
	};

	private static string Envelope(string items)
		=> $$"""{"@odata.context":"https://defender.test/api/$metadata#Indicators","value":[{{items}}]}""";

	private static string Failure(string code, string message)
		=> $$$"""{"error":{"code":"{{{code}}}","message":"{{{message}}}","target":"corr-1"}}""";

	private static IReadOnlyList<IndicatorSubmission> Submissions(int count)
		=> [.. Enumerable.Range(0, count).Select(_ => MinimalSubmission())];
}
