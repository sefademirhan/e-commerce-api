using System.Data;
using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Services;

public enum CheckoutResultType { Success, CartNotFound, EmptyCart, InsufficientStock }

public record CheckoutResult(CheckoutResultType Type, Order? Order = null);

public class CheckoutService(StoreDbContext db)
{
    public async Task<CheckoutResult> CheckoutAsync(Guid cartId, CancellationToken ct = default)
    {
        // All product quantities, cart removal, and order creation commit atomically.
        // Serializable isolation protects stock checks from overlapping checkouts.
        await using var transaction =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var cart = await db.Carts.Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == cartId, ct);
        if (cart is null)
            return new CheckoutResult(CheckoutResultType.CartNotFound);
        if (cart.Items.Count == 0)
            return new CheckoutResult(CheckoutResultType.EmptyCart);

        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);
        if (products.Count != productIds.Count ||
            cart.Items.Any(i => i.Quantity <= 0 ||
                !products.TryGetValue(i.ProductId, out var p) || p.StockQuantity < i.Quantity))
            return new CheckoutResult(CheckoutResultType.InsufficientStock);

        var order = new Order { CartId = cartId };
        foreach (var item in cart.Items)
        {
            var product = products[item.ProductId];
            product.StockQuantity -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }
        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        db.Orders.Add(order);
        db.Carts.Remove(cart);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new CheckoutResult(CheckoutResultType.Success, order);
    }
}
