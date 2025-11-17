// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;

namespace GraphQL.Linq;

/// <summary>
/// An implementation of <see cref="IEfDbContextTypeProvider"/> that always returns a single DbContext type.
/// </summary>
/// <typeparam name="TDbContext">The DbContext type.</typeparam>
public class EfDbSingleContextTypeProvider<TDbContext> : IEfDbContextTypeProvider
{
    /// <inheritdoc/>
    public Type? GetDbContextType(IQueryable queryable) => typeof(TDbContext);
}
