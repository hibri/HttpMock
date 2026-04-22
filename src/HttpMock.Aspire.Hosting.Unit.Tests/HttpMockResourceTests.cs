using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using HttpMock;
using HttpMock.Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace HttpMock.Aspire.Hosting.Unit.Tests;

[TestFixture]
public class HttpMockResourceTests
{
    // ── Unit tests (no running app) ──────────────────────────────────────────

    [Test]
    public void ConnectionStringExpression_IsNotNull_WhenUrlNotSet()
    {
        var resource = new HttpMockResource("test");

        Assert.That(resource.ConnectionStringExpression, Is.Not.Null);
    }

    [Test]
    public async Task ConnectionStringExpression_ReturnsUrl_AfterUrlSet()
    {
        var resource = new HttpMockResource("test");
        resource.Url = "http://localhost:54321";

        var value = await resource.ConnectionStringExpression.GetValueAsync(CancellationToken.None);

        Assert.That(value, Is.EqualTo("http://localhost:54321"));
    }

    [Test]
    public void Dispose_ClearsMockServerProperty()
    {
        var resource = new HttpMockResource("test");
        resource.MockServer = HttpMockRepository.At($"http://localhost:{HttpMockRepository.FindFreePort()}");

        resource.Dispose();

        Assert.That(resource.MockServer, Is.Null);
    }

    [Test]
    public void AddHttpMock_ResourceHasCorrectName()
    {
        var builder = DistributedApplication.CreateBuilder(
            new DistributedApplicationOptions
            {
                DisableDashboard = true,
                AssemblyName = typeof(HttpMockResourceTests).Assembly.GetName().Name
            });

        var resourceBuilder = builder.AddHttpMock("my-mock");

        Assert.That(resourceBuilder.Resource.Name, Is.EqualTo("my-mock"));
    }

    // ── Subscriber integration test (in-process, no DCP required) ────────────

    [Test]
    public async Task Subscriber_StartsServerForHttpMockResource()
    {
        var resource = new HttpMockResource("test-mock");
        var model = new DistributedApplicationModel([resource]);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHostApplicationLifetime, NoOpHostLifetime>();
        services.AddSingleton<ResourceLoggerService>();
        services.AddSingleton<ResourceNotificationService>();
        await using var sp = services.BuildServiceProvider();
        var notificationService = sp.GetRequiredService<ResourceNotificationService>();

        using var subscriber = new HttpMockEventingSubscriber(notificationService);
        var eventing = new CapturingEventing();

        var context = new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run);
        await subscriber.SubscribeAsync(eventing, context);

        // Fire BeforeStartEvent directly — no DCP needed
        var evt = new BeforeStartEvent(sp, model);
        await eventing.PublishAsync(evt, CancellationToken.None);

        Assert.That(resource.Url, Does.StartWith("http://localhost:"));
        Assert.That(resource.MockServer, Is.Not.Null);
        Assert.That(resource.MockServer!.IsAvailable(), Is.True);

        var connStr = await resource.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
        Assert.That(connStr, Is.EqualTo(resource.Url));
    }

    // ── Test helpers ─────────────────────────────────────────────────────────

    /// <summary>Minimal <see cref="IHostApplicationLifetime"/> with no-op token sources.</summary>
    private sealed class NoOpHostLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;
        public void StopApplication() { }
    }

    /// <summary>
    /// Minimal <see cref="IDistributedApplicationEventing"/> that captures subscriptions and
    /// allows tests to fire events directly without a running Aspire host.
    /// </summary>
    private sealed class CapturingEventing : IDistributedApplicationEventing
    {
        private readonly Dictionary<Type, List<Func<IDistributedApplicationEvent, CancellationToken, Task>>> _subs = [];

        public DistributedApplicationEventSubscription Subscribe<T>(Func<T, CancellationToken, Task> callback)
            where T : IDistributedApplicationEvent
        {
            if (!_subs.TryGetValue(typeof(T), out var list))
                _subs[typeof(T)] = list = [];

            Task Wrapper(IDistributedApplicationEvent e, CancellationToken ct) => callback((T)e, ct);
            list.Add(Wrapper);
            return new DistributedApplicationEventSubscription(Wrapper);
        }

        DistributedApplicationEventSubscription IDistributedApplicationEventing.Subscribe<T>(IResource resource, Func<T, CancellationToken, Task> callback)
            => ((IDistributedApplicationEventing)this).Subscribe(callback);

        public void Unsubscribe(DistributedApplicationEventSubscription subscription) { }

        public async Task PublishAsync<T>(T evt, CancellationToken cancellationToken = default)
            where T : IDistributedApplicationEvent
        {
            if (_subs.TryGetValue(typeof(T), out var callbacks))
                foreach (var cb in callbacks)
                    await cb(evt, cancellationToken);
        }

        public Task PublishAsync<T>(T evt, EventDispatchBehavior behavior, CancellationToken cancellationToken = default)
            where T : IDistributedApplicationEvent
            => PublishAsync(evt, cancellationToken);
    }
}
