// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.DI;
using GraphQL.Types;

namespace TechMart.Graphs;

public class TechMartSchema : Schema
{
    public TechMartSchema(IServiceProvider provider) : base(provider)
    {
        Query = provider.GetRequiredService<DIObjectGraphType<QueryGraphType>>();
        Mutation = provider.GetRequiredService<DIObjectGraphType<MutationGraphType>>();
    }
}
