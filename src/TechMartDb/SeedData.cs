// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using TechMartDb.Models;

namespace TechMartDb;

public static class SeedData
{
    public static readonly IEnumerable<User> Users = new[]
    {
        new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@techmart.com",
            Role = UserRole.Admin,
            IsActive = true
        },
        new User
        {
            Id = 2,
            Username = "jsmith",
            Email = "john.smith@techmart.com",
            Role = UserRole.Sales,
            IsActive = true
        },
        new User
        {
            Id = 3,
            Username = "mjones",
            Email = "mary.jones@techmart.com",
            Role = UserRole.Manager,
            IsActive = true
        }
    };

    public static readonly IEnumerable<Customer> Customers = new[]
    {
        new Customer
        {
            Id = 1,
            Email = "jane.doe@email.com",
            FullName = "Jane Doe",
            PhoneNumber = "+1-555-0123",
            LoyaltyPoints = 1250,
            CreatedAt = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero)
        },
        new Customer
        {
            Id = 2,
            Email = "bob.smith@email.com",
            FullName = "Bob Smith",
            PhoneNumber = "+1-555-0456",
            LoyaltyPoints = 500,
            CreatedAt = new DateTimeOffset(2024, 2, 20, 0, 0, 0, TimeSpan.Zero)
        },
        new Customer
        {
            Id = 3,
            Email = "alice.wong@email.com",
            FullName = "Alice Wong",
            PhoneNumber = null,
            LoyaltyPoints = 0,
            CreatedAt = new DateTimeOffset(2024, 3, 10, 0, 0, 0, TimeSpan.Zero)
        }
    };

    public static readonly IEnumerable<Category> Categories = new[]
    {
        new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            IsActive = true
        },
        new Category
        {
            Id = 2,
            Name = "Computers",
            Description = "Desktop and laptop computers",
            IsActive = true
        },
        new Category
        {
            Id = 3,
            Name = "Accessories",
            Description = "Computer accessories and peripherals",
            IsActive = true
        },
        new Category
        {
            Id = 4,
            Name = "Gaming",
            Description = "Gaming hardware and accessories",
            IsActive = true
        }
    };

    public static readonly IEnumerable<Product> Products = new[]
    {
        new Product
        {
            Id = 1,
            Name = "UltraBook Pro 15",
            Description = "High-performance laptop with 16GB RAM and 512GB SSD",
            Price = 1299.99m,
            StockQuantity = 45,
            IsActive = true,
            CreatedByUserId = 1
        },
        new Product
        {
            Id = 2,
            Name = "Wireless Mouse",
            Description = "Ergonomic wireless mouse with precision tracking",
            Price = 29.99m,
            StockQuantity = 150,
            IsActive = true,
            CreatedByUserId = 2
        },
        new Product
        {
            Id = 3,
            Name = "USB-C Hub",
            Description = "7-in-1 USB-C adapter with HDMI, USB 3.0, and SD card reader",
            Price = 49.99m,
            StockQuantity = 80,
            IsActive = true,
            CreatedByUserId = 2
        },
        new Product
        {
            Id = 4,
            Name = "Mechanical Keyboard",
            Description = "RGB mechanical gaming keyboard with Cherry MX switches",
            Price = 149.99m,
            StockQuantity = 60,
            IsActive = true,
            CreatedByUserId = 1
        },
        new Product
        {
            Id = 5,
            Name = "4K Monitor",
            Description = "27-inch 4K UHD monitor with HDR support",
            Price = 399.99m,
            StockQuantity = 30,
            IsActive = true,
            CreatedByUserId = 1
        }
    };

    public static readonly IEnumerable<ProductCategory> ProductCategories = new[]
    {
        new ProductCategory { ProductId = 1, CategoryId = 1 }, // UltraBook in Electronics
        new ProductCategory { ProductId = 1, CategoryId = 2 }, // UltraBook in Computers
        new ProductCategory { ProductId = 2, CategoryId = 1 }, // Mouse in Electronics
        new ProductCategory { ProductId = 2, CategoryId = 3 }, // Mouse in Accessories
        new ProductCategory { ProductId = 3, CategoryId = 1 }, // Hub in Electronics
        new ProductCategory { ProductId = 3, CategoryId = 3 }, // Hub in Accessories
        new ProductCategory { ProductId = 4, CategoryId = 1 }, // Keyboard in Electronics
        new ProductCategory { ProductId = 4, CategoryId = 3 }, // Keyboard in Accessories
        new ProductCategory { ProductId = 4, CategoryId = 4 }, // Keyboard in Gaming
        new ProductCategory { ProductId = 5, CategoryId = 1 }, // Monitor in Electronics
        new ProductCategory { ProductId = 5, CategoryId = 2 }  // Monitor in Computers
    };

    public static readonly IEnumerable<Order> Orders = new[]
    {
        new Order
        {
            Id = 1,
            OrderNumber = "ORD-2024-00001",
            CustomerId = 1,
            OrderStatus = OrderStatus.Delivered,
            OrderDate = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero),
            ShippedDate = new DateTimeOffset(2024, 1, 16, 14, 0, 0, TimeSpan.Zero)
        },
        new Order
        {
            Id = 2,
            OrderNumber = "ORD-2024-00002",
            CustomerId = 2,
            OrderStatus = OrderStatus.Shipped,
            OrderDate = new DateTimeOffset(2024, 2, 20, 15, 45, 0, TimeSpan.Zero),
            ShippedDate = new DateTimeOffset(2024, 2, 21, 9, 30, 0, TimeSpan.Zero)
        },
        new Order
        {
            Id = 3,
            OrderNumber = "ORD-2024-00003",
            CustomerId = 1,
            OrderStatus = OrderStatus.Processing,
            OrderDate = new DateTimeOffset(2024, 3, 10, 11, 20, 0, TimeSpan.Zero),
            ShippedDate = null
        },
        new Order
        {
            Id = 4,
            OrderNumber = "ORD-2024-00004",
            CustomerId = 3,
            OrderStatus = OrderStatus.Pending,
            OrderDate = new DateTimeOffset(2024, 3, 15, 14, 0, 0, TimeSpan.Zero),
            ShippedDate = null
        }
    };

    public static readonly IEnumerable<OrderItem> OrderItems = new[]
    {
        new OrderItem
        {
            Id = 1,
            OrderId = 1,
            ProductId = 1,
            Quantity = 1,
            UnitPrice = 1299.99m
        },
        new OrderItem
        {
            Id = 2,
            OrderId = 1,
            ProductId = 3,
            Quantity = 1,
            UnitPrice = 49.99m
        },
        new OrderItem
        {
            Id = 3,
            OrderId = 2,
            ProductId = 2,
            Quantity = 2,
            UnitPrice = 29.99m
        },
        new OrderItem
        {
            Id = 4,
            OrderId = 2,
            ProductId = 3,
            Quantity = 1,
            UnitPrice = 49.99m
        },
        new OrderItem
        {
            Id = 5,
            OrderId = 3,
            ProductId = 4,
            Quantity = 1,
            UnitPrice = 149.99m
        },
        new OrderItem
        {
            Id = 6,
            OrderId = 3,
            ProductId = 5,
            Quantity = 1,
            UnitPrice = 399.99m
        },
        new OrderItem
        {
            Id = 7,
            OrderId = 4,
            ProductId = 5,
            Quantity = 1,
            UnitPrice = 399.99m
        }
    };
}
