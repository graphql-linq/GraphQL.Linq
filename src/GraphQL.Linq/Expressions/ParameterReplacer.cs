// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq.Expressions;

/// <summary>
/// Contains utility methods for manipulating expression trees, 
/// including replacing parameters with new expressions and 
/// chaining lambda expressions together.
/// </summary>
public static class ParameterReplacer
{
    /// <summary>
    /// Produces an expression identical to <paramref name="expression"/> 
    /// except with the <paramref name="oldParameter"/> parameter replaced with the <paramref name="newBody"/> expression.
    /// </summary>
    /// <param name="expression">The original expression to be replaced.</param>
    /// <param name="oldParameter">The parameter to be replaced.</param>
    /// <param name="newBody">The new expression to replace the old parameter.</param>
    /// <returns>A new expression with the specified parameter replaced.</returns>
    public static Expression Replace(this Expression expression, ParameterExpression oldParameter, Expression newBody)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        if (oldParameter == null)
            throw new ArgumentNullException(nameof(oldParameter));
        if (newBody == null)
            throw new ArgumentNullException(nameof(newBody));
        if (expression is LambdaExpression)
            throw new InvalidOperationException("The search & replace operation must be performed on the body of the lambda.");
        return new ParameterReplacerVisitor(oldParameter, newBody).Visit(expression);
    }

    /// <summary>
    /// Chains two lambda expressions together, as illustrated in the following example:
    /// <para>
    /// Given these inputs:
    /// </para>
    /// <example>
    /// <code>
    /// var parentExpression = customer => customer.PrimaryAddress;
    /// var childExpression = address => address.Street;
    /// </code>
    /// </example>
    /// Produces:
    /// <example>
    /// <code>
    /// customer => customer.PrimaryAddress.Street;
    /// </code>
    /// </example>
    /// This function only supports parents with a single input parameter and children with a single output parameter.
    /// </summary>
    /// <typeparam name="TSource">The type of the parent expression input.</typeparam>
    /// <typeparam name="TIntermediate">The type of the parent expression output/child input.</typeparam>
    /// <typeparam name="TResult">The type of the child expression output.</typeparam>
    /// <param name="parentExpression">The parent expression to be chained.</param>
    /// <param name="childExpression">The child expression to be chained.</param>
    /// <returns>A new chained expression.</returns>
    public static Expression<Func<TSource, TResult>> ChainWith<TSource, TIntermediate, TResult>(this Expression<Func<TSource, TIntermediate>> parentExpression, Expression<Func<TIntermediate, TResult>> childExpression)
    {
        // Could call Chain, but some of the checks are unnecessary since the inputs are strongly typed
        if (parentExpression == null)
            throw new ArgumentNullException(nameof(parentExpression));
        if (childExpression == null)
            throw new ArgumentNullException(nameof(childExpression));
        // Since the lambda is strongly defined, we can be sure that there exists one and only one parameter on the parent and child expressions
        return Expression.Lambda<Func<TSource, TResult>>(
            Replace(childExpression.Body, childExpression.Parameters[0], parentExpression.Body),
            parentExpression.Parameters);
    }

    /// <summary>
    /// Chains two lambda expressions together, as illustrated in the following example:
    /// <para>
    /// Given these inputs:
    /// </para>
    /// <example>
    /// <code>
    /// var parentExpression = (customers, index) => customers[index].PrimaryAddress;
    /// var childExpression = address => Console.WriteLine(address.Street);
    /// </code>
    /// </example>
    /// Produces:
    /// <example>
    /// <code>
    /// (customers, index) => Console.WriteLine(customers[index].PrimaryAddress.Street);
    /// </code>
    /// </example>
    /// This function supports parent expressions with any number of input parameters (including 0) and child expressions with no output value (Action&lt;&gt;s).
    /// However, it is not strongly typed, and validity cannot be verified at compile time.
    /// </summary>
    /// <param name="parentExpression">The parent expression to be chained.</param>
    /// <param name="childExpression">The child expression to be chained.</param>
    /// <returns>A new chained lambda expression.</returns>
    public static LambdaExpression Chain(LambdaExpression parentExpression, LambdaExpression childExpression)
    {
        if (parentExpression == null)
            throw new ArgumentNullException(nameof(parentExpression));
        if (childExpression == null)
            throw new ArgumentNullException(nameof(childExpression));
        if (parentExpression.ReturnType.Equals(typeof(void)))
            throw new ArgumentException("The parent expression must return a value.", nameof(parentExpression));
        if (childExpression.Parameters.Count != 1 || !childExpression.Parameters[0].Type.Equals(parentExpression.ReturnType))
            throw new ArgumentException("The child expression must have a single parameter of the same type as the parent expression's return type.", nameof(childExpression));
        // This code could provide a conversion between compatible types, but for now just throws an error; the types must be identical.
        return Expression.Lambda(Replace(childExpression.Body, childExpression.Parameters[0], parentExpression.Body), parentExpression.Parameters);
    }

    /// <summary>
    /// A visitor class that replaces occurrences of a specified parameter expression
    /// within an expression tree with a new expression.
    /// </summary>
    private sealed class ParameterReplacerVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _sourceParameter;
        private readonly Expression _replacementExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterReplacerVisitor"/> class.
        /// </summary>
        /// <param name="sourceParameter">The parameter expression to replace.</param>
        /// <param name="replacementExpression">The expression to replace with.</param>
        public ParameterReplacerVisitor(ParameterExpression sourceParameter, Expression replacementExpression)
        {
            _sourceParameter = sourceParameter ?? throw new ArgumentNullException(nameof(sourceParameter));
            _replacementExpression = replacementExpression ?? throw new ArgumentNullException(nameof(replacementExpression));
        }

        /// <summary>
        /// Visits a <see cref="ParameterExpression"/> node, replacing it with the target expression if it matches
        /// the source parameter.
        /// </summary>
        /// <param name="node">The parameter expression to visit.</param>
        /// <returns>
        /// The replacement expression if the visited parameter matches the source parameter; otherwise, the base visit result.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            // If the node matches the source parameter, replace it with the target expression.
            // Otherwise, visit other parameters as usual.
            return node.Equals(_sourceParameter) ? _replacementExpression : base.VisitParameter(node);
        }
    }
}
