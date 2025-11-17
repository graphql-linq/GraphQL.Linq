// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.DataLoader;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;

namespace GraphQL.Linq;

/// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}"/>
public class EfFieldBuilder<TDbContext, TSourceType, TReturnType> : FieldBuilder<TSourceType, TReturnType>
{
    internal EfFieldBuilder(FieldBuilder<TSourceType, TReturnType> fieldBuilder) : base(fieldBuilder.FieldType)
    {
    }

    #region DelayLoadEntry & DelayLoadList

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadEntry{TDbContext, TSource, TKey, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, IQueryable{TReturn}}, Expression{Func{TReturn, TKey}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, IQueryable<TReturn>> baseQueryFunc,
        Expression<Func<TReturn, TReturnType>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadEntry(this, baseQueryFunc, keySelector, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadEntry{TDbContext, TSource, TKey, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TReturn}}}, Expression{Func{TReturn, TKey}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, Task<IQueryable<TReturn>>> baseQueryFunc,
        Expression<Func<TReturn, TReturnType>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadEntry(this, baseQueryFunc, keySelector, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadEntry{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, IQueryable{TObject}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TObject, TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, IQueryable<TObject>> baseQueryFunc,
        Expression<Func<TObject, TReturnType>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadEntry(this, baseQueryFunc, keySelector, itemSelector, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadEntry{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<EfSource<TReturn>>> DelayLoadEntry<TObject, TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, Task<IQueryable<TObject>>> baseQueryFunc,
        Expression<Func<TObject, TReturnType>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadEntry(this, baseQueryFunc, keySelector, itemSelector, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadList{TDbContext, TSource, TKey, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, IQueryable{TReturn}}, Expression{Func{TReturn, TKey}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, IQueryable<TReturn>> baseQueryFunc,
        Expression<Func<TReturn, TReturnType>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadList(this, baseQueryFunc, keySelector, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadList{TDbContext, TSource, TKey, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TReturn}}}, Expression{Func{TReturn, TKey}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, Task<IQueryable<TReturn>>> baseQueryFunc,
        Expression<Func<TReturn, TReturnType>> keySelector,
        Type? graphType = null)
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadList(this, baseQueryFunc, keySelector, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadList{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, IQueryable{TObject}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TObject, TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, IQueryable<TObject>> baseQueryFunc,
        Expression<Func<TObject, TReturnType>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadList(this, baseQueryFunc, keySelector, itemSelector, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.DelayLoadList{TDbContext, TSource, TKey, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TKey}, Func{IResolveEfFieldContext{TDbContext, TSource}, Task{IQueryable{TObject}}}, Expression{Func{TObject, TKey}}, Expression{Func{TObject, TReturn}}, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, IDataLoaderResult<IEnumerable<EfSource<TReturn>>>> DelayLoadList<TObject, TReturn>(
        Func<IResolveEfFieldContext<TDbContext, TSourceType>, Task<IQueryable<TObject>>> baseQueryFunc,
        Expression<Func<TObject, TReturnType>> keySelector,
        Expression<Func<TObject, TReturn>> itemSelector,
        Type? graphType = null)
        where TObject : class
        where TReturn : class
        => FieldBuilderExtensions.DelayLoadList(this, baseQueryFunc, keySelector, itemSelector, graphType);

    #endregion

    #region ThenResolve

    /// <inheritdoc cref="FieldBuilderExtensions.ThenResolve{TSource, TObject, TReturn}(FieldBuilder{TSource, TObject}, Func{TObject, TReturn}, bool?, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, TReturn> ThenResolve<TReturn>(
        Func<TReturnType, TReturn> resolver,
        bool? nullable = null,
        Type? graphType = null)
        => FieldBuilderExtensions.ThenResolve(this, resolver, nullable, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.ThenResolve{TSource, TObject, TReturn}(FieldBuilder{TSource, TObject}, Func{IResolveFieldContext{TSource}, TObject, TReturn}, bool?, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, TReturn> ThenResolve<TReturn>(
        Func<IResolveFieldContext<TSourceType>, TReturnType, TReturn> resolver,
        bool? nullable = null,
        Type? graphType = null)
        => FieldBuilderExtensions.ThenResolve(this, resolver, nullable, graphType);

    /// <inheritdoc cref="FieldBuilderExtensions.ThenResolve{TSource, TObject, TReturn}(FieldBuilder{TSource, TObject}, Func{IResolveFieldContext{TSource}, TObject, Task{TReturn}}, bool?, Type?)"/>
    public EfFieldBuilder<TDbContext, TSourceType, TReturn> ThenResolve<TReturn>(
        Func<IResolveFieldContext<TSourceType>, TReturnType, Task<TReturn>> resolver,
        bool? nullable = null,
        Type? graphType = null)
        => FieldBuilderExtensions.ThenResolve(this, resolver, nullable, graphType);

    #endregion

    #region Overrides of FieldBuilder fluent methods

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Argument(Type, string, Action{QueryArgument}?)"/>/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Argument(Type type, string name, Action<QueryArgument>? configure = null)
        => base.Argument(type, name, configure).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Argument(IGraphType, string, Action{QueryArgument}?)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Argument(IGraphType type, string name, Action<QueryArgument>? configure = null)
        => base.Argument(type, name, configure).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Argument{TArgumentClrType}(string, bool, Action{QueryArgument}?)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Argument<TArgumentClrType>(string name, bool nullable = false, Action<QueryArgument>? configure = null)
        => base.Argument<TArgumentClrType>(name, nullable, configure).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Argument{TArgumentClrType}(string, bool, string?, Action{QueryArgument}?)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Argument<TArgumentClrType>(string name, bool nullable, string? description, Action<QueryArgument>? configure = null)
        => base.Argument<TArgumentClrType>(name, nullable, description, configure).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Argument{TArgumentGraphType}(string)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Argument<TArgumentGraphType>(string name)
        where TArgumentGraphType : IGraphType
        => base.Argument<TArgumentGraphType>(name).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Argument{TArgumentGraphType}(string, Action{QueryArgument}?)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Argument<TArgumentGraphType>(string name, Action<QueryArgument>? configure = null)
        where TArgumentGraphType : IGraphType
        => base.Argument<TArgumentGraphType>(name, configure).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Argument{TArgumentGraphType}(string, string?, Action{QueryArgument}?)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Argument<TArgumentGraphType>(string name, string? description, Action<QueryArgument>? configure = null)
        where TArgumentGraphType : IGraphType
        => base.Argument<TArgumentGraphType>(name, description, configure).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Arguments(IEnumerable{QueryArgument})"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Arguments(IEnumerable<QueryArgument> arguments)
        => base.Arguments(arguments).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Arguments(QueryArgument[])"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Arguments(params QueryArgument[] arguments)
        => base.Arguments(arguments).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Description(string?)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Description(string? description)
        => base.Description(description).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.DeprecationReason(string?)"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> DeprecationReason(string? deprecationReason)
        => base.DeprecationReason(deprecationReason).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    /// <inheritdoc cref="FieldBuilder{TSourceType, TReturnType}.Configure(Action{FieldType})"/>
    public new EfFieldBuilder<TDbContext, TSourceType, TReturnType> Configure(Action<FieldType> configure)
        => base.Configure(configure).AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>();

    #endregion
}
