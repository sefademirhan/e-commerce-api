# E-Commerce API

[![.NET build and tests](https://github.com/sefademirhan/e-commerce-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sefademirhan/e-commerce-api/actions/workflows/dotnet.yml)

A portfolio **ASP.NET Core 8** REST API with **Entity Framework Core and SQL Server**. Product and category management now includes a guest shopping cart, stock-aware checkout and immutable order line snapshots.

## Features

- Create, list, edit and remove products
- Create and browse product categories
- Create a guest cart with a random GUID identifier
- Add, change and remove cart items
- Checkout using a database transaction with serializable isolation
- Reject purchases with insufficient stock
- Reduce inventory and save order/line-item price snapshots atomically
- View an order by its random public order GUID
- Automated SQLite-backed checkout tests and GitHub Actions

## Quick start on Windows

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), SQL Server Express LocalDB.

In PowerShell, from the extracted repository root:

```powershell
dotnet restore src/ECommerceApi/ECommerceApi.csproj
dotnet tool restore
```

If your working LocalDB instance is `StockDemo`, set the connection for **both** the migration tool and the API:

```powershell
$env:ConnectionStrings__StoreDatabase = 'Server=(localdb)\StockDemo;Database=ECommerceApiDb;Integrated Security=True;TrustServerCertificate=True;'
```

Then:

```powershell
dotnet tool run dotnet-ef database update --project src/ECommerceApi/ECommerceApi.csproj
dotnet run --project src/ECommerceApi/ECommerceApi.csproj --launch-profile http
```

Open **http://localhost:5071/swagger** while the terminal is running.

> The first migration (`InitialCreate`) and the second migration (`AddCartAndOrders`) are **already in GitHub**. Do **not** run `migrations add InitialCreate` again. The `database update` command applies only pending migrations, preserving existing products.

Without the environment override, the API uses `(localdb)\\MSSQLLocalDB` as configured in `appsettings.json`.

## Shopping demo (Swagger)

1. `POST /api/categories` with `{ "name": "Electronics" }`.
2. `POST /api/products` with:

```json
{
  "name": "Gaming Keyboard",
  "description": "Mechanical keyboard",
  "price": 1500,
  "stockQuantity": 10,
  "categoryId": 1
}
```

3. `POST /api/carts` (no body). Copy the `cartId` GUID returned.
4. `PUT /api/carts/{cartId}/items/{productId}` with `{ "quantity": 2 }`.
5. `GET /api/carts/{cartId}` to check totals.
6. `POST /api/carts/{cartId}/checkout` (no body). Copy `orderId`.
7. `GET /api/orders/{orderId}` to see the saved order; `GET /api/products` shows reduced stock.

Use the IDs returned by your own API. A cart can only be checked out once.

## Endpoints

| Method | Path | Description |
| --- | --- | --- |
| GET / POST | `/api/categories` | List / create category |
| GET | `/api/categories/{id}` | Get category |
| GET / POST | `/api/products` | List / create product |
| GET / PUT / DELETE | `/api/products/{id}` | Get / update / delete product |
| POST | `/api/carts` | Create a guest cart |
| GET | `/api/carts/{cartId}` | View cart and totals |
| PUT / DELETE | `/api/carts/{cartId}/items/{productId}` | Set quantity / remove line |
| POST | `/api/carts/{cartId}/checkout` | Create order and reduce stock |
| GET | `/api/orders/{orderId}` | Retrieve saved order |

## Tests

```powershell
dotnet test tests/ECommerceApi.Tests/ECommerceApi.Tests.csproj
```

Tests use independent SQLite databases in memory and do not affect SQL Server data.

## Scope and safety

This is a **portfolio demo**, not a payment-ready online store. Guest cart IDs and order IDs are capability-style identifiers, not a substitute for user authentication. There are **no payments, customer accounts, shipping, refunds, admin permissions or production concurrency/retry guarantees**. Do not expose write endpoints publicly without authentication, authorization, input hardening and operational safeguards.

**Tech stack:** C#, ASP.NET Core, EF Core, SQL Server, Swagger, xUnit, GitHub Actions.
