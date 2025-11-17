// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.Expressions;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;

namespace GraphQL.Linq;

public static partial class EfFieldHelpers
{
    // =============== EfNavigationField ===============

    /// <summary>
    /// Defines an entity navigational property mapped from the source such as:
    /// <code>
    /// order =&gt; order.Customer
    /// </code>
    /// When building the query, the selected subfields will be examined and individually included in the query.
    /// Using <see cref="EfNavigationFieldLink{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, IEnumerable{TProperty}}}, bool, Type)">EfNavigationFieldLink</see>
    /// instead of this method will generate a better translation to LINQ/SQL.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, EfSource<TProperty>> EfNavigationField<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        Expression<Func<TSource, TProperty?>> expression,
        bool? nullable = null,
        Type? graphType = null)
        where TSource : class
        where TProperty : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
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

        return EfNavigationField(graph, name, expression, nullable, graphType);
    }

    /// <inheritdoc cref="EfNavigationField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?, Type)"/>
    public static FieldBuilder<EfSource<TSource>, EfSource<TProperty>> EfNavigationField<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Expression<Func<TSource, TProperty?>> expression,
        bool? nullable = null,
        Type? graphType = null)
        where TSource : class
        where TProperty : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        nullable ??= TypeHelper.GetNullable<TProperty>(expression);
        return EfNavigationFieldFromContext(graph, name, context => expression, nullable.Value, graphType);
    }

    /// <summary>
    /// Defines an entity navigational property mapped from the source and database such as:
    /// <code>
    /// (db, order) =&gt; db.Customers.FirstOrDefault(x => x.Id == order.CustomerId)
    /// </code>
    /// When building the query, the selected subfields will be examined and individually included in the query.
    /// Using <see cref="EfNavigationFieldLink{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, IEnumerable{TProperty}}}, bool, Type)">EfNavigationFieldLink</see>
    /// instead of this method will generate a better translation to LINQ/SQL.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, EfSource<TReturn>> EfNavigationField<TDbContext, TSource, TReturn>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Expression<Func<TDbContext, TSource, TReturn?>> expression,
        bool? nullable = null,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        //rewrite expression to pull from context
        Expression<Func<TSource, TReturn?>> func(IResolveEfFieldContext<TDbContext, object> context)
        {
            var dbContext = context.DbContext;
            Expression<Func<TDbContext>> func2 = () => dbContext;
            var expression2 = Expression.Lambda<Func<TSource, TReturn?>>(ParameterReplacer.Replace(expression.Body, expression.Parameters[0], func2.Body), expression.Parameters[1]);
            return expression2;
        }
        nullable ??= TypeHelper.GetNullable<TReturn>(expression);
        return EfNavigationFieldFromContext(graph, name, func, nullable.Value, graphType);
    }

    private static FieldBuilder<EfSource<TSource>, EfSource<TReturn>> EfNavigationFieldFromContext<TDbContext, TSource, TReturn>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, TReturn?>>> resolveExpression,
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
            Graph = true,
        });
        graph.AddField(builder.FieldType);
        return builder;
    }
}
