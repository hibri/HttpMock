using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register an HttpClient for the downstream weather service.
// The base address is configured via the "ExternalWeatherApi" connection string
// which Aspire service discovery resolves automatically.
builder.Services.AddHttpClient("ExternalWeatherApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("ExternalWeatherApi")
                                 ?? "http://localhost:5200");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// This endpoint calls the downstream "external" weather API and returns its response.
// In production this would be a real third-party service; in tests we mock it with HttpMock.
app.MapGet("/weatherforecast", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("ExternalWeatherApi");
    var response = await client.GetAsync("/api/weather");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    var forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    return Results.Ok(forecasts);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make the implicit Program class accessible to the test project
public partial class Program { }
