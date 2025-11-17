// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.DataLoader;
using GraphQL.Linq.GraphApi;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.DataLoaders;

/// <summary>
/// A simplified data loader that loads single entities by key, without requiring a TDbContext type parameter.
/// The database context is inferred from the provided IQueryable.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TReturn">The type of the objects returned by the query.</typeparam>
public class EfEntityDataLoader<TKey, TReturn>
    : EfEntityDataLoader<TKey, TReturn, TReturn>
    where TReturn : class
{
    /// <summary>
    /// Initializes a new instance with the specified query and key selector.
    /// </summary>
    /// <param name="query">The base query to execute.</param>
    /// <param name="keySelector">Expression that identifies the key property.</param>
    public EfEntityDataLoader(
        IQueryable<TReturn> query,
        Expression<Func<TReturn, TKey>> keySelector)
        : base(query, keySelector, x => x)
    {
    }
}

/// <summary>
/// A simplified data loader that loads single entities by key with projection, without requiring a TDbContext type parameter.
/// The database context is inferred from the provided IQueryable.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TObject">The type of the entity in the database query.</typeparam>
/// <typeparam name="TReturn">The type of the objects returned after projection.</typeparam>
public class EfEntityDataLoader<TKey, TObject, TReturn>
    : DataLoaderBase<(IResolveFieldContext Context, TKey Key), EfSource<TReturn>>
    where TObject : class
    where TReturn : class
{
    private readonly IQueryable<TObject> _query;
    private readonly Expression<Func<TObject, TKey>> _keySelector;
    private readonly Expression<Func<TObject, TReturn>> _itemSelector;
    internal static readonly IDataLoaderResult<EfSource<TReturn>> _nullResult = new DataLoaderResult<EfSource<TReturn>>((EfSource<TReturn>)null!);

    /// <summary>
    /// Initializes a new instance with the specified query, key selector, and item selector.
    /// </summary>
    /// <param name="query">The base query to execute.</param>
    /// <param name="keySelector">Expression that identifies the key property.</param>
    /// <param name="itemSelector">Expression that projects the entity to the return type.</param>
    public EfEntityDataLoader(
        IQueryable<TObject> query,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector)
        : base(false, 1000)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _itemSelector = itemSelector ?? throw new ArgumentNullException(nameof(itemSelector));
    }

    /// <summary>
    /// Loads a single entity by key using the data loader.
    /// </summary>
    /// <param name="context">The GraphQL field resolution context.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>A data loader result containing the entity.</returns>
    public virtual IDataLoaderResult<EfSource<TReturn>> LoadAsync(IResolveFieldContext context, TKey key)
    {
        if (key == null)
            return _nullResult;
        return LoadAsync((context, key));
    }

    /// <inheritdoc/>
    protected override async Task FetchAsync(IEnumerable<DataLoaderPair<(IResolveFieldContext Context, TKey Key), EfSource<TReturn>>> list, CancellationToken cancellationToken)
    {
        var groupedList = list.GroupBy(x => x.Key.Context.FieldAst);
        foreach (var group in groupedList) {
            var keys = group.Select(x => x.Key.Key).Distinct();
            var context = group.First().Key.Context;
            var linqGraphExecuter = context.RequestServices!.GetRequiredService<ILinqGraphExecuter>();

            var results = (await linqGraphExecuter.ExecuteQueryForKeysAsync(
                context,
                _query,
                _keySelector,
                keys,
                _itemSelector)).ToDictionary(x => x.Item1, x => x.Item2);

            foreach (var inputObject in group) {
                if (results.TryGetValue(inputObject.Key.Key, out var value)) {
                    inputObject.SetResult(value);
                } else {
                    inputObject.SetResult(null!);
                }
            }
        }
    }
}
