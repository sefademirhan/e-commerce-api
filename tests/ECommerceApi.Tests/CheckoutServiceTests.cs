using ECommerceApi.Data;
using ECommerceApi.Models;
using ECommerceApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerceApi.Tests;

public class CheckoutServiceTests
{
    private static StoreDbContext CreateDb(SqliteConnection connection)
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseSqlite(connection).Options;
        var db = new StoreDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static (Cart Cart, Product Product) SeedCart(StoreDbContext db, int stock, int quantity)
    {
        var category = new Category { Name = "Electronics" };
        var product = new Product
        {
            Name = "Keyboard", Price = 150m, StockQuantity = stock,
            Category = category
        };
        var cart = new Cart();
        cart.Items.Add(new CartItem { Product = product, Quantity = quantity });
        db.Carts.Add(cart);
        db.SaveChanges();
        return (cart, product);
    }

    [Fact]
    public async Task Checkout_DecreasesStockAndCreatesOrder()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDb(connection);
        var (cart, product) = SeedCart(db, 10, 2);

        var result = await new CheckoutService(db).CheckoutAsync(cart.Id);

        Assert.Equal(CheckoutResultType.Success, result.Type);
        Assert.Equal(300m, result.Order!.TotalPrice);
        Assert.Equal(8, db.Products.AsNoTracking().Single(p => p.Id == product.Id).StockQuantity);
        Assert.Empty(db.Carts.AsNoTracking());
        Assert.Single(db.Orders.AsNoTracking());
        Assert.Single(db.OrderItems.AsNoTracking());
    }

    [Fact]
    public async Task Checkout_RejectsInsufficientStockWithoutCreatingOrder()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDb(connection);
        var (cart, product) = SeedCart(db, 1, 4);

        var result = await new CheckoutService(db).CheckoutAsync(cart.Id);

        Assert.Equal(CheckoutResultType.InsufficientStock, result.Type);
        Assert.Single(db.Carts.AsNoTracking());
        Assert.Empty(db.Orders.AsNoTracking());
        Assert.Equal(1, db.Products.AsNoTracking().Single(p => p.Id == product.Id).StockQuantity);
    }

    [Fact]
    public async Task EmptyCart_CannotBeCheckedOut()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDb(connection);
        var cart = new Cart();
        db.Carts.Add(cart);
        db.SaveChanges();

        var result = await new CheckoutService(db).CheckoutAsync(cart.Id);
        Assert.Equal(CheckoutResultType.EmptyCart, result.Type);
    }

    [Fact]
    public async Task SameCart_CannotCheckoutTwice()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDb(connection);
        var (cart, _) = SeedCart(db, 10, 2);
        var service = new CheckoutService(db);

        Assert.Equal(CheckoutResultType.Success, (await service.CheckoutAsync(cart.Id)).Type);
        Assert.Equal(CheckoutResultType.CartNotFound, (await service.CheckoutAsync(cart.Id)).Type);
        Assert.Single(db.Orders.AsNoTracking());
    }
}
