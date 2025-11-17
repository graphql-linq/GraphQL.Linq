// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.Expressions;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;

namespace GraphQL.Linq;

public static partial class EfFieldHelpers
{
    // =============== EfNavigationFieldLink ===============

    /// <summary>
    /// Defines an entity navigational property mapped from the source and database such as:
    /// <code>
    /// (db, order) =&gt; db.Customers.Where(x => x.Id == order.CustomerId)
    /// </code>
    /// This method allows for a query without the FirstOrDefault call, but still acts as a navigational property returning a single entity as the result.
    /// The benefit is that GraphQL.Linq can generate a much more optimized expression tree (which translates to a SQL query) than using
    /// <see cref="EfNavigationField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?, Type)">EfNavigationField</see>.
    /// <para>
    /// When building the query, the selected subfields will be examined and individually included in the query.
    /// </para>
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, EfSource<TProperty>> EfNavigationFieldLink<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Expression<Func<TDbContext, TSource, IEnumerable<TProperty>>> expression,
        bool nullable = false,
        Type? graphType = null)
        where TSource : class
        where TProperty : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        //rewrite expression to pull from context
        Expression<Func<TSource, IEnumerable<TProperty>>> func(IResolveEfFieldContext<TDbContext, object> context)
        {
            var dbContext = context.DbContext;
            Expression<Func<TDbContext>> func2 = () => dbContext;
            var expression2 = Expression.Lambda<Func<TSource, IEnumerable<TProperty>>>(ParameterReplacer.Replace(expression.Body, expression.Parameters[0], func2.Body), expression.Parameters[1]);
            return expression2;
        }
        return EfNavigationFieldLinkFromContext(graph, name, func, nullable, graphType);
    }

    private static FieldBuilder<EfSource<TSource>, EfSource<TReturn>> EfNavigationFieldLinkFromContext<TDbContext, TSource, TReturn>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, IEnumerable<TReturn>>>> resolveExpression,
        bool nullable = false,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (resolveExpression == null)
            throw new ArgumentNullException(nameof(resolveExpression));

        //obtain the type
        graphType = graphType.FindGraphForEfType<TReturn>(nullable);

        var builder = FieldBuilder<EfSource<TSource>, EfSource<TReturn>>.Create(name, graphType)
            .Resolve(FieldResolver.Instance)
            //.Description(expression.DescriptionOf())
            //.DeprecationReason(expression.DeprecationReasonOf())
            //.DefaultValue(expression.DefaultValueOf())
            ;

        builder.FieldType.SetEfMetadata(new EfMetadata {
            Expression = resolveExpression,
            Type = typeof(TReturn),
            Query = true,
            Single = true,
        });

        graph.AddField(builder.FieldType);
        return builder;
    }
}
