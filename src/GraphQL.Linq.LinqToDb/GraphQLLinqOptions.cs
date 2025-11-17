// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using LinqToDB;

namespace GraphQL.Linq.LinqToDb;

/// <summary>
/// Configuration options for GraphQL.Linq integration with LinqToDB.
/// </summary>
public class GraphQLLinqOptions<TDbContext>
    where TDbContext : IDataContext
{
    /// <summary>
    /// Gets or sets whether to use SQL Server string splitting for query optimization.
    /// </summary>
    public bool UseStringSplit { get; set; }
}
