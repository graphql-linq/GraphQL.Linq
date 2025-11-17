// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq;

/// <summary>
/// Maps <see cref="Connection{TNode}">Connection&lt;EfSource&lt;T&gt;&gt;</see> to <see cref="EfConnectionType{T}"/>.
/// </summary>
public sealed class EfConnectionMapper : IGraphTypeMappingProvider
{
    /// <inheritdoc/>
    public Type? GetGraphTypeFromClrType(Type clrType, bool isInputType, Type? preferredGraphType)
    {
        if (isInputType)
            return preferredGraphType;

        // check if clrType is Connection<EfSource<T>> and if so map to EfConnectionType<T>,
        // overriding prior preferences
        if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Connection<>)) {
            var innerType = clrType.GetGenericArguments()[0];
            if (innerType.IsGenericType && innerType.GetGenericTypeDefinition() == typeof(EfSource<>)) {
                var innerInnerType = innerType.GetGenericArguments()[0];
                return typeof(EfConnectionType<>).MakeGenericType(innerInnerType);
            }
        }

        // no change
        return preferredGraphType;
    }
}
