using HttpMock.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var externalWeatherApi = builder.AddHttpMock("ExternalWeatherApi");

builder.AddProject<Projects.AspireExample_WeatherApi>("weatherapi")
    .WithReference(externalWeatherApi);

builder.Build().Run();
