// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Globalization;
using GraphQL.Linq;

namespace Tests;

[TestClass]
public class EfFieldBuilderTests : QueryTestBase
{
    public EfFieldBuilderTests()
    {
        QueryType = typeof(Query);
        MappedGraphTypes.Add(typeof(ProductGraphType));
        MappedGraphTypes.Add(typeof(UserGraphType));
        MappedGraphTypes.Add(typeof(CategoryGraphType));
        MappedGraphTypes.Add(typeof(ProductCategoryGraphType));
    }

    private sealed class Query : QueryGraphType<SampleDbContext>
    {
        public Query(IEfGraphQLService<SampleDbContext> efGraphQLService) : base(efGraphQLService)
        {
            EfSingleField("product", context => context.DbContext.Products);
        }
    }

    // =================== Test Graph Types ===================

    private sealed class ProductGraphType : EfObjectGraphType<SampleDbContext, Product>
    {
        public ProductGraphType()
        {
            Name = "Product";

            // Basic fields for reference
            EfField(x => x.Id);
            EfField(x => x.Name);
            EfField(x => x.CreatedByUserId);

            // =================== DelayLoadEntry Tests ===================

            // DelayLoadEntry - basic (sync)
            EfField("delayLoadEntryBasic", x => x.CreatedByUserId)
                .DelayLoadEntry<User>(
                    ctx => ctx.DbContext.Users,
                    u => u.Id);

            // DelayLoadEntry - with explicit graph type (sync)
            EfField("delayLoadEntryWithGraphType", x => x.CreatedByUserId)
                .DelayLoadEntry<User>(
                    ctx => ctx.DbContext.Users,
                    u => u.Id,
                    typeof(UserGraphType));

            // DelayLoadEntry - async
            EfField("delayLoadEntryAsync", x => x.CreatedByUserId)
                .DelayLoadEntry<User>(
                    ctx => Task.FromResult(ctx.DbContext.Users.AsQueryable()),
                    u => u.Id);

            // DelayLoadEntry - with projection (sync)
            EfField("delayLoadEntryWithProjection", x => x.CreatedByUserId)
                .DelayLoadEntry<User, User>(
                    ctx => ctx.DbContext.Users,
                    u => u.Id,
                    u => u);

            // DelayLoadEntry - with projection and graph type (sync)
            EfField("delayLoadEntryWithProjectionAndGraphType", x => x.CreatedByUserId)
                .DelayLoadEntry<User, User>(
                    ctx => ctx.DbContext.Users,
                    u => u.Id,
                    u => u,
                    typeof(UserGraphType));

            // DelayLoadEntry - with projection (async)
            EfField("delayLoadEntryWithProjectionAsync", x => x.CreatedByUserId)
                .DelayLoadEntry<User, User>(
                    ctx => Task.FromResult(ctx.DbContext.Users.AsQueryable()),
                    u => u.Id,
                    u => u);

            // DelayLoadEntry - with projection and graph type (async)
            EfField("delayLoadEntryWithProjectionAndGraphTypeAsync", x => x.CreatedByUserId)
                .DelayLoadEntry<User, User>(
                    ctx => Task.FromResult(ctx.DbContext.Users.AsQueryable()),
                    u => u.Id,
                    u => u,
                    typeof(UserGraphType));

            // =================== DelayLoadList Tests ===================

            // DelayLoadList - basic (sync)
            EfField("delayLoadListBasic", x => x.Id)
                .DelayLoadList<ProductCategory>(
                    ctx => ctx.DbContext.ProductCategories,
                    pc => pc.ProductId);

            // DelayLoadList - with explicit graph type (sync)
            EfField("delayLoadListWithGraphType", x => x.Id)
                .DelayLoadList<ProductCategory>(
                    ctx => ctx.DbContext.ProductCategories,
                    pc => pc.ProductId,
                    typeof(ListGraphType<ProductCategoryGraphType>));

            // DelayLoadList - async
            EfField("delayLoadListAsync", x => x.Id)
                .DelayLoadList<ProductCategory>(
                    ctx => Task.FromResult(ctx.DbContext.ProductCategories.AsQueryable()),
                    pc => pc.ProductId);

            // DelayLoadList - with projection (sync)
            EfField("delayLoadListWithProjection", x => x.Id)
                .DelayLoadList<ProductCategory, Category>(
                    ctx => ctx.DbContext.ProductCategories,
                    pc => pc.ProductId,
                    pc => pc.Category);

            // DelayLoadList - with projection and graph type (sync)
            EfField("delayLoadListWithProjectionAndGraphType", x => x.Id)
                .DelayLoadList<ProductCategory, Category>(
                    ctx => ctx.DbContext.ProductCategories,
                    pc => pc.ProductId,
                    pc => pc.Category,
                    typeof(ListGraphType<CategoryGraphType>));

            // DelayLoadList - with projection (async)
            EfField("delayLoadListWithProjectionAsync", x => x.Id)
                .DelayLoadList<ProductCategory, Category>(
                    ctx => Task.FromResult(ctx.DbContext.ProductCategories.AsQueryable()),
                    pc => pc.ProductId,
                    pc => pc.Category);

            // DelayLoadList - with projection and graph type (async)
            EfField("delayLoadListWithProjectionAndGraphTypeAsync", x => x.Id)
                .DelayLoadList<ProductCategory, Category>(
                    ctx => Task.FromResult(ctx.DbContext.ProductCategories.AsQueryable()),
                    pc => pc.ProductId,
                    pc => pc.Category,
                    typeof(ListGraphType<CategoryGraphType>));

            // =================== ThenResolve Tests ===================

            // ThenResolve - simple function
            EfField("thenResolveSimple", x => x.Name)
                .ThenResolve(name => name.ToUpper(CultureInfo.InvariantCulture));

            // ThenResolve - simple function with nullable
            EfField("thenResolveSimpleNullable", x => x.Description)
                .ThenResolve(desc => desc?.ToUpper(CultureInfo.InvariantCulture), nullable: true);

            // ThenResolve - simple function with graph type
            EfField("thenResolveSimpleWithGraphType", x => x.Name)
                .ThenResolve(name => name.Length, graphType: typeof(IntGraphType));

            // ThenResolve - with context
            EfField("thenResolveWithContext", x => x.Name)
                .ThenResolve((ctx, name) => {
                    var prefix = ctx.GetArgument<string>("prefix", "Product: ");
                    return prefix + name;
                })
                .Argument<StringGraphType>("prefix");

            // ThenResolve - with context and nullable
            EfField("thenResolveWithContextNullable", x => x.Description)
                .ThenResolve((ctx, desc) => desc?.ToUpper(CultureInfo.InvariantCulture), nullable: true);

            // ThenResolve - with context and graph type
            EfField("thenResolveWithContextAndGraphType", x => x.Name)
                .ThenResolve((ctx, name) => name.Length, graphType: typeof(IntGraphType));

            // ThenResolve - async
            EfField("thenResolveAsync", x => x.Name)
                .ThenResolve((ctx, name) => Task.FromResult(name.ToUpper(CultureInfo.InvariantCulture)));

            // ThenResolve - async with nullable
            EfField("thenResolveAsyncNullable", x => x.Description)
                .ThenResolve((ctx, desc) => Task.FromResult(desc?.ToUpper(CultureInfo.InvariantCulture)), nullable: true);

            // ThenResolve - async with graph type
            EfField("thenResolveAsyncWithGraphType", x => x.Name)
                .ThenResolve((ctx, name) => Task.FromResult(name.Length), graphType: typeof(IntGraphType));

            // =================== Argument Tests ===================

            // Argument - with Type
            EfField("argumentWithType", x => x.Name)
                .Argument(typeof(StringGraphType), "filter");

            // Argument - with Type and configure
            EfField("argumentWithTypeAndConfigure", x => x.Name)
                .Argument<StringGraphType>("filter", arg => arg.Description = "Filter description");

            // Argument - with IGraphType
            EfField("argumentWithIGraphType", x => x.Name)
                .Argument(new StringGraphType(), "filter");

            // Argument - with IGraphType and configure
            EfField("argumentWithIGraphTypeAndConfigure", x => x.Name)
                .Argument(new StringGraphType(), "filter", arg => arg.Description = "Filter description");

            // Argument - generic CLR type
            EfField("argumentGenericClrType", x => x.Name)
                .Argument<string>("filter");

            // Argument - generic CLR type with nullable
            EfField("argumentGenericClrTypeNullable", x => x.Name)
                .Argument<string>("filter", nullable: true);

            // Argument - generic CLR type with configure
            EfField("argumentGenericClrTypeWithConfigure", x => x.Name)
                .Argument<string>("filter", nullable: false, arg => arg.Description = "Filter description");

            // Argument - generic CLR type with description
            EfField("argumentGenericClrTypeWithDescription", x => x.Name)
                .Argument<string>("filter", nullable: false, "Filter description");

            // Argument - generic CLR type with description and configure
            EfField("argumentGenericClrTypeWithDescriptionAndConfigure", x => x.Name)
                .Argument<string>("filter", nullable: false, "Filter description", arg => arg.DefaultValue = "default");

            // Argument - generic graph type
            EfField("argumentGenericGraphType", x => x.Name)
                .Argument<StringGraphType>("filter");

            // Argument - generic graph type with configure
            EfField("argumentGenericGraphTypeWithConfigure", x => x.Name)
                .Argument<StringGraphType>("filter", arg => arg.Description = "Filter description");

            // Argument - generic graph type with description
            EfField("argumentGenericGraphTypeWithDescription", x => x.Name)
                .Argument<StringGraphType>("filter", "Filter description");

            // Argument - generic graph type with description and configure
            EfField("argumentGenericGraphTypeWithDescriptionAndConfigure", x => x.Name)
                .Argument<StringGraphType>("filter", "Filter description", arg => arg.DefaultValue = "default");

            // =================== Arguments Tests ===================

            // Arguments - with IEnumerable
            EfField("argumentsWithIEnumerable", x => x.Name)
                .Arguments(new List<QueryArgument> {
                    new QueryArgument<StringGraphType> { Name = "filter1" },
                    new QueryArgument<IntGraphType> { Name = "filter2" }
                });

            // Arguments - with params array
            EfField("argumentsWithParams", x => x.Name)
                .Arguments(
                    new QueryArgument<StringGraphType> { Name = "filter1" },
                    new QueryArgument<IntGraphType> { Name = "filter2" }
                );

            // =================== Description Test ===================

            // Description
            EfField("fieldWithDescription", x => x.Name)
                .Description("This is a field description");

            // Description - null
            EfField("fieldWithNullDescription", x => x.Name)
                .Description(null);

            // =================== DeprecationReason Test ===================

            // DeprecationReason
            EfField("fieldWithDeprecation", x => x.Name)
                .DeprecationReason("This field is deprecated");

            // DeprecationReason - null
            EfField("fieldWithNullDeprecation", x => x.Name)
                .DeprecationReason(null);

            // =================== Configure Test ===================

            // Configure
            EfField("fieldWithConfigure", x => x.Name)
                .Configure(field => field.Metadata["custom"] = "value");

            // =================== Chained Methods Tests ===================

            // Multiple methods chained
            EfField("chainedMethods", x => x.Name)
                .Description("A chained field")
                .Argument<StringGraphType>("filter")
                .ThenResolve((ctx, name) => {
                    var filter = ctx.GetArgument<string>("filter");
                    return string.IsNullOrEmpty(filter) ? name : name + " - " + filter;
                });

            // Complex chain with DelayLoadEntry
            EfField("complexChainWithDelayLoad", x => x.CreatedByUserId)
                .Description("Complex delayed load")
                .DelayLoadEntry(
                    ctx => ctx.DbContext.Users,
                    u => u.Id,
                    typeof(UserGraphType));

            // Complex chain with ThenResolve and multiple arguments
            EfField("complexChainWithThenResolve", x => x.Price)
                .Description("Price with markup")
                .Argument<FloatGraphType>("markup", "Markup percentage")
                .Argument<BooleanGraphType>("includeSymbol", "Include currency symbol")
                .ThenResolve((ctx, price) => {
                    var markup = ctx.GetArgument<decimal>("markup", 0);
                    var includeSymbol = ctx.GetArgument<bool>("includeSymbol", false);
                    var finalPrice = price * (1 + markup / 100);
                    return includeSymbol ? "$" + finalPrice.ToString("F2", CultureInfo.InvariantCulture) : finalPrice.ToString("F2", CultureInfo.InvariantCulture);
                });
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
        }
    }

