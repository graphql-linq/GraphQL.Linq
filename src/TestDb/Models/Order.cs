// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TestDb.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int CustomerId { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public DateTimeOffset? ShippedDate { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = null!;
}
