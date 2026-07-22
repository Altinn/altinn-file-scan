# AGENTS.md

This file provides guidance to AI agents when working with code.

Altinn FileScan is the platform component that performs asynchronous malware scanning of files (data elements) uploaded to Altinn 3 apps. It picks up scan requests from a queue, streams the file through a ClamAV-based scanner, and reports the result back to Altinn Storage.

## Backend Stack

- .NET 10 throughout
- ASP.NET Core Web API (`Altinn.FileScan`) — the scanning REST API
- Azure Functions isolated worker, v4 (`Altinn.FileScan.Functions`) — the queue trigger that feeds the API
- Azure Blob Storage for reading app-owner file payloads; Azure Storage Queues (Azurite locally) for the inbound queue
- [Muescheli](https://github.com/Altinn/muescheli) — a ClamAV wrapper the API calls over HTTP to do the actual scan
- Azure Key Vault (`DefaultAzureCredential`) for certificates/secrets; client-certificate-based platform access tokens via `Altinn.Common.AccessToken`/`AccessTokenClient`
- JWT cookie authentication (`AltinnCore.Authentication.JwtCookie`) with an `AccessTokenRequirement` authorization handler
- OpenTelemetry with an Azure Monitor exporter (API) and Application Insights (Functions)
- xUnit + Moq for tests; `WebApplicationFactory` for controller integration tests
- StyleCop analyzers (enabled in Debug); Swashbuckle/Swagger for API docs

## Project Structure

Solution file: `Altinn.FileScan.sln` (classic format — use the `dotnet` CLI or Visual Studio). Two independently deployable applications:

### `src/Altinn.FileScan/` — the scanning Web API
- `Controllers/` — `DataElementController`, the single endpoint `POST filescan/api/v1/dataelement` (guarded by the `PlatformAccess` policy)
- `Services/` — business logic. `DataElementService` orchestrates the whole scan flow. Also token/Key Vault services (`AccessTokenService`, `AppOwnerKeyVaultService`, `PlatformKeyVaultService`)
- `Clients/` — HTTP clients to external services: `MuescheliClient` (the scanner) and `StorageClient` (Altinn Storage callback)
- `Repository/` — `AppOwnerBlobRepository` / `BlobContainerClientProvider` for reading blobs from the app owner's storage account
- `Models/`, `Configuration/`, `Exceptions/`, `Health/`, `Telemetry/`, `Extensions/`
- `Program.cs` — top-level statements; all DI, auth, telemetry, and Swagger wiring lives here

### `src/Altinn.FileScan.Functions/` — the queue-trigger worker
- `FileScanInbound.cs` — `[QueueTrigger("file-scan-inbound")]` function; forwards each message to the API via `FileScanClient`
- `Clients/FileScanClient.cs` — posts to the API, authenticating with a platform access token minted from a client certificate
- `Services/` — `CertificateResolverService`, `KeyVaultService` (certificate resolution for token signing)
- `Program.cs` — isolated-worker `HostBuilder` with all DI wiring
- `host.json` / `local.settings.json` — Functions host config (not committed for secrets)

### `test/`
- `test/Altinn.FileScan.Tests/` — API tests: `TestingControllers/`, `TestingServices/`, `TestingClients/`, plus `Mocks/` (auth stubs, JWT token mock) and `Utils/PrincipalUtil.cs`
- `test/Altinn.FileScan.Functions.Tests/` — Functions tests (`TestingServices/`)

### End-to-end flow
Storage enqueues a scan request → **Functions** `FileScanInbound` dequeues it and `FileScanClient` POSTs it to the **API** → `DataElementController` → `DataElementService.Scan`:
1. read blob properties from the app-owner blob store (`AppOwnerBlobRepository`);
2. abort if the blob's last-modified timestamp doesn't match the request (or skip if neither blob nor data element exists);
3. stream the blob to Muescheli (`MuescheliClient.ScanStream`);
4. map `ScanResult` → `FileScanResult` and PATCH the status back to Storage (`StorageClient.PatchFileScanStatus`).

## Development Commands

### Build & Run
- `dotnet build Altinn.FileScan.sln` — build the whole solution
- `dotnet run --project src/Altinn.FileScan` — run the API at http://localhost:5200 (swagger at `/filescan/swagger`)
- `func start` in `src/Altinn.FileScan.Functions` — run the Functions host locally (requires [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) and a running [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for the queue)
- Running an actual scan locally also needs Muescheli running (see its repo README)

### Testing
- `dotnet test Altinn.FileScan.sln` — run all tests
- `dotnet test test/Altinn.FileScan.Tests/Altinn.FileScan.Tests.csproj` — run just the API tests
- `dotnet test --filter "FullyQualifiedName~DataElementControllerTests"` — run a single test class/method

CI (`.github/workflows/build-and-analyze.yml`) runs `dotnet build` + `dotnet test` in Release with coverage and SonarCloud analysis on pushes to `main` and PRs touching `src/**` or the test projects.

## Coding Guidelines

- **StyleCop is active in Debug builds** and most `SA*` rules are at `warning` (some, like `SA0001`, at `error`) in `.editorconfig`. Keep code warning-clean. `using` directives go **outside** the namespace.
- `GenerateDocumentationFile` is `true` in both apps, so **public members need XML doc comments** (`/// <summary>`), matching the existing style. Tests document scenarios in `/// <summary>` blocks (Scenario / Expected result / Success criteria).
- Both projects have `<Nullable>enable</Nullable>`, but most existing files start with `#nullable disable`. Match the file you are editing rather than flipping annotations piecemeal.
- Prefer the existing idioms: file-scoped namespaces, **primary constructors** (see `DataElementController`, `DataElementService`, `FileScanInbound`), expression-bodied members.
- **Log-injection guard:** when logging any request-controlled string, strip newlines first — `value.Replace(Environment.NewLine, string.Empty)` — as `DataElementService` does consistently.
- Register services and configuration bindings in the respective `Program.cs`; the API binds settings via `services.Configure<T>(config.GetSection(...))`, the Functions worker via `AddOptions<T>().Configure<IConfiguration>(...)`.
- External calls go through typed `HttpClient` clients (`AddHttpClient<TInterface, TImpl>`) behind an interface in `Clients/Interfaces/`; throw the dedicated exception types (`MuescheliHttpException`, `MuescheliScanResultException`, `PlatformHttpException`) on failures.

## Testing Guidelines

- Tests use **xUnit** with **Moq**; follow the Arrange / Act / Assert layout and mirror the source folder structure.
- API controller tests are integration tests over `WebApplicationFactory<T>`; authentication is faked via `JwtCookiePostConfigureOptionsStub` and `PublicSigningKeyProviderMock`, and tokens are minted with `PrincipalUtil` / `JwtTokenMock`. No external services or Azurite are needed — dependencies are mocked and injected through `ConfigureTestServices`.
- Mock external dependencies (blob repository, HTTP clients) with `Moq` rather than hitting real infrastructure.
