// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.Expressions;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;

namespace GraphQL.Linq;

public static partial class EfFieldHelpers
{
    // =============== EfNavigationListField ===============
    /// <summary>
    /// Creates a field that returns a list of navigation entities resolved from a LINQ expression.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TProperty>>> EfNavigationListField<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        Expression<Func<TSource, IEnumerable<TProperty>>> expression,
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
        return EfNavigationListField(graph, name, expression, graphType);
    }

    /// <summary>
    /// Creates a named field that returns a list of navigation entities resolved from a LINQ expression.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TProperty>>> EfNavigationListField<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProperty>>> expression,
        Type? graphType = null)
        where TSource : class
        where TProperty : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        return EfNavigationListFieldFromContext(graph, name, context => expression, graphType);
    }

    /// <summary>
    /// Creates a named field that returns a list of navigation entities resolved from a LINQ expression that includes the data context.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TProperty>>> EfNavigationListField<TDbContext, TSource, TProperty>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Expression<Func<TDbContext, TSource, IEnumerable<TProperty>>> expression,
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
        return EfNavigationListFieldFromContext(graph, name, func, graphType);
    }

    /// <summary>
    /// Creates a named field that returns a list of navigation entities resolved from a context-dependent LINQ expression.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TReturn>>> EfNavigationListFieldFromContext<TDbContext, TSource, TReturn>(
        IEfObjectGraphType<TDbContext, TSource> graph,
        string name,
        Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, IEnumerable<TReturn>>>> resolveExpression,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null)
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
        graphType = graphType.FindGraphForEfListType<TReturn>();

        var builder = FieldBuilder<EfSource<TSource>, IEnumerable<EfSource<TReturn>>>.Create(name, graphType)
            .Resolve(FieldResolver.Instance)
            //.Description(expression.DescriptionOf())
            //.DeprecationReason(expression.DeprecationReasonOf())
            //.DefaultValue(expression.DefaultValueOf())
            ;

        builder.FieldType.SetEfMetadata(new EfMetadata {
            Expression = resolveExpression,
            Type = typeof(TReturn),
            Query = true,
        });

        if (arguments != null)
            foreach (var argument in arguments)
                (builder.FieldType.Arguments ??= new QueryArguments()).Add(argument);

        graph.AddField(builder.FieldType);
        return builder;
    }

}
