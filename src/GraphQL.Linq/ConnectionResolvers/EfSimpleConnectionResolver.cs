// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Globalization;
using GraphQL.Linq.Expressions;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq.ConnectionResolvers;

/// <summary>
/// A connection resolver implementation that supports index-based pagination.
/// Pagination is based on the index of the item in the result set.
/// Cursors are string representations of the indexes.
/// </summary>
/// <remarks>
/// This is the default connection resolver used by the various EfNavigationConnectionField methods.
/// </remarks>
public class EfSimpleConnectionResolver<TDbContext, TObjectType> : IEfConnectionResolver<TDbContext, TObjectType> where TObjectType : class
{
    /// <summary>
    /// Returns the default page size.
    /// </summary>
    public virtual int? DefaultPageSize { get; }

    /// <summary>
    /// Constructs a new instance of <see cref="EfSimpleConnectionResolver{TDbContext, TObjectType}"/>.
    /// </summary>
    public EfSimpleConnectionResolver(int? defaultPageSize)
    {
        DefaultPageSize = defaultPageSize;
    }

    /// <inheritdoc/>
    public virtual void ConfigureConnectionField(FieldType fieldType)
    {
    }

    /// <inheritdoc/>
    public virtual Expression<Func<T, IEnumerable<TObjectType>>> FilterExpression<T>(IResolveEfConnectionContext<TDbContext, object> context, Expression<Func<T, IEnumerable<TObjectType>>> expression) where T : class
    {
        var skipTake = GetStartIndex(context);
        if (skipTake.skip > 0)
            expression = expression.Skip(skipTake.skip);
        if (skipTake.pageSize.HasValue)
            expression = expression.Take(skipTake.pageSize.Value + 1);
        return expression;
    }

    /// <inheritdoc/>
    public virtual IQueryable<TObjectType> FilterQueryable<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context, IQueryable<TObjectType> query)
    {
        var skipTake = GetStartIndex(context);
        if (skipTake.skip > 0)
            query = query.Skip(skipTake.skip);
        if (skipTake.pageSize.HasValue)
            query = query.Take(skipTake.pageSize.Value + 1);
        return query;
    }

    /// <inheritdoc/>
    public virtual Expression<Func<TObjectType, object>>? GetCursorExpression<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context)
    {
        return null;
    }

    /// <inheritdoc/>
    public virtual async Task<Connection<T>> ResolveConnectionObject<TSource, T>(IResolveEfConnectionContext<TDbContext, TSource> context, IList<(string cursor, T node)>? items, Func<Task<int>>? countFunction)
    {
        if (items == null) {
            return new Connection<T>() { TotalCount = countFunction == null ? -1 : await countFunction() };
        }
        var skipTake = GetStartIndex(context);
        var edges = skipTake.pageSize.HasValue ? items.Take(skipTake.pageSize.Value) : items;
        var ret = new Connection<T>() {
            Edges = edges.Select(x => new Edge<T>() { Cursor = x.cursor, Node = x.node }).ToList(),
            PageInfo = new PageInfo() {
                StartCursor = edges.FirstOrDefault().cursor ?? "",
                EndCursor = edges.LastOrDefault().cursor ?? "",
                HasNextPage = skipTake.pageSize.HasValue && edges.Count() < items.Count,
                HasPreviousPage = skipTake.skip > 0
            }
        };
        if (!ret.PageInfo.HasNextPage && edges.Any())
            ret.TotalCount = skipTake.skip + edges.Count();
        else {
            ret.TotalCount = countFunction == null ? -1 : await countFunction();
            if (ret.TotalCount == 0)
                ret.PageInfo.HasPreviousPage = false;
        }
        return ret;
    }

    /// <inheritdoc/>
    public virtual string SerializeCursor<T>(IResolveEfConnectionContext<TDbContext, T> context, int index, object? value)
    {
        var skipTake = GetStartIndex(context);
        return (skipTake.skip + index).ToString(CultureInfo.InvariantCulture)!;
    }

    /// <summary>
    /// Examines the context to determine the start index and page size from the 'before', 'after',
    /// 'first' and 'last' arguments.
    /// </summary>
    protected virtual (int skip, int? pageSize) GetStartIndex<T>(IResolveEfConnectionContext<TDbContext, T> context)
    {
        if (!context.PageSize.HasValue) {
            //return all records
            if (context.After != null) {
                if (!int.TryParse(context.After, NumberStyles.Integer, CultureInfo.InvariantCulture, out var after))
                    after = -1;
                return (after + 1, null);
            } else if (context.Before != null) {
                if (!int.TryParse(context.Before, NumberStyles.Integer, CultureInfo.InvariantCulture, out var before))
                    return (0, null);
                return (0, before);
            } else {
                return (0, null);
            }
        }
        var pageSize = context.PageSize.Value;
        if (pageSize < 0)
            pageSize = 0;
        if (context.First.HasValue) {
            if (!int.TryParse(context.After, NumberStyles.Integer, CultureInfo.InvariantCulture, out var after))
                after = -1;
            return (after + 1, pageSize);
        } else {
            if (!int.TryParse(context.Before, NumberStyles.Integer, CultureInfo.InvariantCulture, out var before)) {
                throw new ExecutionError("Before is required when using Last");
            }
            return before - pageSize < 0 ? (0, pageSize - (pageSize - before)) : (before - pageSize, pageSize);
        }
    }

    /// <inheritdoc/>
    public virtual Task<int> CountQueryable<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context, IQueryable<TObjectType> query)
    {
        return context.EfGraphQLService.QueryCountAsync(query);
    }

    /// <inheritdoc/>
    public virtual Expression<Func<T, int>> CountExpression<T>(IResolveEfConnectionContext<TDbContext, object> context, Expression<Func<T, IEnumerable<TObjectType>>> expression) where T : class
    {
        return expression.ChainWith(x => x.Count());
    }

    /// <inheritdoc/>
    public virtual bool IsBidirectional => true;

    /// <inheritdoc/>
    public virtual void ValidateArguments<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context)
    {
        if (context.First.HasValue && context.Last.HasValue)
            throw new ExecutionError("Cannot specify both first and last");
        if (context.Before != null && context.After != null)
            throw new ExecutionError("Cannot specify both before and after");
        if (context.First.HasValue && context.Before != null)
            throw new ExecutionError("Cannot specify both first and before");
        if (context.Last.HasValue && context.After != null)
            throw new ExecutionError("Cannot specify both last and after");
        if (context.Last.HasValue && context.Before == null)
            throw new ExecutionError("Must specify before when using last");
        if (context.First.HasValue && context.First.Value < 0)
            throw new ExecutionError("First must not be negative");
        if (context.Last.HasValue && context.Last.Value < 0)
            throw new ExecutionError("Last must not be negative");
    }
}
