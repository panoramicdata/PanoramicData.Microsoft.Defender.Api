# PanoramicData.Microsoft.Defender.Api

An unofficial, typed .NET 11 client for the [Microsoft Defender for Endpoint REST API](https://learn.microsoft.com/en-us/defender-endpoint/api/apis-intro) (v1.0), built on Refit and System.Text.Json.

PanoramicData.Microsoft.Defender.Api is an independent open-source project by Panoramic Data Limited. It is not affiliated with, endorsed by or supported by Microsoft. Microsoft and Defender are trademarks of Microsoft Corporation.

The full documentation is still being written; the vulnerabilities group is the first one implemented.

## Installation

```shell
dotnet add package PanoramicData.Microsoft.Defender.Api
```

## Quick start

The client authenticates with any `Azure.Core.TokenCredential`, for example `ClientSecretCredential` from `Azure.Identity`.

```csharp
using Azure.Identity;
using PanoramicData.MicrosoftDefender.Api;

using var client = new DefenderClient(new DefenderClientOptions
{
	Credential = new ClientSecretCredential("<tenant id>", "<client id>", "<client secret>"),
	IsReadOnly = true
});

await foreach (var vulnerability in client.GetAllVulnerabilitiesAsync(null, cancellationToken))
{
	Console.WriteLine($"{vulnerability.Id}: {vulnerability.Severity}");
}
```

## Licence

MIT. See [LICENSE](LICENSE).
