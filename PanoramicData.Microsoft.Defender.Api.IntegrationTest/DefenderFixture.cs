using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace PanoramicData.MicrosoftDefender.Api.IntegrationTest;

/// <summary>
/// A <see cref="DefenderClient"/> for a live Microsoft Defender for Endpoint tenant, configured from user secrets or
/// environment variables.
/// </summary>
/// <remarks>
/// Requires <c>Defender:TenantId</c>, <c>Defender:ClientId</c> and <c>Defender:ClientSecret</c> (an Entra application with the
/// needed Defender for Endpoint application permissions). Optional: <c>Defender:Cloud</c> (a <see cref="DefenderCloud"/> name
/// such as <c>Europe</c>), <c>Defender:BaseUri</c> and <c>Defender:Scope</c>. Set them with, for example,
/// <c>dotnet user-secrets set Defender:TenantId &lt;value&gt; --project PanoramicData.Microsoft.Defender.Api.IntegrationTest</c>,
/// or as <c>Defender__TenantId</c> etc. in the environment (see docs/TESTING.md). A missing setting fails the tests with a
/// message naming it; nothing is skipped. The client is read-only unless a test asks otherwise.
/// </remarks>
public sealed class DefenderFixture : IDisposable
{
	private const string Project = "PanoramicData.Microsoft.Defender.Api.IntegrationTest";

	private readonly Lazy<DefenderClient> _client;

	public DefenderFixture()
	{
		Configuration = new ConfigurationBuilder()
			.AddUserSecrets<DefenderFixture>()
			.AddEnvironmentVariables()
			.Build();
		_client = new Lazy<DefenderClient>(() => new DefenderClient(CreateOptions()));
	}

	public IConfiguration Configuration { get; }

	/// <summary>A read-only client authenticated as configured.</summary>
	public DefenderClient Client => _client.Value;

	/// <summary>Builds read-only options from configuration, failing loudly when a required setting is missing.</summary>
	public DefenderClientOptions CreateOptions()
	{
		var options = new DefenderClientOptions
		{
			Credential = new ClientSecretCredential(Required("Defender:TenantId"), Required("Defender:ClientId"), Required("Defender:ClientSecret")),
			IsReadOnly = true
		};
		if (Configuration["Defender:Cloud"] is { Length: > 0 } cloud)
		{
			options.UseCloud(Enum.Parse<DefenderCloud>(cloud, ignoreCase: true));
		}

		if (Configuration["Defender:BaseUri"] is { Length: > 0 } baseUri)
		{
			options.BaseUri = new Uri(baseUri);
		}

		if (Configuration["Defender:Scope"] is { Length: > 0 } scope)
		{
			options.Scope = scope;
		}

		return options;
	}

	private string Required(string key)
		=> Configuration[key] is { Length: > 0 } value
			? value
			: throw new InvalidOperationException(
				$"Integration tests need '{key}'. Set it with 'dotnet user-secrets set {key} <value> --project {Project}', "
				+ "or as an environment variable (Defender__TenantId, Defender__ClientId, Defender__ClientSecret). See docs/TESTING.md.");

	public void Dispose()
	{
		if (_client.IsValueCreated)
		{
			_client.Value.Dispose();
		}
	}
}
