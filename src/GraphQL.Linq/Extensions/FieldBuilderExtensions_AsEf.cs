// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;

namespace GraphQL.Linq;

public static partial class FieldBuilderExtensions
{
    internal static EfFieldBuilder<TDbContext, TSourceType, TReturnType> AsEfFieldBuilder<TDbContext, TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> fieldBuilder)
    {
        return new EfFieldBuilder<TDbContext, TSourceType, TReturnType>(fieldBuilder);
    }
}