    private sealed class CategoryGraphType : EfObjectGraphType<SampleDbContext, Category>
    {
        public CategoryGraphType()
        {
            Name = "Category";
            EfField(x => x.Id);
            EfField(x => x.Name);
        }
    }

    private sealed class ProductCategoryGraphType : EfObjectGraphType<SampleDbContext, ProductCategory>
    {
        public ProductCategoryGraphType()
        {
            Name = "ProductCategory";
            EfField(x => x.ProductId);
            EfField(x => x.CategoryId);
        }
    }

    // =================== Schema SDL Test ===================

    [TestMethod]
    public async Task GetSDL()
    {
        var sdl = await GetSchemaSDLAsync();
        sdl.ShouldMatchApproved();
    }

    // =================== DelayLoadEntry Tests ===================

    [TestMethod]
    public async Task DelayLoadEntry_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadEntryBasic { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadEntry_WithGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadEntryWithGraphType { id username email }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadEntry_Async()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadEntryAsync { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadEntry_WithProjection()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadEntryWithProjection { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadEntry_WithProjectionAndGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadEntryWithProjectionAndGraphType { id username email }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadEntry_WithProjectionAsync()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadEntryWithProjectionAsync { id username }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadEntry_WithProjectionAndGraphTypeAsync()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadEntryWithProjectionAndGraphTypeAsync { id username email }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== DelayLoadList Tests ===================

    [TestMethod]
    public async Task DelayLoadList_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadListBasic { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadList_WithGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadListWithGraphType { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadList_Async()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadListAsync { productId categoryId }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadList_WithProjection()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadListWithProjection { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadList_WithProjectionAndGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadListWithProjectionAndGraphType { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadList_WithProjectionAsync()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadListWithProjectionAsync { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task DelayLoadList_WithProjectionAndGraphTypeAsync()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    delayLoadListWithProjectionAndGraphTypeAsync { id name }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== ThenResolve Tests ===================

    [TestMethod]
    public async Task ThenResolve_Simple()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveSimple }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_SimpleNullable()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveSimpleNullable }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_SimpleNullable_WithNullValue()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 6) { thenResolveSimpleNullable }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_SimpleWithGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveSimpleWithGraphType }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_WithContext()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    withPrefix: thenResolveWithContext(prefix: "Item: ")
                    withoutPrefix: thenResolveWithContext
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_WithContextNullable()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveWithContextNullable }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_WithContextAndGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveWithContextAndGraphType }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_Async()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveAsync }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_AsyncNullable()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveAsyncNullable }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ThenResolve_AsyncWithGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { thenResolveAsyncWithGraphType }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Argument Tests ===================

