// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TestDb.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<ProductCategory> ProductCategories { get; set; } = null!;
}
