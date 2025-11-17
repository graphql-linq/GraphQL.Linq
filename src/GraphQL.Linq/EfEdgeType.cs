// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay;

namespace GraphQL.Linq;

/// <inheritdoc cref="EdgeType{TNodeType}" />
public class EfEdgeType<T> : EdgeType<GraphQLClrOutputTypeReference<EfSource<T>>>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EfEdgeType{T}"/> class.
    /// </summary>
    public EfEdgeType()
    {
        var name = typeof(T).GraphQLName();
        Name = name + "Edge";
        Description = $"An edge in a connection from an object to another object of type '{name}'.";
        Fields.Find("node")!.Type = typeof(NonNullGraphType<GraphQLClrOutputTypeReference<EfSource<T>>>);
    }
}
