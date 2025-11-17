// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Linq.EntityFrameworkCore6;

/// <inheritdoc cref="IEfGraphQLService{TDbContext}"/>
public class EfGraphQLService<TDbContext> : EfGraphQLServiceBase<TDbContext>
    where TDbContext : DbContext
{
    /// <inheritdoc cref="EfGraphQLServiceBase{TDbContext}.EfGraphQLServiceBase(IEfDbPrimaryKeyNamesProvider{TDbContext})"/>
    public EfGraphQLService(IEfDbPrimaryKeyNamesProvider<TDbContext> efDbKeyNames)
        : base(efDbKeyNames)
    {
    }

    /// <inheritdoc/>
    public override async Task<IList<TReturn>> QueryToListAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken = default)
    {
        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<int> QueryCountAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken = default)
    {
        return query.CountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<TReturn?> QuerySingleOrDefaultAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken = default)
        where TReturn : class
    {
        return await query.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }
}
