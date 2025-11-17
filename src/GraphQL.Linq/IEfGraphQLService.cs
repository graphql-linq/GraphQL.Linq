// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;

namespace GraphQL.Linq;

/// <summary>
/// Asynchronously executes database queries against a database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public partial interface IEfGraphQLService<TDbContext> : IEfDbPrimaryKeyNamesProvider<TDbContext>
{
    /// <summary>
    /// Adds the database context to the GraphQL context, returning an <see cref="IResolveEfFieldContext{TDbContext, TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object of the GraphQL context.</typeparam>
    IResolveEfFieldContext<TDbContext, TSource> BuildResolveEfFieldContext<TSource>(IResolveFieldContext context);

    /// <summary>
    /// Executes a query asynchronously and returns the results as a list.
    /// </summary>
    /// <typeparam name="TReturn">The type of the objects returned by the query.</typeparam>
    Task<IList<TReturn>> QueryToListAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken = default) where TReturn : class;

    /// <summary>
    /// Executes a query asynchronously and returns the result, or <see langword="null"/> if no results are found.
    /// If multiple rows are returns, an exception is thrown.
    /// </summary>
    /// <typeparam name="TReturn">The type of the object returned by the query.</typeparam>
    Task<TReturn?> QuerySingleOrDefaultAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken = default) where TReturn : class;

    /// <summary>
    /// Executes a query asynchronously and returns the total number of rows in the result.
    /// </summary>
    /// <typeparam name="TReturn">The type of the objects counted by the query.</typeparam>
    Task<int> QueryCountAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an expression that can be used to filter a query based on a list of keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TObject">The type of the objects being filtered.</typeparam>
    Expression<Func<TObject, bool>> CreateWhereInExpression<TKey, TObject>(Func<TDbContext> dbContextFactory, Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys);
}
