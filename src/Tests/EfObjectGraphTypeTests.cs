// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Linq.Expressions;
using GraphQL.Linq;
using GraphQL.Linq.ConnectionResolvers;

namespace Tests;

[TestClass]
public class EfObjectGraphTypeTests : QueryTestBase
{
    public EfObjectGraphTypeTests()
    {
        QueryType = typeof(Query);
        MappedGraphTypes.Add(typeof(ProductGraphType));
        MappedGraphTypes.Add(typeof(UserGraphType));
        MappedGraphTypes.Add(typeof(CategoryGraphType));
        MappedGraphTypes.Add(typeof(ProductCategoryGraphType));
        MappedGraphTypes.Add(typeof(OrderItemGraphType));
    }

    private sealed class Query : QueryGraphType<SampleDbContext>
    {
        public Query(IEfGraphQLService<SampleDbContext> efGraphQLService) : base(efGraphQLService)
        {
            // Simple query to get a product by ID for testing
            EfSingleField("product", context => context.DbContext.Products);
        }
    }

    // =================== Test Graph Types ===================

    private sealed class ProductGraphType : EfObjectGraphType<SampleDbContext, Product>
    {
        public ProductGraphType()
        {
            Name = "Product";

            // EfField - basic property mapping
            EfField(x => x.Name);
            EfField(x => x.Description, nullable: true);
            EfField(x => x.Price);
            EfField(x => x.StockQuantity);
            EfField(x => x.IsActive);

            // EfField with custom name
            EfField("productName", x => x.Name);

            // EfField with explicit nullable
            EfField("nullableDescription", x => x.Description, nullable: true);

            // EfField with explicit non-nullable on nullable property
            EfField("nonNullableDescription", x => x.Description, nullable: false);

            // EfField with explicit graph type
            EfField("priceAsFloat", x => x.Price, graphType: typeof(FloatGraphType));

            // EfField with DbContext parameter
            EfField("createdByUserName", (db, p) => p.CreatedByUser.Username);

            // EfFieldFromContext - dynamic field based on context
            EfFieldFromContext<string>("dynamicField", context => {
                var includeDescription = context.GetArgument<bool>("includeDescription");
                return p => includeDescription ? p.Description ?? "N/A" : p.Name;
            }, nullable: false, graphType: typeof(StringGraphType))
                .Argument<BooleanGraphType>("includeDescription");

            // EfIdField - basic ID field
            EfIdField(x => x.Id);

            // EfIdField with custom name
            EfIdField("productId", x => x.Id);

            // EfIdField with nullable
            EfIdField("nullableId", x => (int?)x.Id, nullable: true);

            // EfNavigationField - single navigation property
            EfNavigationField(x => x.CreatedByUser);

            // EfNavigationField with custom name
            EfNavigationField("creator", x => x.CreatedByUser);

            // EfNavigationField with non-nullable
            EfNavigationField("nonNullableCreator", x => x.CreatedByUser, nullable: false);

            // EfNavigationField with nullable
            EfNavigationField("nullableCreator", x => x.CreatedByUser, nullable: true);

            // EfNavigationField with explicit graph type
            EfNavigationField("creatorWithType", x => x.CreatedByUser, graphType: typeof(UserGraphType));

            // EfNavigationField with DbContext parameter
            EfNavigationField("createdBy", (db, p) => p.CreatedByUser);

            // EfNavigationFieldLink - navigation via DbContext
            EfNavigationFieldLink("createdByLink", (db, p) => db.Users.Where(u => u.Id == p.CreatedByUserId));

            // EfNavigationListField - collection navigation
            EfNavigationListField(x => x.ProductCategories);

            // EfNavigationListField with custom name
            EfNavigationListField("categories", x => x.ProductCategories);

            // EfNavigationListField with explicit graph type
            EfNavigationListField("categoriesWithType", x => x.ProductCategories, graphType: typeof(ListGraphType<ProductCategoryGraphType>));

            // EfNavigationListField with DbContext parameter
            EfNavigationListField("productCategoriesFromDb", (db, p) => db.ProductCategories.Where(pc => pc.ProductId == p.Id));

            // EfNavigationListField that gets categories from product categories
            EfNavigationListField("categoriesFromProductCategories", (db, p) => p.ProductCategories.Select(pc => pc.Category));

            // EfNavigationListFieldFromContext - dynamic list field
            EfNavigationListFieldFromContext<ProductCategory>("filteredCategories", context => {
                var activeOnly = context.GetArgument<bool>("activeOnly");
                return p => activeOnly
                    ? p.ProductCategories.Where(pc => pc.Category.IsActive)
                    : p.ProductCategories;
            }, arguments: new[] { new QueryArgument<BooleanGraphType> { Name = "activeOnly" } });

            // EfNavigationConnectionField with unspecified name and resolver
            var connectionResolver2 = new EfSimpleConnectionResolver<SampleDbContext, OrderItem>(10);
            EfNavigationConnectionField(x => x.OrderItems, connectionResolver2)
                .FieldType.Name = "categoriesWithResolverNoName";

            // EfNavigationConnectionField - basic connection
            EfNavigationConnectionField(x => x.OrderItems);

            // EfNavigationConnectionField with custom page size
            EfNavigationConnectionField("productCategoriesConnection", x => x.ProductCategories, defaultPageSize: 5);

            // EfNavigationConnectionField with custom name and page size
            EfNavigationConnectionField("categoriesConnection", x => x.ProductCategories, defaultPageSize: 10);

            // EfNavigationConnectionField that projects categories from product categories
            EfNavigationConnectionField("categoriesConnectionProjected", (db, p) => p.ProductCategories.Select(pc => pc.Category), defaultPageSize: 5);

            // EfNavigationConnectionField with explicit graph type - should throw NotSupportedException
            Assert.Throws<NotSupportedException>(() =>
                EfNavigationConnectionField("categoriesConnectionWithType", x => x.ProductCategories, defaultPageSize: 100, graphType: typeof(ProductCategoryGraphType)));

            // EfNavigationConnectionField with connection resolver
            var connectionResolver = new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(10);
            EfNavigationConnectionField("categoriesWithResolver", x => x.ProductCategories, connectionResolver);

            // EfNavigationConnectionField with DbContext parameter
            EfNavigationConnectionField("productCategoriesFromDbConnection", (db, p) => p.ProductCategories, defaultPageSize: 20);

            // EfNavigationConnectionField with DbContext and connection resolver
            EfNavigationConnectionField("categoriesFromDbWithResolver", (db, p) => p.ProductCategories, connectionResolver);

            // EfNavigationConnectionFieldFromContext - dynamic connection field
            EfNavigationConnectionFieldFromContext<ProductCategory>("filteredCategoriesConnection", context => {
                var activeOnly = context.GetArgument<bool>("activeOnly");
                return p => activeOnly
                    ? p.ProductCategories.Where(pc => pc.Category.IsActive)
                    : p.ProductCategories;
            }, defaultPageSize: 15, arguments: new[] { new QueryArgument<BooleanGraphType> { Name = "activeOnly" } });

            // EfNavigationConnectionFieldFromContext with connection resolver
            EfNavigationConnectionFieldFromContext<ProductCategory>("filteredCategoriesWithResolver", context => {
                var activeOnly = context.GetArgument<bool>("activeOnly");
                return p => activeOnly
                    ? p.ProductCategories.Where(pc => pc.Category.IsActive)
                    : p.ProductCategories;
            }, connectionResolver, arguments: new[] { new QueryArgument<BooleanGraphType> { Name = "activeOnly" } });

            // Data loader fields
            EfField("delayLoadedCreator", x => x.CreatedByUserId)
                .ThenResolve((context, createdByUserId) => {
                    var loader = CreateDelayedEntryLoader<int, User>(
                        ctx => ctx.DbContext.Users,
                        u => u.Id);
                    return loader.LoadAsync(context, createdByUserId);
                });

            EfField("delayLoadedCategories", x => x.Id)
                .ThenResolve((context, productId) => {
                    var loader = CreateDelayedListLoader<int, ProductCategory>(
                        ctx => ctx.DbContext.ProductCategories,
                        pc => pc.ProductId);
                    return loader.LoadAsync(context, productId);
                });

            EfField("delayLoadedCategoriesProjected", x => x.Id)
                .ThenResolve((context, productId) => {
                    var loader = CreateDelayedListLoader<int, ProductCategory, Category>(
                        ctx => ctx.DbContext.ProductCategories,
                        pc => pc.ProductId,
                        pc => pc.Category);
                    return loader.LoadAsync(context, productId);
                });

            // =================== Null Argument Tests ===================

            // Test ArgumentNullException for graph parameter being null
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfField<SampleDbContext, Product, int>(null!, x => x.Id));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfField<SampleDbContext, Product, int>(null!, "id", x => x.Id));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfField<SampleDbContext, Product, int>(null!, "id", (db, x) => x.Id));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfFieldFromContext<SampleDbContext, Product, int>(null!, "id", ctx => x => x.Id));

            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfIdField<SampleDbContext, Product, int>(null!, x => x.Id));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfIdField<SampleDbContext, Product, int>(null!, "id", x => x.Id));

            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationField<SampleDbContext, Product, User>(null!, x => x.CreatedByUser));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationField<SampleDbContext, Product, User>(null!, "user", x => x.CreatedByUser));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationField<SampleDbContext, Product, User>(null!, "user", (db, x) => x.CreatedByUser));

            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationListField<SampleDbContext, Product, ProductCategory>(null!, x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationListField<SampleDbContext, Product, ProductCategory>(null!, "categories", x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationListField<SampleDbContext, Product, ProductCategory>(null!, "categories", (db, x) => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationListFieldFromContext<SampleDbContext, Product, ProductCategory>(null!, "categories", ctx => x => x.ProductCategories));

            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionField<SampleDbContext, Product, ProductCategory>(null!, x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionField<SampleDbContext, Product, ProductCategory>(null!, x => x.ProductCategories, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionField<SampleDbContext, Product, ProductCategory>(null!, "categories", x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionField<SampleDbContext, Product, ProductCategory>(null!, "categories", x => x.ProductCategories, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionField<SampleDbContext, Product, ProductCategory>(null!, "categories", (db, x) => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionField<SampleDbContext, Product, ProductCategory>(null!, "categories", (db, x) => x.ProductCategories, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionFieldFromContext<SampleDbContext, Product, ProductCategory>(null!, "categories", ctx => x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationConnectionFieldFromContext<SampleDbContext, Product, ProductCategory>(null!, "categories", ctx => x => x.ProductCategories, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));

            Assert.Throws<ArgumentNullException>(() => EfFieldHelpers.EfNavigationFieldLink<SampleDbContext, Product, User>(null!, "user", (db, x) => db.Users.Where(u => u.Id == x.CreatedByUserId)));

            // Test ArgumentNullException for expression parameter being null
            Assert.Throws<ArgumentNullException>(() => EfField<int>(null!));
            Assert.Throws<ArgumentNullException>(() => EfField("id", (Expression<Func<Product, int?>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfField("id", (Expression<Func<SampleDbContext, Product, int?>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfFieldFromContext<int>("id", null!));

            Assert.Throws<ArgumentNullException>(() => EfIdField<int>(null!));
            Assert.Throws<ArgumentNullException>(() => EfIdField<int>("id", null!));

            Assert.Throws<ArgumentNullException>(() => EfNavigationField((Expression<Func<Product, User?>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationField("user", (Expression<Func<Product, User?>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationField("user", (Expression<Func<SampleDbContext, Product, User?>>)null!));

            Assert.Throws<ArgumentNullException>(() => EfNavigationListField((Expression<Func<Product, IEnumerable<ProductCategory>>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationListField("categories", (Expression<Func<Product, IEnumerable<ProductCategory>>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationListField("categories", (Expression<Func<SampleDbContext, Product, IEnumerable<ProductCategory>>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationListFieldFromContext<ProductCategory>("categories", null!));

            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionField((Expression<Func<Product, IEnumerable<ProductCategory>>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionField((Expression<Func<Product, IEnumerable<ProductCategory>>>)null!, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionField("categories", (Expression<Func<Product, IEnumerable<ProductCategory>>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionField("categories", (Expression<Func<Product, IEnumerable<ProductCategory>>>)null!, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionField("categories", (Expression<Func<SampleDbContext, Product, IEnumerable<ProductCategory>>>)null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionField("categories", (Expression<Func<SampleDbContext, Product, IEnumerable<ProductCategory>>>)null!, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionFieldFromContext<ProductCategory>("categories", null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionFieldFromContext<ProductCategory>("categories", null!, new EfSimpleConnectionResolver<SampleDbContext, ProductCategory>(100)));

            Assert.Throws<ArgumentNullException>(() => EfNavigationFieldLink<User>("user", null!));

            // Test ArgumentNullException for name parameter being null/whitespace
            Assert.Throws<ArgumentNullException>(() => EfFieldFromContext<int>(null!, ctx => x => x.Id));
            Assert.Throws<ArgumentNullException>(() => EfFieldFromContext<int>("", ctx => x => x.Id));
            Assert.Throws<ArgumentNullException>(() => EfFieldFromContext<int>("  ", ctx => x => x.Id));

            Assert.Throws<ArgumentNullException>(() => EfNavigationListFieldFromContext<ProductCategory>(null!, ctx => x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfNavigationListFieldFromContext<ProductCategory>("", ctx => x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfNavigationListFieldFromContext<ProductCategory>("  ", ctx => x => x.ProductCategories));

            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionFieldFromContext<ProductCategory>(null!, ctx => x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionFieldFromContext<ProductCategory>("", ctx => x => x.ProductCategories));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionFieldFromContext<ProductCategory>("  ", ctx => x => x.ProductCategories));

            Assert.Throws<ArgumentNullException>(() => EfNavigationFieldLink<User>(null!, (db, x) => db.Users.Where(u => u.Id == x.CreatedByUserId)));
            Assert.Throws<ArgumentNullException>(() => EfNavigationFieldLink<User>("", (db, x) => db.Users.Where(u => u.Id == x.CreatedByUserId)));
            Assert.Throws<ArgumentNullException>(() => EfNavigationFieldLink<User>("  ", (db, x) => db.Users.Where(u => u.Id == x.CreatedByUserId)));

            // Test ArgumentNullException for resolveExpression parameter being null
            Assert.Throws<ArgumentNullException>(() => EfFieldFromContext<int>("id", null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationListFieldFromContext<ProductCategory>("categories", null!));
            Assert.Throws<ArgumentNullException>(() => EfNavigationConnectionFieldFromContext<ProductCategory>("categories", null!));

            // Test ArgumentException where field name cannot be inferred
            Assert.Throws<ArgumentException>(() => EfField(x => x.Equals(null)));
            Assert.Throws<ArgumentException>(() => EfIdField(x => x.Equals(null)));
            Assert.Throws<ArgumentException>(() => EfNavigationField(x => "dummy"));
            Assert.Throws<ArgumentException>(() => EfNavigationListField(x => x.ProductCategories.Where(pc => pc.ProductId == x.Id)));
            Assert.Throws<ArgumentException>(() => EfNavigationConnectionField(x => x.ProductCategories.Where(pc => pc.ProductId == x.Id)));
        }
    }

    private sealed class UserGraphType : EfObjectGraphType<SampleDbContext, User>
    {
        public UserGraphType()
        {
            Name = "User";
            EfField(x => x.Id);
            EfField(x => x.Username);
            EfField(x => x.Email);
            EfField(x => x.IsActive);
        }
    }

    private sealed class CategoryGraphType : EfObjectGraphType<SampleDbContext, Category>
    {
        public CategoryGraphType()
        {
            Name = "Category";
            EfField(x => x.Id);
            EfField(x => x.Name);
            EfField(x => x.Description, nullable: true);
            EfField(x => x.IsActive);
        }
    }

    private sealed class ProductCategoryGraphType : EfObjectGraphType<SampleDbContext, ProductCategory>
    {
        public ProductCategoryGraphType()
        {
            Name = "ProductCategory";
            EfField(x => x.ProductId);
            EfField(x => x.CategoryId);
            EfNavigationField(x => x.Product);
            EfNavigationField(x => x.Category);
        }
    }

    private sealed class OrderItemGraphType : EfObjectGraphType<SampleDbContext, OrderItem>
    {
        public OrderItemGraphType()
        {
            Name = "OrderItem";
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

    // =================== EfField Tests ===================

    [TestMethod]
    public async Task EfField_BasicProperty()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { id name price }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfField_WithCustomName()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { productName }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfField_NullableProperty()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { description nullableDescription }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfField_NullableProperty_WithNullValue()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 6) { description nullableDescription }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfField_NonNullableProperty_WithNullValue()
    {
        ExpectFailure = true;
        var json = await ExecuteQueryAsync("""
            {
                product(id: 6) { nonNullableDescription }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfField_WithExplicitGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { priceAsFloat }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfField_WithDbContext()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { createdByUserName }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfFieldFromContext_WithArgument()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    withDescription: dynamicField(includeDescription: true)
                    withoutDescription: dynamicField(includeDescription: false)
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfIdField Tests ===================

    [TestMethod]
    public async Task EfIdField_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { id }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfIdField_WithCustomName()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { productId }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfIdField_Nullable()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { nullableId }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfNavigationField Tests ===================

    [TestMethod]
    public async Task EfNavigationField_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    createdByUser { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationField_WithCustomName()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    creator { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationField_NonNullable()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    nonNullableCreator { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationField_Nullable()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    nullableCreator { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationField_WithExplicitGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    creatorWithType { id username email }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationField_WithDbContext()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    createdBy { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationFieldLink_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    createdByLink { id username email }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfNavigationListField Tests ===================

    [TestMethod]
    public async Task EfNavigationListField_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    productCategories { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationListField_WithCustomName()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    categories { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationListField_WithExplicitGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    categoriesWithType { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationListField_WithDbContext()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    productCategoriesFromDb { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationListField_CategoriesFromProductCategories()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    categoriesFromProductCategories { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationListFieldFromContext_WithFilter()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    all: filteredCategories(activeOnly: false) { categoryId }
                    activeOnly: filteredCategories(activeOnly: true) { categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== EfNavigationConnectionField Tests ===================

    [TestMethod]
    public async Task EfNavigationConnectionField_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    productCategoriesConnection(first: 2) {
                        edges { node { productId categoryId } }
                        totalCount
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_OrderItems()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    orderItems(first: 2) {
                        edges { node { orderId productId } }
                        totalCount
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_WithCustomPageSize()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    productCategoriesConnection {
                        edges { node { productId categoryId } }
                        pageInfo { hasNextPage }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_WithCustomName()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    categoriesConnection(first: 3) {
                        edges { node { productId categoryId } }
                        totalCount
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_WithConnectionResolver()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    categoriesWithResolver {
                        edges { node { productId categoryId } }
                        pageInfo { hasNextPage }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_WithConnectionResolverNoName()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    categoriesWithResolverNoName {
                        edges { node { orderId productId } }
                        pageInfo { hasNextPage }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_WithDbContext()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    productCategoriesFromDbConnection(first: 2) {
                        edges { node { productId categoryId } }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_WithDbContextAndResolver()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    categoriesFromDbWithResolver {
                        edges { node { productId categoryId } }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_ProjectedCategories()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    categoriesConnectionProjected(first: 3) {
                        edges { node { id name } }
                        totalCount
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionFieldFromContext_WithFilter()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    all: filteredCategoriesConnection(activeOnly: false, first: 5) {
                        edges { node { categoryId } }
                        totalCount
                    }
                    activeOnly: filteredCategoriesConnection(activeOnly: true, first: 5) {
                        edges { node { categoryId } }
                        totalCount
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionFieldFromContext_WithResolver()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    filteredCategoriesWithResolver(activeOnly: true) {
                        edges { node { categoryId } }
                        pageInfo { hasNextPage }
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Data Loader Tests ===================

    [TestMethod]
    public async Task CreateDelayedEntryLoader_LoadsSingleEntity()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadedCreator { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task CreateDelayedListLoader_LoadsEntityList()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadedCategories { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task CreateDelayedListLoader_WithProjection()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadedCategoriesProjected { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfNavigationConnectionField_TypenameOnly()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    orderItems(first: 2) {
                        __typename
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleField_WithInlineFragment()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    id
                    name
                    ... on Product {
                        id
                        price
                    }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task EfSingleField_WithNamedFragment()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) {
                    id
                    name
                    ...ProductFields
                }
            }
            
            fragment ProductFields on Product {
                id
                price
                description
            }
            """);
        json.ShouldMatchApproved();
    }
}
