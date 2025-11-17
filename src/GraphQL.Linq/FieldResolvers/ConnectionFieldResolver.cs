// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.FieldResolvers;

/// <summary>
/// GraphQL <see cref="IFieldResolver"/> for EF connection fields.
/// </summary>
internal sealed class ConnectionFieldResolver<TDbContext, TSourceType, TObjectType> : IFieldResolver where TSourceType : class where TObjectType : class
{
    private readonly IEfConnectionResolver<TDbContext, TObjectType> _efConnectionResolver;

    public ConnectionFieldResolver(IEfConnectionResolver<TDbContext, TObjectType> efConnectionResolver)
    {
        _efConnectionResolver = efConnectionResolver ?? throw new ArgumentNullException(nameof(efConnectionResolver));
    }

    public async ValueTask<object?> ResolveAsync(IResolveFieldContext context)
    {
        var requestServices = context.RequestServices
            ?? throw new InvalidOperationException("RequestServices is not available in the current context.");
        var efGraphQLService = requestServices.GetRequiredService<IEfGraphQLService<TDbContext>>();
        var connectionContext = new ResolveEfConnectionContext<TDbContext, EfSource<TSourceType>>(
            efGraphQLService,
            context,
            _efConnectionResolver.IsBidirectional,
            _efConnectionResolver.DefaultPageSize);
        var objects = (IDictionary<string, object>)((IDictionary<string, object>)context.Source!)[context.FieldAst.Name.StringValue];
        objects.TryGetValue("items", out var itemsObj);
        objects.TryGetValue("count", out var countObj);
        var items = (List<EfSource<TObjectType>>)itemsObj;
        var items2 = items?.Select((item, index) => {
            item.TryGetValue("__EF_Cursor", out object? obj);
            return (_efConnectionResolver.SerializeCursor(connectionContext, index, obj), (IDictionary<string, object?>)item);
        }).ToList();
        var count = (int?)countObj;
        var ret = await _efConnectionResolver.ResolveConnectionObject(connectionContext, items2, count.HasValue ? () => Task.FromResult(count.Value) : null);
        return ret;
    }
}
