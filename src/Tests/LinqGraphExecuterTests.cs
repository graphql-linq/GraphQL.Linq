// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Types.Relay.DataObjects;

namespace Tests;

[TestClass]
public class LinqGraphExecuterTests
{
    // Custom test DbContext type for testing
    private sealed class TestDbContext
    {
    }

    // Custom test entity type
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    // Custom context type provider for testing
    private sealed class TestDbContextTypeProvider : IEfDbContextTypeProvider
    {
        public Type? GetDbContextType(IQueryable queryable)
        {
            // Return TestDbContext type for all queryables in tests
            return typeof(TestDbContext);
        }
    }

    // Test implementation of ILinqGraphExecuter<TestDbContext>
    private sealed class TestLinqGraphExecuter : ILinqGraphExecuter<TestDbContext>
    {
        public IResolveFieldContext? LastContext { get; private set; }
        public IQueryable? LastQuery { get; private set; }
        public string? LastMethodCalled { get; private set; }
        public object? LastAdditionalParameter { get; private set; }

        public Task<EfSource<TReturn>> ExecuteSingleAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            LastContext = context;
            LastQuery = query;
            LastMethodCalled = nameof(ExecuteSingleAsync);
            return Task.FromResult(new EfSource<TReturn> { ["Id"] = 1, ["Name"] = "ExecuteSingleAsync" });
        }

