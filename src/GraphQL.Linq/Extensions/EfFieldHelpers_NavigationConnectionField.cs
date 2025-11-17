// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.Expressions;
using GraphQL.Linq.FieldResolvers;
using GraphQL.Linq.GraphApi;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Linq;

public static partial class EfFieldHelpers
{
    //=================== EfNavigationConnectionField ==========================

    /// <summary>
    /// Creates a connection field for paginated navigation entities resolved from a LINQ expression.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TProperty>>> EfNavigationConnectionField<TDbContext, TSource, TProperty>(IEfObjectGraphType<TDbContext, TSource> graph, Expression<Func<TSource, IEnumerable<TProperty>>> expression, int defaultPageSize = 100, Type? graphType = null) where TSource : class where TProperty : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        string name;
        try {
            name = expression.NameOf();
        } catch {
            throw new ArgumentException(
                $"Cannot infer a Field name from the expression: '{expression.Body}' " +
                $"on parent GraphQL type: '{graph.Name ?? graph.GetType().Name}'.");
        }
        return EfNavigationConnectionField(graph, name, expression, defaultPageSize, graphType);
    }

    /// <summary>
    /// Creates a connection field for paginated navigation entities resolved from a LINQ expression using a custom connection resolver.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TProperty>>> EfNavigationConnectionField<TDbContext, TSource, TProperty>(IEfObjectGraphType<TDbContext, TSource> graph, Expression<Func<TSource, IEnumerable<TProperty>>> expression, IEfConnectionResolver<TDbContext, TProperty> connectionResolver, Type? graphType = null) where TSource : class where TProperty : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        string name;
        try {
            name = expression.NameOf();
        } catch {
            throw new ArgumentException(
                $"Cannot infer a Field name from the expression: '{expression.Body}' " +
                $"on parent GraphQL type: '{graph.Name ?? graph.GetType().Name}'.");
        }
        return EfNavigationConnectionField(graph, name, expression, connectionResolver, graphType);
    }

    /// <summary>
    /// Creates a named connection field for paginated navigation entities resolved from a LINQ expression.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TDbContext, TSource, TReturn>(IEfObjectGraphType<TDbContext, TSource> graph, string name, Expression<Func<TSource, IEnumerable<TReturn>>> expression, int defaultPageSize = 100, Type? graphType = null) where TSource : class where TReturn : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        return EfNavigationConnectionFieldFromContext(graph, name, (context) => expression, defaultPageSize, graphType);
    }

    /// <summary>
    /// Creates a named connection field for paginated navigation entities resolved from a LINQ expression using a custom connection resolver.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TDbContext, TSource, TReturn>(IEfObjectGraphType<TDbContext, TSource> graph, string name, Expression<Func<TSource, IEnumerable<TReturn>>> expression, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null) where TSource : class where TReturn : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        return EfNavigationConnectionFieldFromContext(graph, name, (context) => expression, connectionResolver, graphType);
    }

    /// <summary>
    /// Creates a named connection field for paginated navigation entities resolved from a LINQ expression that includes the data context.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TDbContext, TSource, TReturn>(IEfObjectGraphType<TDbContext, TSource> graph, string name, Expression<Func<TDbContext, TSource, IEnumerable<TReturn>>> expression, int defaultPageSize = 100, Type? graphType = null) where TSource : class where TReturn : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        //rewrite expression to pull from context
        Expression<Func<TSource, IEnumerable<TReturn>>> func(IResolveEfFieldContext<TDbContext, object> context)
        {
            var dbContext = context.DbContext;
            Expression<Func<TDbContext>> func2 = () => dbContext;
            var expression2 = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(ParameterReplacer.Replace(expression.Body, expression.Parameters[0], func2.Body), expression.Parameters[1]);
            return expression2;
        }
        return EfNavigationConnectionFieldFromContext(graph, name, func, defaultPageSize, graphType);
    }

    /// <summary>
    /// Creates a named connection field for paginated navigation entities resolved from a LINQ expression that includes the data context using a custom connection resolver.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionField<TDbContext, TSource, TReturn>(IEfObjectGraphType<TDbContext, TSource> graph, string name, Expression<Func<TDbContext, TSource, IEnumerable<TReturn>>> expression, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null) where TSource : class where TReturn : class
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        //rewrite expression to pull from context
        Expression<Func<TSource, IEnumerable<TReturn>>> func(IResolveEfFieldContext<TDbContext, object> context)
        {
            var dbContext = context.DbContext;
            Expression<Func<TDbContext>> func2 = () => dbContext;
            var expression2 = Expression.Lambda<Func<TSource, IEnumerable<TReturn>>>(ParameterReplacer.Replace(expression.Body, expression.Parameters[0], func2.Body), expression.Parameters[1]);
            return expression2;
        }
        return EfNavigationConnectionFieldFromContext(graph, name, func, connectionResolver, graphType);
    }

    /// <summary>
    /// Creates a named connection field for paginated navigation entities resolved from a context-dependent LINQ expression.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionFieldFromContext<TDbContext, TSource, TReturn>(IEfObjectGraphType<TDbContext, TSource> graph, string name, Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, IEnumerable<TReturn>>>> expression, int? defaultPageSize = 100, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TSource : class where TReturn : class
    {
        return EfNavigationConnectionFieldFromContext(graph, name, expression, new EfSimpleConnectionResolver<TDbContext, TReturn>(defaultPageSize), graphType, arguments);
    }

    /// <summary>
    /// Creates a named connection field for paginated navigation entities resolved from a context-dependent LINQ expression using a custom connection resolver.
    /// </summary>
    public static FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>> EfNavigationConnectionFieldFromContext<TDbContext, TSource, TReturn>(IEfObjectGraphType<TDbContext, TSource> graph, string name, Func<IResolveEfFieldContext<TDbContext, object>, Expression<Func<TSource, IEnumerable<TReturn>>>> expression, IEfConnectionResolver<TDbContext, TReturn> connectionResolver, Type? graphType = null, IEnumerable<QueryArgument>? arguments = null) where TSource : class where TReturn : class
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        //obtain the type
        //graphType = graphType.FindGraphForEfType<TReturn>(true); // false);
        if (graphType != null)
            throw new NotSupportedException("The graphType parameter is not supported in this version of the library.");

        var graphType2 = typeof(NonNullGraphType<>).MakeGenericType(typeof(EfConnectionType<>).MakeGenericType(typeof(TReturn)));
        var builder = FieldBuilder<EfSource<TSource>, Connection<EfSource<TReturn>>>.Create(name, graphType2);
        builder.Argument<IntGraphType>("first", "Specifies the number of edges to return starting from `after` or the first entry if `after` is not specified.");
        builder.Argument<StringGraphType>("after", "Only look at connected edges with cursors greater than the value of `after`.");
        if (connectionResolver.IsBidirectional) {
            builder.Argument<IntGraphType>("last", "Specifies the number of edges to return counting reversely from `before`, or the last entry if `before` is not specified.");
            builder.Argument<StringGraphType>("before", "Only look at connected edges with cursors smaller than the value of `before`.");
        }
        if (arguments != null)
            foreach (var argument in arguments)
                (builder.FieldType.Arguments ??= new QueryArguments()).Add(argument);
        var fieldResolver = new ConnectionFieldResolver<TDbContext, TSource, TReturn>(connectionResolver);
        builder.Resolve(fieldResolver);

        var fieldType = builder.FieldType;
        fieldType.SetEfMetadata(new EfMetadata {
            Expression = expression,
            ConnectionResolver = connectionResolver,
            Type = typeof(TReturn),
        });

        graph.AddField(fieldType);
        return builder;
    }


}
