// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.DI;
using GraphQL.Linq.GraphApi;
using LinqToDB;

namespace GraphQL.Linq.LinqToDb6;

/// <summary>
/// Helper methods for configuring GraphQL.Linq with LinqToDB.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Adds GraphQL.Linq services for LinqToDB to the GraphQL builder.
    /// </summary>
    public static IGraphQLBuilder AddLinq(this IGraphQLBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        //register the IEfGraphQLService
        builder.Services.TryRegister(typeof(IEfGraphQLService<>), typeof(EfGraphQLServiceWithStringSplit<>), DI.ServiceLifetime.Singleton);

        //register the ILinqGraphExecuter
        builder.Services.TryRegister(typeof(ILinqGraphExecuter<>), typeof(LinqGraphExecuter<>), DI.ServiceLifetime.Singleton);
        builder.Services.TryRegister<ILinqGraphExecuter, LinqGraphExecuter>(DI.ServiceLifetime.Singleton);

        //register the IEfDbKeyNames
        builder.Services.TryRegister(typeof(IEfDbPrimaryKeyNamesProvider<>), typeof(EfDbPrimaryKeyNamesProvider<>), DI.ServiceLifetime.Singleton);

        //register the IEfDbContextTypeProvider
        builder.Services.TryRegister<IEfDbContextTypeProvider, EfDbContextTypeProvider>(DI.ServiceLifetime.Singleton);

        //register the connection type
        builder.Services.TryRegister(typeof(EfConnectionType<>), typeof(EfConnectionType<>), DI.ServiceLifetime.Singleton);
        builder.Services.TryRegister(typeof(EfEdgeType<>), typeof(EfEdgeType<>), DI.ServiceLifetime.Singleton);
        builder.AddGraphTypeMappingProvider<EfConnectionMapper>();

        //enable resolve field context accessor
        builder.AddResolveFieldContextAccessor();

        //enable data loaders
        builder.AddDataLoader();

        return builder;
    }

    /// <summary>
    /// Adds GraphQL.Linq services for LinqToDB to the GraphQL builder with configuration options.
    /// </summary>
    public static IGraphQLBuilder AddLinq<TDbContext>(this IGraphQLBuilder builder, Action<GraphQLLinqOptions<TDbContext>>? setupAction = null)
        where TDbContext : IDataContext
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        //set up options
        var options = new GraphQLLinqOptions<TDbContext>();
        setupAction?.Invoke(options);

        //register the IEfGraphQLService
        if (options.UseStringSplit)
            builder.Services.TryRegister<IEfGraphQLService<TDbContext>, EfGraphQLServiceWithStringSplit<TDbContext>>(DI.ServiceLifetime.Singleton);
        else
            builder.Services.TryRegister<IEfGraphQLService<TDbContext>, EfGraphQLService<TDbContext>>(DI.ServiceLifetime.Singleton);

        //register the IEfDbKeyNames
        builder.Services.TryRegister<IEfDbPrimaryKeyNamesProvider<TDbContext>, EfDbPrimaryKeyNamesProvider<TDbContext>>(DI.ServiceLifetime.Singleton);

        //register the ILinqGraphExecuter
        builder.Services.TryRegister<ILinqGraphExecuter<TDbContext>, LinqGraphExecuter<TDbContext>>(DI.ServiceLifetime.Singleton);
        builder.Services.TryRegister<ILinqGraphExecuter, LinqGraphExecuter<TDbContext>>(DI.ServiceLifetime.Singleton);

        //register the connection type
        builder.Services.TryRegister(typeof(EfConnectionType<>), typeof(EfConnectionType<>), DI.ServiceLifetime.Singleton);
        builder.Services.TryRegister(typeof(EfEdgeType<>), typeof(EfEdgeType<>), DI.ServiceLifetime.Singleton);
        builder.AddGraphTypeMappingProvider<EfConnectionMapper>();

        //enable resolve field context accessor
        builder.AddResolveFieldContextAccessor();

        //enable data loaders
        builder.AddDataLoader();

        return builder;
    }
}
