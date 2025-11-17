// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Types;

namespace GraphQL.Linq;

public static partial class EfFieldHelpers
{
    // ====================== EfIdField ================================
    /// <inheritdoc cref="EfField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?, Type?)"/>
    /// <remarks>
    /// This field will be assigned the <see cref="IdGraphType">ID graph type</see>.
    /// </remarks>
    public static EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfIdField<TDbContext, TSource, TProperty>(IEfObjectGraphType<TDbContext, TSource> graph, Expression<Func<TSource, TProperty?>> expression, bool? nullable = null)
        where TSource : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        //obtain the name
        string name;
        try {
            name = expression.NameOf();
        } catch {
            throw new ArgumentException(
                $"Cannot infer a Field name from the expression: '{expression.Body}' " +
                $"on parent GraphQL type: '{graph.Name ?? graph.GetType().Name}'.");
        }

        return EfIdField(graph, name, expression, nullable);
    }

    /// <inheritdoc cref="EfIdField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?)"/>
    public static EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfIdField<TDbContext, TSource, TProperty>(IEfObjectGraphType<TDbContext, TSource> graph, string name, Expression<Func<TSource, TProperty?>> expression, bool? nullable = null)
        where TSource : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var graphType = TypeHelper.GetGraphType<TProperty>(graph.Name ?? graph.GetType().Name, name, expression, nullable, null, true);
        return EfField(graph, name, expression, nullable, graphType);
    }
}
