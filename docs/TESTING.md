# Testing

## Status of live verification

**Live verification is pending.** No Microsoft Defender for Endpoint tenant was available while the client core and the
vulnerabilities group were written, so nothing here has been run against the real service. Every request shape is pinned
by unit tests built from the documented interface, and the integration tests exist and compile, but they have never run.
Treat response models as unverified until the integration tests have passed against a tenant, and record the result in
the relevant `docs/endpoints/*.md` rows.

## Unit tests

`PanoramicData.Microsoft.Defender.Api.Test` needs no network and no credentials, and runs in CI on every push and pull
request. `global.json` selects the Microsoft.Testing.Platform runner, so run it with `dotnet test --project`:

```shell
dotnet test --project PanoramicData.Microsoft.Defender.Api.Test/PanoramicData.Microsoft.Defender.Api.Test.csproj
```

Read the exit code: 0 means every test passed, 2 means at least one failed, and 5 means the runner could not start.
Skipped tests fail the run (`failSkips`), and nothing sleeps: waits, token expiry and timeouts run on an injected
`TimeProvider` (`FakeTimeProvider`).

Every operation has a test that pins its exact HTTP method, path, query string and body, and one that maps a realistic
response. `InventoryTests` keeps `docs/endpoints/*.md` (111 operations) and the Refit interfaces in step.

### Coverage

Coverage as CI collects it (build first, then run the test executable with the estate settings):

```shell
dotnet build PanoramicData.Microsoft.Defender.Api.Test/PanoramicData.Microsoft.Defender.Api.Test.csproj
./PanoramicData.Microsoft.Defender.Api.Test/bin/Debug/net11.0/PanoramicData.Microsoft.Defender.Api.Test --coverage --coverage-settings "$PWD/coverage.config" --coverage-output-format cobertura --coverage-output coverage.cobertura.xml
```

(`--coverage-settings` must be an absolute path.) The target is 100% of lines and branches of the library.
`coverage.config` excludes compiler-generated code, which also hides the bodies of async methods. Occasionally copy it
without the `CompilerGeneratedAttribute` line and run once more: only compiler-generated iterator branches (the dispose
paths of `await foreach`) may remain uncovered.

## Integration tests

`PanoramicData.Microsoft.Defender.Api.IntegrationTest` runs against a real tenant, using the client's `IsReadOnly` guard
unless a test needs otherwise. Nothing is skipped: a missing setting fails the tests with a message naming it.

### Create an Entra application

1. In the Entra admin center, register an application in a tenant with Defender for Endpoint (a test tenant, not
   production). Create a client secret for it.
2. Under API permissions, add **APIs my organization uses > WindowsDefenderATP** (the Microsoft Threat Protection /
   "WindowsDefenderATP" resource), choose **Application permissions**, and grant admin consent for:
   - `Vulnerability.Read.All` (vulnerabilities, assessment exports),
   - `Software.Read.All` (software, browser extensions),
   - `Score.Read.All` (exposure and configuration scores),
   - `Machine.Read.All` (machines, authenticated scans),
   - `Alert.ReadWrite.All` (the documented permission that also reads alerts and investigations; there is no read-only one),
   - `SecurityRecommendation.Read.All`, `RemediationTasks.Read.All`, `SecurityBaselinesAssessment.Read.All`,
   - `User.Read.All`, `File.Read.All`, `Ip.Read.All`, `URL.Read.All`,
   - `AdvancedQuery.Read.All` (advanced hunting; Microsoft is retiring that API in favour of Microsoft Graph).

   The vulnerabilities tests need only `Vulnerability.Read.All`. Grant the write permissions (`Machine.ReadWrite.All`,
   `Machine.Isolate`, `Ti.ReadWrite.All` and so on) only if you deliberately run write tests, and see the safety rules in
   `docs/IMPLEMENTING.md` first.

### Set the user secrets

Run these from the repository root. Each command prompts for nothing and echoes nothing; substitute your own values, and
never commit them or paste them into an issue:

```shell
dotnet user-secrets set "Defender:TenantId" "<directory (tenant) id>" --project PanoramicData.Microsoft.Defender.Api.IntegrationTest
dotnet user-secrets set "Defender:ClientId" "<application (client) id>" --project PanoramicData.Microsoft.Defender.Api.IntegrationTest
dotnet user-secrets set "Defender:ClientSecret" "<client secret value>" --project PanoramicData.Microsoft.Defender.Api.IntegrationTest
```

Optional settings:

| Key | Purpose |
|---|---|
| `Defender:Cloud` | A `DefenderCloud` name (`Europe`, `UnitedKingdom`, `UsGovernmentGcc`...) to use that host instead of the global one. |
| `Defender:BaseUri` | An explicit base address, with the `/api` prefix. Overrides `Defender:Cloud`. |
| `Defender:Scope` | The token scope. Defaults to `https://api.securitycenter.microsoft.com/.default`. The US Government clouds need the scope for their own audience: set it when you set a government cloud. |

The same keys work as environment variables with double underscores, for example `Defender__TenantId`.

### Run them

```shell
dotnet test --project PanoramicData.Microsoft.Defender.Api.IntegrationTest/PanoramicData.Microsoft.Defender.Api.IntegrationTest.csproj
```

The tests need data to exist (for example at least one onboarded device with an exposed vulnerability); where it does
not, they fail with a message saying what is missing rather than skipping. Several list operations answer 404 instead of
an empty page when there is nothing to list.

CI does not run the integration tests: it has no tenant.

### Rate limits

Most operations allow 100 calls a minute; the "via files" exports allow 5 calls a minute and 20 an hour, the "by machine"
exports 30 a minute, and advanced hunting 45 a minute. The client honours `Retry-After`, but write live tests that stay
well inside the limits, and run them serially.
