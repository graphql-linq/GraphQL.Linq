// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Linq.GraphApi;
using LinqToDB;

namespace GraphQL.Linq.LinqToDb6;

/// <inheritdoc cref="EfGraphQLService{TDbContext}"/>
/// <remarks>
/// <see cref="CreateWhereInExpression{TKey, TObject}(Func{TDbContext}, Expression{Func{TObject, TKey}}, IEnumerable{TKey})">CreateWhereInExpression</see> utilizes STRING_SPLIT
/// to create an expression that checks if a key is in a list of keys.  The key type must be int, long, short, byte, or Guid.
/// </remarks>
public class EfGraphQLServiceWithStringSplit<TDbContext> : EfGraphQLService<TDbContext>
    where TDbContext : IDataContext
{
    /// <inheritdoc cref="EfGraphQLServiceBase{TDbContext}.EfGraphQLServiceBase(IEfDbPrimaryKeyNamesProvider{TDbContext})"/>
    public EfGraphQLServiceWithStringSplit(IEfDbPrimaryKeyNamesProvider<TDbContext> efDbKeyNames)
        : base(efDbKeyNames, 1)
    {
    }

    static EfGraphQLServiceWithStringSplit()
    {
        Expression<Func<IQueryable<object>, object, bool>> expr = (q, v) => q.Contains(v);
        _queryableContainsMethod = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();
    }

    private static readonly MethodInfo _queryableContainsMethod;
    private static readonly ConcurrentDictionary<Type, MethodInfo> _typedContainsMethods = new ConcurrentDictionary<Type, MethodInfo>();
    /// <inheritdoc/>
    public override Expression<Func<TObject, bool>> CreateWhereInExpression<TKey, TObject>(Func<TDbContext> dbContextFactory, Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys)
    {
        if (typeof(TKey) != typeof(int) &&
            typeof(TKey) != typeof(long) &&
            typeof(TKey) != typeof(short) &&
            typeof(TKey) != typeof(byte) &&
            typeof(TKey) != typeof(Guid)) {
            return base.CreateWhereInExpression(dbContextFactory, keySelector, keys);
        }

        Expression<Func<TObject, bool>> ReturnFalse() => _ => false;

        Expression<Func<TObject, bool>> ReturnParameterize()
        {
            return Expression.Lambda<Func<TObject, bool>>(
                keys
                    .Select(key => Expression.Equal(keySelector.Body, Expression.Constant(key)))
                    .Aggregate((aggregate, next) => Expression.OrElse(aggregate, next)),
                keySelector.Parameters);
        }

        if (MaxParameterizeContainsVariables >= 0) {
            int? count = null;
            if (keys is ICollection<TKey> keyCollection)
                count = keyCollection.Count;
            else if (keys is System.Collections.ICollection collection)
                count = collection.Count;
            else if (keys is IReadOnlyCollection<TKey> readOnlyCollection)
                count = readOnlyCollection.Count;

            if (count.HasValue) {
                if (count.Value == 0) {
                    return ReturnFalse();
                } else if (count.Value <= MaxParameterizeContainsVariables) {
                    return ReturnParameterize();
                }
            }
        }

        var values = string.Join(",", keys.Select(x => x?.ToString() ?? ""));
        if (values.Length == 0)
            return ReturnFalse();

        var tableValues = dbContextFactory().FromSql<StringSplitTable<TKey>>($"STRING_SPLIT({values},',')").Select(x => x.value);

        var containsMethodTyped = _typedContainsMethods.GetOrAdd(typeof(TKey), t => _queryableContainsMethod.MakeGenericMethod(t));
        //e.g. var table = dbContext.FromSql<StringSplitTable>($"STRING_SPLIT({values},',')");
        //     var tableValues = table.Select(x => (int)x.value);
        //     return (product) => Enumerable.Contains(tableValues, product.Id)
        var ret = Expression.Lambda<Func<TObject, bool>>(
            Expression.Call(
                containsMethodTyped,
                Expression.Constant(tableValues),
                keySelector.Body),
            keySelector.Parameters);

        return ret;
    }

    private sealed class StringSplitTable<T>
    {
        public T value { get; set; } = default!;
    }
}
