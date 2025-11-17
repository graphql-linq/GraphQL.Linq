// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.DataLoader;
using GraphQL.DI;
using GraphQL.Linq;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace Tests;

[TestClass]
public class QueryableExtensionsTests : QueryTestBase
{
    public QueryableExtensionsTests()
    {
        QueryType = typeof(DIObjectGraphType<Query>);
        MappedGraphTypes.Add(typeof(ProductType));
        MappedGraphTypes.Add(typeof(OrderItemType));
    }

    private sealed class Query : DIObjectGraphBase
    {
        private readonly SampleDbContext _dbContext;

        public Query(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ToGraphSingleAsync tests
        public async Task<EfSource<Product>> Product([Id] int id)
        {
            return await _dbContext.Products.Where(p => p.Id == id).ToGraphSingleAsync();
        }

        public async Task<EfSource<Product>> ProductWithPredicate([Id] int id)
        {
            return await _dbContext.Products.ToGraphSingleAsync(p => p.Id == id);
        }

        // ToGraphSingleOrDefaultAsync tests
        public async Task<EfSource<Product>?> ProductNullable([Id] int id)
        {
            return await _dbContext.Products.Where(p => p.Id == id).ToGraphSingleOrDefaultAsync();
        }

        public async Task<EfSource<Product>?> ProductNullableWithPredicate([Id] int id)
        {
            return await _dbContext.Products.ToGraphSingleOrDefaultAsync(p => p.Id == id);
        }

        // ToGraphAsync tests
        public async Task<IList<EfSource<Product>>> Products()
        {
            return await _dbContext.Products.ToGraphAsync();
        }

        public async Task<IList<EfSource<Product>>> ProductsFiltered(bool isActive)
        {
            return await _dbContext.Products.Where(p => p.IsActive == isActive).ToGraphAsync();
        }

        // ToGraphConnectionAsync tests
        public async Task<Connection<EfSource<Product>>> ProductsConnection(int? first, int? last, string? after, string? before)
        {
            return await _dbContext.Products.ToGraphConnectionAsync(first, last, after, before);
        }

        public async Task<Connection<EfSource<Product>>> ProductsConnectionCustomPageSize(int? first, int? last, string? after, string? before)
        {
            return await _dbContext.Products.ToGraphConnectionAsync(first, last, after, before, defaultPageSize: 2);
        }

        public async Task<Connection<EfSource<Product>>> ProductsConnectionFiltered(int minStock, int? first, int? last, string? after, string? before)
        {
            return await _dbContext.Products.Where(p => p.StockQuantity > minStock).ToGraphConnectionAsync(first, last, after, before);
        }

        // ToGraphSingleDelayed tests
        public IDataLoaderResult<EfSource<Product>> ProductDelayed([Id] int id)
        {
            return _dbContext.Products.ToGraphSingleDelayed(id, p => p.Id);
        }

        public IDataLoaderResult<EfSource<Product>> ProductDelayedWithProjection([Id] int id)
        {
            return _dbContext.Products.ToGraphSingleDelayed(id, p => p.Id, p => p);
        }

        // ToGraphSingleOrDefaultDelayed tests
        public IDataLoaderResult<EfSource<Product>?> ProductNullableDelayed([Id] int id)
        {
            return _dbContext.Products.ToGraphSingleOrDefaultDelayed(id, p => p.Id);
        }

        public IDataLoaderResult<EfSource<Product>?> ProductNullableDelayedWithProjection([Id] int id)
        {
            return _dbContext.Products.ToGraphSingleOrDefaultDelayed(id, p => p.Id, p => p);
        }

        // ToGraphDelayed tests (for list loading by parent key)
        public IDataLoaderResult<IEnumerable<EfSource<OrderItem>>> OrderItemsByOrderId([Id] int orderId)
        {
            return _dbContext.OrderItems.ToGraphDelayed(orderId, oi => oi.OrderId);
        }

        public IDataLoaderResult<IEnumerable<EfSource<OrderItem>>> OrderItemsByOrderIdWithProjection([Id] int orderId)
        {
            return _dbContext.OrderItems.ToGraphDelayed(orderId, oi => oi.OrderId, oi => oi);
        }
    }

    private sealed class ProductType : EfObjectGraphType<SampleDbContext, Product>
    {
        public ProductType()
        {
            EfField(x => x.Id);
            EfField(x => x.Name);
            EfField(x => x.Price);
            EfField(x => x.StockQuantity);
            EfField(x => x.IsActive);
        }
    }

    private sealed class OrderItemType : EfObjectGraphType<SampleDbContext, OrderItem>
    {
        public OrderItemType()
        {
            EfField(x => x.Id);
            EfField(x => x.OrderId);
            EfField(x => x.ProductId);
            EfField(x => x.Quantity);
            EfField(x => x.UnitPrice);
        }
    }

