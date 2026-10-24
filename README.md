# User Management API

An ASP.NET Core Web API example for managing users and demonstrating
authentication, role-based authorization, and SQL Server data access.

## Features

- .NET 10 Web API with OpenAPI and Scalar documentation in development
- SQL Server access through `Microsoft.Data.SqlClient`
- User registration and listing endpoints
- Cookie-based authentication and session handling
- Role- and permission-based authorization examples
- Antiforgery support and custom API error/security middleware

## Requirements

- .NET 10 SDK
- SQL Server with the tables expected by the application

Database schema setup or migrations are not included in this repository. The
application seeds roles, permissions, and a demo administrator at startup, so
the configured database must be reachable and have the required tables before
the API can run.

## Configuration

Configure a local SQL Server connection string and replace the sample keys
using .NET User Secrets. Run these commands from the repository root:

```powershell
dotnet user-secrets init --project UserManagementApi\UserManagementApi.csproj
dotnet user-secrets set "ConnectionStrings:DemoAppDb" "<your SQL Server connection string>" --project UserManagementApi\UserManagementApi.csproj
dotnet user-secrets set "Auth:Key" "<a long random secret>" --project UserManagementApi\UserManagementApi.csproj
dotnet user-secrets set "Auth:DataKey" "<your data-protection key>" --project UserManagementApi\UserManagementApi.csproj
dotnet user-secrets set "AppSettings:Token" "<a long random secret>" --project UserManagementApi\UserManagementApi.csproj
```

The checked-in `appsettings.json` contains development placeholders, including
a sample connection string. Do not use those values or the seeded credentials
in a shared or deployed environment.

## Run

```powershell
dotnet restore UserManagementApi\UserManagementApi.csproj
dotnet run --project UserManagementApi\UserManagementApi.csproj
```

The included Development launch profile uses `http://localhost:5143` and
`https://localhost:7044`. When running with the HTTPS profile, the Scalar API
reference is available at `/scalar`.

## Example endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `POST` | `/api/users/register-user` | Register a user |
| `GET` | `/api/users/view-users` | List users |
| `GET` | `/api/users/xsrf-token` | Issue an antiforgery token |
| `POST` | `/api/auth/login` | Sign in and create a session cookie |
| `GET` | `/api/auth/logout` | Sign out |
| `POST` | `/api/users/check-login` | Example authenticated endpoint |
| `GET` | `/api/users/check-admin` | Example admin-only endpoint |

The `UserManagementApi\UserManagementApi.http` file includes sample requests
for editors that support `.http` files.

## Demo credentials and limitations

The startup seeder currently creates an `Admin` account with password `0000`.
This is a local demonstration credential only. The project is an example, not a
production-ready authentication system; review its password storage, key
management, and session handling before adapting it for a deployed service.
