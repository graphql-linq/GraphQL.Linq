// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using LinqToDB;

namespace TechMart;

public static class DataContextExtensions
{
    extension(IDataContext dataContext)
    {
        public ITable<OrderItem> OrderItems => dataContext.GetTable<OrderItem>();
        public ITable<Order> Orders => dataContext.GetTable<Order>();
        public ITable<Customer> Customers => dataContext.GetTable<Customer>();
        public ITable<Product> Products => dataContext.GetTable<Product>();
        public ITable<Category> Categories => dataContext.GetTable<Category>();
        public ITable<ProductCategory> ProductCategories => dataContext.GetTable<ProductCategory>();
        public ITable<User> Users => dataContext.GetTable<User>();
    }
}
