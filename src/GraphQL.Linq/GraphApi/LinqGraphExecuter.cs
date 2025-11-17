// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.GraphApi;

/// <inheritdoc/>
public sealed class LinqGraphExecuter : ILinqGraphExecuter
{
    /// <summary>
    /// The providers used to determine the DbContext type from an IQueryable.
    /// </summary>
    private readonly IEfDbContextTypeProvider _dbContextTypeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinqGraphExecuter"/> class.
    /// </summary>
    /// <param name="dbContextTypeProvider">The provider used to determine the DbContext type from an IQueryable.</param>
    public LinqGraphExecuter(IEfDbContextTypeProvider dbContextTypeProvider)
    {
        _dbContextTypeProvider = dbContextTypeProvider ?? throw new ArgumentNullException(nameof(dbContextTypeProvider));
    }

    /// <summary>
    /// Gets the typed <see cref="ILinqGraphExecuter{TDbContext}"/> for the specified query and casts it to <see cref="ILinqGraphExecuter"/>.
    /// </summary>
    /// <param name="context">The GraphQL field resolution context.</param>
    /// <param name="query">The query to determine the DbContext type from.</param>
    /// <returns>The untyped <see cref="ILinqGraphExecuter"/> instance.</returns>
    private ILinqGraphExecuter GetExecuter(IResolveFieldContext context, IQueryable query)
    {
        var dbContextType = _dbContextTypeProvider.GetDbContextType(query)
            ?? throw new InvalidOperationException("Unable to determine DbContext type from the provided IQueryable. No registered IEfDbContextTypeProvider could identify the DbContext type.");

        var executerType = typeof(ILinqGraphExecuter<>).MakeGenericType(dbContextType);
        var requestServices = context.RequestServices
            ?? throw new InvalidOperationException("The IResolveFieldContext does not have a valid RequestServices IServiceProvider.");
        var executer = requestServices.GetRequiredService(executerType);

        return (ILinqGraphExecuter)executer;
    }

    /// <inheritdoc/>
    public Task<EfSource<TReturn>> ExecuteSingleAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
    {
        var executer = GetExecuter(context, query);
        return executer.ExecuteSingleAsync(context, query);
    }

    /// <inheritdoc/>
    public Task<EfSource<TReturn>?> ExecuteSingleOrDefaultAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
    {
        var executer = GetExecuter(context, query);
        return executer.ExecuteSingleOrDefaultAsync(context, query);
    }

    /// <inheritdoc/>
    public Task<IList<EfSource<TReturn>>> ExecuteQueryAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
    {
        var executer = GetExecuter(context, query);
        return executer.ExecuteQueryAsync(context, query);
    }

    /// <inheritdoc/>
    public Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, int defaultPageSize = 100) where TReturn : class
    {
        var executer = GetExecuter(context, query);
        return executer.ExecuteConnectionAsync<TSource, TReturn>(context, query, first, last, after, before, defaultPageSize);
    }

    /// <inheritdoc/>
    public Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, IEfConnectionResolver<TReturn> connectionResolver) where TReturn : class
    {
        var executer = GetExecuter(context, query);
        return executer.ExecuteConnectionAsync<TSource, TReturn>(context, query, first, last, after, before, connectionResolver);
    }

    /// <inheritdoc/>
    public Task<IList<Tuple<TKey, EfSource<TReturn>>>> ExecuteQueryForKeysAsync<TKey, TObject, TReturn>(IResolveFieldContext context, IQueryable<TObject> query, Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys, Expression<Func<TObject, TReturn>> itemSelector) where TObject : class where TReturn : class
    {
        var executer = GetExecuter(context, query);
        return executer.ExecuteQueryForKeysAsync(context, query, keySelector, keys, itemSelector);
    }
}

/// <inheritdoc/>
public class LinqGraphExecuter<TDbContext> : ILinqGraphExecuter<TDbContext>
{
    /// <summary>
    /// The GraphQL service used for query execution and context building.
    /// </summary>
    private readonly IEfGraphQLService<TDbContext> _efGraphQLService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinqGraphExecuter{TDbContext}"/> class.
    /// </summary>
    /// <param name="efGraphQLService">The GraphQL service used for query execution and context building.</param>
    public LinqGraphExecuter(IEfGraphQLService<TDbContext> efGraphQLService)
    {
        _efGraphQLService = efGraphQLService ?? throw new ArgumentNullException(nameof(efGraphQLService));
    }

