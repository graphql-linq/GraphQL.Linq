// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.DataLoader;
using GraphQL.Linq.GraphApi;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.DataLoaders;

/// <summary>
/// A data loader that loads entities from an <see cref="IEfGraphQLService{TDbContext}"/>.
/// </summary>
public class EfEntityDataLoader<TDbContext, TKey, TContextSource, TReturn>
    : EfEntityDataLoader<TDbContext, TKey, TContextSource, TReturn, TReturn>
    where TReturn : class
{
    /// <summary>
    /// Initializes a new instance with the specified base query function, key selector and item selector.
    /// </summary>
    public EfEntityDataLoader(
        Func<IResolveEfFieldContext<TDbContext, TContextSource>, IQueryable<TReturn>> baseQueryFunc,
        Expression<Func<TReturn, TKey>> keySelector)
        : base(baseQueryFunc, keySelector, x => x)
    {
    }

    /// <inheritdoc cref="EfEntityDataLoader{TDbContext, TKey, TContextSource, TReturn}.EfEntityDataLoader(Func{IResolveEfFieldContext{TDbContext, TContextSource}, IQueryable{TReturn}}, Expression{Func{TReturn, TKey}})"/>
    public EfEntityDataLoader(
        Func<IResolveEfFieldContext<TDbContext, TContextSource>, Task<IQueryable<TReturn>>> baseQueryFunc,
        Expression<Func<TReturn, TKey>> keySelector)
        : base(baseQueryFunc, keySelector, x => x)
    {
    }
}

/// <summary>
/// A data loader that loads entities from an <see cref="IEfGraphQLService{TDbContext}"/>.
/// </summary>
public class EfEntityDataLoader<TDbContext, TKey, TContextSource, TObject, TReturn>
    : DataLoaderBase<(IResolveFieldContext Context, TKey Key), EfSource<TReturn>>
    where TObject : class
    where TReturn : class
{
    private readonly Func<IResolveEfFieldContext<TDbContext, TContextSource>, Task<IQueryable<TObject>>> _baseQueryFuncAsync;
    private readonly Expression<Func<TObject, TKey>> _keySelector;
    private readonly Expression<Func<TObject, TReturn>> _itemSelector;

    /// <summary>
    /// Initializes a new instance with the specified base query function, key selector and item selector.
    /// </summary>
    public EfEntityDataLoader(
        Func<IResolveEfFieldContext<TDbContext, TContextSource>, IQueryable<TObject>> baseQueryFunc,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector)
        : this((context) => Task.FromResult(baseQueryFunc(context)), keySelector, itemSelector)
    {
    }

    /// <inheritdoc cref="EfEntityDataLoader{TDbContext, TKey, TContextSource, TObject, TReturn}.EfEntityDataLoader(Func{IResolveEfFieldContext{TDbContext, TContextSource}, IQueryable{TObject}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}})"/>
    public EfEntityDataLoader(
        Func<IResolveEfFieldContext<TDbContext, TContextSource>, Task<IQueryable<TObject>>> baseQueryFunc,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector)
        : base(true, 900)
    {
        _baseQueryFuncAsync = baseQueryFunc ?? throw new ArgumentNullException(nameof(baseQueryFunc));
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _itemSelector = itemSelector ?? throw new ArgumentNullException(nameof(itemSelector));
    }

    /// <inheritdoc cref="DataLoaderBase{TKey, T}.LoadAsync(TKey)"/>
    public virtual IDataLoaderResult<EfSource<TReturn>> LoadAsync(IResolveFieldContext context, TKey key)
    {
        if (key == null)
            return EfEntityDataLoader<TKey, TObject, TReturn>._nullResult;
        return LoadAsync((context, key));
    }

    /// <inheritdoc/>
    protected override async Task FetchAsync(IEnumerable<DataLoaderPair<(IResolveFieldContext Context, TKey Key), EfSource<TReturn>>> list, CancellationToken cancellationToken)
    {
        var executer = new QueryExecuter<TDbContext, TReturn>();
        var groupedList = list.GroupBy(x => x.Key.Context.FieldAst);
        foreach (var group in groupedList) {
            var keys = group.Select(x => x.Key.Key).Distinct();
            var context = group.First().Key.Context;
            var efGraphQLService = context.RequestServices!.GetRequiredService<IEfGraphQLService<TDbContext>>();
            if (context is not IResolveEfFieldContext<TDbContext, TContextSource> efContext)
                efContext = efGraphQLService.BuildResolveEfFieldContext<TContextSource>(context);

            var baseQuery = await _baseQueryFuncAsync(efContext);
            var query = executer.GenerateQueryForKeys(
                efContext: efContext,
                baseQuery: baseQuery,
                keySelector: _keySelector,
                itemSelector: _itemSelector,
                keys: keys);

            var results = (await efGraphQLService.QueryToListAsync(query, context.CancellationToken)).ToDictionary(x => x.Item1, x => x.Item2);
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
