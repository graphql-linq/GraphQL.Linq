// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Resolvers;

namespace GraphQL.Linq.FieldResolvers;

/// <summary>
/// Field resolver that executes LINQ queries and returns lists of results.
/// </summary>
public class QueryResolver<TDbContext, TSource, TReturn> : QueryExecuter<TDbContext, TReturn>, IFieldResolver where TReturn : class
{
    /// <summary>
    /// The function that resolves the LINQ query for this field.
    /// </summary>
    protected Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> Resolver { get; }
    /// <summary>
    /// The GraphQL service used for query execution and context building.
    /// </summary>
    protected IEfGraphQLService<TDbContext> EfGraphQLService { get; }

    /// <summary>
    /// Initializes a new instance of the QueryResolver class.
    /// </summary>
    public QueryResolver(IEfGraphQLService<TDbContext> efGraphQLService, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve)
    {
        EfGraphQLService = efGraphQLService ?? throw new ArgumentNullException(nameof(efGraphQLService));
        Resolver = resolve ?? throw new ArgumentNullException(nameof(resolve));
    }

    /// <summary>
    /// Resolves the field by executing the LINQ query and returning a list of results.
    /// </summary>
    public virtual async Task<IList<EfSource<TReturn>>> ResolveAsync(IResolveFieldContext context)
    {
        var ret = await ResolveQueryAsync(context);

        return await EfGraphQLService.QueryToListAsync(ret, context.CancellationToken);
    }

    async ValueTask<object?> IFieldResolver.ResolveAsync(IResolveFieldContext context)
        => await ResolveAsync(context);

    /// <summary>
    /// Builds and returns the LINQ query based on the GraphQL field context and selected sub-fields.
    /// </summary>
    protected virtual async Task<IQueryable<EfSource<TReturn>>> ResolveQueryAsync(IResolveFieldContext context)
    {
        //copy the context to into efContext (a IResolveEfFieldContext class) along with the DbContext (provided by _efGraphQLService)
        var efContext = EfGraphQLService.BuildResolveEfFieldContext<TSource>(context ?? throw new ArgumentNullException(nameof(context)));

        //run the resolve function that was configured for this field, which will return an IQueryable<>
        // -- e.g. _resolve = (context) => context.DbContext.Products.Where(p => !p.Deleted)
        var baseQuery = await Resolver(efContext); //get starting query
        // -- e.g. baseQuery is now an IQueryable<Product>, such as equivalent to:
        //    context.DbContext.Products.Where(p => !p.Deleted)

        //apply where, skip, take, and orderby arguments to baseQuery
        // -- e.g. orderby name
        //var whereQuery = baseQuery.ApplyGraphQlArguments(efContext, _efGraphQLService.GetKeyNames<TReturn>());
        // -- e.g. whereQuery would be the same as:
        //    context.DbContext.Products.Where(p => !p.Deleted).OrderBy(p => p.Name)

        //examine selected child nodes, generate a query, and return the unexecuted query
        return GenerateQuery(efContext, baseQuery);
    }

}
