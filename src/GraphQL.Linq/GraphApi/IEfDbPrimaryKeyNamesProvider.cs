// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq.GraphApi;

/// <summary>
/// Provides primary key names for entities within a specified database context.
/// </summary>
public interface IEfDbPrimaryKeyNamesProvider<TDbContext>
{
    /// <summary>
    /// Retrieves the names of the primary keys for a specified model type within the database context.
    /// </summary>
    IEnumerable<string> GetPrimaryKeyNames<TModel>();

    /// <summary>
    /// Returns an expression that will be translated into a SQL constant to create a dummy column in a result set.
    /// </summary>
    Expression<Func<TSource, object>> GetDummyExpression<TSource>();
}
