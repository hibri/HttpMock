using System.Text.Json;
using Aspire.Hosting;

namespace AspireExample.Tests;

/// <summary>
/// Demonstrates how to use the HttpMock Aspire resource to mock a downstream API
/// during integration testing. The WeatherApi service calls an external weather
/// API; these tests replace that external dependency with an HttpMock stub server
/// registered via <see cref="HttpMockResourceBuilderExtensions.AddHttpMock"/>.
///
/// The Aspire application is created once for the entire test suite in
/// <see cref="OneTimeSetUp"/>. Each test calls
/// <see cref="IHttpServer.WithNewContext"/> to clear previous stubs and register
/// fresh ones, following the same pattern used in the main HttpMock integration tests.
/// </summary>
public class WeatherApiTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    private DistributedApplication _app = null!;
    private IHttpServer _mockServer = null!;
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AspireExample_AppHost>(cancellationToken);

        _app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await _app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Retrieve the HttpMock resource that was started by the Aspire lifecycle.
        var mockResource = _app.Services
            .GetRequiredService<DistributedApplicationModel>()
            .Resources
            .OfType<HttpMockResource>()
            .Single(r => r.Name == "ExternalWeatherApi");

        _mockServer = mockResource.MockServer!;

        _httpClient = _app.CreateHttpClient("weatherapi");

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("weatherapi", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _httpClient?.Dispose();

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    [Test]
    public async Task GetWeatherForecast_ReturnsMockedData()
    {
        // Arrange – clear previous stubs and register a new one
        var mockedResponse = JsonSerializer.Serialize(new[]
        {
            new { Date = "2026-04-17", TemperatureC = 22, Summary = "Warm" },
            new { Date = "2026-04-18", TemperatureC = 15, Summary = "Cool" }
        });

        _mockServer.WithNewContext()
            .Stub(x => x.Get("/api/weather"))
            .Return(mockedResponse)
            .AsContentType("application/json")
            .OK();

        // Act
        using var response = await _httpClient.GetAsync("/weatherforecast");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(forecasts, Is.Not.Null);
        Assert.That(forecasts!.Length, Is.EqualTo(2));
        Assert.That(forecasts[0].GetProperty("summary").GetString(), Is.EqualTo("Warm"));
        Assert.That(forecasts[1].GetProperty("summary").GetString(), Is.EqualTo("Cool"));
    }

    [Test]
    public async Task GetWeatherForecast_WhenDownstreamReturns500_ReturnsError()
    {
        // Arrange – HttpMock returns a 500 to simulate a downstream failure
        _mockServer.WithNewContext()
            .Stub(x => x.Get("/api/weather"))
            .Return("Internal Server Error")
            .WithStatus(HttpStatusCode.InternalServerError);

        // Act
        using var response = await _httpClient.GetAsync("/weatherforecast");

        // Assert – the WeatherApi should propagate the failure
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetWeatherForecast_WithDelayedResponse_StillReturnsData()
    {
        // Arrange – simulate a slow downstream service using HttpMock's WithDelay
        var mockedResponse = JsonSerializer.Serialize(new[]
        {
            new { Date = "2026-04-17", TemperatureC = 30, Summary = "Hot" }
        });

        _mockServer.WithNewContext()
            .Stub(x => x.Get("/api/weather"))
            .Return(mockedResponse)
            .AsContentType("application/json")
            .WithDelay(500)
            .OK();

        // Act
        using var response = await _httpClient.GetAsync("/weatherforecast");

        // Assert – the response should still come through despite the delay
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(forecasts, Is.Not.Null);
        Assert.That(forecasts!.Length, Is.EqualTo(1));
        Assert.That(forecasts[0].GetProperty("summary").GetString(), Is.EqualTo("Hot"));
    }
}
