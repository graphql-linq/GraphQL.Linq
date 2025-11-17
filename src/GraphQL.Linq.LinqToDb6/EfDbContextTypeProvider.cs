// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using LinqToDB;
using LinqToDB.Internal.Linq;

namespace GraphQL.Linq.LinqToDb6;

/// <summary>
/// Provides the DataConnection type for LinqToDb IQueryables.
/// </summary>
public sealed class EfDbContextTypeProvider : IEfDbContextTypeProvider
{
    /// <inheritdoc/>
    public Type? GetDbContextType(IQueryable queryable)
    {
        if (queryable == null)
            return null;

        // This should work for most any case
        if (queryable is IExpressionQuery expressionQuery)
            return expressionQuery.DataContext?.GetType();

        // Alternative approach, if the queryable returns a class type
        if (queryable is IQueryable<object> classQueryable)
            return Internals.GetDataContext(classQueryable)?.GetType();

        // Otherwise, try to get the DataContext through reflection
        var queryableType = queryable.GetType();

        // Look for DataContext property on the query object
        var dataContextProperty = queryableType.GetProperty("DataContext",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (dataContextProperty != null) {
            var dataContext = dataContextProperty.GetValue(queryable);
            if (dataContext is IDataContext) {
                return dataContext.GetType();
            }
        }

        return null;
    }
}
