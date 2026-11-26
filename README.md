# PanoramicData.Microsoft.Defender.Api

[![NuGet](https://img.shields.io/nuget/v/PanoramicData.Microsoft.Defender.Api.svg)](https://www.nuget.org/packages/PanoramicData.Microsoft.Defender.Api)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 11.0](https://img.shields.io/badge/.NET-11.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![CI](https://github.com/panoramicdata/PanoramicData.Microsoft.Defender.Api/actions/workflows/ci.yml/badge.svg)](https://github.com/panoramicdata/PanoramicData.Microsoft.Defender.Api/actions/workflows/ci.yml)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/09929a29f4f74d6690254424b2a5a19f)](https://app.codacy.com/gh/panoramicdata/PanoramicData.Microsoft.Defender.Api/dashboard)
[![Codacy Coverage](https://app.codacy.com/project/badge/Coverage/09929a29f4f74d6690254424b2a5a19f)](https://app.codacy.com/gh/panoramicdata/PanoramicData.Microsoft.Defender.Api/dashboard)

> **Need help with APIs or .NET?** Panoramic Data are experts in APIs and .NET, and we can help with your development. [Contact us](https://www.panoramicdata.com/contact-us) or see our [software services](https://www.panoramicdata.com/services/software).

An unofficial, typed .NET client for the [Microsoft Defender for Endpoint REST API](https://learn.microsoft.com/en-us/defender-endpoint/api/apis-intro)
(v1.0), including the Defender Vulnerability Management operations, built on Refit and System.Text.Json.

Supports .NET 11.0.

PanoramicData.Microsoft.Defender.Api is an independent open-source project by Panoramic Data Limited. It is not
affiliated with, endorsed by or supported by Microsoft. Microsoft and Defender are trademarks of Microsoft Corporation.

## Installation

```shell
dotnet add package PanoramicData.Microsoft.Defender.Api
```

The package targets .NET 11.0. Its version follows the API version it implements, so `1.0.x` implements API v1.0.

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

The root namespace is `PanoramicData.MicrosoftDefender.Api`. It has no `Microsoft` segment on purpose: a namespace
called `PanoramicData.Microsoft` would hide `Microsoft.Extensions.*` for every consumer that has a
`using PanoramicData...` directive in scope.

## What is covered

All 111 documented operations of the in-scope API, in 21 groups. Each group is a property on `DefenderClient`.

| Group | Property | Operations |
|---|---|---|
| Alerts | `Alerts` | 10 |
| Machines (devices) | `Machines` | 15 |
| Machine actions | `MachineActions` | 14 |
| Investigations | `Investigations` | 3 |
| Indicators | `Indicators` | 5 |
| Live response library | `Library` | 3 |
| Files, domains, IPs, users | `Files`, `Domains`, `Ips`, `Users` | 4, 3, 2, 2 |
| Vulnerabilities | `Vulnerabilities` | 4 |
| Software | `Software` | 6 |
| Recommendations | `Recommendations` | 5 |
| Exposure score | `Scores` | 3 |
| Remediation | `Remediation` | 3 |
| Assessment exports (JSON and files) | `AssessmentExports` | 16 |
| Device health | `DeviceHealth` | 1 |
| Browser extensions | `BrowserExtensions` | 1 |
| Security baselines | `SecurityBaselines` | 2 |
| Authenticated scan | `AuthenticatedScan` | 8 |
| Advanced hunting (retiring, `[Obsolete]`) | `AdvancedHunting` | 1 |

Every operation is listed with its client method and test in [docs/endpoints](docs/endpoints).

Advanced hunting is implemented but marked `[Obsolete]`: Microsoft is retiring that API in favour of the Microsoft
Graph `runHuntingQuery` API, which this package does not cover.

## Authentication

Pass a `TokenCredential`. The token is cached until shortly before it expires, fetched once however many requests
are in flight, and refreshed once if the service answers 401.

- `Scope` defaults to `https://api.securitycenter.microsoft.com/.default`. Microsoft documents that some APIs still
  need tokens for that legacy audience, and answer 403 otherwise.
- `BaseUri` defaults to `https://api.security.microsoft.com/api`. `UseCloud(DefenderCloud.Europe)` and friends switch
  to a regional host. For the US Government clouds you must also set `Scope` to the audience your tenant uses.
- The application needs the Entra permissions of the operations you call, for example `Vulnerability.Read.All`,
  `Software.Read.All` or `Score.Read.All`.

## Paging and exports

List operations return an `ODataPage<T>`. The `GetAll...Async` helpers follow `@odata.nextLink` for you and yield
every item. A next link is refused if it points anywhere other than the configured host, so the bearer token is never
sent elsewhere.

The assessment exports come in two forms:

```csharp
// JSON by machine: paged, with a page size of up to 200,000 records.
await foreach (var item in client.GetAllSoftwareVulnerabilityAssessmentsAsync(
	new ExportByMachineOptions { PageSize = 50_000 }, cancellationToken))
{
	// ...
}

// Via files: the API returns signed download links; the records are streamed from gzip NDJSON.
var export = await client.AssessmentExports.ExportSoftwareVulnerabilitiesAsync(null, cancellationToken);
await foreach (var item in client.StreamSoftwareVulnerabilityAssessmentsAsync(export, cancellationToken))
{
	// ...
}
```

Downloads use a separate connection that never carries your bearer token, and the signature in each link is kept out
of logs and exception messages. Microsoft allows only 5 export calls a minute and 20 an hour.

## Changing things, and read-only mode

Some operations change state: isolating a device, restricting code execution, offboarding, deleting indicators,
uploading to the library, and a few more. They are implemented and unit-tested.

```csharp
await client.MachineActions.IsolateAsync(machineId,
	new IsolateRequest { Comment = "Isolated pending investigation" }, cancellationToken);
```

Set `IsReadOnly = true` and the client refuses every state-changing request before it is sent, and before any token is
fetched. The three read-only operations that Microsoft implements as POST, advanced hunting and the two
authenticated-scan history queries, are still allowed.

## Errors, retries and timeouts

A failed call throws `DefenderApiException` with the status code, the service's error code and message, the
correlation id Microsoft returns in `target`, and any `Retry-After`. The client retries 429 and transient failures,
waiting at least as long as `Retry-After` asks; it never retries sooner. Set `MaxRetries`, `RetryBaseDelay`,
`MaxRetryDelay` and `Timeout` on the options.

You can pin the service certificate with `TrustedServerCertificateThumbprint`. Certificate validation is never turned
off.

## Quality

- Every operation has a unit test pinning its exact HTTP method, path, query and body, and a test mapping a response
  shaped from Microsoft's documentation. The library has 100% line and branch coverage.
- The tests run on Windows and Linux.
- Zero compiler warnings, nullable reference types, XML documentation on every public member, Codacy grade A.

### Not yet verified against a live tenant

There is no public test instance of Defender, so **the integration tests are written but have not been run**. The
response models are built from Microsoft's documentation, which disagrees with itself in places (for example on the
maximum `sasValidHours`, the device-health path, and the casing of some request-body fields). Where it did, the
choice made is noted in the XML documentation of the member. Treat any mismatch as a bug and
[report it](https://github.com/panoramicdata/PanoramicData.Microsoft.Defender.Api/issues).

To run the integration tests against your own tenant, see [docs/TESTING.md](docs/TESTING.md). They only read data.

## Links

- NuGet: https://www.nuget.org/packages/PanoramicData.Microsoft.Defender.Api
- Source: https://github.com/panoramicdata/PanoramicData.Microsoft.Defender.Api
- Issues: https://github.com/panoramicdata/PanoramicData.Microsoft.Defender.Api/issues
- Contributing: [CONTRIBUTING.md](CONTRIBUTING.md), adding endpoints: [docs/IMPLEMENTING.md](docs/IMPLEMENTING.md)

## License

MIT. See [LICENSE](LICENSE).
