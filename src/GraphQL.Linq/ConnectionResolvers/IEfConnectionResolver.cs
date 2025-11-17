// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq.ConnectionResolvers;

/// <summary>
/// This interface defines methods for resolving GraphQL Relay Connections.
/// </summary>
public interface IEfConnectionResolver<TDbContext, TObjectType> where TObjectType : class
{
    /// <summary>
    /// Configures the connection GraphQL field to include any optional parameters such as a 'where' clause.
    /// </summary>
    void ConfigureConnectionField(FieldType fieldType);

    /// <summary>
    /// Generates an expression that defines how to retrieve a cursor value for pagination from
    /// an object of type <typeparamref name="TObjectType"/>.  May return <see langword="null"/>
    /// if this is not needed, such as when the cursor is generated based on the index of the item.
    /// </summary>
    Expression<Func<TObjectType, object>>? GetCursorExpression<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context);

    /// <summary>
    /// Serializes the provided cursor value into a string for use in GraphQL Relay Connection objects.
    /// The <paramref name="value"/> may be <see langword="null"/> if <see cref="GetCursorExpression{TSource}(IResolveEfConnectionContext{TDbContext, TSource})"/>
    /// returned <see langword="null"/>.  This is typical when the cursor is generated based on the
    /// index of the item.
    /// </summary>
    string SerializeCursor<T>(IResolveEfConnectionContext<TDbContext, T> context, int index, object? value);

    /// <summary>
    /// Filters an <see cref="IQueryable"/> based on the conditions defined in the provided context, such as
    /// any where conditions, as well as any pagination parameters.
    /// </summary>
    IQueryable<TObjectType> FilterQueryable<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context, IQueryable<TObjectType> query);

    /// <summary>
    /// Filters an <see cref="IEnumerable{T}"/> based on the conditions defined in the provided context, such as
    /// any where conditions, as well as any pagination parameters.  This is within an <see cref="IEnumerable{T}"/>
    /// so it can be used within other expression trees.
    /// </summary>
    Expression<Func<T, IEnumerable<TObjectType>>> FilterExpression<T>(IResolveEfConnectionContext<TDbContext, object> context, Expression<Func<T, IEnumerable<TObjectType>>> expression) where T : class;

    /// <summary>
    /// Counts the number of items in a filtered <see cref="IQueryable">IQueryable</see>.
    /// </summary>
    Task<int> CountQueryable<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context, IQueryable<TObjectType> query);

    /// <summary>
    /// Generates an expression that counts the number of items in a filtered <see cref="IQueryable"/>.
    /// </summary>
    Expression<Func<T, int>> CountExpression<T>(IResolveEfConnectionContext<TDbContext, object> context, Expression<Func<T, IEnumerable<TObjectType>>> expression) where T : class;

    /// <summary>
    /// Builds a connection object based on the records matching the query.
    /// </summary>
    Task<Connection<T>> ResolveConnectionObject<TSource, T>(IResolveEfConnectionContext<TDbContext, TSource> context, IList<(string cursor, T node)>? items, Func<Task<int>>? countFunction);

    /// <summary>
    /// Indicates whether the connection supports bidirectional navigation (i.e., forward and backward).
    /// </summary>
    bool IsBidirectional { get; }

    /// <summary>
    /// Specifies the default page size for connections, used if no specific page size is defined in the query.
    /// </summary>
    int? DefaultPageSize { get; }

    /// <summary>
    /// Validates the arguments provided in the context to ensure they are appropriate for the
    /// connection resolution process.  Throw an <see cref="ExecutionError"/> if the arguments are invalid.
    /// </summary>
    void ValidateArguments<TSource>(IResolveEfConnectionContext<TDbContext, TSource> context);
}
