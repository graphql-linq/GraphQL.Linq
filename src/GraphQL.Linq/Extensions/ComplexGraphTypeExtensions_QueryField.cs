// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;

namespace GraphQL.Linq;

public static partial class ComplexGraphTypeExtensions
{
    //=================== EfQueryField ======================

    /// <summary>
    /// Adds a field to the graph type that resolves data using a LINQ query and returns a list of results.
    /// </summary>
    public static FieldBuilder<TSource, IEnumerable<EfSource<TReturn>>> EfQueryField<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfQueryFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), graphType, arguments);
    }

    /// <summary>
    /// Adds a field to the graph type that resolves data using a LINQ query and returns a list of results.
    /// </summary>
    public static FieldBuilder<object, IEnumerable<EfSource<TReturn>>> EfQueryField<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfQueryFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), graphType, arguments);
    }

    /// <summary>
    /// Adds a field to the graph type that asynchronously resolves data using a LINQ query and returns a list of results.
    /// </summary>
    public static FieldBuilder<TSource, IEnumerable<EfSource<TReturn>>> EfQueryFieldAsync<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfQueryFieldAsyncInternal(graph, efGraphQLService, name, resolve, graphType, arguments);
    }

    /// <summary>
    /// Adds a field to the graph type that asynchronously resolves data using a LINQ query and returns a list of results.
    /// </summary>
    public static FieldBuilder<object, IEnumerable<EfSource<TReturn>>> EfQueryFieldAsync<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfQueryFieldAsyncInternal(graph, efGraphQLService, name, resolve, graphType, arguments);
    }

    private static FieldBuilder<TSource, IEnumerable<EfSource<TReturn>>> EfQueryFieldAsyncInternal<TDbContext, TSource, TReturn>(IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (efGraphQLService == null)
            throw new ArgumentNullException(nameof(efGraphQLService));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));

        //obtain the type
        graphType = graphType.FindGraphForEfListType<TReturn>();

        var builder = FieldBuilder<TSource, IEnumerable<EfSource<TReturn>>>.Create(name, graphType)
            .Resolve(new QueryResolver<TDbContext, TSource, TReturn>(efGraphQLService, resolve));
        if (arguments != null)
            foreach (var argument in arguments)
                (builder.FieldType.Arguments ??= new QueryArguments()).Add(argument);

        graph.AddField(builder.FieldType);
        return builder;
    }



}
