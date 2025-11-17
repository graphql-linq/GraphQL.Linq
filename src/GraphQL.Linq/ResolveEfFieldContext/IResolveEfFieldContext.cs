// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq;

/// <summary>
/// Contains parameters pertaining to the currently executing field resolver, including access to the database context and the <see cref="IEfGraphQLService{TDbContext}"/> instance.
/// </summary>
public interface IResolveEfFieldContext<TDbContext> : IResolveFieldContext
{
    /// <summary>
    /// Returns the database context.
    /// </summary>
    TDbContext DbContext { get; }

    /// <summary>
    /// Returns the <see cref="IEfGraphQLService{TDbContext}"/> instance.
    /// </summary>
    IEfGraphQLService<TDbContext> EfGraphQLService { get; }
}

/// <inheritdoc cref="IResolveEfFieldContext{TDbContext}"/>
public interface IResolveEfFieldContext<TDbContext, TSource> : IResolveFieldContext<TSource>, IResolveEfFieldContext<TDbContext>
{
}
