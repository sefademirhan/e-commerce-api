using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerceApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace ECommerceApi.Tests;

public class ShoppingApiTests
{
    [Fact]
    public async Task CategoryProductCartCheckoutAndOrderWorkTogether()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        factory.CreateDatabase();

        var categoryResult = await client.PostAsJsonAsync("/api/categories", new { name = "Electronics" });
        Assert.Equal(HttpStatusCode.Created, categoryResult.StatusCode);
        using var category = JsonDocument.Parse(await categoryResult.Content.ReadAsStringAsync());
        var categoryId = category.RootElement.GetProperty("id").GetInt32();

        var productResult = await client.PostAsJsonAsync("/api/products", new
        {
            name = "Keyboard",
            description = "Mechanical",
            price = 150m,
            stockQuantity = 10,
            categoryId
        });
        Assert.Equal(HttpStatusCode.Created, productResult.StatusCode);
        using var product = JsonDocument.Parse(await productResult.Content.ReadAsStringAsync());
        var productId = product.RootElement.GetProperty("id").GetInt32();

        var cartResult = await client.PostAsJsonAsync("/api/carts", new { });
        Assert.Equal(HttpStatusCode.Created, cartResult.StatusCode);
        using var cart = JsonDocument.Parse(await cartResult.Content.ReadAsStringAsync());
        var cartId = cart.RootElement.GetProperty("cartId").GetGuid();

        var put = await client.PutAsJsonAsync($"/api/carts/{cartId}/items/{productId}",
            new { quantity = 2 });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var cartDetails = await client.GetFromJsonAsync<JsonElement>($"/api/carts/{cartId}");
        Assert.Equal(300m, cartDetails.GetProperty("totalPrice").GetDecimal());

        var checkout = await client.PostAsJsonAsync($"/api/carts/{cartId}/checkout", new { });
        Assert.Equal(HttpStatusCode.Created, checkout.StatusCode);
        using var result = JsonDocument.Parse(await checkout.Content.ReadAsStringAsync());
        var orderId = result.RootElement.GetProperty("orderId").GetGuid();

        var orderResult = await client.GetAsync($"/api/orders/{orderId}");
        Assert.Equal(HttpStatusCode.OK, orderResult.StatusCode);

        var productAfter = await client.GetFromJsonAsync<JsonElement>($"/api/products/{productId}");
        Assert.Equal(8, productAfter.GetProperty("stockQuantity").GetInt32());

        // The cart was consumed and must not be billed twice.
        var twice = await client.PostAsJsonAsync($"/api/carts/{cartId}/checkout", new { });
        Assert.Equal(HttpStatusCode.NotFound, twice.StatusCode);
    }

    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            connection.Open();
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<StoreDbContext>>();
                services.RemoveAll<StoreDbContext>();
                services.AddDbContext<StoreDbContext>(options => options.UseSqlite(connection));
            });
        }

        public void CreateDatabase()
        {
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<StoreDbContext>().Database.EnsureCreated();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            connection.Dispose();
        }
    }
}
