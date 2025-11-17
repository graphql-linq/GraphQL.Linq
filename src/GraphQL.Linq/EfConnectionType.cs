// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay;

namespace GraphQL.Linq;

/// <inheritdoc cref="ConnectionType{TNodeType, TEdgeType}" />
public class EfConnectionType<T> : ConnectionType<GraphQLClrOutputTypeReference<EfSource<T>>, EfEdgeType<T>>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EfConnectionType{T}"/> class.
    /// </summary>
    public EfConnectionType()
    {
        var name = typeof(T).GraphQLName();
        Name = name + "Connection";
        Description = $"A connection from an object to a list of objects of type '{name}'.";

        Fields.Find("totalCount")!.Type = typeof(NonNullGraphType<IntGraphType>);
        Fields.Find("edges")!.Type = typeof(NonNullGraphType<ListGraphType<NonNullGraphType<EfEdgeType<T>>>>);
        Fields.Find("items")!.Type = typeof(NonNullGraphType<ListGraphType<NonNullGraphType<GraphQLClrOutputTypeReference<EfSource<T>>>>>);
    }
}