    // =================== Schema SDL Test ===================

    [TestMethod]
    public async Task GetSDL()
    {
        var sdl = await GetSchemaSDLAsync();
        sdl.ShouldMatchApproved();
    }

    // =================== ToGraphSingleAsync Tests ===================

    [TestMethod]
    public async Task ToGraphSingleAsync_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleAsync_NotFound()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                product(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleAsync_WithPredicate_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productWithPredicate(id: 2) { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleAsync_WithPredicate_NotFound()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productWithPredicate(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ToGraphSingleOrDefaultAsync Tests ===================

    [TestMethod]
    public async Task ToGraphSingleOrDefaultAsync_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullable(id: 1) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleOrDefaultAsync_NotFound()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullable(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleOrDefaultAsync_WithPredicate_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullableWithPredicate(id: 3) { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleOrDefaultAsync_WithPredicate_NotFound()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullableWithPredicate(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ToGraphAsync Tests ===================

    [TestMethod]
    public async Task ToGraphAsync_AllProducts()
    {
        var json = await ExecuteQueryAsync("""
            {
                products { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphAsync_WithFilter()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsFiltered(isActive: true) { id name isActive }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ToGraphConnectionAsync Tests ===================

    [TestMethod]
    public async Task ToGraphConnectionAsync_FirstPage()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(first: 2) {
                    edges {
                        node { id name }
                        cursor
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                        startCursor
                        endCursor
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_SecondPage()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(first: 2, after: "1") {
                    edges {
                        node { id name }
                        cursor
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                        startCursor
                        endCursor
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_LastPage()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(last: 2) {
                    edges {
                        node { id name }
                        cursor
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                        startCursor
                        endCursor
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_BeforeCursor()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(last: 2, before: "5") {
                    edges {
                        node { id name }
                        cursor
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                        startCursor
                        endCursor
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_CustomPageSize()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionCustomPageSize {
                    edges {
                        node { id name }
                    }
                    pageInfo {
                        hasNextPage
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_WithFilter()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionFiltered(minStock: 50, first: 10) {
                    edges {
                        node { id name stockQuantity }
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_ItemsOnly()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(first: 3) {
                    items { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_CountOnly()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection {
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphConnectionAsync_EdgesAndItems()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(first: 3) {
                    edges {
                        node { id name }
                    }
                    items { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ToGraphSingleDelayed Tests ===================

    [TestMethod]
    public async Task ToGraphSingleDelayed_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productDelayed(id: 1) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleDelayed_NotFound()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productDelayed(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleDelayed_WithProjection_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productDelayedWithProjection(id: 2) { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleDelayed_WithProjection_NotFound()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productDelayedWithProjection(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleDelayed_MultipleCalls_Batched()
    {
        var json = await ExecuteQueryAsync("""
            {
                p1: productDelayed(id: 1) { id name }
                p2: productDelayed(id: 2) { id name }
                p3: productDelayed(id: 3) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ToGraphSingleOrDefaultDelayed Tests ===================

    [TestMethod]
    public async Task ToGraphSingleOrDefaultDelayed_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullableDelayed(id: 1) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleOrDefaultDelayed_NotFound()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullableDelayed(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleOrDefaultDelayed_WithProjection_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullableDelayedWithProjection(id: 3) { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleOrDefaultDelayed_WithProjection_NotFound()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullableDelayedWithProjection(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphSingleOrDefaultDelayed_MultipleCalls_Batched()
    {
        var json = await ExecuteQueryAsync("""
            {
                p1: productNullableDelayed(id: 1) { id name }
                p2: productNullableDelayed(id: 2) { id name }
                p3: productNullableDelayed(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ToGraphDelayed Tests ===================

    [TestMethod]
    public async Task ToGraphDelayed_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                orderItemsByOrderId(orderId: 1) { id orderId productId quantity }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphDelayed_NotFound()
    {
        var json = await ExecuteQueryAsync("""
            {
                orderItemsByOrderId(orderId: 999) { id orderId }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphDelayed_WithProjection_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                orderItemsByOrderIdWithProjection(orderId: 2) { id orderId productId quantity unitPrice }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ToGraphDelayed_MultipleCalls_Batched()
    {
        var json = await ExecuteQueryAsync("""
            {
                o1: orderItemsByOrderId(orderId: 1) { id orderId }
                o2: orderItemsByOrderId(orderId: 2) { id orderId }
                o3: orderItemsByOrderId(orderId: 3) { id orderId }
            }
            """);
        json.ShouldMatchApproved();
    }
}
