using HttpMock.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Register a mock server named "mock-api". Aspire starts the server before any
// dependent project launches and makes its URL available as:
//   ConnectionStrings__mock-api = http://localhost:<port>
builder.AddHttpMock("mock-api");

builder.Build().Run();
