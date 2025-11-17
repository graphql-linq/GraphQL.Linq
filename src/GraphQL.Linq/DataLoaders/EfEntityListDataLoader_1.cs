// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.DataLoader;
using GraphQL.Linq.GraphApi;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.DataLoaders;

/// <summary>
/// A simplified data loader that loads lists of entities grouped by parent key, without requiring a TDbContext type parameter.
/// The database context is inferred from the provided IQueryable.
/// </summary>
/// <typeparam name="TParentKey">The type of the parent key used for grouping.</typeparam>
/// <typeparam name="TReturn">The type of the objects returned by the query.</typeparam>
public class EfEntityListDataLoader<TParentKey, TReturn>
    : EfEntityListDataLoader<TParentKey, TReturn, TReturn>
    where TReturn : class
{
    /// <summary>
    /// Initializes a new instance with the specified query and key selector.
    /// </summary>
    /// <param name="query">The base query to execute.</param>
    /// <param name="keySelector">Expression that identifies the parent key property.</param>
    public EfEntityListDataLoader(
        IQueryable<TReturn> query,
        Expression<Func<TReturn, TParentKey>> keySelector)
        : base(query, keySelector, x => x)
    {
    }
}

/// <summary>
/// A simplified data loader that loads lists of entities grouped by parent key with projection, without requiring a TDbContext type parameter.
/// The database context is inferred from the provided IQueryable.
/// </summary>
/// <typeparam name="TParentKey">The type of the parent key used for grouping.</typeparam>
/// <typeparam name="TObject">The type of the entity in the database query.</typeparam>
/// <typeparam name="TReturn">The type of the objects returned after projection.</typeparam>
public class EfEntityListDataLoader<TParentKey, TObject, TReturn>
    : DataLoaderBase<(IResolveFieldContext Context, TParentKey Key), IEnumerable<EfSource<TReturn>>>
    where TObject : class
    where TReturn : class
{
    private readonly IQueryable<TObject> _query;
    private readonly Expression<Func<TObject, TParentKey>> _keySelector;
    private readonly Expression<Func<TObject, TReturn>> _itemSelector;

    /// <summary>
    /// Initializes a new instance with the specified query, key selector, and item selector.
    /// </summary>
    /// <param name="query">The base query to execute.</param>
    /// <param name="keySelector">Expression that identifies the parent key property.</param>
    /// <param name="itemSelector">Expression that projects the entity to the return type.</param>
    public EfEntityListDataLoader(
        IQueryable<TObject> query,
        Expression<Func<TObject, TParentKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector)
        : base(true, 900)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _itemSelector = itemSelector ?? throw new ArgumentNullException(nameof(itemSelector));
    }

    /// <summary>
    /// Loads entities for the specified parent key using the data loader.
    /// </summary>
    /// <param name="context">The GraphQL field resolution context.</param>
    /// <param name="key">The parent key to look up.</param>
    /// <returns>A data loader result containing the list of entities for the specified parent key.</returns>
    public virtual IDataLoaderResult<IEnumerable<EfSource<TReturn>>> LoadAsync(IResolveFieldContext context, TParentKey key)
    {
        return LoadAsync((context, key));
    }

    /// <inheritdoc/>
    protected override async Task FetchAsync(IEnumerable<DataLoaderPair<(IResolveFieldContext Context, TParentKey Key), IEnumerable<EfSource<TReturn>>>> list, CancellationToken cancellationToken)
    {
        var groupedList = list.GroupBy(x => x.Key.Context.FieldAst);
        foreach (var group in groupedList) {
            var keys = group.Select(x => x.Key.Key).Distinct();
            var context = group.First().Key.Context;
            var linqGraphExecuter = context.RequestServices!.GetRequiredService<ILinqGraphExecuter>();

            var resultList = await linqGraphExecuter.ExecuteQueryForKeysAsync(
                context,
                _query,
                _keySelector,
                keys,
                _itemSelector);

            var results = resultList.ToLookup(x => x.Item1, x => x.Item2);

            foreach (var inputObject in group) {
                inputObject.SetResult(results[inputObject.Key.Key]);
            }
        }
    }
}
