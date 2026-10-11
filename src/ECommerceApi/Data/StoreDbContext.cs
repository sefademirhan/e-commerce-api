using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Data;

public class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Cart>().HasKey(c => c.Id);
        modelBuilder.Entity<CartItem>()
            .HasOne(i => i.Cart).WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CartItem>()
            .HasOne(i => i.Product).WithMany()
            .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CartItem>()
            .HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();

        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        modelBuilder.Entity<Order>().HasIndex(o => o.PublicId).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(o => o.CartId).IsUnique();
        modelBuilder.Entity<Order>().Property(o => o.TotalPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order).WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderItem>()
            .Property(i => i.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>()
            .Property(i => i.ProductName).HasMaxLength(200);
    }
}
