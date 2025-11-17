// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.DataLoader;
using GraphQL.Linq.DataLoaders;
using GraphQL.Linq.GraphApi;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq;

/// <summary>
/// Provides extension methods for FieldBuilders.
/// </summary>
public static partial class FieldBuilderExtensions
{
    // =============== EfDelayLoad ===============

    private static readonly ConcurrentDictionary<(Type TDbContextType, Type TReturnType, Type TKeyType), LambdaExpression> _keySelectorCache = new();

    /// <inheritdoc cref="DelayLoadEntry{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TDbContext, TSource, TKey, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> baseQueryFunc,
        Expression<Func<TReturn, TKey>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => DelayLoadEntry(fieldBuilder, baseQueryFunc, keySelector, x => x, graphType);

    /// <inheritdoc cref="DelayLoadEntry{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TDbContext, TSource, TKey, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> baseQueryFunc,
        Expression<Func<TReturn, TKey>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => DelayLoadEntry(fieldBuilder, baseQueryFunc, keySelector, x => x, graphType);

    /// <inheritdoc cref="DelayLoadEntry{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TDbContext, TSource, TKey, TObject, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TObject>> baseQueryFunc,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
        => DelayLoadEntry(fieldBuilder, context => Task.FromResult(baseQueryFunc(context)), keySelector, itemSelector, graphType);

    /// <summary>
    /// Alters the field to take the result of the existing resolver and pass it to a DataLoader as a key for deferred loading of a entity type.
    /// </summary>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TDbContext, TSource, TKey, TObject, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TObject>>> baseQueryFunc,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
    {
        var dataLoaderId = Guid.NewGuid().ToString();
        var initialResolver = fieldBuilder.FieldType.Resolver!;

        Func<EfEntityDataLoader<TDbContext, TKey, TSource, TObject, TReturn>> delayLoaderFactory =
            () => new EfEntityDataLoader<TDbContext, TKey, TSource, TObject, TReturn>(
                baseQueryFunc: baseQueryFunc,
                keySelector: keySelector,
                itemSelector: itemSelector);

        var resolver2 = new FuncFieldResolver<IDataLoaderResult<EfSource<TReturn>>>(async context => {
            var delayLoader = context.RequestServices!
                .GetRequiredService<IDataLoaderContextAccessor>().Context
                .GetOrAdd(dataLoaderId, delayLoaderFactory);

            var key = await initialResolver.ResolveAsync(context).ConfigureAwait(false);
            if (key == null)
                return null;

            return delayLoader.LoadAsync(context, (TKey)key);
        });

        fieldBuilder.FieldType.Resolver = resolver2;
        var nullable = !(typeof(NonNullGraphType).IsAssignableFrom(fieldBuilder.FieldType.Type));
        fieldBuilder.FieldType.Type = graphType.FindGraphForEfType<TReturn>(nullable);
        return fieldBuilder.Returns<IDataLoaderResult<EfSource<TReturn>>>().AsEfFieldBuilder<TDbContext, TSource, IDataLoaderResult<EfSource<TReturn>>>();
    }





    /// <inheritdoc cref="DelayLoadList{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TDbContext, TSource, TKey, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> baseQueryFunc,
        Expression<Func<TReturn, TKey>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => DelayLoadList(fieldBuilder, baseQueryFunc, keySelector, x => x, graphType);

    /// <inheritdoc cref="DelayLoadList{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TDbContext, TSource, TKey, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> baseQueryFunc,
        Expression<Func<TReturn, TKey>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => DelayLoadList(fieldBuilder, baseQueryFunc, keySelector, x => x, graphType);

    /// <inheritdoc cref="DelayLoadList{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TDbContext, TSource, TKey, TObject, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TObject>> baseQueryFunc,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
        => DelayLoadList(fieldBuilder, context => Task.FromResult(baseQueryFunc(context)), keySelector, itemSelector, graphType);

    /// <summary>
    /// Alters the field to take the result of the existing resolver and pass it to a DataLoader as a key for deferred loading of a list of entities.
    /// </summary>
    public static EfFieldBuilder<TDbContext, TSource, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TDbContext, TSource, TKey, TObject, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TKey> fieldBuilder,
        Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TObject>>> baseQueryFunc,
        Expression<Func<TObject, TKey>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
    {
        Func<EfEntityListDataLoader<TDbContext, TKey, TSource, TObject, TReturn>> delayLoaderFactory =
            () => new EfEntityListDataLoader<TDbContext, TKey, TSource, TObject, TReturn>(
                baseQueryFunc: baseQueryFunc,
                keySelector: keySelector,
                itemSelector: itemSelector);

        var dataLoaderId = Guid.NewGuid().ToString();
        var initialResolver = fieldBuilder.FieldType.Resolver!;
        var resolver3 = new FuncFieldResolver<IDataLoaderResult<IEnumerable<EfSource<TReturn>>>>(async context => {
            var delayLoader = context.RequestServices!
                .GetRequiredService<IDataLoaderContextAccessor>().Context!
                .GetOrAdd(dataLoaderId, delayLoaderFactory);
            var key = await initialResolver.ResolveAsync(context);
            if (key == null)
                return null;
            return delayLoader.LoadAsync(context, (TKey)key);
        });
        fieldBuilder.FieldType.Resolver = resolver3;
        if (graphType != null)
            graphType = graphType.GetNamedType();
        graphType = graphType.FindGraphForEfType<TReturn>(false);
        graphType = typeof(NonNullGraphType<>).MakeGenericType(typeof(ListGraphType<>).MakeGenericType(graphType));
        fieldBuilder.FieldType.Type = graphType;
        return fieldBuilder.Returns<IDataLoaderResult<IEnumerable<EfSource<TReturn>>>>().AsEfFieldBuilder<TDbContext, TSource, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>>();
    }
}
