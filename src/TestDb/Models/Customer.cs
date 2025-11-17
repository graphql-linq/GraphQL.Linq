// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TestDb.Models;

public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int LoyaltyPoints { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = null!;
}
