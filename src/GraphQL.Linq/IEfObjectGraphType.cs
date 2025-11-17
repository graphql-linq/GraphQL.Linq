// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Types;

namespace GraphQL.Linq;

/// <summary>
/// Interface for GraphQL object types that integrate with LINQ data sources.
/// </summary>
public interface IEfObjectGraphType<TDbContext, TSource> : IComplexGraphType where TSource : class
{
}
