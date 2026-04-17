using System.Net;
using System.Text;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using HttpMock;
using HttpMock.Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace HttpMock.Aspire.Hosting.IntegrationTests;

/// <summary>
/// End-to-end integration tests that demonstrate how to use the
/// <see cref="HttpMockResourceBuilderExtensions.AddHttpMock"/> API with .NET Aspire.
///
/// Pattern:
/// <list type="number">
///   <item>
///     The AppHost registers <c>builder.AddHttpMock("mock-api")</c>.
///     Aspire starts the server automatically when the application launches.
///   </item>
///   <item>
///     Tests retrieve the <see cref="HttpMockResource"/> by name from the
///     <see cref="DistributedApplicationModel"/> and configure stubs via
///     <see cref="HttpMockResource.MockServer"/>.
///   </item>
///   <item>
///     Each test calls <see cref="IHttpServer.WithNewContext"/> to clear any
///     leftover stubs from previous tests.
///   </item>
/// </list>
/// </summary>
[TestFixture]
public class HttpMockResourceIntegrationTests
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(60);

    private DistributedApplication _app = null!;
    private IHttpServer _mockServer = null!;
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        using var cts = new CancellationTokenSource(StartupTimeout);
        var cancellationToken = cts.Token;

        // Build a real Aspire DistributedApplication from the companion AppHost project.
        // The AppHost registers: builder.AddHttpMock("mock-api")
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.HttpMock_Aspire_Hosting_IntegrationTests_AppHost>(cancellationToken);

        _app = await appHost.BuildAsync(cancellationToken);
        await _app.StartAsync(cancellationToken);

        // Wait until the mock resource reports Running so the server is definitely ready.
        await _app.ResourceNotifications
            .WaitForResourceAsync("mock-api", KnownResourceStates.Running, cancellationToken)
            .WaitAsync(StartupTimeout, cancellationToken);

        // Retrieve the resource to access its live MockServer and URL.
        var resource = _app.Services
            .GetRequiredService<DistributedApplicationModel>()
            .Resources
            .OfType<HttpMockResource>()
            .Single(r => r.Name == "mock-api");

        _mockServer = resource.MockServer
            ?? throw new InvalidOperationException(
                $"HttpMockResource '{resource.Name}' has no MockServer. " +
                "Ensure the application has started before accessing MockServer.");

        _httpClient = new HttpClient { BaseAddress = new Uri(resource.Url!) };
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

    // ── Basic verb/status stubs ──────────────────────────────────────────────

    [Test]
    public async Task Stub_Get_Returns200WithBody()
    {
        _mockServer.WithNewContext()
            .Stub(x => x.Get("/hello"))
            .Return("world")
            .OK();

        using var response = await _httpClient.GetAsync("/hello");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.EqualTo("world"));
    }

    [Test]
    public async Task Stub_Get_ReturnsJson()
    {
        const string json = """{"temperature":22,"summary":"Warm"}""";

        _mockServer.WithNewContext()
            .Stub(x => x.Get("/api/weather"))
            .Return(json)
            .AsContentType("application/json")
            .OK();

        using var response = await _httpClient.GetAsync("/api/weather");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo("application/json"));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.EqualTo(json));
    }

    [Test]
    public async Task Stub_Get_ReturnsNotFound()
    {
        _mockServer.WithNewContext()
            .Stub(x => x.Get("/missing"))
            .NotFound();

        using var response = await _httpClient.GetAsync("/missing");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Stub_Post_ReturnsCreated()
    {
        _mockServer.WithNewContext()
            .Stub(x => x.Post("/api/items"))
            .Return("""{"id":42}""")
            .AsContentType("application/json")
            .WithStatus(HttpStatusCode.Created);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/items")
        {
            Content = new StringContent(
                """{"name":"Widget"}""", Encoding.UTF8, "application/json")
        };
        using var response = await _httpClient.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("42"));
    }

    // ── Context isolation ────────────────────────────────────────────────────

    [Test]
    public async Task WithNewContext_ClearsPreviousStubs()
    {
        // First context: stub returns 200
        _mockServer.WithNewContext()
            .Stub(x => x.Get("/endpoint"))
            .Return("first")
            .OK();

        using var first = await _httpClient.GetAsync("/endpoint");
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // New context: no stubs registered — should return 404
        _mockServer.WithNewContext();
        using var second = await _httpClient.GetAsync("/endpoint");
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // ── Aspire resource model ────────────────────────────────────────────────

    [Test]
    public async Task ConnectionStringExpression_ReturnsRunningServerUrl()
    {
        var resource = _app.Services
            .GetRequiredService<DistributedApplicationModel>()
            .Resources
            .OfType<HttpMockResource>()
            .Single(r => r.Name == "mock-api");

        var connStr = await resource.ConnectionStringExpression
            .GetValueAsync(CancellationToken.None);

        Assert.That(connStr, Is.EqualTo(resource.Url));
        Assert.That(connStr, Does.StartWith("http://localhost:"));
    }
}
