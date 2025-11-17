// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;

namespace GraphQL.Linq.Expressions;

/// <summary>
/// Provides functionality to build lambda expressions for handling various data structures.
/// </summary>
public static class LambdaBuilder
{
    /// <summary>
    /// Caches the method information for adding typed values.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, MethodInfo> _addTyped = new();
    private static readonly Type[] _stringObjectTypeArray = [typeof(string), typeof(object)];

    /// <summary>
    /// Builds a lambda expression to map an object to an <see cref="EfSource{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="fields">
    /// A read-only collection of key-value pairs where the key is a field name and the value is an expression mapping 
    /// the source object to the corresponding field value.
    /// </param>
    /// <returns>A lambda expression mapping the object to an <see cref="EfSource{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fields"/> parameter is null.</exception>
    public static Expression<Func<T, EfSource<T>>> Build<T>(IReadOnlyCollection<KeyValuePair<string, Expression<Func<T, object>>>> fields) where T : class
    {
        if (fields == null)
            throw new ArgumentNullException(nameof(fields));

        var newParameter = Expression.Parameter(typeof(T));

        if (fields.Count == 0) {
            // When there are no fields to return, we return a precomputed object; can't use Expression.Constant either.
            var constant = new EfSource<T>();
            return x => constant;
        }

        var newDictionary = Expression.New(typeof(EfSource<T>));
        var addMethod = _addTyped.GetOrAdd(typeof(EfSource<T>), valueFactory: static t => t.GetMethod(nameof(EfSource<object>.Add), _stringObjectTypeArray));

        // Replaces the parameter of an original expression with a new one.
        Expression ReplaceParameter(Expression<Func<T, object>> originalExpression)
        {
            var body = originalExpression.Body;
            var originalParameter = originalExpression.Parameters.Single();
            if (!originalParameter.Type.Equals(newParameter.Type))
                throw new InvalidOperationException("Parameter types must match"); // should not be possible to hit this line of code
            var replaced = ParameterReplacer.Replace(body, originalParameter, newParameter);
            return replaced;
        }

        var memberInitFields = fields.Select(x => Expression.ElementInit(
            addMethod,
            Expression.Constant(x.Key, typeof(string)),
            ReplaceParameter(x.Value)));

        var listInit = Expression.ListInit(newDictionary, memberInitFields);

        var lambda = Expression.Lambda<Func<T, EfSource<T>>>(listInit, newParameter);

        return lambda;
    }

    // private static readonly MethodInfo _toList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));
    // private static readonly ConcurrentDictionary<Type, MethodInfo> _toListTyped = new ConcurrentDictionary<Type, MethodInfo>();
    private static readonly MethodInfo _select = typeof(Enumerable).GetMethods()
        .Where(x => x.Name == nameof(Enumerable.Select) && x.GetParameters()[1].ParameterType.GenericTypeArguments.Length == 2).Single();
    private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _selectTyped = new ConcurrentDictionary<(Type, Type), MethodInfo>();

    /// <summary>
    /// Builds a lambda expression for navigating and transforming a collection to an object.
    /// </summary>
    /// <typeparam name="TSource">The type of the root object containing the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items in the collection to be transformed.</typeparam>
    /// <param name="mediasSelector">
    /// An expression to select the collection from the root object.
    /// </param>
    /// <param name="mediasExp">
    /// An expression to map the collection items to <see cref="EfSource{TItem}"/>.
    /// </param>
    /// <returns>A lambda expression mapping the root object to the transformed collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either the <paramref name="mediasSelector"/> or <paramref name="mediasExp"/> is null.
    /// </exception>
    public static Expression<Func<TSource, object>> BuildNavigation<TSource, TItem>(Expression<Func<TSource, IEnumerable<TItem>>> mediasSelector, Expression<Func<TItem, EfSource<TItem>>> mediasExp) where TItem : class
    {
        if (mediasSelector == null)
            throw new ArgumentNullException(nameof(mediasSelector));
        if (mediasExp == null)
            throw new ArgumentNullException(nameof(mediasExp));

        var selectTyped = _selectTyped.GetOrAdd((typeof(TItem), typeof(EfSource<TItem>)), valueFactory: ((Type tU, Type tEfSourceU) types) => _select.MakeGenericMethod(types.tU, types.tEfSourceU));

        var mediasParameter = Expression.Parameter(typeof(IEnumerable<TItem>));
        var selectExp = Expression.Call(selectTyped, mediasParameter, mediasExp);

        // The following lines are commented out since they involve ToList, which isn't required:
        // var toListTyped = _toListTyped.GetOrAdd(typeof(EfSource<TItem>), valueFactory: t => _toList.MakeGenericMethod(t));
        // var toListExp = Expression.Call(toListTyped, selectExp);

        var expression2 = ParameterReplacer.Replace(/*toListExp*/ selectExp, mediasParameter, mediasSelector.Body);
        var lambda2 = Expression.Lambda<Func<TSource, object>>(expression2, mediasSelector.Parameters[0]);

        return lambda2;
    }
}
