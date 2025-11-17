// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq.GraphApi;

/// <summary>
/// Provides the DbContext type for an IQueryable.
/// </summary>
public interface IEfDbContextTypeProvider
{
    /// <summary>
    /// Returns the Type of the DbContext used by the specified IQueryable.
    /// </summary>
    /// <param name="queryable">The IQueryable to get the DbContext type from.</param>
    /// <returns>The Type of the DbContext, or null if the DbContext type cannot be determined.</returns>
    Type? GetDbContextType(IQueryable queryable);
}
