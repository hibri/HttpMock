using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

namespace HttpMock.Aspire.Hosting;

/// <summary>
/// Fluent extension methods for registering <see cref="HttpMockResource"/> instances in a
/// .NET Aspire app host.
/// </summary>
public static class HttpMockResourceBuilderExtensions
{
    /// <summary>
    /// Adds an HttpMock stub server as a first-class Aspire resource.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">
    /// The logical name of the resource. Aspire uses this name to inject the mock server's URL
    /// into dependent projects as <c>ConnectionStrings__&lt;name&gt;</c>.
    /// </param>
    /// <returns>An <see cref="IResourceBuilder{HttpMockResource}"/> for further configuration.</returns>
    /// <example>
    /// <code>
    /// // App host
    /// var mockApi = builder.AddHttpMock("external-weather-api");
    /// builder.AddProject&lt;Projects.WeatherApi&gt;("weatherapi")
    ///        .WithReference(mockApi);
    ///
    /// // Test
    /// var mock = _app.Resources.OfType&lt;HttpMockResource&gt;().Single(r => r.Name == "external-weather-api");
    /// mock.MockServer.WithNewContext().Stub(x => x.Get("/api/weather")).Return(json).OK();
    /// </code>
    /// </example>
    public static IResourceBuilder<HttpMockResource> AddHttpMock(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        builder.Services.TryAddEventingSubscriber<HttpMockEventingSubscriber>();
        var resource = new HttpMockResource(name);
        return builder.AddResource(resource);
    }
}
