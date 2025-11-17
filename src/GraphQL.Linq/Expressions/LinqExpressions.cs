// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq.Expressions;

/// <summary>
/// Provides extension methods for building and manipulating LINQ expressions.
/// </summary>
public static class LinqExpressions
{
    private static readonly MethodInfo _where = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Where) && x.GetParameters()[1].ParameterType.GenericTypeArguments.Length == 2).Single();
    private static readonly ConcurrentDictionary<Type, MethodInfo> _whereTyped = new();
    private static readonly MethodInfo _whereQueryable;
    private static readonly ConcurrentDictionary<Type, MethodInfo> _whereQueryableTyped = new();

    static LinqExpressions()
    {
        Expression<Func<IQueryable<string>>> func = () => Queryable.Where((IQueryable<string>)null!, s => true);
        _whereQueryable = ((MethodCallExpression)func.Body).Method.GetGenericMethodDefinition();
    }

    /// <summary>
    /// Creates a new expression that applies a Where clause to the source query expression.
    /// </summary>
    public static Expression<Func<TSource, IEnumerable<TReturn>>> Where<TSource, TReturn>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery, Expression<Func<TReturn, bool>> whereQuery)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));
        if (whereQuery == null)
            throw new ArgumentNullException(nameof(whereQuery));

        //get a MethodInfo reference to Enumerable.Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        var whereTyped = _whereTyped.GetOrAdd(typeof(TReturn), valueFactory: t => _where.MakeGenericMethod(t));

        //create the new expression body
        var whereExp = Expression.Call(whereTyped, sourceQuery.Body, whereQuery);
        //whereExp now calls Enumerable.Where, passing in the result of sourceQuery, and the where expression as the parameter

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(whereExp, sourceQuery.Parameters);

        return lambda;
    }

    private static readonly MethodInfo _take = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Take) && x.GetParameters()[1].ParameterType == typeof(int)).Single();
    private static readonly ConcurrentDictionary<Type, MethodInfo> _takeTyped = new();
    /// <summary>
    /// Creates a new expression that applies a Take clause to the source query expression, limiting the number of results.
    /// </summary>
    public static Expression<Func<TSource, IEnumerable<TReturn>>> Take<TSource, TReturn>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery, int count)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));

        //get a MethodInfo reference to Enumerable.Take<TSource>(this IEnumerable<TSource> source, int count)
        var takeTyped = _takeTyped.GetOrAdd(typeof(TReturn), valueFactory: t => _take.MakeGenericMethod(t));

        //create the new expression body
        var takeExp = Expression.Call(takeTyped, sourceQuery.Body, Expression.Constant(count, typeof(int)));
        //takeExp now calls Enumerable.Take, passing in the result of sourceQuery, and 'count' as the parameter

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(takeExp, sourceQuery.Parameters);

        return lambda;
    }

    private static readonly MethodInfo _skip = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Skip) && x.GetParameters()[1].ParameterType == typeof(int)).Single();
    private static readonly ConcurrentDictionary<Type, MethodInfo> _skipTyped = new();
    /// <summary>
    /// Creates a new expression that applies a Skip clause to the source query expression, skipping the specified number of results.
    /// </summary>
    public static Expression<Func<TSource, IEnumerable<TReturn>>> Skip<TSource, TReturn>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery, int count)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));

        //get a MethodInfo reference to Enumerable.Skip<TSource>(this IEnumerable<TSource> source, int count)
        var skipTyped = _skipTyped.GetOrAdd(typeof(TReturn), valueFactory: t => _skip.MakeGenericMethod(t));

        //create the new expression body
        var skipExp = Expression.Call(skipTyped, sourceQuery.Body, Expression.Constant(count, typeof(int)));
        //skipExp now calls Enumerable.Skip, passing in the result of sourceQuery, and 'count' as the parameter

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(skipExp, sourceQuery.Parameters);

        return lambda;
    }

    private static readonly MethodInfo _count = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Count) && x.GetParameters().Length == 1).Single();
    private static readonly ConcurrentDictionary<Type, MethodInfo> _countTyped = new();
    /// <summary>
    /// Creates a new expression that applies a Count operation to the source query expression, returning the number of elements.
    /// </summary>
    public static Expression<Func<TSource, int>> Count<TSource, TReturn>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));

        //get a MethodInfo reference to Enumerable.Skip<TSource>(this IEnumerable<TSource> source, int count)
        var countTyped = _countTyped.GetOrAdd(typeof(TReturn), valueFactory: t => _count.MakeGenericMethod(t));

        //create the new expression body
        var countExp = Expression.Call(countTyped, sourceQuery.Body);
        //countExp now calls Enumerable.Count, passing in the result of sourceQuery

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, int>>(countExp, sourceQuery.Parameters);

        return lambda;
    }

    private static readonly MethodInfo _orderBy = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.OrderBy) && x.GetParameters().Length == 2).Single();
    private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _orderByTyped = new();
    /// <summary>
    /// Creates a new expression that applies an OrderBy clause to the source query expression, sorting elements in ascending order.
    /// </summary>
    public static Expression<Func<TSource, IEnumerable<TReturn>>> OrderBy<TSource, TReturn, TKey>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery, Expression<Func<TReturn, TKey>> orderByQuery)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));
        if (orderByQuery == null)
            throw new ArgumentNullException(nameof(orderByQuery));

        //get a MethodInfo reference to Enumerable.OrderBy<TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        var orderByTyped = _orderByTyped.GetOrAdd((typeof(TReturn), typeof(TKey)), valueFactory: ((Type tReturn, Type tKey) types) => _orderBy.MakeGenericMethod(types.tReturn, types.tKey));

        //create the new expression body
        var orderByExp = Expression.Call(orderByTyped, sourceQuery.Body, orderByQuery);
        //orderByExp now calls Enumerable.OrderBy, passing in the result of sourceQuery, and the orderBy expression as the parameter

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(orderByExp, sourceQuery.Parameters);

        return lambda;
    }

    private static readonly MethodInfo _orderByDescending = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.OrderByDescending) && x.GetParameters().Length == 2).Single();
    private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _orderByDescendingTyped = new();
    /// <summary>
    /// Creates a new expression that applies an OrderByDescending clause to the source query expression, sorting elements in descending order.
    /// </summary>
    public static Expression<Func<TSource, IEnumerable<TReturn>>> OrderByDescending<TSource, TReturn, TKey>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery, Expression<Func<TReturn, TKey>> orderByQuery)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));
        if (orderByQuery == null)
            throw new ArgumentNullException(nameof(orderByQuery));

        //get a MethodInfo reference to Enumerable.OrderBy<TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        var orderByTyped = _orderByDescendingTyped.GetOrAdd((typeof(TReturn), typeof(TKey)), valueFactory: ((Type tReturn, Type tKey) types) => _orderByDescending.MakeGenericMethod(types.tReturn, types.tKey));

        //create the new expression body
        var orderByExp = Expression.Call(orderByTyped, sourceQuery.Body, orderByQuery);
        //orderByExp now calls Enumerable.OrderBy, passing in the result of sourceQuery, and the orderBy expression as the parameter

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(orderByExp, sourceQuery.Parameters);

        return lambda;
    }

    private static readonly MethodInfo _thenBy = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.ThenBy) && x.GetParameters().Length == 2).Single();
    private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _thenByTyped = new();
    /// <summary>
    /// Creates a new expression that applies a ThenBy clause to the source query expression, performing a secondary sort in ascending order.
    /// </summary>
    public static Expression<Func<TSource, IEnumerable<TReturn>>> ThenBy<TSource, TReturn, TKey>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery, Expression<Func<TReturn, TKey>> thenByQuery)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));
        if (thenByQuery == null)
            throw new ArgumentNullException(nameof(thenByQuery));

        //get a MethodInfo reference to Enumerable.ThenBy<TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        var thenByTyped = _thenByTyped.GetOrAdd((typeof(TReturn), typeof(TKey)), valueFactory: ((Type tReturn, Type tKey) types) => _thenBy.MakeGenericMethod(types.tReturn, types.tKey));

        //create the new expression body
        var thenByExp = Expression.Call(thenByTyped, sourceQuery.Body, thenByQuery);
        //thenByExp now calls Enumerable.ThenBy, passing in the result of sourceQuery, and the thenBy expression as the parameter

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(thenByExp, sourceQuery.Parameters);

        return lambda;
    }

    private static readonly MethodInfo _thenByDescending = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.ThenByDescending) && x.GetParameters().Length == 2).Single();
    private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _thenByDescendingTyped = new();
    /// <summary>
    /// Creates a new expression that applies a ThenByDescending clause to the source query expression, performing a secondary sort in descending order.
    /// </summary>
    public static Expression<Func<TSource, IEnumerable<TReturn>>> ThenByDescending<TSource, TReturn, TKey>(this Expression<Func<TSource, IEnumerable<TReturn>>> sourceQuery, Expression<Func<TReturn, TKey>> thenByQuery)
    {
        if (sourceQuery == null)
            throw new ArgumentNullException(nameof(sourceQuery));
        if (thenByQuery == null)
            throw new ArgumentNullException(nameof(thenByQuery));

        //get a MethodInfo reference to Enumerable.ThenBy<TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        var thenByTyped = _thenByDescendingTyped.GetOrAdd((typeof(TReturn), typeof(TKey)), valueFactory: ((Type tReturn, Type tKey) types) => _thenByDescending.MakeGenericMethod(types.tReturn, types.tKey));

        //create the new expression body
        var thenByExp = Expression.Call(thenByTyped, sourceQuery.Body, thenByQuery);
        //thenByExp now calls Enumerable.ThenBy, passing in the result of sourceQuery, and the thenBy expression as the parameter

        //reattach the parameter and create a lambda
        var lambda = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(thenByExp, sourceQuery.Parameters);

        return lambda;
    }

    /// <summary>
    /// Creates a new expression that combines two boolean predicate expressions using logical OR.
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> predicate1, Expression<Func<T, bool>> predicate2)
    {
        if (predicate1 == null)
            throw new ArgumentNullException(nameof(predicate1));
        if (predicate2 == null)
            throw new ArgumentNullException(nameof(predicate2));

        var ret = Expression.OrElse(predicate1.Body, predicate2.Body.Replace(predicate2.Parameters[0], predicate1.Parameters[0]));
        return Expression.Lambda<Func<T, bool>>(ret, predicate1.Parameters[0]);
    }

    /// <summary>
    /// Creates a new expression that combines multiple boolean predicate expressions using logical OR.
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(this IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        if (predicates == null)
            throw new ArgumentNullException(nameof(predicates));

        return predicates.Aggregate((aggregatePredicate, nextPredicate) => aggregatePredicate.Or(nextPredicate));
    }

    /// <summary>
    /// Creates a new expression that combines two boolean predicate expressions using logical AND.
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> predicate1, Expression<Func<T, bool>> predicate2)
    {
        if (predicate1 == null)
            throw new ArgumentNullException(nameof(predicate1));
        if (predicate2 == null)
            throw new ArgumentNullException(nameof(predicate2));

        var ret = Expression.AndAlso(predicate1.Body, predicate2.Body.Replace(predicate2.Parameters[0], predicate1.Parameters[0]));
        return Expression.Lambda<Func<T, bool>>(ret, predicate1.Parameters[0]);
    }

    /// <summary>
    /// Creates a new expression that combines multiple boolean predicate expressions using logical AND.
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(this IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        if (predicates == null)
            throw new ArgumentNullException(nameof(predicates));

        return predicates.Aggregate((aggregatePredicate, nextPredicate) => aggregatePredicate.And(nextPredicate));
    }

    /// <summary>
    /// Creates a new expression that negates a boolean predicate expression using logical NOT.
    /// </summary>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var ret = Expression.Not(predicate.Body);
        return Expression.Lambda<Func<T, bool>>(ret, predicate.Parameters[0]);
    }

    /// <summary>
    /// Determines if a lambda expression ends with a FirstOrDefault call and removes or converts it to a Where clause as applicable.
    /// </summary>
    public static bool EndsWithFirstOrDefault(this LambdaExpression expression, out LambdaExpression? ret)
    {
        var lastMethodCall = expression.Body;
        if (lastMethodCall is MethodCallExpression methodCall) {
            if (methodCall.Method.DeclaringType == typeof(Enumerable) && methodCall.Method.Name == nameof(Enumerable.FirstOrDefault)) {
                if (methodCall.Arguments.Count == 1) {
                    // remove the FirstOrDefault call
                    // -- e.g. subFieldExpression = (product) => db.Categories.Where(x => x.Id == product.CategoryId).FirstOrDefault
                    ret = Expression.Lambda(methodCall.Arguments[0], expression.Parameters);
                    // -- e.g. subFieldExpression = (product) => db.Categories.Where(x => x.Id == product.CategoryId)
                    return true;
                } else if (methodCall.Arguments.Count == 2 && methodCall.Arguments[1].Type != methodCall.Method.GetGenericArguments()[0]) {
                    // replace the FirstOrDefault call with Where
                    // -- e.g. subFieldExpression = (product) => db.Categories.FirstOrDefault(x => x.Id == product.CategoryId)
                    var whereMethodCall = _whereTyped.GetOrAdd(
                        methodCall.Method.GetGenericArguments()[0],
                        t => _where.MakeGenericMethod(t));
                    ret = Expression.Lambda(
                        Expression.Call(
                            whereMethodCall,
                            methodCall.Arguments[0],
                            methodCall.Arguments[1]),
                        expression.Parameters);
                    // -- e.g. subFieldExpression = (product) => db.Categories.Where(x => x.Id == product.CategoryId)
                    return true;
                }
            }

            if (methodCall.Method.DeclaringType == typeof(Queryable) && methodCall.Method.Name == nameof(Queryable.FirstOrDefault)) {
                if (methodCall.Arguments.Count == 1) {
                    // remove the FirstOrDefault call
                    // -- e.g. subFieldExpression = (product) => db.Categories.Where(x => x.Id == product.CategoryId).FirstOrDefault
                    ret = Expression.Lambda(methodCall.Arguments[0], expression.Parameters);
                    // -- e.g. subFieldExpression = (product) => db.Categories.Where(x => x.Id == product.CategoryId)
                    return true;
                } else if (methodCall.Arguments.Count == 2 && methodCall.Arguments[1].Type != methodCall.Method.GetGenericArguments()[0]) {
                    // replace the FirstOrDefault call with Where
                    // -- e.g. subFieldExpression = (product) => db.Categories.FirstOrDefault(x => x.Id == product.CategoryId)
                    var whereMethodCall = _whereQueryableTyped.GetOrAdd(
                        methodCall.Method.GetGenericArguments()[0],
                        t => _whereQueryable.MakeGenericMethod(t));
                    ret = Expression.Lambda(
                        Expression.Call(
                            whereMethodCall,
                            methodCall.Arguments[0],
                            methodCall.Arguments[1]),
                        expression.Parameters);
                    // -- e.g. subFieldExpression = (product) => db.Categories.Where(x => x.Id == product.CategoryId)
                    return true;
                }
            }
        }

        ret = null;
        return false;
    }
}
