# HttpMock

HttpMock enables you to mock the behaviour of HTTP services, that your application depends on, during testing.
It's particularly useful for Integration and Acceptance testing.

HttpMock returns canned responses at run time.


## Usage.

First, in the application you are testing, change the url of the HTTP service you want to mock, with the url for HttpMock.

Tell HttpMock to listen on the port you've provided. This is always localhost For example:

	_stubHttp = HttpMockRepository.At("http://localhost:9191");

Setup the stub that will return the canned response.

	_stubHttp.Stub(x => x.Get("/endpoint"))
		.Return(expected)
		.OK();

There are three essential parts to setting up a stub.

1. The path that will respond.
	
	`stubHttp.Stub(x => x.Get("/endpoint"))`

2. The content that will be returned. Supported body types can be Json, file and string content. 

	`.Return(expected)`

3. The status code of the response.
	
 	`.OK()`




Example usage:

```csharp
[Test]
public async Task SUT_should_return_stubbed_response()
{
	_stubHttp = HttpMockRepository.At("http://localhost:9191");

	const string expected = "<xml><response>Hello World</response></xml>";
	_stubHttp.Stub(x => x.Get("/endpoint"))
			.Return(expected)
			.OK();

	string result = await new HttpClient().GetStringAsync("http://localhost:9191/endpoint");

	Console.WriteLine("RESPONSE: {0}", result);

	Assert.That(result, Is.EqualTo(expected));
}
```


## HTTP Methods

HttpMock supports the standard HTTP verbs as well as arbitrary custom verbs.

```csharp
stubHttp.Stub(x => x.Get("/resource")).Return("got it").OK();
stubHttp.Stub(x => x.Post("/resource")).Return("created").OK();
stubHttp.Stub(x => x.Put("/resource")).Return("updated").OK();
stubHttp.Stub(x => x.Delete("/resource")).Return("deleted").OK();
stubHttp.Stub(x => x.Head("/resource")).Return("").OK();

// Custom / non-standard verbs
stubHttp.Stub(x => x.CustomVerb("/resource", "PURGE")).Return("purged").OK();
```


## Response Status Codes

Use the built-in helpers or supply any `HttpStatusCode` value directly.

```csharp
stubHttp.Stub(x => x.Get("/ok")).Return("Hello").OK();

stubHttp.Stub(x => x.Get("/missing")).Return("Not here").NotFound();

stubHttp.Stub(x => x.Get("/secret")).Return("Denied").WithStatus(HttpStatusCode.Unauthorized);
```


## Matching by Query Parameters

Use `.WithParams` to match only requests whose query string contains a specific set of key-value pairs. Additional query parameters on the incoming request are ignored.

```csharp
var firstParams = new Dictionary<string, string>
{
    { "trackId", "1" },
    { "formatId", "1" }
};

var secondParams = new Dictionary<string, string>
{
    { "trackId", "2" },
    { "formatId", "2" }
};

stubHttp.Stub(x => x.Get("/endpoint"))
    .WithParams(firstParams)
    .Return("first result")
    .OK();

stubHttp.Stub(x => x.Get("/endpoint"))
    .WithParams(secondParams)
    .Return("second result")
    .OK();
```


## Matching by Request Headers

Use `.WithHeaders` to match only requests that carry a specific set of headers.

```csharp
var headersA = new Dictionary<string, string>
{
    { "X-HeaderOne", "one" },
    { "X-HeaderTwo", "a" }
};

var headersB = new Dictionary<string, string>
{
    { "X-HeaderOne", "one" },
    { "X-HeaderTwo", "b" }
};

stubHttp.Stub(x => x.Get("/endpoint"))
    .WithHeaders(headersA)
    .Return("response A")
    .OK();

stubHttp.Stub(x => x.Get("/endpoint"))
    .WithHeaders(headersB)
    .Return("response B")
    .OK();
```


## URL Constraints

Use `.WithUrlConstraint` to match requests using an arbitrary predicate on the full request URL.

```csharp
stubHttp.Stub(x => x.Post("/api"))
    .WithUrlConstraint(url => url.Contains("/admin") == false)
    .Return("OK")
    .OK();
```

The stub above responds only when the URL does **not** contain `/admin`; otherwise HttpMock returns a 404.


## Matching by Request Body

Use `.WithBody` to match only requests whose body equals a specific string, or satisfies an arbitrary predicate. This lets you register multiple stubs for the same path and method and route by body content.

```csharp
// Match an exact body string
stubHttp.Stub(x => x.Post("/orders"))
    .WithBody("{\"type\":\"create\"}")
    .Return("created")
    .OK();

stubHttp.Stub(x => x.Post("/orders"))
    .WithBody("{\"type\":\"cancel\"}")
    .Return("cancelled")
    .OK();

// Match using a predicate
stubHttp.Stub(x => x.Post("/search"))
    .WithBody(body => body != null && body.Contains("urgent"))
    .Return("priority result")
    .OK();
```

Requests whose body does not satisfy any registered body constraint for a path receive a 404.


