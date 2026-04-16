# HttpMock with .NET Aspire — Example

This example demonstrates how to use **HttpMock** to mock a downstream API when integration-testing a **.NET Aspire** distributed application.

## Scenario

A `WeatherApi` service exposes a `/weatherforecast` endpoint that internally calls an external weather API (`/api/weather`). In production, this would be a real third-party service. During integration testing, **HttpMock** stands in for the external dependency, returning canned responses so tests are fast, deterministic, and don't require network access.

```
┌──────────────────┐         ┌──────────────────────┐
│   Test Runner    │────────▶│     WeatherApi        │
│  (NUnit + Aspire │         │  /weatherforecast     │
│   Hosting.Testing)         │         │              │
└──────────────────┘         └─────────┼──────────────┘
                                       │  HttpClient
                                       ▼
                             ┌──────────────────────┐
                             │   HttpMock Server     │
                             │   /api/weather        │
                             │  (stubbed responses)  │
                             └──────────────────────┘
```

## Project Structure

| Project | Description |
|---------|-------------|
| `AspireExample.AppHost` | Aspire orchestrator. Registers the `WeatherApi` and wires up the external API connection string. |
| `AspireExample.WeatherApi` | Minimal API that calls a downstream weather service via `HttpClient`. The base address is read from the `ExternalWeatherApi` connection string. |
| `AspireExample.ServiceDefaults` | Shared Aspire service defaults (OpenTelemetry, health checks, resilience, service discovery). |
| `AspireExample.Tests` | NUnit integration tests using `Aspire.Hosting.Testing` and `HttpMock`. |

## How It Works

### 1. The WeatherApi reads the downstream URL from configuration

```csharp
builder.Services.AddHttpClient("ExternalWeatherApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration.GetConnectionString("ExternalWeatherApi")
        ?? "http://localhost:5200");
});
```

### 2. The AppHost registers it as a connection string reference

```csharp
var externalWeatherApi = builder.AddConnectionString("ExternalWeatherApi");

builder.AddProject<Projects.AspireExample_WeatherApi>("weatherapi")
    .WithReference(externalWeatherApi);
```

### 3. Tests create the app once and reuse it across the suite

The Aspire application and HttpMock server are created once in `[OneTimeSetUp]`.
Each test calls `WithNewContext()` to clear previous stubs and register fresh ones —
exactly the same pattern used in HttpMock's own integration tests.

```csharp
// OneTimeSetUp — runs once for the entire test fixture
var mockUrl = $"http://localhost:{FindAvailablePort()}";
_mockServer = HttpMockRepository.At(mockUrl);

var appHost = await DistributedApplicationTestingBuilder
    .CreateAsync<Projects.AspireExample_AppHost>();
appHost.Configuration["ConnectionStrings:ExternalWeatherApi"] = mockUrl;

_app = await appHost.BuildAsync();
await _app.StartAsync();
_httpClient = _app.CreateHttpClient("weatherapi");

// Each test — swap stubs without rebuilding the app
_mockServer.WithNewContext()
    .Stub(x => x.Get("/api/weather"))
    .Return(jsonPayload)
    .AsContentType("application/json")
    .OK();

var response = await _httpClient.GetAsync("/weatherforecast");
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A development environment that supports Aspire's DCP (Developer Compute Platform). This is included in the `Aspire.Hosting.AppHost` SDK NuGet package.

## Running the Example

```bash
# Build
dotnet build

# Run the tests
dotnet test
```

## Test Cases

The example includes three integration tests:

| Test | What it demonstrates |
|------|---------------------|
| `GetWeatherForecast_ReturnsMockedData` | Basic stub: HttpMock returns a JSON array, the WeatherApi forwards it to the caller. |
| `GetWeatherForecast_WhenDownstreamReturns500_ReturnsError` | Error simulation: HttpMock returns HTTP 500, verifying the WeatherApi propagates the failure. |
| `GetWeatherForecast_WithDelayedResponse_StillReturnsData` | Slow service simulation: HttpMock adds a 500ms delay via `WithDelay()`, verifying the WeatherApi still returns data. |
