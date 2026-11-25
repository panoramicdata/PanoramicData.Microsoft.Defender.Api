using PanoramicData.MicrosoftDefender.Api.Test.Support;

namespace PanoramicData.MicrosoftDefender.Api.Test.Core;

public class OptionsTests
{
	private static DefenderClientOptions Valid() => TestClient.Options(TimeProvider.System);

	[Fact]
	public void Defaults_AreTheGlobalCloudAndTheLegacyAudience()
	{
		var options = new DefenderClientOptions();

		options.BaseUri.Should().Be(new Uri("https://api.security.microsoft.com/api"));
		options.Scope.Should().Be("https://api.securitycenter.microsoft.com/.default");
		options.IsReadOnly.Should().BeFalse();
		options.TimeProvider.Should().BeSameAs(TimeProvider.System);
		options.MaxRetries.Should().Be(3);
	}

	[Theory]
	[InlineData(DefenderCloud.Global, "https://api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.UnitedStates, "https://us.api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.Europe, "https://eu.api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.UnitedKingdom, "https://uk.api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.Australia, "https://au.api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.Sweden, "https://swa.api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.India, "https://ina.api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.UnitedArabEmirates, "https://aea.api.security.microsoft.com/api")]
	[InlineData(DefenderCloud.UsGovernmentGcc, "https://api-gcc.securitycenter.microsoft.us/api")]
	[InlineData(DefenderCloud.UsGovernmentGccHigh, "https://api-gov.securitycenter.microsoft.us/api")]
	public void UseCloud_SetsTheBaseUri_AndLeavesTheScopeAlone(DefenderCloud cloud, string expected)
	{
		var options = new DefenderClientOptions { Scope = "custom" };

		options.UseCloud(cloud);

		options.BaseUri.Should().Be(new Uri(expected));
		options.Scope.Should().Be("custom");
		DefenderEndpoints.BaseUri(cloud).Should().Be(new Uri(expected));
	}

	[Fact]
	public void UnknownCloud_Throws()
	{
		var act = () => DefenderEndpoints.BaseUri((DefenderCloud)99);

		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Validate_AcceptsValidOptions()
		=> ((Action)Valid().Validate).Should().NotThrow();

	public static TheoryData<string, Action<DefenderClientOptions>> Invalid => new()
	{
		{ "Credential", o => o.Credential = null },
		{ "Scope", o => o.Scope = " " },
		{ "BaseUri", o => o.BaseUri = null! },
		{ "BaseUri", o => o.BaseUri = new Uri("/relative", UriKind.Relative) },
		{ "BaseUri", o => o.BaseUri = new Uri("http://defender.test/api") },
		{ "BaseUri", o => o.BaseUri = new Uri("https://user:secret@defender.test/api") },
		{ "BaseUri", o => o.BaseUri = new Uri("https://defender.test/api?x=1") },
		{ "BaseUri", o => o.BaseUri = new Uri("https://defender.test/api#frag") },
		{ "TimeProvider", o => o.TimeProvider = null! },
		{ "thumbprint", o => o.TrustedServerCertificateThumbprint = "abc" },
		{ "MaxRetries", o => o.MaxRetries = -1 },
		{ "Timeout", o => o.Timeout = TimeSpan.Zero },
		{ "Timeout", o => o.Timeout = TimeSpan.FromDays(30) },
		{ "RetryBaseDelay", o => o.RetryBaseDelay = TimeSpan.FromSeconds(-1) },
		{ "MaxRetryDelay", o => o.MaxRetryDelay = TimeSpan.Zero },
		{ "MaxRetryDelay", o => o.MaxRetryDelay = TimeSpan.FromDays(30) },
		{ "TokenRefreshMargin", o => o.TokenRefreshMargin = TimeSpan.FromSeconds(-1) }
	};

	[Theory]
	[MemberData(nameof(Invalid))]
	public void Validate_RejectsInvalidOptions(string parameter, Action<DefenderClientOptions> break_)
	{
		var options = Valid();
		break_(options);

		var act = options.Validate;

		act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be(parameter);
	}

	[Fact]
	public void ToString_NamesTheCredentialType_NeverASecret()
	{
		var options = Valid();
		options.IsReadOnly = true;

		var text = options.ToString();

		text.Should().Contain("FakeCredential").And.Contain("https://defender.test/api/").And.Contain("ReadOnly = True");
		new DefenderClientOptions().ToString().Should().Contain("Credential = (none)");
	}
}
