# Contributing to HttpMock

Thank you for taking the time to contribute! Please read the guidelines below before opening a pull request.

## Building

```bash
dotnet build HttpMock.sln
```

## Running the tests

```bash
dotnet test HttpMock.sln
```

## Project structure

| Folder | Purpose |
|--------|---------|
| `src/HttpMock` | Core library |
| `src/HttpMock.Aspire.Hosting` | .NET Aspire hosting integration |
| `src/HttpMock.Verify.NUnit` | NUnit verification helpers |
| `examples/` | Runnable example applications |

## Test project naming conventions

Test projects must follow this naming scheme:

| Test type | Suffix | Example |
|-----------|--------|---------|
| Unit tests | `*.Unit.Tests` | `HttpMock.Unit.Tests` |
| Integration tests | `*.Integration.Tests` | `HttpMock.Integration.Tests` |

Both suffixes apply to any feature area. For example, tests for the Aspire hosting package should be named:

- `HttpMock.Aspire.Hosting.Unit.Tests`
- `HttpMock.Aspire.Hosting.Integration.Tests`

### Rationale

The consistent suffixes make it easy to:

- Filter test runs by type (`--filter FullyQualifiedName~Integration.Tests`).
- Identify the scope of a project at a glance.
- Apply CI policies (e.g. run unit tests on every push, integration tests only on PRs).

## Reporting issues

When reporting a bug, please provide a failing test that demonstrates the problem.