## Delayed Responses

Introduce an artificial delay to simulate slow services. Pass a value in milliseconds or a `TimeSpan`.

```csharp
// Delay in milliseconds
stubHttp.Stub(x => x.Get("/slow"))
    .Return("eventually")
    .OK()
    .WithDelay(500);

// Delay as a TimeSpan
stubHttp.Stub(x => x.Get("/slower"))
    .Return("even later")
    .OK()
    .WithDelay(TimeSpan.FromSeconds(1));
```

Delayed stubs run on their own background thread so they do not block other stubs from responding.


## Dynamic Responses

Supply a `Func<string>` to `.Return` to generate the response body at call time. The delegate is evaluated on every request, so the response can change between calls.

```csharp
string value = "initial";

stubHttp.Stub(x => x.Get("/dynamic"))
    .Return(() => value)
    .OK();

// First request returns "initial"
// After value = "updated", subsequent requests return "updated"
```


## File Responses

Serve a file directly from disk, or serve a byte range of a file (useful for streaming and partial-content scenarios).

```csharp
// Return a full file
stubHttp.Stub(x => x.Get("/download"))
    .ReturnFile("/path/to/file.mp3")
    .OK();

// Return a byte range (e.g. bytes 0–1023 of the file)
stubHttp.Stub(x => x.Get("/stream"))
    .ReturnFileRange("/path/to/file.mp3", 0, 1023)
    .WithStatus(HttpStatusCode.PartialContent);
```


## Asserting Requests Were Made

After exercising the system under test, verify that the expected requests reached the stub server.

```csharp
// Assert a request was made
stubHttp.AssertWasCalled(x => x.Get("/api/status"));

// Assert a request was NOT made
stubHttp.AssertWasNotCalled(x => x.Get("/api/echo"));

// Assert a POST was made with a specific body
stubHttp.AssertWasCalled(x => x.Post("/endpoint")).WithBody("postdata");

// Assert a POST body matches a constraint
stubHttp.AssertWasCalled(x => x.Post("/endpoint")).WithBody(Does.StartWith("post"));

// Assert an exact call count
stubHttp.AssertWasCalled(x => x.Post("/endpoint")).Times(2);

// Assert a specific request header was present
stubHttp.AssertWasCalled(x => x.Put("/endpoint")).WithHeader("X-Custom", Is.EqualTo("value"));
```


## Inspecting Received Requests

Cast the handler returned by `.Stub` to `RequestHandler` to inspect the raw requests that were received.

```csharp
var handler = (RequestHandler)stubHttp.Stub(x => x.Post("/endpoint"));
handler.Return("OK").OK();

// ... make requests ...

// Body of the most-recently received request
string lastBody = handler.LastRequest().Body;

// All requests received, in order
IEnumerable<ReceivedRequest> all = handler.GetObservedRequests();
foreach (var req in all)
{
    Console.WriteLine(req.Body);
}
```


## Reusing a Server Across Tests

Call `.WithNewContext()` to clear all previously registered stubs and start fresh. This lets you share a single server instance across an entire test fixture without stubs from one test leaking into another.

```csharp
[TestFixture]
public class MyTests
{
    private IHttpServer _server;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _server = HttpMockRepository.At("http://localhost:8080");
    }

    [Test]
    public async Task FirstTest()
    {
        _server.WithNewContext()
            .Stub(x => x.Post("/firsttest"))
            .Return("Response for first test")
            .OK();

        // ... exercise the SUT ...
    }

    [Test]
    public async Task SecondTest()
    {
        _server.WithNewContext()
            .Stub(x => x.Post("/secondtest"))
            .Return("Response for second test")
            .OK();

        // ... exercise the SUT ...
    }
}
```


## Logging

Pass an `ILoggerFactory` to `HttpMockRepository.At` (or directly to `HttpServer`) to enable structured logging via any [Microsoft.Extensions.Logging](https://learn.microsoft.com/dotnet/core/extensions/logging)-compatible provider.

```csharp
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
_stubHttp = HttpMockRepository.At("http://localhost:9191", loggerFactory);
```

Alternatively, configure a global factory once at startup:

```csharp
HttpMockLogging.Configure(loggerFactory);
```

## OpenTelemetry Tracing

HttpMock emits [OpenTelemetry](https://opentelemetry.io/)-compatible `Activity` spans for every request it handles (source name: `"HttpMock"`).  Each span carries the following tags:

| Tag | Description |
|-----|-------------|
| `http.request.method` | HTTP method (GET, POST, …) |
| `url.path` | Request path and query string |
| `httpmock.matched` | `true` when a stub was matched; `false` otherwise |
| `http.response.status_code` | Status code returned to the caller |

To capture these spans, subscribe to the source when configuring the OpenTelemetry SDK:

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(HttpMockActivitySource.Name)   // "HttpMock"
    .AddConsoleExporter()
    .Build();
```

No additional NuGet packages are required in HttpMock itself — `ActivitySource` is built into .NET.

## Reporting Issues.
When reporting issues, please provide a failing test. 
