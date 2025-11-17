// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Linq;
using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Types;

namespace Tests;

[TestClass]
public class QueryGraphTypeTests : QueryTestBase
{
    public QueryGraphTypeTests()
    {
        QueryType = typeof(Query);
        MappedGraphTypes.Add(typeof(ProductType));
    }

    private sealed class Query : QueryGraphType<SampleDbContext>
    {
        public Query(IEfGraphQLService<SampleDbContext> efGraphQLService) : base(efGraphQLService)
        {
            // EfSingleField tests
            EfSingleField("product", context => context.DbContext.Products);
            EfSingleField("productNullable", context => context.DbContext.Products, nullable: true);
            EfSingleField("productNoIdArg", context => context.DbContext.Products.Where(p => p.Id == 1), addIdArgument: false);
            EfSingleField("productWithCustomArg", context => context.DbContext.Products.Where(p => p.Name == context.GetArgument<string>("name")!),
                addIdArgument: false,
                arguments: new[] { new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" } });
            ((IComplexGraphType)this).EfSingleField(efGraphQLService, "productNoSource", context => context.DbContext.Products);

            // EfSingleFieldAsync tests
            EfSingleFieldAsync("productAsync", context => Task.FromResult(context.DbContext.Products.AsQueryable()));
            EfSingleFieldAsync("productAsyncNullable", context => Task.FromResult(context.DbContext.Products.AsQueryable()), nullable: true);

            // EfQueryField tests
            EfQueryField("products", context => context.DbContext.Products);
            ((IComplexGraphType)this).EfQueryField(efGraphQLService, "productsNoSource", context => context.DbContext.Products);
            EfQueryField("productsFiltered", context => context.DbContext.Products.Where(p => p.IsActive == context.GetArgument<bool>("isActive")),
                arguments: new[] { new QueryArgument<NonNullGraphType<BooleanGraphType>> { Name = "isActive" } });

            // EfQueryFieldAsync tests
            EfQueryFieldAsync("productsAsync", context => Task.FromResult(context.DbContext.Products.AsQueryable()));
            EfQueryFieldAsync("productsAsyncFiltered", context => Task.FromResult(context.DbContext.Products.Where(p => p.Price > context.GetArgument<decimal>("minPrice")).AsQueryable()),
                arguments: new[] { new QueryArgument<NonNullGraphType<DecimalGraphType>> { Name = "minPrice" } });

            // EfQueryConnectionField tests
            EfQueryConnectionField("productsConnection", context => context.DbContext.Products);
            EfQueryConnectionField("productsConnectionCustomPageSize", context => context.DbContext.Products, defaultPageSize: 2);
            EfQueryConnectionField("productsConnectionNullPageSize", context => context.DbContext.Products, defaultPageSize: null);
            EfQueryConnectionField("productsConnectionFiltered", context => context.DbContext.Products.Where(p => p.StockQuantity > context.GetArgument<int>("minStock")),
                arguments: new[] { new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "minStock" } });

            // EfQueryConnectionFieldAsync tests
            EfQueryConnectionFieldAsync("productsConnectionAsync", context => Task.FromResult(context.DbContext.Products.AsQueryable()));
            EfQueryConnectionFieldAsync("productsConnectionAsyncCustomPageSize", context => Task.FromResult(context.DbContext.Products.AsQueryable()), defaultPageSize: 3);

            // EfQueryConnectionField with connectionResolver tests
            EfQueryConnectionField("productsConnectionWithResolver", context => context.DbContext.Products, new EfSimpleConnectionResolver<SampleDbContext, Product>(100));
            EfQueryConnectionFieldAsync("productsConnectionAsyncWithResolver", context => Task.FromResult(context.DbContext.Products.AsQueryable()), new EfSimpleConnectionResolver<SampleDbContext, Product>(100));

            // ComplexGraphTypeExtensions.EfQueryConnectionField tests
            ((IComplexGraphType)this).EfQueryConnectionField(efGraphQLService, "productsConnectionNoSource", context => context.DbContext.Products);
            ((IComplexGraphType)this).EfQueryConnectionField(efGraphQLService, "productsConnectionNoSourceWithResolver", context => context.DbContext.Products, new EfSimpleConnectionResolver<SampleDbContext, Product>(100));

            Assert.Throws<ArgumentNullException>(() => EfSingleField<string>("test", null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfSingleField<SampleDbContext, string>(null!, efGraphQLService, "test", _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfSingleField<SampleDbContext, string>(this, null!, "test", _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfSingleField<SampleDbContext, string>(this, efGraphQLService, null!, _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfSingleField<SampleDbContext, string>(this, efGraphQLService, "test", null!));
            Assert.Throws<ArgumentNullException>(() => EfQueryField<string>("test", null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryField<SampleDbContext, string>(null!, efGraphQLService, "test", _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryField<SampleDbContext, string>(this, null!, "test", _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryField<SampleDbContext, string>(this, efGraphQLService, null!, _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryField<SampleDbContext, string>(this, efGraphQLService, "test", null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryField<SampleDbContext, object, string>(this, efGraphQLService, "test", null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryFieldAsync<SampleDbContext, string>(this, efGraphQLService, "test", null!));
            Assert.Throws<ArgumentNullException>(() => EfQueryConnectionField<string>("test", null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryConnectionField<SampleDbContext, string>(null!, efGraphQLService, "test", _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryConnectionField<SampleDbContext, string>(this, null!, "test", _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryConnectionField<SampleDbContext, string>(this, efGraphQLService, null!, _ => null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryConnectionField<SampleDbContext, string>(this, efGraphQLService, "test", null!));
            Assert.Throws<ArgumentNullException>(() => ComplexGraphTypeExtensions.EfQueryConnectionField<SampleDbContext, string>(this, efGraphQLService, "test", null!, new EfSimpleConnectionResolver<SampleDbContext, string>(100)));
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

    // =================== Schema SDL Test ===================

    [TestMethod]
    public async Task GetSDL()
    {
        var sdl = await GetSchemaSDLAsync();
        sdl.ShouldMatchApproved();
    }

    // =================== EfSingleField Tests ===================

    [TestMethod]
    public async Task EfSingleField_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleField_NotFound()
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
    public async Task EfSingleField_Nullable_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullable(id: 1) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleField_Nullable_NotFound()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNullable(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleField_NoIdArgument()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNoIdArg { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleField_WithCustomArgument()
    {
        var json = await ExecuteQueryAsync("""
            {
                productWithCustomArg(name: "Wireless Mouse") { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleField_NoSource()
    {
        var json = await ExecuteQueryAsync("""
            {
                productNoSource(id: 1) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfSingleFieldAsync Tests ===================

    [TestMethod]
    public async Task EfSingleFieldAsync_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productAsync(id: 2) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleFieldAsync_NotFound()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productAsync(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleFieldAsync_Nullable_Found()
    {
        var json = await ExecuteQueryAsync("""
            {
                productAsyncNullable(id: 3) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleFieldAsync_Nullable_NotFound()
    {
        var json = await ExecuteQueryAsync("""
            {
                productAsyncNullable(id: 999) { id name }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfQueryField Tests ===================

    [TestMethod]
    public async Task EfQueryField_AllProducts()
    {
        var json = await ExecuteQueryAsync("""
            {
                products { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryField_AllProducts_NoSource()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsNoSource { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryField_WithFilter()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsFiltered(isActive: true) { id name isActive }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfQueryFieldAsync Tests ===================

    [TestMethod]
    public async Task EfQueryFieldAsync_AllProducts()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsAsync { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryFieldAsync_WithFilter()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsAsyncFiltered(minPrice: 100) { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfQueryConnectionField Tests ===================

    [TestMethod]
    public async Task EfQueryConnectionField_FirstPage()
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
    public async Task EfQueryConnectionField_SecondPage()
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
    public async Task EfQueryConnectionField_LastPage()
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
    public async Task EfQueryConnectionField_BeforeCursor()
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
    public async Task EfQueryConnectionField_CustomPageSize()
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
    public async Task EfQueryConnectionField_WithFilter()
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
    public async Task EfQueryConnectionField_ItemsOnly()
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
    public async Task EfQueryConnectionField_CountOnly()
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
    public async Task EfQueryConnectionField_EdgesAndItems()
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

    // =================== EfQueryConnectionFieldAsync Tests ===================

    [TestMethod]
    public async Task EfQueryConnectionFieldAsync_FirstPage()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionAsync(first: 2) {
                    edges {
                        node { id name }
                        cursor
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionFieldAsync_CustomPageSize()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionAsyncCustomPageSize {
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
    public async Task EfQueryConnectionFieldAsync_WithPagination()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionAsync(first: 2, after: "2") {
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

    // =================== EfQueryConnectionField with ConnectionResolver Tests ===================

    [TestMethod]
    public async Task EfQueryConnectionField_WithConnectionResolver()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionWithResolver(first: 2) {
                    edges {
                        node { id name }
                        cursor
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionFieldAsync_WithConnectionResolver()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionAsyncWithResolver(first: 2) {
                    edges {
                        node { id name }
                        cursor
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ComplexGraphTypeExtensions.EfQueryConnectionField Tests ===================

    [TestMethod]
    public async Task EfQueryConnectionField_NoSource()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionNoSource(first: 2) {
                    edges {
                        node { id name }
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionField_NoSource_WithConnectionResolver()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionNoSourceWithResolver(first: 2) {
                    edges {
                        node { id name }
                    }
                    totalCount
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Basic Connection Field Parameter Tests ===================

    [TestMethod]
    public async Task EfQueryConnectionField_After0()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(after: "0") {
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
    public async Task EfQueryConnectionField_FirstNegative1()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(first: -1) {
                    edges {
                        node { id name }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionField_Last10()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(last: 10) {
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
    public async Task EfQueryConnectionField_Before1Last3()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(before: "1", last: 3) {
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
    public async Task EfQueryConnectionField_Before2Last1()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(before: "2", last: 1) {
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
    public async Task EfQueryConnectionField_Before5After1()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(before: "5", after: "1") {
                    edges {
                        node { id name }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionField_First10Last10()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(first: 10, last: 10) {
                    edges {
                        node { id name }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionField_First10Before5()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(first: 10, before: "5") {
                    edges {
                        node { id name }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionField_Last10After5()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(last: 10, after: "5") {
                    edges {
                        node { id name }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfQueryConnectionField_LastNegative1()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                productsConnection(last: -1, before: "6") {
                    edges {
                        node { id name }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Null Page Size Connection Field Tests ===================

    [TestMethod]
    public async Task EfQueryConnectionField_NullPageSize_After2()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionNullPageSize(after: "2") {
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
    public async Task EfQueryConnectionField_NullPageSize_Before2()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionNullPageSize(before: "2") {
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
    public async Task EfQueryConnectionField_NullPageSize_NoParams()
    {
        var json = await ExecuteQueryAsync("""
            {
                productsConnectionNullPageSize {
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
}
