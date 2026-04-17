var builder = DistributedApplication.CreateBuilder(args);

// In production, this would be a real external weather API.
// In tests, we replace it with an HttpMock server using a connection string override.
var externalWeatherApi = builder.AddConnectionString("ExternalWeatherApi");

var weatherApi = builder.AddProject<Projects.AspireExample_WeatherApi>("weatherapi")
    .WithReference(externalWeatherApi);

builder.Build().Run();
