// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TestDb.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<Product> CreatedProducts { get; set; } = null!;
}
