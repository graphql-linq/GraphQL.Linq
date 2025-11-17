// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Resolvers;

namespace GraphQL.Linq.FieldResolvers;

/// <summary>
/// GraphQL <see cref="IFieldResolver"/> for EF non-connection fields.
/// </summary>
internal class FieldResolver : IFieldResolver, IRequiresResolveFieldContextAccessor
{
    /// <inheritdoc cref="FieldResolver"/>
    public static readonly FieldResolver Instance = new FieldResolver();

    private FieldResolver()
    {
    }

    public bool RequiresResolveFieldContextAccessor => false;

    public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
    {
        return new ValueTask<object?>(((IDictionary<string, object?>)(context ?? throw new ArgumentNullException(nameof(context))).Source!)[context.FieldAst.Name.StringValue]);
    }
}
