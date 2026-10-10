using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(StoreDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll(CancellationToken ct) =>
        Ok(await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<Category>> Create(CategoryInput input, CancellationToken ct)
    {
        var name = input.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Category name is required.");
        if (await db.Categories.AnyAsync(c => c.Name == name, ct))
            return Conflict("A category with this name already exists.");

        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
    }

    public record CategoryInput(string Name);
}
