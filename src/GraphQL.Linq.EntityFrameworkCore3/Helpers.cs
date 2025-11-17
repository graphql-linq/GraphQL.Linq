// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.DI;
using GraphQL.Linq.GraphApi;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Linq.EntityFrameworkCore3;

/// <summary>
/// Helper methods for configuring GraphQL.Linq with Entity Framework Core.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Adds GraphQL.Linq services for Entity Framework Core to the GraphQL builder.
    /// </summary>
    public static IGraphQLBuilder AddLinq<TDbContext>(this IGraphQLBuilder builder, Action<GraphQLLinqOptions<TDbContext>>? setupAction = null)
        where TDbContext : DbContext
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        //set up options
        var options = new GraphQLLinqOptions<TDbContext>();
        setupAction?.Invoke(options);

        //register the IEfGraphQLService
        builder.Services.TryRegister<IEfDbPrimaryKeyNamesProvider<TDbContext>, EfDbPrimaryKeyNamesProvider<TDbContext>>(
            DI.ServiceLifetime.Singleton);
        builder.Services.TryRegister<IEfGraphQLService<TDbContext>, EfGraphQLService<TDbContext>>(
            DI.ServiceLifetime.Singleton);

        //register the ILinqGraphExecuter
        builder.Services.TryRegister<ILinqGraphExecuter<TDbContext>, LinqGraphExecuter<TDbContext>>(
            DI.ServiceLifetime.Singleton);
        builder.Services.TryRegister<ILinqGraphExecuter, LinqGraphExecuter<TDbContext>>(
            DI.ServiceLifetime.Singleton);

        //register the connection type
        builder.Services.TryRegister(typeof(EfConnectionType<>), typeof(EfConnectionType<>), DI.ServiceLifetime.Singleton);
        builder.Services.TryRegister(typeof(EfEdgeType<>), typeof(EfEdgeType<>), DI.ServiceLifetime.Singleton);
        builder.AddGraphTypeMappingProvider<EfConnectionMapper>();

        //enable resolve field context accessor
        builder.AddResolveFieldContextAccessor();

        return builder;
    }
}
