// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.ConnectionResolvers;

namespace GraphQL.Linq;

/// <summary>
/// A connection context that uses explicitly provided pagination parameters instead of reading from GraphQL arguments.
/// </summary>
public class ExplicitResolveEfConnectionContext<TDbContext, TSource> : ResolveEfConnectionContext<TDbContext, TSource>
{
    private readonly int? _first;
    private readonly int? _last;
    private readonly string? _after;
    private readonly string? _before;

    /// <summary>
    /// Initializes a new instance with explicit pagination parameters.
    /// </summary>
    /// <param name="efGraphQLService">The EF GraphQL service.</param>
    /// <param name="baseContext">The base field context.</param>
    /// <param name="biDirectional">Whether the connection supports bidirectional navigation.</param>
    /// <param name="defaultPageSize">The default page size.</param>
    /// <param name="first">The number of items to return from the start.</param>
    /// <param name="last">The number of items to return from the end.</param>
    /// <param name="after">The cursor after which to start returning items.</param>
    /// <param name="before">The cursor before which to start returning items.</param>
    public ExplicitResolveEfConnectionContext(
        IEfGraphQLService<TDbContext> efGraphQLService,
        IResolveFieldContext baseContext,
        bool biDirectional,
        int? defaultPageSize,
        int? first,
        int? last,
        string? after,
        string? before)
        : base(efGraphQLService, baseContext, biDirectional, defaultPageSize)
    {
        _first = first;
        _last = last;
        _after = after;
        _before = before;
    }

    /// <inheritdoc/>
    protected override int? FirstInternal => _first;

    /// <inheritdoc/>
    protected override int? LastInternal => _last;

    /// <inheritdoc/>
    public override string? After => _after;

    /// <inheritdoc/>
    public override string? Before => _before;
}
