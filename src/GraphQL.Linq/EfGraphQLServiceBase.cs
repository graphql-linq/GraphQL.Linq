// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;

namespace GraphQL.Linq;

/// <summary>
/// Provides an abstract implementation of the <see cref="IEfGraphQLService{TDbContext}"/> interface which
/// depends on the <see cref="IEfDbPrimaryKeyNamesProvider{TDbContext}"/> services.
/// Requires the database-specific asynchronous query methods to be implemented.
/// </summary>
public abstract class EfGraphQLServiceBase<TDbContext> : IEfGraphQLService<TDbContext>, IEfDbPrimaryKeyNamesProvider<TDbContext>
{
    /// <summary>
    /// Returns the <see cref="IEfDbPrimaryKeyNamesProvider{TDbContext}"/> instance.
    /// </summary>
    protected IEfDbPrimaryKeyNamesProvider<TDbContext> EfDbKeyNamesProvider { get; }

    /// <summary>
    /// When loading keys from a data loader, this sets the number of unique keys that gets inlined directly
    ///   into the query -- e.g. "WHERE ParentId == @p1 OR ParentId == @p2 OR ParentId == @p3" -- versus using the
    ///   IN operator -- e.g. "WHERE ParentId IN (33, 48, 293)"
    /// </summary>
    protected int MaxParameterizeContainsVariables { get; } = 10;

    /// <summary>
    /// Initializes a new instance from the specified <see cref="IEfDbPrimaryKeyNamesProvider{TDbContext}"/> service.
    /// </summary>
    public EfGraphQLServiceBase(IEfDbPrimaryKeyNamesProvider<TDbContext> efDbKeyNames)
    {
        EfDbKeyNamesProvider = efDbKeyNames ?? throw new ArgumentNullException(nameof(efDbKeyNames));
    }

    /// <inheritdoc cref="EfGraphQLServiceBase{TDbContext}.EfGraphQLServiceBase(IEfDbPrimaryKeyNamesProvider{TDbContext})"/>
    public EfGraphQLServiceBase(IEfDbPrimaryKeyNamesProvider<TDbContext> efDbKeyNames, int maxParameterizeContainsVariables)
        : this(efDbKeyNames)
    {
        MaxParameterizeContainsVariables = maxParameterizeContainsVariables;
    }

    /// <inheritdoc/>
    IResolveEfFieldContext<TDbContext, TSource> IEfGraphQLService<TDbContext>.BuildResolveEfFieldContext<TSource>(IResolveFieldContext context)
    {
        return new ResolveEfFieldContext<TDbContext, TSource>(context, this);
    }

    /// <inheritdoc/>
    public virtual IEnumerable<string> GetPrimaryKeyNames<TSource>()
    {
        return EfDbKeyNamesProvider.GetPrimaryKeyNames<TSource>();
    }

    /// <inheritdoc/>
    public virtual Expression<Func<TSource, object>> GetDummyExpression<TSource>()
    {
        return EfDbKeyNamesProvider.GetDummyExpression<TSource>();
    }

    /// <inheritdoc/>
    public abstract Task<IList<TReturn>> QueryToListAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken) where TReturn : class;

    /// <inheritdoc/>
    public abstract Task<int> QueryCountAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<TReturn?> QuerySingleOrDefaultAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken) where TReturn : class;

    /// <inheritdoc/>
    public virtual Expression<Func<TObject, bool>> CreateWhereInExpression<TKey, TObject>(Func<TDbContext> dbContextFactory, Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys)
    {
        if (MaxParameterizeContainsVariables >= 0) {
            int? count = null;
            if (keys is ICollection<TKey> keyCollection)
                count = keyCollection.Count;
            else if (keys is System.Collections.ICollection collection)
                count = collection.Count;
            else if (keys is IReadOnlyCollection<TKey> readOnlyCollection)
                count = readOnlyCollection.Count;

            if (count.HasValue) {
                if (count.Value == 0) {
                    Expression<Func<TObject, bool>> ret = _ => false;
                    return ret;
                } else if (count.Value <= MaxParameterizeContainsVariables) {
                    // e.g. (product) => product.Id == list[0] || product.Id == list[1] || product.Id == list[2]
                    var list = keys is IList<TKey> keysList ? keysList : keys.ToList();
                    return Expression.Lambda<Func<TObject, bool>>(
                        keys
                            .Select((key, index) => {
                                Expression<Func<TKey>> accessor = () => list[index];
                                return Expression.Equal(keySelector.Body, accessor.Body);
                            })
                            .Aggregate((aggregate, next) => Expression.OrElse(aggregate, next)),
                        keySelector.Parameters);
                }
            }
        }

        Expression<Func<IEnumerable<TKey>>> keysAccessExpression = () => keys;
        //e.g. (product) => Enumerable.Contains(list, product.Id)
        return Expression.Lambda<Func<TObject, bool>>(
            Expression.Call(
                new Func<IEnumerable<TKey>, TKey, bool>(Enumerable.Contains).Method,
                //don't use Expression.Constant(keys) as EF Core thinks it's inlined and won't change, so it doesn't parameterize it
                keysAccessExpression.Body,
                keySelector.Body),
            keySelector.Parameters);
    }
}
