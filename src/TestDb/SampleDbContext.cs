// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using DbHelpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TestDb.Models;

namespace TestDb;

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    private static readonly Lazy<IDbCreator> _dbCreator = new(() => new DbCreator<SqlConnection>("GraphQLLinqSampleDb"), LazyThreadSafetyMode.ExecutionAndPublication);
    public static string GetConnectionString() => _dbCreator.Value.ConnectionString;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Seed data
            entity.HasData(SeedData.Users.ToArray());
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.LoyaltyPoints).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();

            // Seed data
            entity.HasData(SeedData.Customers.ToArray());
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            // Seed data
            entity.HasData(SeedData.Categories.ToArray());
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.StockQuantity).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired();

            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedProducts)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            entity.HasData(SeedData.Products.ToArray());
        });

        // ProductCategory configuration (junction table)
        modelBuilder.Entity<ProductCategory>(entity => {
            entity.HasKey(e => new { e.ProductId, e.CategoryId });

            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            entity.HasData(SeedData.ProductCategories.ToArray());
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.OrderStatus).IsRequired();
            entity.Property(e => e.OrderDate).IsRequired();

            entity.HasIndex(e => e.OrderNumber).IsUnique();

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            entity.HasData(SeedData.Orders.ToArray());
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).IsRequired();

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            entity.HasData(SeedData.OrderItems.ToArray());
        });
    }
}
