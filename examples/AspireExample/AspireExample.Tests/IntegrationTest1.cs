using System.Text.Json;
using HttpMock;
using Microsoft.Extensions.Logging;

namespace AspireExample.Tests;

/// <summary>
/// Demonstrates how to use HttpMock with .NET Aspire to mock a downstream API
/// during integration testing. The WeatherApi service calls an external weather
/// API; these tests replace that external dependency with an HttpMock stub server.
/// </summary>
public class WeatherApiTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    [Test]
    public async Task GetWeatherForecast_ReturnsMockedData()
    {
        // Arrange – start an HttpMock server to act as the downstream weather API
        var mockPort = FindAvailablePort();
        var mockUrl = $"http://localhost:{mockPort}";
        using var mockServer = HttpMockRepository.At(mockUrl);

        var mockedResponse = JsonSerializer.Serialize(new[]
        {
            new { Date = "2026-04-17", TemperatureC = 22, Summary = "Warm" },
            new { Date = "2026-04-18", TemperatureC = 15, Summary = "Cool" }
        });

        mockServer.Stub(x => x.Get("/api/weather"))
            .Return(mockedResponse)
            .AsContentType("application/json")
            .OK();

        // Build the Aspire app host, overriding the downstream API connection string
        // to point at the HttpMock server instead of a real external service.
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AspireExample_AppHost>(cancellationToken);

        // Override the ExternalWeatherApi connection string to point to our mock
        appHost.Configuration["ConnectionStrings:ExternalWeatherApi"] = mockUrl;

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Act – call the WeatherApi endpoint which internally calls the mock
        using var httpClient = app.CreateHttpClient("weatherapi");
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("weatherapi", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var response = await httpClient.GetAsync("/weatherforecast", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
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
        var mockPort = FindAvailablePort();
        var mockUrl = $"http://localhost:{mockPort}";
        using var mockServer = HttpMockRepository.At(mockUrl);

        mockServer.Stub(x => x.Get("/api/weather"))
            .Return("Internal Server Error")
            .WithStatus(HttpStatusCode.InternalServerError);

        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AspireExample_AppHost>(cancellationToken);

        appHost.Configuration["ConnectionStrings:ExternalWeatherApi"] = mockUrl;

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("weatherapi");
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("weatherapi", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var response = await httpClient.GetAsync("/weatherforecast", cancellationToken);

        // Assert – the WeatherApi should propagate the failure
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetWeatherForecast_WithDelayedResponse_StillReturnsData()
    {
        // Arrange – simulate a slow downstream service using HttpMock's WithDelay
        var mockPort = FindAvailablePort();
        var mockUrl = $"http://localhost:{mockPort}";
        using var mockServer = HttpMockRepository.At(mockUrl);

        var mockedResponse = JsonSerializer.Serialize(new[]
        {
            new { Date = "2026-04-17", TemperatureC = 30, Summary = "Hot" }
        });

        mockServer.Stub(x => x.Get("/api/weather"))
            .Return(mockedResponse)
            .AsContentType("application/json")
            .WithDelay(500)
            .OK();

        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AspireExample_AppHost>(cancellationToken);

        appHost.Configuration["ConnectionStrings:ExternalWeatherApi"] = mockUrl;

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("weatherapi");
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("weatherapi", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var response = await httpClient.GetAsync("/weatherforecast", cancellationToken);

        // Assert – the response should still come through despite the delay
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(forecasts, Is.Not.Null);
        Assert.That(forecasts!.Length, Is.EqualTo(1));
        Assert.That(forecasts[0].GetProperty("summary").GetString(), Is.EqualTo("Hot"));
    }

    private static int FindAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(
            System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
