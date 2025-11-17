// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.Expressions;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq;

/// <summary>
/// Extension methods for adding fields to GraphQL object types that integrate with LINQ data sources.
/// </summary>
public static partial class EfFieldHelpers
{
    // ====================== EfField ================================

    /// <summary>
    /// Defines an entity field mapped from the source such as:
    /// <code>
    /// x =&gt; x.Name
    /// </code>
    /// This will be passed to the database and so should be a simple field or calculation, not a navigation property.
    /// </summary>
    public static EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfField<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        Expression<Func<TSource, TProperty?>> expression,
        bool? nullable = null,
        Type? graphType = null)
        where TSource : class
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

        return EfField(graph, name, expression, nullable, graphType);
    }

    /// <inheritdoc cref="EfField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?, Type?)"/>
    public static EfFieldBuilder<TDbContext, EfSource<TSource>, TProperty> EfField<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Expression<Func<TSource, TProperty?>> expression,
        bool? nullable = null,
        Type? graphType = null)
        where TSource : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        graphType = TypeHelper.GetGraphType<TProperty>(graph.Name ?? graph.GetType().Name, name, expression, nullable, graphType);
        return EfFieldFromContext(graph, name, _ => expression, true, graphType);
    }

    /// <summary>
    /// Defines an entity field mapped from the source and database, such as:
    /// <code>
    /// (db, user) =&gt; db.Sales.Where(sale => sale.CustomerId == user.Id).Count()
    /// </code>
    /// Do not use this to return other database entities; use <see cref="EfNavigationField{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, Expression{Func{TSource, TProperty}}, bool?, Type)">EfNavigationField</see> or <see cref="EfNavigationFieldLink{TDbContext, TSource, TProperty}(IEfObjectGraphType{TDbContext, TSource}, string, Expression{Func{TDbContext, TSource, IEnumerable{TProperty}}}, bool, Type)">EfNavigationFieldLink</see> instead.
    /// </summary>
    public static EfFieldBuilder<TDbContext, EfSource<TSource>, TReturn> EfField<TDbContext, TSource, TReturn>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Expression<Func<TDbContext, TSource, TReturn?>> expression,
        bool? nullable = null,
        Type? graphType = null)
        where TSource : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
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
        graphType = TypeHelper.GetGraphType<TReturn>(graph.Name ?? graph.GetType().Name, name, expression, nullable, graphType);
        return EfFieldFromContext(graph, name, func, true, graphType);
    }

    /// <summary>
    /// Defines an entity field mapped from the field context, such as:
    /// <code>
    /// (context, user) =&gt; {
    ///   var status = context.GetArgument&lt;SaleStatus&gt;("status");
    ///   return context.DbContext.Sales.Where(sale => sale.CustomerId == user.Id &amp;&amp; sale.Status == status).Count();
    /// }
    /// </code>
    /// </summary>
    public static EfFieldBuilder<TDbContext, EfSource<TSource>, TReturn> EfFieldFromContext<TDbContext, TSource, TReturn>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, TReturn?>>> resolveExpression,
        bool nullable = false,
        Type? graphType = null)
        where TSource : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (resolveExpression == null)
            throw new ArgumentNullException(nameof(resolveExpression));

        //obtain the type
        try {
            graphType ??= typeof(TReturn).GetGraphTypeFromType(nullable, TypeMappingMode.OutputType);
        } catch (ArgumentOutOfRangeException exp) {
            throw new ArgumentException(
                $"The GraphQL type for Field: '{name}' on parent type: '{graph.Name ?? graph.GetType().Name}' could not be derived implicitly. \n",
                exp
             );
        }

        var builder = FieldBuilder<EfSource<TSource>, TReturn>.Create(name, graphType)
            .Resolve(FieldResolver.Instance)
            //.Description(expression.DescriptionOf())
            //.DeprecationReason(expression.DeprecationReasonOf())
            //.DefaultValue(expression.DefaultValueOf())
            ;

        builder.FieldType.SetEfMetadata(new EfMetadata {
            Expression = resolveExpression,
            Type = typeof(TReturn),
        });

        graph.AddField(builder.FieldType);
        return builder.AsEfFieldBuilder<TDbContext, EfSource<TSource>, TReturn>();
    }
}
