// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

#pragma warning disable EF1001 // Internal EF Core API usage.

using GraphQL.Linq.GraphApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace GraphQL.Linq.EntityFrameworkCore10;

/// <summary>
/// Provides the DbContext type for Entity Framework Core 10 IQueryables.
/// </summary>
public sealed class EfDbContextTypeProvider : IEfDbContextTypeProvider
{
    /// <inheritdoc/>
    public Type? GetDbContextType(IQueryable queryable)
    {
        if (queryable == null)
            return null;

        // Check if this is an EF Core query by examining the provider
        var provider = queryable.Provider;

        if (provider is EntityQueryProvider) { // all IAsyncQueryProvider are EntityQueryProvider
            var field = typeof(EntityQueryProvider).GetField("_queryCompiler",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) {
                var value = field.GetValue(provider)! as IQueryCompiler;
                if (value is QueryCompiler queryCompiler) { // all IQueryCompiler are QueryCompiler
                    var field2 = typeof(QueryCompiler).GetField("_contextType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field2 != null) {
                        var value2 = field2.GetValue(queryCompiler) as Type;
                        return value2;
                    }
                }
            }
        }

        return null;
    }
}
