// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Resolvers;

namespace GraphQL.Linq;

/// <summary>
/// Contains parameters pertaining to the currently executing <see cref="IFieldResolver"/>, along
/// with helper properties for resolving forward and backward pagination requests on a
/// connection type, retrieving the database context, and retrieving the <see cref="IEfGraphQLService{TDbContext}"/> instance.
/// </summary>
public interface IResolveEfConnectionContext<TDbContext> : IResolveConnectionContext, IResolveEfFieldContext<TDbContext>
{
}

/// <inheritdoc cref="IResolveEfConnectionContext{TDbContext}"/>
public interface IResolveEfConnectionContext<TDbContext, TSource> : IResolveConnectionContext<TSource>, IResolveEfFieldContext<TDbContext, TSource>, IResolveEfConnectionContext<TDbContext>
{
}
