// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq;

/// <summary>
/// Extension methods for adding query connection fields to GraphQL complex types.
/// </summary>
public static partial class ComplexGraphTypeExtensions
{

    //=================== EfQueryConnectionField ======================

    /// <summary>
    /// Adds a paginated connection field to the graph type that resolves data using a LINQ query with a default page size.
    /// </summary>
    public static FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionField<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfQueryConnectionFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), defaultPageSize, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field to the graph type that resolves data using a LINQ query with a custom connection resolver.
    /// </summary>
    public static FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionField<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfQueryConnectionFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), connectionResolver, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field to the graph type that resolves data using a LINQ query with a default page size.
    /// </summary>
    public static FieldBuilder<object, Connection<EfSource<TReturn>>> EfQueryConnectionField<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfQueryConnectionFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), defaultPageSize, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field to the graph type that resolves data using a LINQ query with a custom connection resolver.
    /// </summary>
    public static FieldBuilder<object, Connection<EfSource<TReturn>>> EfQueryConnectionField<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfQueryConnectionFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), connectionResolver, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field to the graph type that asynchronously resolves data using a LINQ query with a default page size.
    /// </summary>
    public static FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsync<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfQueryConnectionFieldAsyncInternal(graph, efGraphQLService, name, resolve, defaultPageSize, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field to the graph type that asynchronously resolves data using a LINQ query with a custom connection resolver.
    /// </summary>
    public static FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsync<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfQueryConnectionFieldAsyncInternal(graph, efGraphQLService, name, resolve, connectionResolver, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field to the graph type that asynchronously resolves data using a LINQ query with a default page size.
    /// </summary>
    public static FieldBuilder<object, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsync<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfQueryConnectionFieldAsyncInternal(graph, efGraphQLService, name, resolve, defaultPageSize, graphType, arguments);
    }

    /// <summary>
    /// Adds a paginated connection field to the graph type that asynchronously resolves data using a LINQ query with a custom connection resolver.
    /// </summary>
    public static FieldBuilder<object, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsync<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfQueryConnectionFieldAsyncInternal(graph, efGraphQLService, name, resolve, connectionResolver, graphType, arguments);
    }

    private static FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsyncInternal<TDbContext, TSource, TReturn>(IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (efGraphQLService == null)
            throw new ArgumentNullException(nameof(efGraphQLService));
        return EfQueryConnectionFieldAsyncInternal(graph, efGraphQLService, name, resolve, new EfSimpleConnectionResolver<TDbContext, TReturn>(defaultPageSize), graphType, arguments);
    }

    private static FieldBuilder<TSource, Connection<EfSource<TReturn>>> EfQueryConnectionFieldAsyncInternal<TDbContext, TSource, TReturn>(
        IComplexGraphType graph,
        IEfGraphQLService<TDbContext> efGraphQLService,
        string name,
        Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, IEfConnectionResolver<TDbContext, TReturn> connectionResolver,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null)
        where TReturn : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (efGraphQLService == null)
            throw new ArgumentNullException(nameof(efGraphQLService));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        if (connectionResolver == null)
            throw new ArgumentNullException(nameof(connectionResolver));
        //obtain the type
        //graphType = graphType.FindGraphForEfType<TReturn>(true);// false);
        if (graphType != null)
            throw new NotSupportedException("The graphType parameter is not supported in this version of the method.");

        var queryResolver = new QueryConnectionResolver<TDbContext, TSource, TReturn>(efGraphQLService, resolve, connectionResolver);
        var graphType2 = typeof(NonNullGraphType<>).MakeGenericType(typeof(EfConnectionType<>).MakeGenericType(typeof(TReturn)));
        var builder = FieldBuilder<TSource, Connection<EfSource<TReturn>>>.Create(name, graphType2);
        builder.Argument<IntGraphType>("first", "Specifies the number of edges to return starting from `after` or the first entry if `after` is not specified.");
        builder.Argument<StringGraphType>("after", "Only look at connected edges with cursors greater than the value of `after`.");
        if (connectionResolver.IsBidirectional) {
            builder.Argument<IntGraphType>("last", "Specifies the number of edges to return counting reversely from `before`, or the last entry if `before` is not specified.");
            builder.Argument<StringGraphType>("before", "Only look at connected edges with cursors smaller than the value of `before`.");
        }
        if (arguments != null)
            foreach (var argument in arguments)
                (builder.FieldType.Arguments ??= new QueryArguments()).Add(argument);
        builder.Resolve(queryResolver);

        var fieldType = builder.FieldType;
        fieldType.SetEfMetadata(new EfMetadata {
            Type = typeof(TReturn),
        });

        graph.AddField(fieldType);
        return builder;
    }
}
