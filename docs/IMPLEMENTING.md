# Implementing endpoints

This is the contract for adding Microsoft Defender for Endpoint REST operations to PanoramicData.Microsoft.Defender.Api.
The **vulnerabilities** group is the worked example; copy its shape:

| Part | File |
|---|---|
| Interface | `PanoramicData.Microsoft.Defender.Api/Interfaces/IVulnerabilitiesApi.cs` |
| Client property and high-level helpers | `PanoramicData.Microsoft.Defender.Api/DefenderClient.Vulnerabilities.cs` |
| Models | `PanoramicData.Microsoft.Defender.Api/Models/Vulnerabilities/*.cs` |
| Shared OData options and page | `Models/ODataListOptions.cs`, `Models/ODataPage.cs` |
| Unit tests | `PanoramicData.Microsoft.Defender.Api.Test/Groups/VulnerabilitiesTests.cs` |
| Paging tests (core, do not copy) | `PanoramicData.Microsoft.Defender.Api.Test/Core/PagingTests.cs` |
| Integration tests | `PanoramicData.Microsoft.Defender.Api.IntegrationTest/Vulnerabilities/VulnerabilitiesIntegrationTests.cs` |
| Endpoint rows | `docs/endpoints/vulnerabilities.md` |

The namespace root is `PanoramicData.MicrosoftDefender.Api` (the package id is `PanoramicData.Microsoft.Defender.Api`; a
namespace with a bare `Microsoft` segment would break every `Microsoft.Extensions.*` reference). Never name a namespace
segment `System`.

## Source of truth, and the own-words rule

- `docs/endpoints/<category>.md` lists the 111 operations of the v1.0 API, one row per path and method. The factual
  inventory (paths, parameters, permissions, limits) is `temp/defender-endpoint-inventory.md` (git-ignored: not in a
  fresh clone). Entity property tables are only partly transcribed there, so re-read the operation's Learn page at
  `https://learn.microsoft.com/en-us/defender-endpoint/api/<page>` for the property names and types.
- **Microsoft's terms forbid copying its prose.** Write every XML doc comment, `<remarks>` and test description in your
  own words, from the facts: path, verb, parameter names, enum members, limits. Never paste or closely paraphrase Learn
  text, and never commit a scraped page (keep downloads under your scratch folder). Example JSON in tests is
  invented, shaped like the response (the same property names and types) with neutral values such as `CVE-2024-0001`.
- Where the documentation is wrong or inconsistent (it has several path and permission typos), follow what the service
  does, and say so in the method's XML `<remarks>`. Mark it "unverified" until a live test confirms it.
- When an operation is implemented, fill its row: `Client method` = `IInterface.Method`, `Test` = `TestClass.TestMethod` of
  the unit test that pins its request. `InventoryTests` checks that each named method exists and that its verb and path
  match the row (placeholder names are ignored and paths are compared ignoring case, because the documentation's casing
  is inconsistent), that each test exists, and that every Refit method appears in a row. Rows whose path ends in
  `(deprecated)` are not implemented.
- A category leaves `docs/pending-categories.txt` once every non-deprecated row is filled; from then on `InventoryTests`
  enforces it. Delete the category's line in the same change.

## Layout and naming

- **Interfaces:** `Interfaces/I<Group>Api.cs`, namespace `PanoramicData.MicrosoftDefender.Api.Interfaces`, one interface per
  category (`IAlertsApi`, `IMachinesApi`, `IMachineActionsApi`, `IVulnerabilitiesApi`...). A very large category may split
  into several, with unique names. Every interface is public with only public members (callers must be able to mock it).
- **Client properties:** one partial file per category, `DefenderClient.<Category>.cs`, with one lazy property per
  interface: `public IVulnerabilitiesApi Vulnerabilities => field ??= For<IVulnerabilitiesApi>();` and an XML summary.
  Property names must be unique.
