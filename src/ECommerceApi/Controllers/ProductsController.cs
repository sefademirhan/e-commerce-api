using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(StoreDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(CancellationToken ct)
    {
        var products = await db.Products.AsNoTracking().OrderBy(p => p.Id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Description,
                p.Price, p.StockQuantity, p.CategoryId, p.Category == null ? "" : p.Category.Name))
            .ToListAsync(ct);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Description,
                p.Price, p.StockQuantity, p.CategoryId, p.Category == null ? "" : p.Category.Name))
            .SingleOrDefaultAsync(ct);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(ProductInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 200 || input.Price < 0 || input.StockQuantity < 0)
            return BadRequest("A name of at most 200 characters is required; price and stock quantity must be non-negative.");

        var category = await db.Categories.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == input.CategoryId, ct);
        if (category is null) return BadRequest("The category does not exist.");

        var product = new Product
        {
            Name = input.Name.Trim(),
            Description = input.Description?.Trim() ?? "",
            Price = input.Price,
            StockQuantity = input.StockQuantity,
            CategoryId = category.Id
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var response = new ProductResponse(product.Id, product.Name, product.Description,
            product.Price, product.StockQuantity, category.Id, category.Name);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 200 || input.Price < 0 || input.StockQuantity < 0)
            return BadRequest("A name of at most 200 characters is required; price and stock quantity must be non-negative.");
        if (!await db.Categories.AnyAsync(c => c.Id == input.CategoryId, ct))
            return BadRequest("The category does not exist.");

        var product = await db.Products.FindAsync([id], ct);
        if (product is null) return NotFound();

        product.Name = input.Name.Trim();
        product.Description = input.Description?.Trim() ?? "";
        product.Price = input.Price;
        product.StockQuantity = input.StockQuantity;
        product.CategoryId = input.CategoryId;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null) return NotFound();

        if (await db.CartItems.AnyAsync(i => i.ProductId == id, ct))
            return Conflict("Remove this product from guest carts before deleting it.");

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    public record ProductInput(string Name, string? Description, decimal Price, int StockQuantity, int CategoryId);
    public record ProductResponse(int Id, string Name, string Description,
        decimal Price, int StockQuantity, int CategoryId, string CategoryName);
}
