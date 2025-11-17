// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TestDb.Models;

public class ProductCategory
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
