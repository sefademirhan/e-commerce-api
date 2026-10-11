namespace ECommerceApi.Models;

public class Order
{
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public DateTime PlacedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal TotalPrice { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