- **Models:** `Models/<Category>/`, namespace `PanoramicData.MicrosoftDefender.Api.Models.<Category>`, one public type per
  file. Category folder names are the PascalCase of the endpoint file (`Alerts`, `Machines`, `MachineActions`,
  `AdvancedHunting`, `AssessmentExports`...). Shared types live in `Models/` and `Exports/` and are listed below; reuse
  them, and say in your report if you must change one.
- Do not edit files outside your category (the core: `DefenderClient.cs`, `DefenderClient.Paging.cs`,
  `DefenderClient.Exports.cs`, `Handlers/`, `Serialization/`) without saying so in your report.

## Interface methods

- Refit attribute paths are **relative, with no leading slash and no `/api` prefix**: `[Get("vulnerabilities")]`. Copy the
  row's casing exactly (`DeviceAuthenticatedScanDefinitions`, `Machines/BrowserExtensionsInventoryByMachine`). The base
  address supplies `/api`, a regional or government host, and any proxy prefix.
- Every method returns `Task`/`Task<T>`, takes a **required** `CancellationToken cancellationToken` last, and has **no
  optional parameters** (Sonar S2360; `NoOptionalParametersTests` enforces it). Optional query parameters travel in one
  nullable options object (`[Query] ODataListOptions? options`; callers pass `null`).
- Path parameters are escaped as one segment (`a/b` becomes `a%2Fb`); name C# parameters in camelCase and use the C#
  name in the braces (`vulnerabilities/{cveId}`). Never rebuild URLs with `UriBuilder` setters (they double-escape `%`).
  Path segments that are not simple (`machines/find(timestamp={time},key={IP})`) are written exactly as documented.
- Names: `ListAsync`, `GetAsync`, `ListXAsync` for a sub-collection, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, or the
  operation's own verb (`IsolateAsync`) for actions.
- **Collections** return `Task<ODataPage<T>>`. **Single entities** return the model. **Deletes/204** return `Task`.
  **Actions** return the created entity (machine actions answer 201 with the action). **Download links**
  (`getPackageUri`) return a small model with `Value`. Export operations are described next.
- XML documentation on every public member, in your own words: what it does, the verb and path in `<c>...</c>`, the
  `$filter` properties and `$top` maximum, the application permission, rate limits, and anything surprising (404 for an
  empty list, prerelease notes).

## Query options (`$filter`, `$top`, `$skip`, `$expand`, `pageSize`, `sinceTime`, `sasValidHours`)

- Put options in a class under `[Query]` with an explicit wire name on every property: `[AliasAs("$filter")]`. Reuse
  `ODataListOptions` (`$filter`, `$top`, `$skip`) when an operation takes exactly those; derive from it to add more
  (`AlertListOptions : ODataListOptions` with `[AliasAs("$expand")] Expand`). The by-machine exports use
  `ExportByMachineOptions` and the via-files exports `ExportViaFilesOptions`.
- Refit escapes query names, so it writes `%24filter`; `Handlers/ODataQueryHandler.cs` restores a literal `$` in names
  only. `RequestEncodingTests` pins this, and pins the escaping of quotes, spaces and `$` inside a value. Tests of
  your operation compare the **raw** `Uri.Query` (never `Uri.UnescapeDataString`, which would hide an escaping bug).
- A `$filter` value is sent as given: callers write OData (`severity eq 'High'`) and double any apostrophe.
- Booleans are `true`/`false`, enums their wire name, dates UTC ISO 8601 (`DefenderUrlParameterFormatter`).

## Paging (the shape every list operation follows)

Collections are OData envelopes `{"@odata.context", "value": [...], "@odata.nextLink"}`, read as `ODataPage<T>`. The
**next link is an absolute URL**, so it cannot go back through Refit. Two methods on `DefenderClient` (not on the
interfaces) do the paging:

- `GetNextPageAsync<T>(ODataPage<T> page, ct)` sends an authenticated GET to the page's `NextLink` as given and returns
  the next page, or `null` on the last page. **It refuses (throws `DefenderLinkException`, before sending anything) a link
  whose scheme, host or port differs from the configured base address**, so the bearer token is never sent anywhere else.