    /// <inheritdoc/>
    public virtual Task<EfSource<TReturn>> ExecuteSingleAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
    {
        var resolver = new SingleQueryResolver<TDbContext, object, TReturn>(_efGraphQLService, ctx => Task.FromResult(query), false);
        return resolver.ResolveAsync(context)!;
    }

    /// <inheritdoc/>
    public virtual Task<EfSource<TReturn>?> ExecuteSingleOrDefaultAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
    {
        var resolver = new SingleQueryResolver<TDbContext, object, TReturn>(_efGraphQLService, ctx => Task.FromResult(query), true);
        return resolver.ResolveAsync(context);
    }

    /// <inheritdoc/>
    public virtual Task<IList<EfSource<TReturn>>> ExecuteQueryAsync<TReturn>(IResolveFieldContext context, IQueryable<TReturn> query) where TReturn : class
    {
        var resolver = new QueryResolver<TDbContext, object, TReturn>(_efGraphQLService, ctx => Task.FromResult(query));
        return resolver.ResolveAsync(context);
    }

    /// <inheritdoc/>
    public virtual Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, int defaultPageSize = 100) where TReturn : class
    {
        var resolver = new EfSimpleConnectionResolver<TDbContext, TReturn>(defaultPageSize);

        var efConnectionContext = new ExplicitResolveEfConnectionContext<TDbContext, TSource>(
            _efGraphQLService,
            context,
            resolver.IsBidirectional,
            resolver.DefaultPageSize,
            first,
            last,
            after,
            before);

        var queryResolver = new QueryConnectionResolver<TDbContext, TSource, TReturn>(_efGraphQLService, ctx => Task.FromResult(query), resolver);
        return queryResolver.ResolveAsync(efConnectionContext);
    }

    /// <inheritdoc/>
    public virtual Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource, TReturn>(IResolveFieldContext context, IQueryable<TReturn> query, int? first, int? last, string? after, string? before, IEfConnectionResolver<TReturn> connectionResolver) where TReturn : class
    {
        if (connectionResolver == null)
            throw new ArgumentNullException(nameof(connectionResolver));

        if (connectionResolver is not IEfConnectionResolver<TDbContext, TReturn> typedResolver)
        {
            throw new ArgumentException(
                $"The provided connection resolver must implement IEfConnectionResolver<{typeof(TDbContext).Name}, {typeof(TReturn).Name}>. " +
                $"The resolver type '{connectionResolver.GetType().Name}' is not compatible with the database context type '{typeof(TDbContext).Name}'.",
                nameof(connectionResolver));
        }

        var efConnectionContext = new ExplicitResolveEfConnectionContext<TDbContext, TSource>(
            _efGraphQLService,
            context,
            typedResolver.IsBidirectional,
            typedResolver.DefaultPageSize,
            first,
            last,
            after,
            before);

        var queryResolver = new QueryConnectionResolver<TDbContext, TSource, TReturn>(_efGraphQLService, ctx => Task.FromResult(query), typedResolver);
        return queryResolver.ResolveAsync(efConnectionContext);
    }

    /// <inheritdoc/>
    public virtual async Task<IList<Tuple<TKey, EfSource<TReturn>>>> ExecuteQueryForKeysAsync<TKey, TObject, TReturn>(IResolveFieldContext context, IQueryable<TObject> query, Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys, Expression<Func<TObject, TReturn>> itemSelector) where TObject : class where TReturn : class
    {
        var executer = new QueryExecuter<TDbContext, TReturn>();
        var efContext = _efGraphQLService.BuildResolveEfFieldContext<object>(context);
        var queryWithKeys = executer.GenerateQueryForKeys(efContext, query, keySelector, keys, itemSelector);
        return await _efGraphQLService.QueryToListAsync(queryWithKeys, context.CancellationToken);
    }
}
