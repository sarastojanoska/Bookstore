# Bookstore API

A .NET bookstore service that provides book management and search endpoints. The solution includes an ASP.NET Core API, a Duende IdentityServer authentication service, and an Aspire AppHost for local orchestration.

## Features

- Create, read, update, and delete books.
- Search books by title and author, with pagination.
- Protect API endpoints with JWT bearer tokens and the `books.crud` and `books.search` scopes.
- Apply Entity Framework Core migrations and add sample authors at API startup.

## Requirements

- .NET 10 SDK. The authentication project targets .NET 10; the other application projects target .NET 9.
- Docker Desktop or another Docker-compatible engine, for the SQL Server container started by Aspire.

## Run Locally

From the repository root, start the Aspire application host:

```sh
dotnet run --project src/AppHost
```

Aspire starts SQL Server, the authentication service, and the API. Open the Aspire dashboard URL printed in the terminal to inspect the services and follow their endpoints. The API Swagger UI is enabled in the Development environment at the API's `/swagger` path.

## API Endpoints

All endpoints require a valid bearer token.

| Method | Endpoint | Scope | Description |
| --- | --- | --- | --- |
| `GET` | `/api/Books` | `books.crud` | List books |
| `GET` | `/api/Books/{id}` | `books.crud` | Get a book |
| `POST` | `/api/Books` | `books.crud` | Create a book |
| `PUT` | `/api/Books/{id}` | `books.crud` | Update a book |
| `DELETE` | `/api/Books/{id}` | `books.crud` | Delete a book |
| `GET` | `/api/BookSearch` | `books.search` | Search books |

Search accepts optional `title` and `authorName` query parameters, and `pageNumber` and `pageSize` parameters (defaulting to `1` and `10`). For example: `/api/BookSearch?title=history&pageNumber=1&pageSize=10`.

The API's Swagger UI exposes OAuth authorization for the configured search client. The authentication service also defines a client-credentials flow for the `books.crud` scope. Client IDs, secrets, and authority URLs are environment-specific; do not use development credentials or commit production secrets.

## Run Tests

From the repository root:

```sh
dotnet test bookstore-api.slnx
```

## Project Structure

- `src/API` - HTTP API, controllers, authorization, and Swagger configuration.
- `src/Authentication` - IdentityServer and user authentication.
- `src/Application` - Application DTOs and service interfaces.
- `src/Services` - Book service implementation.
- `src/Domain` - Domain entities.
- `src/Infrastructure` - Entity Framework Core data access and migrations.
- `src/AppHost` - Aspire orchestration for local development.
- `tests/Tests` - Automated tests.
