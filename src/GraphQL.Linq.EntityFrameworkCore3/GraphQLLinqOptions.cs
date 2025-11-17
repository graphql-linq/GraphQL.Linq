// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.EntityFrameworkCore3;

/// <summary>
/// Configuration options for GraphQL.Linq integration with Entity Framework Core.
/// </summary>
public class GraphQLLinqOptions<TDbContext>
    where TDbContext : DbContext
{
}
