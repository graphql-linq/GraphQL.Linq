// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.ConnectionResolvers;

namespace GraphQL.Linq;

/// <inheritdoc cref="IResolveEfConnectionContext{TDbContext, TSource}"/>
public class ResolveEfConnectionContext<TDbContext, TSource> : ResolveEfFieldContext<TDbContext, TSource>, IResolveEfConnectionContext<TDbContext, TSource>
{
    /// <summary>
    /// Initializes a new instance from a base <see cref="IResolveEfFieldContext{TDbContext, TSource}"/> with specified bidirectionality and default page size.
    /// </summary>
    public ResolveEfConnectionContext(IResolveEfFieldContext<TDbContext, TSource> baseContext, bool biDirectional, int? defaultPageSize)
        : this(baseContext.EfGraphQLService, baseContext, biDirectional, defaultPageSize)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified base field context and other properties.
    /// </summary>
    public ResolveEfConnectionContext(IEfGraphQLService<TDbContext> efGraphQLService, IResolveFieldContext baseContext, bool biDirectional, int? defaultPageSize)
        : base(baseContext, efGraphQLService)
    {
        DefaultPageSize = defaultPageSize;
        IsUnidirectional = !biDirectional;
    }

    /// <inheritdoc cref="IEfConnectionResolver{TDbContext, TObjectType}.DefaultPageSize"/>
    public int? DefaultPageSize { get; }
    /// <inheritdoc cref="IResolveConnectionContext.IsUnidirectional"/>
    public bool IsUnidirectional { get; }

    /// <inheritdoc/>
    public virtual int? First
    {
        get {
            var first = FirstInternal;
            if (!first.HasValue && !LastInternal.HasValue && Before == null) {
                return DefaultPageSize;
            }

            return first;
        }
    }

    /// <inheritdoc cref="First"/>
    protected virtual int? FirstInternal => this.GetArgument<int?>("first");

    /// <inheritdoc/>
    public virtual int? Last
    {
        get {
            var last = LastInternal;
            if (!last.HasValue && !First.HasValue && Before != null) {
                return DefaultPageSize;
            }
            return last;
        }
    }

    /// <inheritdoc cref="Last"/>
    protected virtual int? LastInternal => this.GetArgument<int?>("last");

    /// <inheritdoc/>
    public virtual string? After => this.GetArgument<string?>("after");

    /// <inheritdoc/>
    public virtual string? Before => this.GetArgument<string?>("before");

    /// <inheritdoc/>
    public virtual int? PageSize => First ?? Last ?? DefaultPageSize;
}
