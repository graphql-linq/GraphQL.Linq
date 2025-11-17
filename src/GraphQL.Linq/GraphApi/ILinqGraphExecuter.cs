// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq.GraphApi;

/// <summary>
/// Executes LINQ queries against a database context, selecting only the fields requested in the current GraphQL request,
/// and returns results wrapped in <see cref="EfSource{T}"/> objects.
/// This interface provides methods for executing single, list, and paginated connection queries.
/// The database context is inferred from the provided <see cref="IQueryable{T}"/>.
/// </summary>
public interface ILinqGraphExecuter
{
    /// <summary>
    /// Executes a query asynchronously, selecting only the fields requested in the current GraphQL request, and returns a single result.
    /// Throws an exception if no results are found or if multiple results are returned.
    /// </summary>
    /// <typeparam name="TReturn">The type of the object returned by the query.</typeparam>
    /// <param name="context">The GraphQL field resolution context containing the requested fields.</param>
    /// <param name="query">The LINQ query to execute.</param>
    /// <returns>A task that represents the asynchronous operation, containing the single result with only the requested fields wrapped in an <see cref="EfSource{T}"/>.</returns>
    Task<EfSource<TReturn>> ExecuteSingleAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class;

    /// <summary>
    /// Executes a query asynchronously, selecting only the fields requested in the current GraphQL request, and returns a single result or null if no results are found.
    /// Throws an exception if multiple results are returned.
    /// </summary>
    /// <typeparam name="TReturn">The type of the object returned by the query.</typeparam>
    /// <param name="context">The GraphQL field resolution context containing the requested fields.</param>
    /// <param name="query">The LINQ query to execute.</param>
    /// <returns>A task that represents the asynchronous operation, containing the single result with only the requested fields wrapped in an <see cref="EfSource{T}"/>, or null if no results are found.</returns>
    Task<EfSource<TReturn>?> ExecuteSingleOrDefaultAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class;

    /// <summary>
    /// Executes a query asynchronously, selecting only the fields requested in the current GraphQL request, and returns all results as a list.
    /// </summary>
    /// <typeparam name="TReturn">The type of the objects returned by the query.</typeparam>
    /// <param name="context">The GraphQL field resolution context containing the requested fields.</param>
    /// <param name="query">The LINQ query to execute.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of results with only the requested fields wrapped in <see cref="EfSource{T}"/> objects.</returns>
    Task<IList<EfSource<TReturn>>> ExecuteQueryAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class;

    /// <summary>
    /// Executes a query asynchronously, selecting only the fields requested in the current GraphQL request, and returns paginated results as a Relay-style connection with explicit pagination parameters.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object in the GraphQL context.</typeparam>
    /// <typeparam name="TReturn">The type of the objects returned by the query.</typeparam>
    /// <param name="context">The GraphQL field resolution context containing the requested fields.</param>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="first">The number of items to return from the start of the result set.</param>
    /// <param name="last">The number of items to return from the end of the result set.</param>
    /// <param name="after">The cursor after which to start returning items.</param>
    /// <param name="before">The cursor before which to start returning items.</param>
    /// <param name="defaultPageSize">The default page size to use for pagination if none is specified in the request.</param>
    /// <returns>A task that represents the asynchronous operation, containing a connection with paginated results with only the requested fields wrapped in <see cref="EfSource{T}"/> objects.</returns>
    Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, int defaultPageSize = 100) where TReturn : class;

    /// <summary>
    /// Executes a query asynchronously, selecting only the fields requested in the current GraphQL request, and returns paginated results as a Relay-style connection with explicit pagination parameters and a specific connection resolver.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object in the GraphQL context.</typeparam>
    /// <typeparam name="TReturn">The type of the objects returned by the query.</typeparam>
    /// <param name="context">The GraphQL field resolution context containing the requested fields.</param>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="first">The number of items to return from the start of the result set.</param>
    /// <param name="last">The number of items to return from the end of the result set.</param>
    /// <param name="after">The cursor after which to start returning items.</param>
    /// <param name="before">The cursor before which to start returning items.</param>
    /// <param name="connectionResolver">The connection resolver to use for pagination logic.</param>
    /// <returns>A task that represents the asynchronous operation, containing a connection with paginated results with only the requested fields wrapped in <see cref="EfSource{T}"/> objects.</returns>
    Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, IEfConnectionResolver<TReturn> connectionResolver) where TReturn : class;

    /// <summary>
    /// Executes a query asynchronously for multiple keys with projection, selecting only the fields requested in the current GraphQL request,
    /// and returns results as key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
    /// <typeparam name="TObject">The type of the entity in the database query.</typeparam>
    /// <typeparam name="TReturn">The type of the objects returned after projection.</typeparam>
    /// <param name="context">The GraphQL field resolution context containing the requested fields.</param>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="keySelector">Expression that identifies the key property for grouping.</param>
    /// <param name="keys">The collection of keys to filter by.</param>
    /// <param name="itemSelector">Expression that projects the entity to the return type.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of key-value pairs with only the requested fields wrapped in <see cref="EfSource{T}"/> objects.</returns>
    Task<IList<Tuple<TKey, EfSource<TReturn>>>> ExecuteQueryForKeysAsync<TKey, TObject, TReturn>(IResolveFieldContext context, IQueryable<TObject> query, Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys, Expression<Func<TObject, TReturn>> itemSelector) where TObject : class where TReturn : class;
}

/// <summary>
/// Executes LINQ queries against a database context, selecting only the fields requested in the current GraphQL request,
/// and returns results wrapped in <see cref="EfSource{T}"/> objects.
/// This interface provides methods for executing single, list, and paginated connection queries.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface ILinqGraphExecuter<TDbContext> : ILinqGraphExecuter
{
}