        public Task<EfSource<TReturn>?> ExecuteSingleOrDefaultAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            LastContext = context;
            LastQuery = query;
            LastMethodCalled = nameof(ExecuteSingleOrDefaultAsync);
            return Task.FromResult<EfSource<TReturn>?>(new EfSource<TReturn> { ["Id"] = 2, ["Name"] = "ExecuteSingleOrDefaultAsync" });
        }

        public Task<IList<EfSource<TReturn>>> ExecuteQueryAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            LastContext = context;
            LastQuery = query;
            LastMethodCalled = nameof(ExecuteQueryAsync);
            return Task.FromResult<IList<EfSource<TReturn>>>(new List<EfSource<TReturn>>
            {
                new() { ["Id"] = 3, ["Name"] = "ExecuteQueryAsync" }
            });
        }

        public Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int defaultPageSize = 100) where TReturn : class
        {
            LastContext = context;
            LastQuery = query;
            LastMethodCalled = nameof(ExecuteConnectionAsync) + "_DefaultPageSize";
            LastAdditionalParameter = defaultPageSize;
            return Task.FromResult(new Connection<EfSource<TReturn>> {
                TotalCount = 1,
                PageInfo = new PageInfo { HasNextPage = false, HasPreviousPage = false }
            });
        }

        public Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, int defaultPageSize = 100) where TReturn : class
        {
            LastContext = context;
            LastQuery = query;
            LastMethodCalled = nameof(ExecuteConnectionAsync) + "_ExplicitParameters";
            LastAdditionalParameter = new { first, last, after, before, defaultPageSize };
            return Task.FromResult(new Connection<EfSource<TReturn>> {
                TotalCount = 2,
                PageInfo = new PageInfo { HasNextPage = true, HasPreviousPage = false }
            });
        }

        public Task<IList<Tuple<TKey, EfSource<TReturn>>>> ExecuteQueryForKeysAsync<TKey, TObject, TReturn>(IResolveFieldContext context, IQueryable<TObject> query, System.Linq.Expressions.Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys, System.Linq.Expressions.Expression<Func<TObject, TReturn>> itemSelector) where TObject : class where TReturn : class
        {
            LastContext = context;
            LastQuery = query;
            LastMethodCalled = nameof(ExecuteQueryForKeysAsync);
            LastAdditionalParameter = new { keySelector, keys, itemSelector };
            return Task.FromResult<IList<Tuple<TKey, EfSource<TReturn>>>>(new List<Tuple<TKey, EfSource<TReturn>>>
            {
                Tuple.Create((TKey)(object)1, new EfSource<TReturn> { ["Id"] = 4, ["Name"] = "ExecuteQueryForKeysAsync" })
            });
        }
    }

    // Test implementation that returns null for ExecuteSingleOrDefaultAsync
    private sealed class TestLinqGraphExecuterWithNull : ILinqGraphExecuter<TestDbContext>
    {
        public Task<EfSource<TReturn>> ExecuteSingleAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            return Task.FromResult(new EfSource<TReturn>());
        }

        public Task<EfSource<TReturn>?> ExecuteSingleOrDefaultAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            return Task.FromResult<EfSource<TReturn>?>(null);
        }

        public Task<IList<EfSource<TReturn>>> ExecuteQueryAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            return Task.FromResult<IList<EfSource<TReturn>>>(new List<EfSource<TReturn>>());
        }

        public Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, int defaultPageSize = 100) where TReturn : class
        {
            return Task.FromResult(new Connection<EfSource<TReturn>>());
        }

        public Task<IList<Tuple<TKey, EfSource<TReturn>>>> ExecuteQueryForKeysAsync<TKey, TObject, TReturn>(IResolveFieldContext context, IQueryable<TObject> query, System.Linq.Expressions.Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys, System.Linq.Expressions.Expression<Func<TObject, TReturn>> itemSelector) where TObject : class where TReturn : class
        {
            return Task.FromResult<IList<Tuple<TKey, EfSource<TReturn>>>>(new List<Tuple<TKey, EfSource<TReturn>>>());
        }
    }

    private static ResolveFieldContext CreateContext(IServiceProvider serviceProvider)
    {
        return new ResolveFieldContext {
            RequestServices = serviceProvider,
            CancellationToken = CancellationToken.None
        };
    }

    private static ServiceProvider CreateServiceProvider(TestLinqGraphExecuter testExecuter)
    {
        var services = new ServiceCollection();

        // Register the custom context type provider
        services.AddSingleton<IEfDbContextTypeProvider, TestDbContextTypeProvider>();

        // Register the non-generic LinqGraphExecuter
        services.AddSingleton<ILinqGraphExecuter, LinqGraphExecuter>();

        // Register the test typed executer
        services.AddSingleton<ILinqGraphExecuter<TestDbContext>>(testExecuter);

        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task ExecuteSingleAsync_PassesThroughToTypedExecuter()
    {
        // Arrange
        var testExecuter = new TestLinqGraphExecuter();
        var serviceProvider = CreateServiceProvider(testExecuter);
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();
        var testQuery = new List<TestEntity>().AsQueryable();

        // Act
        var result = await executer.ExecuteSingleAsync(context, testQuery);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("ExecuteSingleAsync", result["Name"]);
        Assert.AreSame(context, testExecuter.LastContext);
        Assert.AreSame(testQuery, testExecuter.LastQuery);
        Assert.AreEqual(nameof(testExecuter.ExecuteSingleAsync), testExecuter.LastMethodCalled);
    }

    [TestMethod]
    public async Task ExecuteSingleOrDefaultAsync_PassesThroughToTypedExecuter()
    {
        // Arrange
        var testExecuter = new TestLinqGraphExecuter();
        var serviceProvider = CreateServiceProvider(testExecuter);
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();
        var testQuery = new List<TestEntity>().AsQueryable();

        // Act
        var result = await executer.ExecuteSingleOrDefaultAsync(context, testQuery);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("ExecuteSingleOrDefaultAsync", result["Name"]);
        Assert.AreSame(context, testExecuter.LastContext);
        Assert.AreSame(testQuery, testExecuter.LastQuery);
        Assert.AreEqual(nameof(testExecuter.ExecuteSingleOrDefaultAsync), testExecuter.LastMethodCalled);
    }

    [TestMethod]
    public async Task ExecuteSingleOrDefaultAsync_ReturnsNull_PassesThroughToTypedExecuter()
    {
        // Arrange
        var testExecuter = new TestLinqGraphExecuterWithNull();
        var services = new ServiceCollection();
        services.AddSingleton<IEfDbContextTypeProvider, TestDbContextTypeProvider>();
        services.AddSingleton<ILinqGraphExecuter, LinqGraphExecuter>();
        services.AddSingleton<ILinqGraphExecuter<TestDbContext>>(testExecuter);
        var serviceProvider = services.BuildServiceProvider();

        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();
        var testQuery = new List<TestEntity>().AsQueryable();

        // Act
        var result = await executer.ExecuteSingleOrDefaultAsync(context, testQuery);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ExecuteQueryAsync_PassesThroughToTypedExecuter()
    {
        // Arrange
        var testExecuter = new TestLinqGraphExecuter();
        var serviceProvider = CreateServiceProvider(testExecuter);
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();
        var testQuery = new List<TestEntity>().AsQueryable();

        // Act
        var result = await executer.ExecuteQueryAsync(context, testQuery);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual("ExecuteQueryAsync", result[0]["Name"]);
        Assert.AreSame(context, testExecuter.LastContext);
        Assert.AreSame(testQuery, testExecuter.LastQuery);
        Assert.AreEqual(nameof(testExecuter.ExecuteQueryAsync), testExecuter.LastMethodCalled);
    }

    [TestMethod]
    public async Task ExecuteConnectionAsync_WithExplicitParameters_PassesThroughToTypedExecuter()
    {
        // Arrange
        var testExecuter = new TestLinqGraphExecuter();
        var serviceProvider = CreateServiceProvider(testExecuter);
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();
        var testQuery = new List<TestEntity>().AsQueryable();
        int? first = 10;
        int? last = null;
        string? after = "cursor1";
        string? before = null;
        const int defaultPageSize = 50;

        // Act
        var result = await executer.ExecuteConnectionAsync<object, TestEntity>(
            context, testQuery, first, last, after, before, defaultPageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.TotalCount);
        Assert.IsTrue(result.PageInfo?.HasNextPage ?? false);
        Assert.AreSame(context, testExecuter.LastContext);
        Assert.AreSame(testQuery, testExecuter.LastQuery);
        Assert.AreEqual("ExecuteConnectionAsync_ExplicitParameters", testExecuter.LastMethodCalled);

        // Verify the parameters were passed through
        var parameters = testExecuter.LastAdditionalParameter as dynamic;
        Assert.IsNotNull(parameters);
        Assert.AreEqual(first, parameters!.first);
        Assert.AreEqual(last, parameters.last);
        Assert.AreEqual(after, parameters.after);
        Assert.AreEqual(before, parameters.before);
        Assert.AreEqual(defaultPageSize, parameters.defaultPageSize);
    }

    [TestMethod]
    public async Task ExecuteQueryForKeysAsync_PassesThroughToTypedExecuter()
    {
        // Arrange
        var testExecuter = new TestLinqGraphExecuter();
        var serviceProvider = CreateServiceProvider(testExecuter);
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();
        var testQuery = new List<TestEntity>().AsQueryable();
        System.Linq.Expressions.Expression<Func<TestEntity, int>> keySelector = x => x.Id;
        var keys = new[] { 1, 2 };
        System.Linq.Expressions.Expression<Func<TestEntity, TestEntity>> itemSelector = x => x;

        // Act
        var result = await executer.ExecuteQueryForKeysAsync(
            context, testQuery, keySelector, keys, itemSelector);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual("ExecuteQueryForKeysAsync", result[0].Item2["Name"]);
        Assert.AreSame(context, testExecuter.LastContext);
        Assert.AreSame(testQuery, testExecuter.LastQuery);
        Assert.AreEqual(nameof(testExecuter.ExecuteQueryForKeysAsync), testExecuter.LastMethodCalled);
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentNullException_WhenProviderIsNull()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => new LinqGraphExecuter(null!));
    }

    [TestMethod]
    public async Task ExecuteSingleAsync_ThrowsInvalidOperationException_WhenContextTypeCannotBeDetermined()
    {
        // Arrange
        var services = new ServiceCollection();
        // Register a provider that returns null for all queryables
        services.AddSingleton<IEfDbContextTypeProvider, NullContextTypeProvider>();
        services.AddSingleton<ILinqGraphExecuter, LinqGraphExecuter>();
        // Note: NOT registering any ILinqGraphExecuter<T> because we expect the provider to return null

        var serviceProvider = services.BuildServiceProvider();
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();

        // Use a regular queryable
        var regularQuery = new List<TestEntity>().AsQueryable();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await executer.ExecuteSingleAsync(context, regularQuery));

        Assert.Contains("Unable to determine DbContext type", exception.Message);
    }

    private sealed class NullContextTypeProvider : IEfDbContextTypeProvider
    {
        public Type? GetDbContextType(IQueryable queryable) => null;
    }

    [TestMethod]
    public async Task ExecuteSingleAsync_ThrowsInvalidOperationException_WhenRequestServicesIsNull()
    {
        // Arrange
        var testExecuter = new TestLinqGraphExecuter();
        var serviceProvider = CreateServiceProvider(testExecuter);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();

        var context = CreateContext(null!);
        var testQuery = new List<TestEntity>().AsQueryable();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await executer.ExecuteSingleAsync(context, testQuery));

        Assert.Contains("does not have a valid RequestServices", exception.Message);
    }

    [TestMethod]
    public async Task ExecuteSingleAsync_ThrowsInvalidOperationException_WhenTypedExecuterNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEfDbContextTypeProvider, TestDbContextTypeProvider>();
        services.AddSingleton<ILinqGraphExecuter, LinqGraphExecuter>();
        // Note: NOT registering ILinqGraphExecuter<TestDbContext>

        var serviceProvider = services.BuildServiceProvider();
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();

        var testQuery = new List<TestEntity>().AsQueryable();

        // Act & Assert
        _ = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await executer.ExecuteSingleAsync(context, testQuery));
    }

    [TestMethod]
    public async Task AllMethods_UseCorrectTypedExecuter_WhenMultipleContextTypesRegistered()
    {
        // Arrange - Register two different context types
        var testExecuter1 = new TestLinqGraphExecuter();
        var testExecuter2 = new OtherTestLinqGraphExecuter();

        var services = new ServiceCollection();
        services.AddSingleton<IEfDbContextTypeProvider, MultiContextTypeProvider>();
        services.AddSingleton<ILinqGraphExecuter, LinqGraphExecuter>();
        services.AddSingleton<ILinqGraphExecuter<TestDbContext>>(testExecuter1);
        services.AddSingleton<ILinqGraphExecuter<OtherTestDbContext>>(testExecuter2);

        var serviceProvider = services.BuildServiceProvider();
        var context = CreateContext(serviceProvider);
        var executer = serviceProvider.GetRequiredService<ILinqGraphExecuter>();

        var testQuery1 = new List<TestEntity>().AsQueryable();
        var testQuery2 = new List<TestEntity>().AsQueryable();

        // Act
        var result1 = await executer.ExecuteSingleAsync(context, testQuery1);
        var result2 = await executer.ExecuteSingleAsync(context, testQuery2);

        // Assert
        Assert.AreEqual("ExecuteSingleAsync", result1["Name"]);
        Assert.AreEqual("OtherExecuteSingleAsync", result2["Name"]);
        Assert.AreSame(testQuery1, testExecuter1.LastQuery);
        Assert.AreSame(testQuery2, testExecuter2.LastQuery);
    }

    // Additional test context and queryable for multi-context test
    private sealed class OtherTestDbContext
    {
    }

    private sealed class OtherTestLinqGraphExecuter : ILinqGraphExecuter<OtherTestDbContext>
    {
        public IQueryable? LastQuery { get; private set; }

        public Task<EfSource<TReturn>> ExecuteSingleAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            LastQuery = query;
            return Task.FromResult(new EfSource<TReturn> { ["Name"] = "OtherExecuteSingleAsync" });
        }

        public Task<EfSource<TReturn>?> ExecuteSingleOrDefaultAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            LastQuery = query;
            return Task.FromResult<EfSource<TReturn>?>(new EfSource<TReturn>());
        }

        public Task<IList<EfSource<TReturn>>> ExecuteQueryAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
        {
            LastQuery = query;
            return Task.FromResult<IList<EfSource<TReturn>>>(new List<EfSource<TReturn>>());
        }

        public Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, int defaultPageSize = 100) where TReturn : class
        {
            LastQuery = query;
            return Task.FromResult(new Connection<EfSource<TReturn>>());
        }

        public Task<IList<Tuple<TKey, EfSource<TReturn>>>> ExecuteQueryForKeysAsync<TKey, TObject, TReturn>(IResolveFieldContext context, IQueryable<TObject> query, System.Linq.Expressions.Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys, System.Linq.Expressions.Expression<Func<TObject, TReturn>> itemSelector) where TObject : class where TReturn : class
        {
            LastQuery = query;
            return Task.FromResult<IList<Tuple<TKey, EfSource<TReturn>>>>(new List<Tuple<TKey, EfSource<TReturn>>>());
        }
    }

    private sealed class MultiContextTypeProvider : IEfDbContextTypeProvider
    {
        private int _callCount;

        public Type? GetDbContextType(IQueryable queryable)
        {
            // Alternate between the two context types for testing
            _callCount++;
            return _callCount % 2 == 1 ? typeof(TestDbContext) : typeof(OtherTestDbContext);
        }
    }
}
