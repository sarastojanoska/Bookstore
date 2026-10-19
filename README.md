# Bookstore API

A .NET bookstore API solution with authentication, SQL Server persistence, and Aspire-based local orchestration.

## Requirements

- .NET 9 SDK
- .NET 10 SDK
- Docker Desktop (or another Docker-compatible container runtime)

The projects target `net9.0`. Docker images for the API and authentication service use the .NET 9 runtime.

## Run Locally

From the repository root, start the Aspire AppHost:

```powershell
dotnet run --project src/AppHost
```

Aspire starts the API and authentication projects and provisions the SQL Server database in a Docker container. Open the Aspire dashboard URL printed in the terminal to view service status and endpoints. The API endpoint may use a dynamically assigned port; open its endpoint with `/swagger` to view Swagger UI.

For the API's direct HTTP launch profile, Swagger is available at:

<http://localhost:3000/swagger>

Swagger is enabled in the Development environment.

## Docker

The API and authentication services have Dockerfiles, and the repository includes a Docker Compose project. Aspire is the recommended local startup path because it also configures service references and the SQL Server dependency.