- `GetAllAsync<T>(Func<CancellationToken, Task<ODataPage<T>>> firstPage, ct)` is the `IAsyncEnumerable<T>` over every page.

For each list operation add a **high-level helper to the client's category partial**, as in
`DefenderClient.Vulnerabilities.cs`:

```csharp
public IAsyncEnumerable<Vulnerability> GetAllVulnerabilitiesAsync(ODataListOptions? options, CancellationToken cancellationToken)
	=> GetAllAsync(token => Vulnerabilities.ListAsync(options, token), cancellationToken);
```

Do not put helpers on the Refit interface (default interface methods are fragile with Refit). Test one helper per
group for paging; the core paging behaviour is already pinned in `Core/PagingTests.cs`.

Many list endpoints answer **404 when there is nothing to list** rather than an empty page: note it in the interface
remarks, do not hide it.

## Exports "via files" and "by machine"

- **By machine** (`...ByMachine`) return a paged JSON collection: `ODataPage<T>` with `[Query] ExportByMachineOptions?`.
- **Via files** (`...Export`, and `Machines/InfoGatheringExport`) return `Exports/ExportFilesResponse` (it reads both
  `exportFiles` and `Export files`) with `[Query] ExportViaFilesOptions?`. The records are read with
  `DefenderClient.StreamExportAsync<T>(response, ct)`, which downloads each signed link with a **separate HTTP client that
  never sends the bearer token**, over https only, and yields the gzip multi-line JSON records as `IAsyncEnumerable<T>`.
- A signed link's query string is a credential. Never log it, put it in an exception, `ToString` or a test failure
  message, and never write a model whose generated `ToString` prints one (override it, as `ExportFilesResponse` does).
- Add a record model per export under `Models/AssessmentExports/` (or the owning category) and a high-level helper
  `Stream<Thing>Async(ExportFilesResponse, ct)` only where it adds clarity; the generic method already works.
- Rate limits are tight (5 calls a minute, 20 an hour for the file exports): never loop over these in tests.

## Request and response models

- Response models are classes with settable properties; **every property has `[JsonPropertyName("wire")]`**. Responses are
  camelCase, but **request-body property names are case-sensitive** on the server (`Comment`, `IsolationType`,
  `MachineIds`...), so request models also name every property explicitly, exactly as documented, and never rely on a
  naming policy.
- Request models: required members use the C# `required` modifier, optional ones are nullable and omitted when `null`.
  Bodies are sent as buffered JSON (`DefenderContentSerializer`), so they can be replayed after a 401 or a retry.
- Reading is tolerant through `DefenderJson.Options`; do not add converters for these: numbers and booleans as strings
  (`"7.5"`, `"true"`), empty strings, JSON `null`, ISO timestamps (`DateTimeOffset?`), and an unrecognised enum name
  reading as `Unknown`. Use `int`/`long`/`double` (nullable where the service may omit them), `bool?`, `string?`,
  `DateTimeOffset?`, and `IReadOnlyList<T>` defaulting to `[]` for lists (a JSON `null` keeps the `[]`).
- Enums only for small closed sets: first member `Unknown = 0`, the rest with `[JsonStringEnumMemberName("Wire")]`. Prefer
  `string` where the service's set is open, version-dependent or inconsistently documented.
- Keep each file small (Codacy grades a file down past about 40 total complexity or 300 lines).

## Tests (PanoramicData.Microsoft.Defender.Api.Test, xUnit v3 + AwesomeAssertions)

- One test class per interface, `Groups/<Group>Tests.cs` (split into partial files when it grows), using `TestClient.Create(stub)`,
  `TestClient.Stub(json)`, `TestClient.CaptureAsync`, `TestClient.ReadAsync` and `TestClient.ShouldFailAsync` from `Support/`.
