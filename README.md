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
