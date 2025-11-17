// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Resolvers;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq.FieldResolvers;

/// <summary>
/// Field resolver that executes LINQ queries and returns paginated connection results.
/// </summary>
public class QueryConnectionResolver<TDbContext, TSource, TReturn> : QueryExecuter<TDbContext, TReturn>, IFieldResolver where TReturn : class
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
    /// The connection resolver that handles pagination logic.
    /// </summary>
    protected IEfConnectionResolver<TDbContext, TReturn> EfConnectionResolver { get; }

    /// <summary>
    /// Initializes a new instance of the QueryConnectionResolver class.
    /// </summary>
    public QueryConnectionResolver(IEfGraphQLService<TDbContext> efGraphQLService, Func<IResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve, IEfConnectionResolver<TDbContext, TReturn> efConnectionResolver)
    {
        EfGraphQLService = efGraphQLService ?? throw new ArgumentNullException(nameof(efGraphQLService));
        Resolver = resolve ?? throw new ArgumentNullException(nameof(resolve));
        EfConnectionResolver = efConnectionResolver ?? throw new ArgumentNullException(nameof(efConnectionResolver));
    }

    /// <summary>
    /// Resolves the field by executing the LINQ query and applying connection pagination.
    /// </summary>
    public virtual async ValueTask<object?> ResolveAsync(IResolveFieldContext context)
    {
        //create the typed efContext
        //var efContext = _efGraphQLService.BuildEfContextFromGraphQlContext<TSource>(context ?? throw new ArgumentNullException(nameof(context)));
        //var resolver = (IEfConnectionResolver<TDbContext, TReturn>)efContext.FieldDefinition.Metadata["_EF_ConnectionResolver"];
        //var connectionContext = IResolveEfConnectionContext<TDbContext, TSource>.Create(efContext, resolver.IsBidirectional, resolver.DefaultPageSize);
        var connectionContext = new ResolveEfConnectionContext<TDbContext, TSource>(
            EfGraphQLService,
            context,
            EfConnectionResolver.IsBidirectional,
            EfConnectionResolver.DefaultPageSize);

        return await ResolveAsync(connectionContext);
    }

    /// <summary>
    /// Resolves the field by executing the LINQ query and applying connection pagination.
    /// Returns a strongly-typed connection result.
    /// </summary>
    public virtual async Task<Connection<EfSource<TReturn>>> ResolveAsync(IResolveEfConnectionContext<TDbContext, TSource> connectionContext)
    {
        //run the resolve function that was configured for this field, which will return an IQueryable<>
        // -- e.g. _resolve = (context) => context.DbContext.Products.Where(p => !p.Deleted)
        var query = await Resolver(connectionContext); //get starting query
        // -- e.g. baseQuery is now an IQueryable<Product>, such as equivalent to:
        //    context.DbContext.Products.Where(p => !p.Deleted)

        return await ExecuteConnectionAsync(connectionContext, EfConnectionResolver, query);
    }
}