- **Every operation** has (1) a test asserting the exact method, **escaped path**, **raw query** and JSON body with
  `call.ShouldBeRequest(method, path, query, body)` (paths start with `/api/`), (2) a test that maps a realistic response and
  asserts every modelled field, (3) a tolerance case where the model has enums, numbers or dates, and (4) an error-path test
  (non-success status raises `DefenderApiException` with the code, message and correlation id).
- Operations that take options test each option's wire name and the escaping of a value with quotes and spaces.
- Operations that change state also test the replay (`ReplayTests` pattern): the body is identical after a 401.
- No skipped tests (`failSkips` is on), no `Thread.Sleep`/real delays, no network, no real credentials. Time comes from
  `FakeTimeProvider`; credentials from `FakeCredential`. Do not define an extension method named `ShouldBe` on
  `RecordedCall` (it collides with the assertion library): the helper is `ShouldBeRequest`.
- Avoid copy-pasted bodies (shared JSON in constants, repeated assertions in a helper) so Codacy finds no duplication.
- **Line and branch coverage of what you add must be 100%**, measured as in `docs/TESTING.md`.

## Live verification (PanoramicData.Microsoft.Defender.Api.IntegrationTest)

- Live verification is **pending**: there is no tenant yet. Write the tests anyway, so they run as soon as one exists.
- Use `[Collection(DefenderTestGroup.Name)]` and `DefenderFixture`, which reads `Defender:TenantId`, `Defender:ClientId` and
  `Defender:ClientSecret` from user secrets or the environment and **fails loudly naming the missing key** (never skips).
  Its client is `IsReadOnly = true`. **Never print or commit a secret.**
- Read operations: assert the shape (non-empty ids, expected enum values), a filter, paging through `GetAll...Async`, and an
  error (an unknown id answers 404 and raises `DefenderApiException`).
- **Safety: write and side-effect operations are implemented and unit-tested, and NEVER run live.** That is every
  `POST`, `PATCH` and `DELETE` except the three read-only POSTs. In particular never call, even against a test tenant:
  machine actions (`isolate`, `unisolate`, `offboard`, `restrictCodeExecution`, `unrestrictCodeExecution`,
  `StopAndQuarantineFile`, `runAntiVirusScan`, `collectInvestigationPackage`, `runliveresponse`), `startInvestigation`,
  indicator creation, import and deletes (`BatchDelete`), library upload and delete, alert updates and
  `CreateAlertByReference`, machine tag and device-value changes, and the authenticated-scan create, update and delete. Their
  integration tests, if any, assert only what a read-only client does (`DefenderReadOnlyException`) or are omitted with a
  note in the endpoint row. Record "unit-verified only" in your report for each.
- The three read-only POSTs are `advancedqueries/run` and the two authenticated-scan history queries
  (`DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId` and `.../GetScanHistoryBySessionId`). They are the
  only POSTs `Handlers/ReadOnlyHandler.cs` allows; if an operation you add also only reads, extend that list **and** its
  tests, and say so.
- Mind the rate limits (docs/TESTING.md); run tests serially and keep the number of calls small.

## Code quality

- Zero warnings (`TreatWarningsAsErrors`), XML docs on all public API, file-scoped namespaces, tabs, CRLF for `.cs`. Files
  written by tools are LF: run `dotnet format whitespace <project>` before building. `dotnet format` does not touch Markdown or
  JSON.
- Build only the projects you touched: `dotnet build PanoramicData.Microsoft.Defender.Api.Test/PanoramicData.Microsoft.Defender.Api.Test.csproj`, then
  `dotnet test --project PanoramicData.Microsoft.Defender.Api.Test/PanoramicData.Microsoft.Defender.Api.Test.csproj --no-build`
  (exit code 0 = all passed). Do not build the whole solution. If another process (a dev server, another agent) locks the
  build, say so.
- A token must never appear in a log, exception message or `ToString`. Note that on a 401 the client fetches a token again
  once; an Azure.Identity credential caches, so it may return the same token and the second 401 is returned to the caller.
- Commit in small logical commits with a clear message. Never add `Co-Authored-By` trailers or any AI attribution.
