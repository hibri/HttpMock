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
| `AspireExample.AppHost` | Aspire orchestrator. Registers the `WeatherApi` and an `HttpMockResource` via `AddHttpMock`. |
| `AspireExample.WeatherApi` | Minimal API that calls a downstream weather service via `HttpClient`. The base address is read from the `ExternalWeatherApi` connection string. |
| `AspireExample.ServiceDefaults` | Shared Aspire service defaults (OpenTelemetry, health checks, resilience, service discovery). |
| `AspireExample.Tests` | NUnit integration tests using `Aspire.Hosting.Testing` and `HttpMock.Aspire.Hosting`. |

## How It Works — First-Class Aspire Resource (`HttpMock.Aspire.Hosting`)

The `HttpMock.Aspire.Hosting` package lets you register an HttpMock stub server as a proper Aspire resource. Aspire starts the server automatically, shows it on the dashboard, and injects its URL into dependent projects — no manual port allocation required.

### 1. The AppHost registers the mock as a first-class resource

```csharp
using HttpMock.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var externalWeatherApi = builder.AddHttpMock("ExternalWeatherApi");

builder.AddProject<Projects.AspireExample_WeatherApi>("weatherapi")
    .WithReference(externalWeatherApi);

builder.Build().Run();
```

`WithReference` automatically injects `ConnectionStrings__ExternalWeatherApi = http://localhost:<port>` into the WeatherApi process — exactly what it reads via `GetConnectionString("ExternalWeatherApi")`.

### 2. The WeatherApi reads the downstream URL from configuration (unchanged)

```csharp
builder.Services.AddHttpClient("ExternalWeatherApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration.GetConnectionString("ExternalWeatherApi")
        ?? "http://localhost:5200");
});
```

### 3. Tests configure stubs via the `MockServer` property

```csharp
// OneTimeSetUp — get the resource that Aspire already started
var mockResource = _app.Services
    .GetRequiredService<DistributedApplicationModel>()
    .Resources
    .OfType<HttpMockResource>()
    .Single(r => r.Name == "ExternalWeatherApi");

_mockServer = mockResource.MockServer!;

// Each test — swap stubs without rebuilding the app
_mockServer.WithNewContext()
    .Stub(x => x.Get("/api/weather"))
    .Return(jsonPayload)
    .AsContentType("application/json")
    .OK();

var response = await _httpClient.GetAsync("/weatherforecast");
```

---

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
