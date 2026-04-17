using Aspire.Hosting.ApplicationModel;
using HttpMock;

namespace HttpMock.Aspire.Hosting;

/// <summary>
/// Represents an HttpMock stub server registered as a first-class .NET Aspire resource.
/// </summary>
/// <remarks>
/// Use <see cref="HttpMockResourceBuilderExtensions.AddHttpMock"/> to register this resource in
/// the Aspire app host. The resource implements <see cref="IResourceWithConnectionString"/> so
/// that Aspire automatically injects the mock server URL into any dependent project as the
/// <c>ConnectionStrings__&lt;name&gt;</c> environment variable.
/// </remarks>
public sealed class HttpMockResource : Resource, IResourceWithConnectionString, IDisposable
{
    /// <summary>
    /// Gets the URL of the running mock server (e.g. <c>http://localhost:54321</c>).
    /// Set by the Aspire lifecycle before any dependent resource starts.
    /// </summary>
    public string? Url { get; internal set; }

    /// <summary>
    /// Gets the live <see cref="IHttpServer"/> instance once the resource has started.
    /// Use this in tests to register stubs:
    /// <code>
    /// resource.MockServer.WithNewContext().Stub(x => x.Get("/path")).Return("body").OK();
    /// </code>
    /// </summary>
    public IHttpServer? MockServer { get; internal set; }

    /// <inheritdoc/>
    public HttpMockResource(string name) : base(name) { }

    /// <inheritdoc/>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{Url ?? string.Empty}");

    /// <inheritdoc/>
    public void Dispose()
    {
        MockServer?.Dispose();
        MockServer = null;
    }
}
