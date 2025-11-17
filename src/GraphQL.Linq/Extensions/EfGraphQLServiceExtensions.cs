// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq;

/// <summary>
/// Provides extension methods for <see cref="IEfGraphQLService{TDbContext}"/>.
/// </summary>
public static class EfGraphQLServiceExtensions
{
    /// <inheritdoc cref="IEfGraphQLService{TDbContext}.BuildResolveEfFieldContext{TSource}(IResolveFieldContext)" />
    public static IResolveEfFieldContext<TDbContext, TSource> BuildResolveEfFieldContext<TDbContext, TSource>(this IEfGraphQLService<TDbContext> efGraphQLService, IResolveFieldContext<TSource> context)
    {
        return efGraphQLService.BuildResolveEfFieldContext<TSource>(context);
    }
}
