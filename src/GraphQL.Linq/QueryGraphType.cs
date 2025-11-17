// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq;

/// <summary>
/// Base class for GraphQL query types that integrate with LINQ data sources.
/// </summary>
public abstract class QueryGraphType<TDbContext> : QueryGraphType<TDbContext, object>
{
    /// <summary>
    /// Initializes a new instance of the QueryGraphType class.
    /// </summary>
    public QueryGraphType(IEfGraphQLService<TDbContext> efGraphQLService) : base(efGraphQLService) { }
}

/// <summary>
/// Base class for GraphQL query types that integrate with LINQ data sources and have a specific source type.
/// </summary>
public abstract class QueryGraphType<TDbContext, TSource> : ObjectGraphType<TSource>
{
    /// <summary>
    /// The GraphQL service used for field resolution and LINQ query execution.
    /// </summary>
    protected IEfGraphQLService<TDbContext> EfGraphQLService { get; }
    /// <summary>
    /// Initializes a new instance of the QueryGraphType class.
    /// </summary>
    public QueryGraphType(IEfGraphQLService<TDbContext> efGraphQLService)
    {
        EfGraphQLService = efGraphQLService ?? throw new ArgumentNullException(nameof(efGraphQLService));
    }

    //=================== EfQueryField =========================

    /// <summary>
    /// Adds a field that resolves data using a LINQ query and returns a list of results.
    /// </summary>
    public virtual FieldBuilder<TSource, IEnumerable<EfSource<TReturn>>> EfQueryField<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfQueryField(this, EfGraphQLService, name, resolve, graphType, arguments);
    }

    /// <summary>
    /// Adds a field that asynchronously resolves data using a LINQ query and returns a list of results.
    /// </summary>
    public virtual FieldBuilder<TSource, IEnumerable<EfSource<TReturn>>> EfQueryFieldAsync<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfQueryFieldAsync(this, EfGraphQLService, name, resolve, graphType, arguments);
    }

    //=================== EfQueryConnectionField ======================

    /// <summary>
    /// Adds a paginated connection field that resolves data using a LINQ query with a default page size.
    /// </summary>
    public virtual FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionField<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfQueryConnectionField(this, EfGraphQLService, name, resolve, defaultPageSize, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field that resolves data using a LINQ query with a custom connection resolver.
    /// </summary>
    public virtual FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionField<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfQueryConnectionField(this, EfGraphQLService, name, resolve, connectionResolver, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field that asynchronously resolves data using a LINQ query with a default page size.
    /// </summary>
    public virtual FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsync<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfQueryConnectionFieldAsync(this, EfGraphQLService, name, resolve, defaultPageSize, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field that asynchronously resolves data using a LINQ query with a custom connection resolver.
    /// </summary>
    public virtual FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsync<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfQueryConnectionFieldAsync(this, EfGraphQLService, name, resolve, connectionResolver, graphType, arguments);
    }

    //================= EfSingleField ===================

    /// <summary>
    /// Adds a field that resolves a single entity using a LINQ query, optionally with an automatic ID argument.
    /// </summary>
    public virtual FieldBuilder<TSource, EfSource<TReturn>> EfSingleField<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, bool nullable = false, Type? graphType = null, bool addIdArgument = true, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfSingleField(this, EfGraphQLService, name, resolve, nullable, graphType, addIdArgument, arguments);
    }

    /// <summary>
    /// Adds a field that asynchronously resolves a single entity using a LINQ query, optionally with an automatic ID argument.
    /// </summary>
    public virtual FieldBuilder<TSource, EfSource<TReturn>> EfSingleFieldAsync<TReturn>(string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, bool nullable = false, Type? graphType = null, bool addIdArgument = true, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return ComplexGraphTypeExtensions.EfSingleFieldAsync(this, EfGraphQLService, name, resolve, nullable, graphType, addIdArgument, arguments);
    }

}
