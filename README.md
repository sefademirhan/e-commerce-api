# E-Commerce API

[![.NET build](https://github.com/sefademirhan/e-commerce-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sefademirhan/e-commerce-api/actions/workflows/dotnet.yml)

A learning-focused **C# / ASP.NET Core 8 REST API** for an online store, using **Entity Framework Core** and **SQL Server**.

## Features
- Create, list, update and delete products
- Create and list categories; get a category by ID
- Validate product input and category references
- Return API response DTOs (avoids exposing EF entity navigation graphs)
- SQL Server configuration and Swagger for local development

## Requirements
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server Express LocalDB** (for the included connection string), or your own SQL Server instance
- PowerShell on Windows

## Set up and run
Extract the ZIP and open PowerShell in the folder containing this README.

```powershell
dotnet --version
dotnet restore src/ECommerceApi/ECommerceApi.csproj
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/ECommerceApi/ECommerceApi.csproj
dotnet run --project src/ECommerceApi/ECommerceApi.csproj --launch-profile http
```

Then open **http://localhost:5071/swagger**. The browser can be used to test the API through Swagger UI.

The `InitialCreate` migration is included in the repository, so **do not create it again**. The repository-local `dotnet-ef` 8.0.11 tool is restored with `dotnet tool restore`, independent of any global `dotnet-ef` version. If SQL Server LocalDB is not installed, install it or change `StoreDatabase` in `src/ECommerceApi/appsettings.json` for your SQL Server.

## Endpoints
| Method | URL | Description |
| --- | --- | --- |
| GET | `/api/categories` | List categories |
| GET | `/api/categories/{id}` | Get category |
| POST | `/api/categories` | Create category |
| GET | `/api/products` | List products |
| GET | `/api/products/{id}` | Get product |
| POST | `/api/products` | Create product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |

Create a category before creating a product.

**Example category request**
```json
{ "name": "Electronics" }
```

**Example product request**
```json
{
  "name": "Keyboard",
  "description": "Mechanical keyboard",
  "price": 1299.99,
  "stockQuantity": 20,
  "categoryId": 1
}
```

## Roadmap
- [x] Product and category APIs
- [x] EF Core and SQL Server setup instructions
- [x] GitHub Actions build checks
- [ ] Cart and order operations
- [ ] Authentication/authorization
- [ ] Automated integration tests

> Portfolio starter only. No cart, payments or checkout are implemented yet. The API needs additional tests, authorization and production hardening before deployment.
