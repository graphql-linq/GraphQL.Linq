// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Runtime.CompilerServices;
using GraphQL.DataLoader;
using GraphQL.Linq.DataLoaders;
using GraphQL.Linq.GraphApi;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq;

/// <summary>
/// Extension methods for IQueryable to convert queries to GraphQL results.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Gets the ILinqGraphExecuter from the current GraphQL resolution context.
    /// </summary>
    /// <param name="resolveFieldContext">The resolved field context.</param>
    /// <returns>The ILinqGraphExecuter instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no IResolveFieldContext or RequestServices is available.</exception>
    private static ILinqGraphExecuter GetLinqGraphExecuter(out IResolveFieldContext resolveFieldContext)
    {
        var resolveFieldContextAccessor = Execution.ResolveFieldContextAccessor.Instance;
        resolveFieldContext = resolveFieldContextAccessor.Context
            ?? throw new InvalidOperationException("No IResolveFieldContext is available in the current context.");
        var requestServices = resolveFieldContext.RequestServices
            ?? throw new InvalidOperationException("No RequestServices is available in the current IResolveFieldContext.");
        var linqGraphExecuter = requestServices.GetRequiredService<ILinqGraphExecuter>();
        return linqGraphExecuter;
    }

    /// <summary>
    /// Executes the query and returns a list of EfSource-wrapped results for GraphQL.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of EfSource-wrapped entities.</returns>
    public static Task<IList<EfSource<T>>> ToGraphAsync<T>(this IQueryable<T> query)
        where T : class
    {
        var linqGraphExecuter = GetLinqGraphExecuter(out var resolveFieldContext);
        return linqGraphExecuter.ExecuteQueryAsync(resolveFieldContext, query);
    }

    /// <summary>
    /// Executes the query as a Relay-style connection with explicit pagination parameters.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="first">The number of items to return from the start of the result set.</param>
    /// <param name="last">The number of items to return from the end of the result set.</param>
    /// <param name="after">The cursor after which to start returning items.</param>
    /// <param name="before">The cursor before which to start returning items.</param>
    /// <param name="defaultPageSize">The default page size for pagination (default: 100).</param>
    /// <returns>A task that represents the asynchronous operation, containing a Connection of EfSource-wrapped entities.</returns>
    public static async Task<Connection<EfSource<T>>> ToGraphConnectionAsync<T>(this IQueryable<T> query, int? first, int? last, string? after, string? before, int defaultPageSize = 100)
        where T : class
    {
        var linqGraphExecuter = GetLinqGraphExecuter(out var resolveFieldContext);
        return await linqGraphExecuter.ExecuteConnectionAsync<object, T>(resolveFieldContext, query, first, last, after, before, defaultPageSize);
    }

    /// <summary>
    /// Executes the query as a Relay-style connection with explicit pagination parameters and a specific connection resolver.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="first">The number of items to return from the start of the result set.</param>
    /// <param name="last">The number of items to return from the end of the result set.</param>
    /// <param name="after">The cursor after which to start returning items.</param>
    /// <param name="before">The cursor before which to start returning items.</param>
    /// <param name="connectionResolver">The connection resolver to use for pagination logic.</param>
    /// <returns>A task that represents the asynchronous operation, containing a Connection of EfSource-wrapped entities.</returns>
    public static async Task<Connection<EfSource<T>>> ToGraphConnectionAsync<T>(this IQueryable<T> query, int? first, int? last, string? after, string? before, ConnectionResolvers.IEfConnectionResolver<T> connectionResolver)
        where T : class
    {
        var linqGraphExecuter = GetLinqGraphExecuter(out var resolveFieldContext);
        return await linqGraphExecuter.ExecuteConnectionAsync<object, T>(resolveFieldContext, query, first, last, after, before, connectionResolver);
    }

    /// <summary>
    /// Executes the query and returns a single EfSource-wrapped result or null if not found.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <returns>A task that represents the asynchronous operation, containing an EfSource-wrapped entity or null.</returns>
    public static Task<EfSource<T>?> ToGraphSingleOrDefaultAsync<T>(this IQueryable<T> query)
        where T : class
    {
        var linqGraphExecuter = GetLinqGraphExecuter(out var resolveFieldContext);
        return linqGraphExecuter.ExecuteSingleOrDefaultAsync(resolveFieldContext, query);
    }

    /// <summary>
    /// Executes the query with a predicate and returns a single EfSource-wrapped result or null if not found.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A task that represents the asynchronous operation, containing an EfSource-wrapped entity or null.</returns>
    public static Task<EfSource<T>?> ToGraphSingleOrDefaultAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        where T : class
        => query.Where(predicate).ToGraphSingleOrDefaultAsync();

    /// <summary>
    /// Executes the query and returns a single EfSource-wrapped result.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <returns>A task that represents the asynchronous operation, containing an EfSource-wrapped entity.</returns>
    /// <exception cref="ExecutionError">Thrown if no entity is found.</exception>
    public static Task<EfSource<T>> ToGraphSingleAsync<T>(this IQueryable<T> query)
        where T : class
    {
        var linqGraphExecuter = GetLinqGraphExecuter(out var resolveFieldContext);
        return linqGraphExecuter.ExecuteSingleAsync(resolveFieldContext, query);
    }

    /// <summary>
    /// Executes the query with a predicate and returns a single EfSource-wrapped result.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A task that represents the asynchronous operation, containing an EfSource-wrapped entity.</returns>
    /// <exception cref="ExecutionError">Thrown if no entity is found.</exception>
    public static Task<EfSource<T>> ToGraphSingleAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
        where T : class
        => query.Where(predicate).ToGraphSingleAsync();

    // =============== Data Loader Extensions ===============

    /// <summary>
    /// Generates a unique data loader key based on the caller's file path and line number.
    /// </summary>
    private static string GenerateDataLoaderKey(string callerFilePath, int callerLineNumber)
    {
        return $"EfDataLoader:{callerFilePath}:{callerLineNumber}";
    }

    // =============== ToGraphSingleDelayed ===============

    /// <summary>
    /// Loads a single entity by key using a data loader for batching and caching.
    /// Throws an exception if the entity is not found.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TReturn">The return type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="key">The key value to look up.</param>
    /// <param name="keySelector">Expression that identifies the key property.</param>
    /// <param name="callerFilePath">Auto-captured caller file path.</param>
    /// <param name="callerLineNumber">Auto-captured caller line number.</param>
    /// <returns>A data loader result containing the entity.</returns>
    public static IDataLoaderResult<EfSource<TReturn>> ToGraphSingleDelayed<TKey, TReturn>(
        this IQueryable<TReturn> query,
        TKey key,
        Expression<Func<TReturn, TKey>> keySelector,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TReturn : class
    {
        return ToGraphSingleDelayed(
            query, key, keySelector, x => x, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Loads a single entity by key using a data loader for batching and caching, with projection.
    /// Throws an exception if the entity is not found.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TObject">The entity type in the database query.</typeparam>
    /// <typeparam name="TReturn">The return type after projection.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="key">The key value to look up.</param>
    /// <param name="keySelector">Expression that identifies the key property.</param>
    /// <param name="itemSelector">Expression that projects the entity to the return type.</param>
    /// <param name="callerFilePath">Auto-captured caller file path.</param>
    /// <param name="callerLineNumber">Auto-captured caller line number.</param>
    /// <returns>A data loader result containing the projected entity.</returns>
    public static IDataLoaderResult<EfSource<TReturn>> ToGraphSingleDelayed<TKey, TObject, TReturn>(
        this IQueryable<TObject> query,
        TKey key,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObject : class
        where TReturn : class
    {
        // GraphQL will handle null checks and throw errors as needed
        return ToGraphSingleOrDefaultDelayed(
            query, key, keySelector, itemSelector, callerFilePath, callerLineNumber)!;
    }

    // =============== ToGraphSingleOrDefaultDelayed ===============

    /// <summary>
    /// Loads a single entity by key using a data loader for batching and caching.
    /// Returns null if the entity is not found.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TReturn">The return type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="key">The key value to look up.</param>
    /// <param name="keySelector">Expression that identifies the key property.</param>
    /// <param name="callerFilePath">Auto-captured caller file path.</param>
    /// <param name="callerLineNumber">Auto-captured caller line number.</param>
    /// <returns>A data loader result containing the entity or null.</returns>
    public static IDataLoaderResult<EfSource<TReturn>?> ToGraphSingleOrDefaultDelayed<TKey, TReturn>(
        this IQueryable<TReturn> query,
        TKey key,
        Expression<Func<TReturn, TKey>> keySelector,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TReturn : class
    {
        return ToGraphSingleOrDefaultDelayed(
            query, key, keySelector, x => x, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Loads a single entity by key using a data loader for batching and caching, with projection.
    /// Returns null if the entity is not found.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TObject">The entity type in the database query.</typeparam>
    /// <typeparam name="TReturn">The return type after projection.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="key">The key value to look up.</param>
    /// <param name="keySelector">Expression that identifies the key property.</param>
    /// <param name="itemSelector">Expression that projects the entity to the return type.</param>
    /// <param name="callerFilePath">Auto-captured caller file path.</param>
    /// <param name="callerLineNumber">Auto-captured caller line number.</param>
    /// <returns>A data loader result containing the projected entity or null.</returns>
    public static IDataLoaderResult<EfSource<TReturn>?> ToGraphSingleOrDefaultDelayed<TKey, TObject, TReturn>(
        this IQueryable<TObject> query,
        TKey key,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObject : class
        where TReturn : class
    {
        return ToGraphSingleOrDefaultDelayedImpl(
            query, key, keySelector, itemSelector, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Internal implementation for loading a single entity by key using a data loader.
    /// </summary>
    private static IDataLoaderResult<EfSource<TReturn>?> ToGraphSingleOrDefaultDelayedImpl<TKey, TObject, TReturn>(
        IQueryable<TObject> query,
        TKey key,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        string callerFilePath,
        int callerLineNumber)
        where TObject : class
        where TReturn : class
    {
        if (key == null)
            return EfEntityDataLoader<TKey, TObject, TReturn>._nullResult!;

        var resolveFieldContextAccessor = Execution.ResolveFieldContextAccessor.Instance;
        var context = resolveFieldContextAccessor.Context
            ?? throw new InvalidOperationException("No IResolveFieldContext is available in the current context.");
        var requestServices = context.RequestServices
            ?? throw new InvalidOperationException("No RequestServices is available in the current IResolveFieldContext.");

        var dataLoaderKey = GenerateDataLoaderKey(callerFilePath, callerLineNumber);

        var dataLoader = requestServices
            .GetRequiredService<IDataLoaderContextAccessor>().Context!
            .GetOrAdd(dataLoaderKey, () => new EfEntityDataLoader<TKey, TObject, TReturn>(
                query,
                keySelector,
                itemSelector));

        return dataLoader.LoadAsync(context, key)!;
    }

    // =============== ToGraphDelayed ===============

    /// <summary>
    /// Loads a list of entities by parent key using a data loader for batching and caching.
    /// </summary>
    /// <typeparam name="TParentKey">The parent key type.</typeparam>
    /// <typeparam name="TReturn">The return type.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="parentKey">The parent key value to look up.</param>
    /// <param name="keySelector">Expression that identifies the parent key property.</param>
    /// <param name="callerFilePath">Auto-captured caller file path.</param>
    /// <param name="callerLineNumber">Auto-captured caller line number.</param>
    /// <returns>A data loader result containing the list of entities.</returns>
    public static IDataLoaderResult<IEnumerable<EfSource<TReturn>>> ToGraphDelayed<TParentKey, TReturn>(
        this IQueryable<TReturn> query,
        TParentKey parentKey,
        Expression<Func<TReturn, TParentKey>> keySelector,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TReturn : class
    {
        return ToGraphDelayed(
            query, parentKey, keySelector, x => x, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Loads a list of entities by parent key using a data loader for batching and caching, with projection.
    /// </summary>
    /// <typeparam name="TParentKey">The parent key type.</typeparam>
    /// <typeparam name="TObject">The entity type in the database query.</typeparam>
    /// <typeparam name="TReturn">The return type after projection.</typeparam>
    /// <param name="query">The IQueryable to execute.</param>
    /// <param name="parentKey">The parent key value to look up.</param>
    /// <param name="keySelector">Expression that identifies the parent key property.</param>
    /// <param name="itemSelector">Expression that projects the entity to the return type.</param>
    /// <param name="callerFilePath">Auto-captured caller file path.</param>
    /// <param name="callerLineNumber">Auto-captured caller line number.</param>
    /// <returns>A data loader result containing the list of projected entities.</returns>
    public static IDataLoaderResult<IEnumerable<EfSource<TReturn>>> ToGraphDelayed<TParentKey, TObject, TReturn>(
        this IQueryable<TObject> query,
        TParentKey parentKey,
        Expression<Func<TObject, TParentKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObject : class
        where TReturn : class
    {
        return ToGraphDelayedImpl(
            query, parentKey, keySelector, itemSelector, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Internal implementation for loading a list of entities by parent key using a data loader.
    /// </summary>
    private static IDataLoaderResult<IEnumerable<EfSource<TReturn>>> ToGraphDelayedImpl<TParentKey, TObject, TReturn>(
        IQueryable<TObject> query,
        TParentKey parentKey,
        Expression<Func<TObject, TParentKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        string callerFilePath,
        int callerLineNumber)
        where TObject : class
        where TReturn : class
    {
        var resolveFieldContextAccessor = Execution.ResolveFieldContextAccessor.Instance;
        var context = resolveFieldContextAccessor.Context
            ?? throw new InvalidOperationException("No IResolveFieldContext is available in the current context.");
        var requestServices = context.RequestServices
            ?? throw new InvalidOperationException("No RequestServices is available in the current IResolveFieldContext.");

        var dataLoaderKey = GenerateDataLoaderKey(callerFilePath, callerLineNumber);

        var dataLoader = requestServices
            .GetRequiredService<IDataLoaderContextAccessor>().Context!
            .GetOrAdd(dataLoaderKey, () => new EfEntityListDataLoader<TParentKey, TObject, TReturn>(
                query,
                keySelector,
                itemSelector));

        return dataLoader.LoadAsync(context, parentKey);
    }
}
