// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;

namespace GraphQL.Linq;

public static partial class ComplexGraphTypeExtensions
{
    //================= EfSingleField ===================

    /// <summary>
    /// Adds a field to the graph type that resolves a single entity using a LINQ query, optionally with an automatic ID argument.
    /// </summary>
    public static FieldBuilder<TSource, EfSource<TReturn>> EfSingleField<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve, bool nullable = false, Type? graphType = null, bool addIdArgument = true, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfSingleFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), nullable, graphType, addIdArgument, arguments);
    }

    /// <summary>
    /// Adds a field to the graph type that resolves a single entity using a LINQ query, optionally with an automatic ID argument.
    /// </summary>
    public static FieldBuilder<object, EfSource<TReturn>> EfSingleField<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve, bool nullable = false, Type? graphType = null, bool addIdArgument = true, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        if (resolve == null)
            throw new ArgumentNullException(nameof(resolve));
        return EfSingleFieldAsync(graph, efGraphQLService, name, context => Task.FromResult(resolve(context)), nullable, graphType, addIdArgument, arguments);
    }

    /// <summary>
    /// Adds a field to the graph type that asynchronously resolves a single entity using a LINQ query, optionally with an automatic ID argument.
    /// </summary>
    public static FieldBuilder<TSource, EfSource<TReturn>> EfSingleFieldAsync<TDbContext, TSource, TReturn>(this ComplexGraphType<TSource> graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, bool nullable = false, Type? graphType = null, bool addIdArgument = true, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfSingleFieldAsyncInternal(graph, efGraphQLService, name, resolve, nullable, graphType, addIdArgument, arguments);
    }

    /// <summary>
    /// Adds a field to the graph type that asynchronously resolves a single entity using a LINQ query, optionally with an automatic ID argument.
    /// </summary>
    public static FieldBuilder<object, EfSource<TReturn>> EfSingleFieldAsync<TDbContext, TReturn>(this IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve, bool nullable = false, Type? graphType = null, bool addIdArgument = true, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
    {
        return EfSingleFieldAsyncInternal(graph, efGraphQLService, name, resolve, nullable, graphType, addIdArgument, arguments);
    }

    private static FieldBuilder<TSource, EfSource<TReturn>> EfSingleFieldAsyncInternal<TDbContext, TSource, TReturn>(IComplexGraphType graph, IEfGraphQLService<TDbContext> efGraphQLService, string name, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, bool nullable = false, Type? graphType = null, bool addIdArgument = true, IEnumerable<QueryArgument>? arguments = null) where TReturn : class
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
        graphType = graphType.FindGraphForEfType<TReturn>(nullable);

        if (addIdArgument) {
            var keyName = efGraphQLService.GetPrimaryKeyNames<TReturn>();
            if (keyName.Count() != 1)
                throw new InvalidOperationException($"The type {typeof(TReturn).Name} does not have exactly one primary key.");
            var applyIdArgumentDelegate = CreateApplyIdArgumentDelegate<TReturn>(keyName.Single());
            var resolveBase = resolve;
            resolve = async context => {
                var query = await resolveBase(context).ConfigureAwait(false);
                return applyIdArgumentDelegate(context, query);
            };
        }

        var builder = FieldBuilder<TSource, EfSource<TReturn>>.Create(name, graphType)
            .Resolve(new SingleQueryResolver<TDbContext, TSource, TReturn>(efGraphQLService, resolve, nullable));

        if (addIdArgument)
            builder.Argument<NonNullGraphType<IdGraphType>>("id");

        if (arguments != null)
            foreach (var argument in arguments)
                (builder.FieldType.Arguments ??= new QueryArguments()).Add(argument);

        graph.AddField(builder.FieldType);
        return builder;
    }

    private static Func<IResolveFieldContext, IQueryable<TReturn>, IQueryable<TReturn>> CreateApplyIdArgumentDelegate<TReturn>(string keyName)
    {
        var propertyInfo = typeof(TReturn).GetProperty(keyName ?? throw new ArgumentNullException(nameof(keyName)))
            ?? throw new ArgumentException($"The type {typeof(TReturn).Name} does not have a property named {keyName}.");

        return (context, query) => {
            var idValue = context.GetArgument(propertyInfo.PropertyType, "id");
            var expressionParameter = Expression.Parameter(typeof(TReturn), "x");
            var predicate = Expression.Lambda<Func<TReturn, bool>>(
                Expression.Equal(
                    Expression.Property(
                        expressionParameter,
                        propertyInfo),
                    Expression.Constant(idValue, propertyInfo.PropertyType)),
                expressionParameter);
            return query.Where(predicate);
        };
    }
}
