using ECommerceApi.Data;
using ECommerceApi.Models;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/carts")]
public class CartsController(StoreDbContext db, CheckoutService checkout) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var cart = new Cart();
        db.Carts.Add(cart);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = cart.Id }, new { cartId = cart.Id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var cart = await db.Carts.AsNoTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .SingleOrDefaultAsync(c => c.Id == id, ct);
        if (cart is null) return NotFound();

        var items = cart.Items.Select(i => new CartLine(
            i.ProductId, i.Product.Name, i.Quantity,
            i.Product.Price, i.Product.Price * i.Quantity)).ToList();
        return Ok(new CartResponse(cart.Id, items, items.Sum(i => i.LineTotal)));
    }

    [HttpPut("{id:guid}/items/{productId:int}")]
    public async Task<IActionResult> PutItem(Guid id, int productId,
        QuantityInput input, CancellationToken ct)
    {
        if (input.Quantity is < 1 or > 1000)
            return BadRequest("Quantity must be between 1 and 1000.");

        var cart = await db.Carts.Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == id, ct);
        if (cart is null) return NotFound("Cart not found.");

        var product = await db.Products.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return NotFound("Product not found.");
        if (product.StockQuantity < input.Quantity)
            return Conflict("Not enough stock.");

        var existing = cart.Items.SingleOrDefault(i => i.ProductId == productId);
        if (existing is null)
            cart.Items.Add(new CartItem { ProductId = productId, Quantity = input.Quantity });
        else
            existing.Quantity = input.Quantity;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(Guid id, int productId, CancellationToken ct)
    {
        var item = await db.CartItems.SingleOrDefaultAsync(
            i => i.CartId == id && i.ProductId == productId, ct);
        if (item is null) return NotFound();
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/checkout")]
    public async Task<IActionResult> Checkout(Guid id, CancellationToken ct)
    {
        var result = await checkout.CheckoutAsync(id, ct);
        return result.Type switch
        {
            CheckoutResultType.CartNotFound => NotFound("Cart not found."),
            CheckoutResultType.EmptyCart => BadRequest("Cart is empty."),
            CheckoutResultType.InsufficientStock => Conflict("One or more products are out of stock."),
            CheckoutResultType.Success => CreatedAtAction("GetById", "Orders",
                new { id = result.Order!.PublicId },
                new { orderId = result.Order.PublicId, totalPrice = result.Order.TotalPrice }),
            _ => Problem("Unexpected checkout error.")
        };
    }

    public record QuantityInput(int Quantity);
    public record CartLine(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
    public record CartResponse(Guid CartId, List<CartLine> Items, decimal TotalPrice);
}
