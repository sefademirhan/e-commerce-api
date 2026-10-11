using ECommerceApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

// Order IDs are random GUIDs; this demo stores no customer personal information.
[ApiController]
[Route("api/orders")]
public class OrdersController(StoreDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.PublicId == id, ct);
        if (order is null) return NotFound();

        return Ok(new
        {
            orderId = order.PublicId,
            placedAtUtc = order.PlacedAtUtc,
            order.TotalPrice,
            items = order.Items.Select(i => new
            {
                i.ProductId, i.ProductName, i.Quantity, i.UnitPrice,
                lineTotal = i.Quantity * i.UnitPrice
            }).ToList()
        });
    }
}
