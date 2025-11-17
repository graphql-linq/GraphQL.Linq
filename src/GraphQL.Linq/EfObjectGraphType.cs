// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.DataLoaders;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq;

/// <summary>
/// Base class for GraphQL object types that integrate with LINQ data sources, providing methods to create fields that can be efficiently resolved using LINQ expressions.
/// </summary>
public abstract class EfObjectGraphType<TDbContext, TSource> : ObjectGraphType<EfSource<TSource>>, IEfObjectGraphType<TDbContext, TSource> where TSource : class
{
    /// <inheritdoc cref="EfFieldHelpers.EfField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?, Type)"/>
    public virtual EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfField<TProperty>(Expression<Func<TSource, TProperty?>> expression, bool? nullable = null, Type? graphType = null)
        => EfFieldHelpers.EfField(this, expression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TSource, TProperty}}, bool?, Type)"/>
    public virtual EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfField<TProperty>(string name, Expression<Func<TSource, TProperty?>> expression, bool? nullable = null, Type? graphType = null)
        => EfFieldHelpers.EfField(this, name, expression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfField{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, TReturn}}, bool?, Type)"/>
    public virtual EfFieldBuilder<TDbContext, EfSource<TSource>, TReturn> EfField<TReturn>(string name, Expression<Func<TDbContext, TSource, TReturn?>> expression, bool? nullable = null, Type? graphType = null)
        => EfFieldHelpers.EfField(this, name, expression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfFieldFromContext{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Func{IResolveEfFieldContext{TDbContext, object}, Expression{Func{TSource, TReturn}}}, bool, Type?)"/>
    public virtual EfFieldBuilder<TDbContext, EfSource<TSource>, TReturn> EfFieldFromContext<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, TReturn?>>> resolveExpression, bool nullable = false, Type? graphType = null)
        => EfFieldHelpers.EfFieldFromContext(this, name, resolveExpression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfIdField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?)"/>
    public virtual EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfIdField<TProperty>(Expression<Func<TSource, TProperty?>> expression, bool? nullable = null)
        => EfFieldHelpers.EfIdField(this, expression, nullable);

    /// <inheritdoc cref="EfFieldHelpers.EfIdField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TSource, TProperty}}, bool?)"/>
    public virtual EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfIdField<TProperty>(string name, Expression<Func<TSource, TProperty?>> expression, bool? nullable = null)
        => EfFieldHelpers.EfIdField(this, name, expression, nullable);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, EfSource<TProperty>> EfNavigationField<TProperty>(Expression<Func<TSource, TProperty?>> expression, bool? nullable = null, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationField(this, expression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TSource, TProperty}}, bool?, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, EfSource<TProperty>> EfNavigationField<TProperty>(string name, Expression<Func<TSource, TProperty?>> expression, bool? nullable = null, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationField(this, name, expression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationField{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, TReturn}}, bool?, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, EfSource<TProperty>> EfNavigationField<TProperty>(string name, Expression<Func<TDbContext, TSource, TProperty?>> expression, bool? nullable = null, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationField(this, name, expression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationFieldLink{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, IEnumerable{TProperty}}}, bool, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, EfSource<TProperty>> EfNavigationFieldLink<TProperty>(string name, Expression<Func<TDbContext, TSource, IEnumerable<TProperty>>> expression, bool nullable = false, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationFieldLink(this, name, expression, nullable, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationListField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, IEnumerable{TProperty}}}, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TProperty>>> EfNavigationListField<TProperty>(Expression<Func<TSource, IEnumerable<TProperty>>> expression, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationListField(this, expression, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationListField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TSource, IEnumerable{TProperty}}}, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TProperty>>> EfNavigationListField<TProperty>(string name, Expression<Func<TSource, IEnumerable<TProperty>>> expression, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationListField(this, name, expression, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationListField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, IEnumerable{TProperty}}}, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TProperty>>> EfNavigationListField<TProperty>(string name, Expression<Func<TDbContext, TSource, IEnumerable<TProperty>>> expression, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationListField(this, name, expression, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationListFieldFromContext{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Func{IResolveEfFieldContext{TDbContext, object}, Expression{Func{TSource, IEnumerable{TReturn}}}}, Type, IEnumerable{QueryArgument})"/>
    public virtual FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TProperty>>> EfNavigationListFieldFromContext<TProperty>(string name, Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, IEnumerable<TProperty>>>> expression, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TProperty : class
        => EfFieldHelpers.EfNavigationListFieldFromContext(this, name, expression, graphType, arguments);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, IEnumerable{TProperty}}}, int, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TProperty>>> EfNavigationConnectionField<TProperty>(Expression<Func<TSource, IEnumerable<TProperty>>> expression, int defaultPageSize = 100, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationConnectionField(this, expression, defaultPageSize, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, IEnumerable{TProperty}}}, IEfConnectionResolver{TDbContext, TProperty}, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TProperty>>> EfNavigationConnectionField<TProperty>(Expression<Func<TSource, IEnumerable<TProperty>>> expression, IEfConnectionResolver<TDbContext, TProperty> connectionResolver, Type? graphType = null) where TProperty : class
        => EfFieldHelpers.EfNavigationConnectionField(this, expression, connectionResolver, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionField{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TSource, IEnumerable{TReturn}}}, int, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TReturn>(string name, Expression<Func<TSource, IEnumerable<TReturn>>> expression, int defaultPageSize = 100, Type? graphType = null) where TReturn : class
        => EfFieldHelpers.EfNavigationConnectionField(this, name, expression, defaultPageSize, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionField{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TSource, IEnumerable{TReturn}}}, IEfConnectionResolver{TDbContext, TReturn}, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TReturn>(string name, Expression<Func<TSource, IEnumerable<TReturn>>> expression, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null) where TReturn : class
        => EfFieldHelpers.EfNavigationConnectionField(this, name, expression, connectionResolver, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionField{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, IEnumerable{TReturn}}}, int, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TReturn>(string name, Expression<Func<TDbContext, TSource, IEnumerable<TReturn>>> expression, int defaultPageSize = 100, Type? graphType = null) where TReturn : class
        => EfFieldHelpers.EfNavigationConnectionField(this, name, expression, defaultPageSize, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionField{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, IEnumerable{TReturn}}}, IEfConnectionResolver{TDbContext, TReturn}, Type)"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TReturn>(string name, Expression<Func<TDbContext, TSource, IEnumerable<TReturn>>> expression, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null) where TReturn : class
        => EfFieldHelpers.EfNavigationConnectionField(this, name, expression, connectionResolver, graphType);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionFieldFromContext{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Func{IResolveEfFieldContext{TDbContext, object}, Expression{Func{TSource, IEnumerable{TReturn}}}}, int?, Type, IEnumerable{QueryArgument})"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionFieldFromContext<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, IEnumerable<TReturn>>>> expression, int defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
        => EfFieldHelpers.EfNavigationConnectionFieldFromContext(this, name, expression, defaultPageSize, graphType, arguments);

    /// <inheritdoc cref="EfFieldHelpers.EfNavigationConnectionFieldFromContext{TDbContext, TSource, TReturn}(IEfObjectGraphType{TDbContext, TSource}, string, Func{IResolveEfFieldContext{TDbContext, object}, Expression{Func{TSource, IEnumerable{TReturn}}}}, IEfConnectionResolver{TDbContext, TReturn}, Type, IEnumerable{QueryArgument})"/>
    public virtual FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionFieldFromContext<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, IEnumerable<TReturn>>>> expression, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
        => EfFieldHelpers.EfNavigationConnectionFieldFromContext(this, name, expression, connectionResolver, graphType, arguments);

    /// <summary>
    /// Creates a data loader for efficiently loading single entities by key with batching support.
    /// </summary>
    public virtual EfEntityDataLoader<TDbContext, TKey, EfSource<TSource>, TReturn, TReturn> CreateDelayedEntryLoader<TKey, TReturn>(Func<IResolveEfFieldContext<TDbContext, EfSource<TSource>>, IQueryable<TReturn>> baseQueryFunc, Expression<Func<TReturn, TKey>> keySelector) where TReturn : class
        => new EfEntityDataLoader<TDbContext, TKey, EfSource<TSource>, TReturn>(baseQueryFunc, keySelector);

    /// <summary>
    /// Creates a data loader for efficiently loading lists of entities by key with batching support.
    /// </summary>
    public virtual EfEntityListDataLoader<TDbContext, TKey, EfSource<TSource>, TReturn, TReturn> CreateDelayedListLoader<TKey, TReturn>(Func<IResolveEfFieldContext<TDbContext, EfSource<TSource>>, IQueryable<TReturn>> baseQueryFunc, Expression<Func<TReturn, TKey>> keySelector) where TReturn : class
        => new EfEntityListDataLoader<TDbContext, TKey, EfSource<TSource>, TReturn>(baseQueryFunc, keySelector);

    /// <summary>
    /// Creates a data loader for efficiently loading lists of projected entities by key with batching support.
    /// </summary>
    public virtual EfEntityListDataLoader<TDbContext, TKey, EfSource<TSource>, TObject, TReturn> CreateDelayedListLoader<TKey, TObject, TReturn>(Func<IResolveEfFieldContext<TDbContext, EfSource<TSource>>, IQueryable<TObject>> baseQueryFunc, Expression<Func<TObject, TKey>> keySelector, Expression<Func<TObject, TReturn>> itemSelector) where TObject : class where TReturn : class
        => new EfEntityListDataLoader<TDbContext, TKey, EfSource<TSource>, TObject, TReturn>(baseQueryFunc, keySelector, itemSelector);
}
