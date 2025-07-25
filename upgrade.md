# Upgrade Plan: HttpMock to .NET 8

## User Instructions

1. Upgrade the entire project to .NET 8.
2. Remove the Kayak dependency completely.
3. Preserve all tests. Do not change any test code.

## Assistant Suggestions & Plan

### 1. Project Upgrade
- Convert all `.csproj` files to SDK-style format.
- Change target framework to `net8.0` for all projects.
- Remove any legacy or obsolete settings from project files.
- Ensure all dependencies are compatible with .NET 8.

### 2. Remove Kayak
- Remove Kayak references from all `.csproj` and `packages.config` files.
- Remove all `using Kayak` and `using Kayak.Http` statements from code.
- Refactor the HTTP server implementation to use `HttpListner' (primarily in `HttpServer.cs`) to use a modern .NET 8 HTTP servers.
- Update or replace any code that depends on Kayak types or APIs.
- Don't break any of the interfaces

### 3. Preserve Tests
- Do not modify any test logic or files.
- Ensure tests still reference the main library and run under .NET 8.

### 4. Build & Test
- Update build/test scripts (e.g., Azure Pipelines, PowerShell) to use .NET 8 if needed.
- Verify that all tests pass after the upgrade and refactor.

---

**Note:**
- The main focus is to modernize the codebase and remove Kayak, while keeping the test suite unchanged and fully functional.
- The HTTP server implementation will need the most attention, as it is tightly coupled to Kayak in the current codebase. 