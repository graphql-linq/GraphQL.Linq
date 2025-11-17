// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.DataLoader;
using GraphQL.Linq.GraphApi;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.DataLoaders;

/// <summary>
/// A data loader that loads entities from an <see cref="IEfGraphQLService{TDbContext}"/>.
/// </summary>
public class EfEntityListDataLoader<TDbContext, TParentKey, TContextSource, TReturn>
    : EfEntityListDataLoader<TDbContext, TParentKey, TContextSource, TReturn, TReturn>
    where TReturn : class
{
    /// <summary>
    /// Initializes a new instance with the specified base query function, key selector and item selector.
    /// </summary>
    public EfEntityListDataLoader(
        Func<IResolveEfFieldContext<TDbContext, TContextSource>, IQueryable<TReturn>> baseQueryFunc,
        Expression<Func<TReturn, TParentKey>> keySelector)
        : base(baseQueryFunc, keySelector, x => x)
    {
    }

    /// <inheritdoc cref="EfEntityListDataLoader{TDbContext, TParentKey, TContextSource, TReturn}.EfEntityListDataLoader(Func{IResolveEfFieldContext{TDbContext, TContextSource}, IQueryable{TReturn}}, Expression{Func{TReturn, TParentKey}})"/>
    public EfEntityListDataLoader(
        Func<IResolveEfFieldContext<TDbContext, TContextSource>, Task<IQueryable<TReturn>>> baseQueryFunc,
        Expression<Func<TReturn, TParentKey>> keySelector)
        : base(baseQueryFunc, keySelector, x => x)
    {
    }
}

/// <summary>
/// A data loader that loads entities from an <see cref="IEfGraphQLService{TDbContext}"/>.
/// </summary>
public class EfEntityListDataLoader<TDbContext, TParentKey, TContextSource, TObject, TReturn>
    : DataLoaderBase<(IResolveFieldContext Context, TParentKey Key), IEnumerable<EfSource<TReturn>>>
    where TObject : class
    where TReturn : class
{
    private readonly Func<IResolveEfFieldContext<TDbContext, TContextSource>, Task<IQueryable<TObject>>> _baseQueryFuncAsync;
    private readonly Expression<Func<TObject, TParentKey>> _keySelector;
    private readonly Expression<Func<TObject, TReturn>> _itemSelector;

    /// <summary>
    /// Initializes a new instance with the specified base query function, key selector and item selector.
    /// </summary>
    public EfEntityListDataLoader(Func<IResolveEfFieldContext<TDbContext, TContextSource>, IQueryable<TObject>> baseQueryFunc, Expression<Func<TObject, TParentKey>> keySelector, Expression<Func<TObject, TReturn>> itemSelector)
        : this((context) => Task.FromResult(baseQueryFunc(context)), keySelector, itemSelector)
    {
    }

    /// <inheritdoc cref="EfEntityListDataLoader{TDbContext, TParentKey, TContextSource, TObject, TReturn}.EfEntityListDataLoader(Func{IResolveEfFieldContext{TDbContext, TContextSource}, IQueryable{TObject}}, Expression{Func{TObject, TParentKey}}, Expression{Func{TObject, TReturn}})"/>
    public EfEntityListDataLoader(Func<IResolveEfFieldContext<TDbContext, TContextSource>, Task<IQueryable<TObject>>> baseQueryFunc, Expression<Func<TObject, TParentKey>> keySelector, Expression<Func<TObject, TReturn>> itemSelector)
        : base(false, 1000)
    {
        _baseQueryFuncAsync = baseQueryFunc ?? throw new ArgumentNullException(nameof(baseQueryFunc));
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _itemSelector = itemSelector ?? throw new ArgumentNullException(nameof(itemSelector));
    }

    /// <inheritdoc cref="DataLoaderBase{TKey, T}.LoadAsync(TKey)"/>
    public virtual IDataLoaderResult<IEnumerable<EfSource<TReturn>>> LoadAsync(IResolveFieldContext context, TParentKey key)
    {
        return LoadAsync((context, key));
    }

    /// <inheritdoc/>
    protected override async Task FetchAsync(IEnumerable<DataLoaderPair<(IResolveFieldContext Context, TParentKey Key), IEnumerable<EfSource<TReturn>>>> list, CancellationToken cancellationToken)
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
            var query = executer.GenerateQueryForKeys(efContext, baseQuery, _keySelector, keys, _itemSelector);

            var results = (await efGraphQLService.QueryToListAsync(query, context.CancellationToken)).ToLookup(x => x.Item1, x => x.Item2);
            foreach (var inputObject in group) {
                inputObject.SetResult(results[inputObject.Key.Key]);
            }
        }
    }
}
