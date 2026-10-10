# E-Commerce API

A REST API starter project for an e-commerce backend, built with C#, ASP.NET Core 8, Entity Framework Core, and SQL Server.

## Features
- Product CRUD endpoints
- Category listing and creation
- Entity Framework Core data access
- SQL Server configuration
- Swagger UI in Development
- Basic input and relationship validation

## Requirements
- .NET 8 SDK
- SQL Server LocalDB or another SQL Server instance

## Run locally
From the repository root:

```powershell
dotnet restore src/ECommerceApi/ECommerceApi.csproj
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/ECommerceApi/ECommerceApi.csproj
dotnet ef database update --project src/ECommerceApi/ECommerceApi.csproj
dotnet run --project src/ECommerceApi/ECommerceApi.csproj
```

Open the Swagger URL printed in the terminal (usually `https://localhost:xxxx/swagger`). Update the `StoreDatabase` connection string in `appsettings.json` if needed.

## Endpoints
- `GET /api/products`, `GET /api/products/{id}`
- `POST /api/products`, `PUT /api/products/{id}`, `DELETE /api/products/{id}`
- `GET /api/categories`, `POST /api/categories`

## Roadmap
- [x] Product and category entities
- [x] Product CRUD API
- [x] SQL Server / EF Core configuration
- [ ] Cart and order workflows
- [ ] Authentication and authorization
- [ ] Automated tests

> This is a learning/portfolio starter, not production-ready. Review validation, error handling, migrations, and security before deployment.