    [TestMethod]
    public async Task Argument_WithType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { argumentWithType }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task Argument_WithTypeAndConfigure()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { argumentWithTypeAndConfigure }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task Argument_GenericClrType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { argumentGenericClrType(filter: "abc") }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task Argument_GenericGraphType()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { argumentGenericGraphType }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Arguments Tests ===================

    [TestMethod]
    public async Task Arguments_WithIEnumerable()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { argumentsWithIEnumerable }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task Arguments_WithParams()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { argumentsWithParams }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Description Tests ===================

    [TestMethod]
    public async Task Description_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { fieldWithDescription }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== DeprecationReason Tests ===================

    [TestMethod]
    public async Task DeprecationReason_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { fieldWithDeprecation }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Configure Tests ===================

    [TestMethod]
    public async Task Configure_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { fieldWithConfigure }
            }
            """);
        json.ShouldMatchApproved();
    }

    // =================== Chained Methods Tests ===================

    [TestMethod]
    public async Task ChainedMethods_Basic()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    withFilter: chainedMethods(filter: "filtered")
                    withoutFilter: chainedMethods
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ChainedMethods_ComplexWithDelayLoad()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    complexChainWithDelayLoad { id username email }
                }
            }
            """);
        json.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ChainedMethods_ComplexWithThenResolve()
    {
        var json = await ExecuteQueryAsync("""
            {
                product(id: 1) { 
                    withMarkup: complexChainWithThenResolve(markup: 10, includeSymbol: true)
                    withoutMarkup: complexChainWithThenResolve(includeSymbol: false)
                }
            }
            """);
        json.ShouldMatchApproved();
    }
}
