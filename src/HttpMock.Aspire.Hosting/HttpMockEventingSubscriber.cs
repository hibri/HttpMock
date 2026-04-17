using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using Aspire.Hosting.Lifecycle;

namespace HttpMock.Aspire.Hosting;

/// <summary>
/// Aspire eventing subscriber that starts an <see cref="HttpServer"/> for every
/// <see cref="HttpMockResource"/> registered in the app model before the distributed
/// application starts, and disposes each server when the
/// <see cref="global::Aspire.Hosting.DistributedApplication"/> is torn down.
/// </summary>
internal sealed class HttpMockEventingSubscriber : IDistributedApplicationEventingSubscriber, IDisposable
{
    private readonly ResourceNotificationService _notificationService;
    private readonly List<HttpMockResource> _startedResources = [];

    public HttpMockEventingSubscriber(ResourceNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <inheritdoc/>
    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken = default)
    {
        eventing.Subscribe<BeforeStartEvent>(async (evt, ct) =>
        {
            foreach (var resource in evt.Model.Resources.OfType<HttpMockResource>())
            {
                var port = HttpMockRepository.FindFreePort();
                var url = $"http://localhost:{port}";

                resource.Url = url;
                resource.MockServer = new HttpServer(new Uri(url));
                resource.MockServer.Start();

                _startedResources.Add(resource);

                await _notificationService.PublishUpdateAsync(resource, s => s with
                {
                    State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success),
                    Urls = [new UrlSnapshot("http", url, IsInternal: false)],
                });
            }
        });

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var resource in _startedResources)
        {
            resource.Dispose();
        }

        _startedResources.Clear();
    }
}
