// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Types;

namespace GraphQL.Linq;

internal static class GraphTypeExtensions
{
    /// <summary>
    /// Finds the appropriate graph type for the EF type, considering nullability.
    /// </summary>
    internal static Type FindGraphForEfType<TReturn>(this Type? graphType, bool nullable) where TReturn : class
    {
        if (graphType == null) {
            graphType = typeof(GraphQLClrOutputTypeReference<EfSource<TReturn>>);
            if (!nullable)
                graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
        }

        return graphType;
    }

    /// <summary>
    /// Finds the appropriate graph type for a list of EF types, considering nullability.
    /// </summary>
    internal static Type FindGraphForEfListType<TReturn>(this Type? graphType) where TReturn : class
    {
        if (graphType == null)
            graphType = typeof(GraphQLClrOutputTypeReference<EfSource<TReturn>>);

        //if it's NOT a NonNull<List<something>>
        if (!(typeof(NonNullGraphType).IsAssignableFrom(graphType) && typeof(ListGraphType).IsAssignableFrom(graphType.GenericTypeArguments[0]))) {
            //AND if it's NOT a List<something>
            if (!typeof(ListGraphType).IsAssignableFrom(graphType)) {
                //then we need to make it a list
                //make it a NonNull<something> if it isn't already
                if (!typeof(NonNullGraphType).IsAssignableFrom(graphType)) {
                    graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
                }
                //now make it a NonNull<List<NonNull<something>>>
                graphType = typeof(NonNullGraphType<>).MakeGenericType(typeof(ListGraphType<>).MakeGenericType(graphType));
            } else {
                //it is a List<something> but not a NonNull<List<something>>
                //just make it a NonNull<List<something>>
                graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);

                //note: we didn't inspect the 'something' to see if it's also a non-null graph type
            }
        }

        return graphType;
    }
}
