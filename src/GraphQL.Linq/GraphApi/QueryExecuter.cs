// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Execution;
using GraphQL.Linq.ConnectionResolvers;
using GraphQL.Linq.Expressions;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using GraphQLParser;
using GraphQLParser.AST;

namespace GraphQL.Linq.GraphApi;

/// <summary>
/// Executes a query for a given DbContext and return type.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TReturn">The type of the objects returned by the query.</typeparam>
public class QueryExecuter<TDbContext, TReturn>
    where TReturn : class
{
    /// <summary>
    /// Examines a GraphQL field context to identify the requested fields and generates an <see cref="IQueryable{T}"/> that can
    /// be executed to retrieve the data for the selected fields.  This method is typically used for a root query that returns
    /// a single record, such as a query for a single product.  It can also be used for a root query that returns a list of records,
    /// such as a query for a list of products.
    /// </summary>
    /// <param name="efContext">The context for the field being resolved.</param>
    /// <param name="baseQuery">The base query that will be used to retrieve the data.</param>
    public virtual IQueryable<EfSource<TReturn>> GenerateQuery(IResolveEfFieldContext<TDbContext> efContext, IQueryable<TReturn> baseQuery)
    {
        //create the select argument based on the selected fields
        var selectArgument = CreateSelectArgument<TReturn>(efContext);
        // -- e.g. (product) => new Dictionary<string, object>() {
        //                          { "Id", (object)product.Id },
        //                          { "Name", (object)product.Name }
        //                      }

        //append this to the above baseQuery
        var ret = baseQuery.Select(selectArgument);
        // -- e.g. ret would now be the same as:
        //    context.DbContext.Products.Where(p => !p.Deleted)
        //        .OrderBy(p => p.Name)
        //        .Select(product => new Dictionary<string, object>() { { "Id", (object)product.Id }, { "Name", (object)product.Name } })

        //return the unexecuted query
        return ret;
    }

    /// <summary>
    /// Examines a GraphQL field context to identify the requested fields and generates an <see cref="IQueryable{T}"/> that can
    /// be executed to retrieve the data for the selected fields.  This method is typically used for data loaders to retrieve
    /// a list of records based on a list of keys.  Optionally a custom item selector can be provided which is applied to the
    /// base query before the select argument based on the GraphQL field context is applied.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TObject">The type of the objects being returned by the base query.</typeparam>
    public virtual IQueryable<Tuple<TKey, EfSource<TReturn>>> GenerateQueryForKeys<TKey, TObject>(IResolveEfFieldContext<TDbContext> efContext, IQueryable<TObject> baseQuery, Expression<Func<TObject, TKey>> keySelector, IEnumerable<TKey> keys, Expression<Func<TObject, TReturn>> itemSelector)
    {
        //assuming that baseQuery was:
        //    context.DbContext.OrderItems
        //and keySelector was:
        //    orderItem => orderItem.OrderId
        //and itemSelector was:
        //    orderItem => orderItem.Product

        //create the select argument based on the selected fields
        var selectArgument1 = CreateSelectArgument<TReturn>(efContext);
        // -- e.g. (product) => new Dictionary<string, object>() {
        //                          { "Id", (object)product.Id },
        //                          { "Name", (object)product.Name }
        //                      }

        //couple the select argument with the itemSelector
        var selectArgument = itemSelector.ChainWith(selectArgument1);
        // -- e.g. (orderItem) => new Dictionary<string, object>() {
        //                            { "Id", (object)orderItem.Product.Id },
        //                            { "Name", (object)orderItem.Product.Name }
        //                        }

        //create a select argument that includes the key
        var tupleArgument = CreateSelectWithKey(keySelector, selectArgument);
        // -- e.g. (orderItem) => new Tuple<int, Dictionary<string, object>>(
        //                            orderItem.OrderId,
        //                            new Dictionary<string, object>() {
        //                                { "Id", (object)orderItem.Product.Id },
        //                                { "Name", (object)orderItem.Product.Name }
        //                            })

        //create a where expression to match on the given keys
        var whereArgument = efContext.EfGraphQLService.CreateWhereInExpression(() => efContext.DbContext, keySelector, keys);
        // -- e.g. (orderItem) => keys.Contains(orderItem.OrderId)

        //append this to the above baseQuery
        var ret = baseQuery.Where(whereArgument).Select(tupleArgument);
        // -- e.g. ret would now be the same as:
        //    context.DbContext.OrderItems
        //        .Where(orderItem => keys.Contains(orderItem.OrderId))
        //        .Select(orderItem => new Tuple<int, Dictionary<string, object>>(orderItem.OrderId, new Dictionary<string, object>() { { "Id", (object)orderItem.Product.Id }, { "Name", (object)orderItem.Product.Name } }))

        //return the unexecuted query
        return ret;
    }

    /// <summary>
    /// Creates a select argument that can be used to select keys and values from a base query.
    /// The return object type is a <see cref="Tuple{T1, T2}"/> where T1 is the key type and T2 is the value type.
    /// The value type is an <see cref="EfSource{T}"/> that contains the fields selected from the GraphQL query.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TObject">The type of the object the base query returns.</typeparam>
    protected virtual Expression<Func<TObject, Tuple<TKey, EfSource<TReturn>>>> CreateSelectWithKey<TKey, TObject>(Expression<Func<TObject, TKey>> keySelector, Expression<Func<TObject, EfSource<TReturn>>> valueSelector)
    {
        var tupleType = typeof(Tuple<TKey, EfSource<TReturn>>);
        var tupleConstructor = tupleType.GetConstructors()[0];
        var param = keySelector.Parameters[0];
        var keyBody = keySelector.Body;
        var valueBody = valueSelector.Body.Replace(valueSelector.Parameters[0], param);
        var newBody = Expression.New(
            tupleConstructor,
            keyBody,
            valueBody);
        return Expression.Lambda<Func<TObject, Tuple<TKey, EfSource<TReturn>>>>(newBody, param);
    }

    /// <summary>
    /// Examines a GraphQL field context to identify the requested fields and generates an <see cref="Connection{TNode}"/> containing
    /// the data for the selected fields.  This method is typically used for a root query that returns a list of records, such as a query
    /// for a list of products.  When selected by the GraphQL query, the connection object contains the data for the selected fields,
    /// for the selected page, and/or the total count of records that would be returned if the query was not paginated.
    /// The query is executed asynchronously against the database and a populated <see cref="Connection{TNode}"/> is returned.
    /// </summary>
    public virtual async Task<Connection<EfSource<TReturn>>> ExecuteConnectionAsync<TSource>(IResolveEfConnectionContext<TDbContext, TSource> connectionContext, IEfConnectionResolver<TDbContext, TReturn> resolver, IQueryable<TReturn> query)
    {
        resolver.ValidateArguments(connectionContext);
        var needsCount = connectionContext.SubFields?.ContainsKey("totalCount") == true;
        var needsItems = connectionContext.SubFields?.ContainsKey("edges") == true || connectionContext.SubFields?.ContainsKey("items") == true;
        var usesCursorExpression = resolver.GetCursorExpression(connectionContext) != null;
        //prep count function
        Task<int> countFunc() => resolver.CountQueryable(connectionContext, query);

        if (needsItems) {
            //filter query with Skip/Take/etc
            var filteredQuery = resolver.FilterQueryable(connectionContext, query);
            //get proper fields
            var selectedQuery = filteredQuery.Select(CreateConnectionSelectArgument<TSource, TReturn>(connectionContext, resolver));
            //execute the query
            var items = await connectionContext.EfGraphQLService.QueryToListAsync(selectedQuery, connectionContext.CancellationToken);
            //add cursors
            var itemsWithCursors = items.Select((item, index) => (resolver.SerializeCursor(connectionContext, index, usesCursorExpression ? item["__EF_Cursor"] : null), item)).ToList();
            //generate connection
            var connection = await resolver.ResolveConnectionObject(connectionContext, itemsWithCursors, needsCount ? (Func<Task<int>>)countFunc : null);
            //return connection
            return connection;
        } else if (needsCount) {
            return await resolver.ResolveConnectionObject<TSource, EfSource<TReturn>>(connectionContext, null, countFunc);
        } else {
            return new();
        }
    }

    /// <summary>
    /// Creates an expression that selects object data from a parent type using the specified selector and context.
    /// </summary>
    protected virtual Expression<Func<TParentType, object>> CreateObject<TParentType>(Type tObjectType, IResolveEfFieldContext<TDbContext, object?> context, LambdaExpression selector)
    {
        //call the generic version of this function (CreateObjectInternal)
        if (tObjectType == null)
            throw new ArgumentNullException(nameof(tObjectType));

        //call the generic version of this function (CreateObjectInternal)
        var method = _createObjectInternalDictionary.GetOrAdd((GetType(), typeof(TParentType), tObjectType), factory);
        return (Expression<Func<TParentType, object>>)method.Invoke(this, new object[] { context, selector });

        static MethodInfo factory((Type thisObj, Type tParentType, Type newType) key) => key.thisObj.GetMethod(nameof(CreateObjectInternal), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(key.tParentType, key.newType);
    }

    private static readonly ConcurrentDictionary<(Type, Type, Type), MethodInfo> _createObjectInternalDictionary = new ConcurrentDictionary<(Type, Type, Type), MethodInfo>();

    /// <summary>
    /// Internal implementation that creates an expression to select object data from a parent type.
    /// </summary>
    protected virtual Expression<Func<TParentType, object>> CreateObjectInternal<TParentType, TObjectType>(IResolveEfFieldContext<TDbContext, object> context, LambdaExpression selector) where TObjectType : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        //check if null, then return CreateSelectArgument

        //select from TParentType
        // -- e.g. (product) => product.Category
        var exp4 = (Expression<Func<TParentType, TObjectType>>)selector;

        //select arguments from TObjectType
        // -- e.g. (category) => new Dictionary<string, object>() { { "Id", category.Id } }
        var exp5 = CreateSelectArgument<TObjectType>(context);

        //replace body with parent selector
        // -- e.g. new Dictionary<string, object>() { { "Id", product.Category.Id } }
        var exp6 = ParameterReplacer.Replace(exp5.Body, exp5.Parameters[0], exp4.Body);

        //check if it's never null
        if (context.FieldDefinition.ResolvedType is NonNullGraphType) {
            //create lambda with original parameters
            // -- e.g. (product) => new Dictionary<string, object>() { { "Id", product.Category.Id } }
            return Expression.Lambda<Func<TParentType, object>>(exp6, exp4.Parameters);
        }

        //create null condition body
        // -- e.g. product.Category == null ? null : new Dictionary<string, object>() { { "Id", product.Category.Id } }
        var exp7 = Expression.Condition(Expression.Equal(exp4.Body, Expression.Constant(null, typeof(TObjectType))), Expression.Constant(null, exp6.Type), exp6);

        //create lambda with original parameters
        // -- e.g. (product) => product.Category == null ? null : new Dictionary<string, object>() { { "Id", product.Category.Id } }
        return Expression.Lambda<Func<TParentType, object>>(exp7, exp4.Parameters);
    }

    private static readonly MethodInfo _firstOrDefault = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == "FirstOrDefault" && x.GetParameters().Length == 1);
    private static readonly ConcurrentDictionary<Type, MethodInfo> _firstOrDefaultTyped = new ConcurrentDictionary<Type, MethodInfo>();

    /// <summary>
    /// Accepts an expression tree that returns an enumerable sequence (e.g. IEnumerable&lt;T&gt;),
    /// appends .FirstOrDefault() and returns the resulting expression.
    /// </summary>
    protected virtual LambdaExpression FirstOrDefault(LambdaExpression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var body = expression.Body;
        var returnType = body.Type;
        Type? objectType = null;
        foreach (var supportedInterface in returnType.GetInterfaces().Append(returnType)) {
            if (supportedInterface.IsGenericType && supportedInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                objectType = supportedInterface.GenericTypeArguments[0];
                break;
            }
        }
        if (objectType == null)
            throw new ArgumentException($"The return type of the lambda expression is not of type IEnumerable<T>");
        var method = _firstOrDefaultTyped.GetOrAdd(objectType, t => _firstOrDefault.MakeGenericMethod(t));
        var newBody = Expression.Call(null, method, body);
        var newLambda = Expression.Lambda(newBody, expression.Parameters);
        return newLambda;
    }

    /// <summary>
    /// Creates an expression that applies a select query to retrieve data from a parent type.
    /// </summary>
    protected virtual Expression<Func<TParentType, object>> CreateSelectQuery<TParentType>(Type tObjectType, IResolveEfFieldContext<TDbContext, object?> context, LambdaExpression selector)
    {
        if (tObjectType == null)
            throw new ArgumentNullException(nameof(tObjectType));
        if (!tObjectType.IsClass)
            throw new ArgumentOutOfRangeException(nameof(tObjectType), "Class type required");

        //call the generic version of this function (CreateSelectQueryInternal)
        var method = _createSelectQueryDictionary.GetOrAdd((GetType(), typeof(TParentType), tObjectType), factory);
        return (Expression<Func<TParentType, object>>)method.Invoke(this, new object[] { context, selector });

        static MethodInfo factory((Type thisObj, Type tParentType, Type newType) key) => key.thisObj.GetMethod(nameof(CreateSelectQueryInternal), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(key.tParentType, key.newType);
    }

    private static readonly ConcurrentDictionary<(Type, Type, Type), MethodInfo> _createSelectQueryDictionary = new ConcurrentDictionary<(Type, Type, Type), MethodInfo>();

    /// <summary>
    /// Internal implementation that creates an expression to apply a select query to retrieve data from a parent type.
    /// </summary>
    protected virtual Expression<Func<TParentType, object>> CreateSelectQueryInternal<TParentType, TObjectType>(IResolveEfFieldContext<TDbContext, object> context, LambdaExpression selector) where TObjectType : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        //create an expression that runs Select from the CreateSelectArgument result

        //cast the selector to its proper type
        var castSelector = (Expression<Func<TParentType, IEnumerable<TObjectType>>>)selector;
        // -- e.g. (category) => category.Products

        //apply where, skip, take, and orderby arguments to baseQuery
        // -- e.g. orderby name
        //var whereAndSelector = ArgumentProcessor.ApplyGraphQlArguments(castSelector, context, _efGraphQLService.GetKeyNames<TReturn>());
        // -- e.g. whereQuery would be the same as:
        //    (category) => category.Products.OrderBy(p => p.Name)

        //build the select argument
        var selectArgument = CreateSelectArgument<TObjectType>(context);
        // -- e.g. (product) => new Dictionary<string, object>() { { "Id", product.Category.Id } }

        //build the navigation between the two, and cast to object
        //var ret = LambdaBuilder.BuildNavigation(whereAndSelector, selectArgument);
        var ret = LambdaBuilder.BuildNavigation(castSelector, selectArgument);
        // -- e.g. (category) => (object)category.Products.OrderBy(p => p.Name).Select(product => new Dictionary<string, object>() { { "Id", product.Category.Id } })

        return ret;
    }

    /// <summary>
    /// Creates a select argument expression that maps object properties to an EfSource dictionary.
    /// </summary>
    protected virtual Expression<Func<TObjectType, EfSource<TObjectType>>> CreateSelectArgument<TObjectType>(IResolveEfFieldContext<TDbContext> context) where TObjectType : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        //this function returns a block like this:
        //  return (product) => new Dictionary() {
        //                           { "Id", (object)product.Id },
        //                           { "Name", (object)product.Name },
        //                     };

        //get all the fields and expressions for each field
        var fieldsToSelect = CreateSelectArgumentDictionary<TObjectType>(context);
        //  -- e.g. new Dictionary<string, Expression<Func<TObjectType, object>>>() {
        //              { "Id", product => (object)product.Id },
        //              { "Name", product => (object)product.Name },
        //          };

        //create select EXPRESSION based on fields in fieldsToSelect
        var ret = LambdaBuilder.Build(fieldsToSelect);

        // -- e.g. (product) => new Dictionary<string, object>() {
        //                          { "Id", (object)product.Id },
        //                          { "Name", (object)product.Name },
        //                          { "Category", (object)new Dictionary<string, object>() { { "Name", product.Category.Name } } },
        //                          { "Pictures", (object)product.Pictures.Select(picture => new Dictionary<string, object>() { { "Id", picture.Id } } ) },
        //                      }

        //return the new expression
        return ret;
    }

    /// <summary>
    /// Creates a dictionary of field names and their corresponding select expressions for building query projections.
    /// </summary>
    protected virtual List<KeyValuePair<string, Expression<Func<TObjectType, object>>>> CreateSelectArgumentDictionary<TObjectType>(IResolveEfFieldContext<TDbContext> context)
        where TObjectType : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        //this function returns a dictionary like this:
        //  return new Dictionary<string, Expression<Func<TObjectType, object>>>() {
        //             { "Id", product => (object)product.Id },
        //             { "Name", product => (object)product.Name },
        //         };

        //field (FieldAst) comes from the query
        var field = context.FieldAst;
        //fieldType (FieldDefinition) comes from the graph
        var fieldType = context.FieldDefinition;
        //to get the chilren of the graph, we need an IComplexGraphType reference
        if (!TryGetObjectGraphType<TObjectType>(fieldType.ResolvedType!, out var complexGraph))
            throw new InvalidOperationException("Invalid graph type");
        var subFields = new List<GraphQLField>();
        GetFields(field.SelectionSet!, subFields);

        //create an list of the fields we need to combine into our output expression
        var fieldsToSelect = new List<KeyValuePair<string, Expression<Func<TObjectType, object>>>>(subFields.Count + 1);
        var fieldsAdded = new HashSet<string>();

        //add a constant field so the row is materialized
        fieldsToSelect.Add(new KeyValuePair<string, Expression<Func<TObjectType, object>>>("__dummy", context.EfGraphQLService.GetDummyExpression<TObjectType>()));

        //loop through each of the child fields that are being selected
        foreach (var subField in subFields) {
            // deduplicate fields
            if (!fieldsAdded.Add(subField.Name.StringValue))
                continue;

            //locate the matching graph from the child graphs of the parent graph
            var subFieldType = complexGraph.Fields.SingleOrDefault(x => x.Name == subField.Name.Value);
            if (subFieldType != null) {
                //found a matching subfield
                //now we have subField (the FieldAst) and subFieldType (the FieldDefinition)
                //subFieldType contains the metadata we have stored for that field

                var subFieldMetadata = subFieldType.GetEfMetadata();
                //check if this is any type of EF field (EfField, EfNavigationField, or EfNavigationListField)
                if (subFieldMetadata != null) {
                    //only the root would have a typed expression context, so we know the type of this expression
                    //(the source is always null, so that's why the source is typed as an object)
                    var resolveExpressionResolver = (Func<IResolveEfFieldContext<TDbContext, object?>, LambdaExpression>)subFieldMetadata.Expression;

                    //retrieve the field type that the LambdaExpression will be returning
                    if (subFieldMetadata.Type == null)
                        throw new InvalidOperationException("_EF_Type metadata missing");

                    //create a context which represents this child field
                    var subContext = CreateChildEfFieldContext(
                        context,
                        subField,
                        subFieldType,
                        complexGraph,
                        context.ExecutionContext.GetArguments(subFieldType, subField)!,
                        context.Path!.Concat(new object[] { subField.Name }));

                    Expression<Func<TObjectType, object>> subFieldExpressionCast;
                    if (subFieldMetadata.ConnectionResolver != null) {
                        subFieldExpressionCast = CreateConnectionExpressionForSubField<TObjectType>(
                            subContext,
                            resolveExpressionResolver,
                            subFieldMetadata.Type,
                            subFieldMetadata.ConnectionResolver);
                    } else {
                        //build the expression for this child field
                        subFieldExpressionCast = CreateExpressionForSubField<TObjectType>(
                            subContext,
                            resolveExpressionResolver,
                            subFieldMetadata);
                    }

                    //add this to our list of fields
                    fieldsToSelect.Add(new KeyValuePair<string, Expression<Func<TObjectType, object>>>(subField.Name.StringValue, subFieldExpressionCast));
                }
            }
        }

        //at this point fieldsToSelect is a Dictionary of fields and expressions that return objects:
        // -- e.g. new Dictionary<string, Expression<Func<TObjectType, object>>>() {
        //             { "Id", product => (object)product.Id },
        //             { "Name", product => (object)product.Name },
        //             { "Category", product => (object)new Dictionary<string, object>() { { "Name", product.Category.Name } } },
        //             { "Pictures", product => (object)product.Pictures.Select(picture => new Dictionary<string, object>() { { "Id", picture.Id } } ) }
        //         }

        return fieldsToSelect;

        void GetFields(GraphQLSelectionSet selectionSet, List<GraphQLField> fields)
        {
            //look at the incoming query, and see what child fields are being requested
            fields.AddRange(selectionSet.Selections.OfType<GraphQLField>());
            //look at the incoming query, and see what inline fragments are being requested
            var inlineFragments = selectionSet.Selections.OfType<GraphQLInlineFragment>();

            //check that the inline fragment matches our graphtype, or is a compatible type
            foreach (var inlineFragment in inlineFragments) {
                if (inlineFragment.TypeCondition == null || !Matches(inlineFragment.TypeCondition.Type.Name))
                    continue;

                GetFields(inlineFragment.SelectionSet, fields);
            }

            //look at the incoming query, and see what fragment spreads are being requested
            var fragmentSpreads = selectionSet.Selections.OfType<GraphQLFragmentSpread>();

            foreach (var fragmentSpread in fragmentSpreads) {
                // find the fragment that matches the fragment spread
                if (context.Document.Definitions.FirstOrDefault(x =>
                    x is GraphQLFragmentDefinition fragmentDefinition &&
                    fragmentDefinition.FragmentName.Name == fragmentSpread.FragmentName.Name) is not GraphQLFragmentDefinition fragment)
                    continue;

                if (!Matches(fragment.TypeCondition.Type.Name))
                    continue;

                GetFields(fragment.SelectionSet, fields);
            }

            bool Matches(ROM typeName)
            {
                // identify the graph type referenced in the inline fragment
                var graphType2 = context.Schema.AllTypes[typeName];

                // for union/interface graph types, find the matching graph type for EfSource<TObjectType>
                return graphType2 == complexGraph ||
                    graphType2 is InterfaceGraphType interfaceGraphType && interfaceGraphType.IsValidInterfaceFor(complexGraph, false) ||
                    graphType2 is UnionGraphType unionGraphType && unionGraphType.IsPossibleType(complexGraph);
            }
        }
    }

    /// <summary>
    /// Creates an expression for a connection sub-field that handles paginated data retrieval.
    /// </summary>
    protected virtual Expression<Func<TObjectType, object>> CreateConnectionExpressionForSubField<TObjectType>(IResolveEfFieldContext<TDbContext, object?> context, Func<IResolveEfFieldContext<TDbContext, object?>, LambdaExpression> resolveExpressionResolver, Type subFieldEfType, object connectionResolver)
    {
        if (subFieldEfType == null)
            throw new ArgumentNullException(nameof(subFieldEfType));

        //call the generic version of this function (CreateConnectionExpressionForSubFieldTyped)
        var method = _createConnectionExpressionForSubFieldTypedDictionary.GetOrAdd((GetType(), typeof(TObjectType), subFieldEfType), factory);
        return (Expression<Func<TObjectType, object>>)method.Invoke(this, new object[] { context, resolveExpressionResolver, connectionResolver });

        static MethodInfo factory((Type thisObj, Type tObjectType, Type subFieldEfType) key) => key.thisObj.GetMethod(nameof(CreateConnectionExpressionForSubFieldTyped), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(key.tObjectType, key.subFieldEfType);
    }

    private static readonly ConcurrentDictionary<(Type, Type, Type), MethodInfo> _createConnectionExpressionForSubFieldTypedDictionary = new ConcurrentDictionary<(Type, Type, Type), MethodInfo>();

    /// <summary>
    /// Internal typed implementation that creates an expression for a connection sub-field with specific child type.
    /// </summary>
    protected virtual Expression<Func<TObjectType, object>> CreateConnectionExpressionForSubFieldTyped<TObjectType, TChildType>(
        IResolveEfFieldContext<TDbContext, object> context,
        Func<IResolveEfFieldContext<TDbContext, object>, LambdaExpression> resolveExpressionResolver,
        IEfConnectionResolver<TDbContext, TChildType> connectionResolver) where TChildType : class where TObjectType : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (resolveExpressionResolver == null)
            throw new ArgumentNullException(nameof(resolveExpressionResolver));
        if (connectionResolver == null)
            throw new ArgumentNullException(nameof(connectionResolver));

        //run the subField resolve function to get the lambda expression
        //note that this does not currently support async functions; although it is possible, it
        //  is designed this way because it should be a simple function returning a lambda
        //  expression based on the arguments; nothing more
        var subFieldExpression = resolveExpressionResolver(context);
        var subFieldExpressionCast = (Expression<Func<TObjectType, IEnumerable<TChildType>>>)subFieldExpression;

        var connectionContext = new ResolveEfConnectionContext<TDbContext, object>(context, connectionResolver.IsBidirectional, connectionResolver.DefaultPageSize);
        connectionResolver.ValidateArguments(connectionContext);
        var needsCount = context.FieldAst.SelectionSet?.Selections.OfType<GraphQLField>().Any(x => x.Name == "totalCount") ?? false;
        var needsItems = context.FieldAst.SelectionSet?.Selections.OfType<GraphQLField>().Any(x => x.Name == "pageInfo" || x.Name == "edges" || x.Name == "items") ?? false;
        var dic = new Dictionary<string, Expression<Func<TObjectType, object>>>();
        if (needsItems) {
            var subFieldExpressionFiltered = connectionResolver.FilterExpression(connectionContext, subFieldExpressionCast);
            var metadata = connectionContext.FieldDefinition.GetEfMetadata()!;
            var resolver = (IEfConnectionResolver<TDbContext, TChildType>)metadata.ConnectionResolver!;
            var selectArgument = CreateConnectionSelectArgument<object, TChildType>(connectionContext, resolver);
            var newExpression = LambdaBuilder.BuildNavigation(subFieldExpressionFiltered, selectArgument);
            dic.Add("items", newExpression);
        }
        if (needsCount) {
            var countExpression = connectionResolver.CountExpression(connectionContext, subFieldExpressionCast);
            var countExpressionCast = Expression.Lambda<Func<TObjectType, object>>(
                Expression.Convert(countExpression.Body, typeof(object)),
                countExpression.Parameters);
            dic.Add("count", countExpressionCast);
        }
        var retLambda = LambdaBuilder.Build(dic);

        var castToObject = Expression.Convert(retLambda.Body, typeof(object));
        return Expression.Lambda<Func<TObjectType, object>>(castToObject, retLambda.Parameters);
    }

    /// <summary>
    /// Creates a select argument expression for connection fields that includes cursor information for pagination.
    /// </summary>
    protected virtual Expression<Func<TObjectType, EfSource<TObjectType>>> CreateConnectionSelectArgument<TContextSource, TObjectType>(IResolveEfConnectionContext<TDbContext, TContextSource> context, IEfConnectionResolver<TDbContext, TObjectType> resolver) where TObjectType : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.FieldAst.SelectionSet?.Selections.OfType<GraphQLField>().Count(x => x.Name == "edges" || x.Name == "items") > 1)
            throw new ExecutionError("Cannot select both edges and items");
        List<KeyValuePair<string, Expression<Func<TObjectType, object>>>> dic;
        if (FindSubField<TObjectType>(context, "items", out var subContextItems)) {
            dic = CreateSelectArgumentDictionary<TObjectType>(subContextItems);
        } else if (FindSubField<TObjectType>(context, "edges", out var subContextEdges) && FindSubField<TObjectType>(subContextEdges, "node", out var subContextNode)) {
            dic = CreateSelectArgumentDictionary<TObjectType>(subContextNode);
        } else {
            dic = new List<KeyValuePair<string, Expression<Func<TObjectType, object>>>>();
        }
        var cursorExpression = resolver.GetCursorExpression(context);
        if (cursorExpression != null)
            dic.Add(new KeyValuePair<string, Expression<Func<TObjectType, object>>>("__EF_Cursor", cursorExpression));
        return LambdaBuilder.Build(dic);
    }

    /// <summary>
    /// Finds a sub-field within the current field context by name and creates a context for it.
    /// </summary>
    protected virtual bool FindSubField<TObjectType>(IResolveEfFieldContext<TDbContext> context, string name, out IResolveEfFieldContext<TDbContext, object?> subContext)
        where TObjectType : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        subContext = null!;
        var subField = context.FieldAst.SelectionSet?.Selections.OfType<GraphQLField>().Where(x => x.Name == name).SingleOrDefault();
        if (subField == null)
            return false;
        var complexGraph = (IComplexGraphType)context.FieldDefinition.ResolvedType!.GetNamedType()!;
        var subFieldType = complexGraph.Fields.Find(subField.Name.StringValue)
            ?? throw new InvalidOperationException($"Field '{name}' not found on '{context.FieldDefinition.ResolvedType}'.");

        //create a context which represents this child field
        subContext = CreateChildEfFieldContext(
            context,
            subField,
            subFieldType,
            complexGraph as IObjectGraphType ?? throw new NotSupportedException($"Parent type '{complexGraph}' must be an IObjectGraphType"), //null if cannot cast
            context.ExecutionContext.GetArguments(subFieldType, subField)!,
            context.Path!.Concat(new object[] { subField.Name }));

        return true;
    }

    /// <summary>
    /// Creates an expression to retrieve a subfield from the specified context based on the provided metadata and resolver function.
    /// Recursively applies <see cref="CreateSelectQuery{TParentType}(Type, IResolveEfFieldContext{TDbContext, object}, LambdaExpression)"/>
    /// and <see cref="CreateObject{TParentType}(Type, IResolveEfFieldContext{TDbContext, object}, LambdaExpression)"/> to build the expression
    /// for fields that are navigational properties.
    /// </summary>
    internal Expression<Func<TObjectType, object>> CreateExpressionForSubField<TObjectType>(
        IResolveEfFieldContext<TDbContext, object?> subContext,
        Func<IResolveEfFieldContext<TDbContext, object?>, LambdaExpression> resolveExpressionResolver,
        EfMetadata efMetadata)
    {
        if (subContext == null)
            throw new ArgumentNullException(nameof(subContext));
        if (resolveExpressionResolver == null)
            throw new ArgumentNullException(nameof(resolveExpressionResolver));
        if (efMetadata == null)
            throw new ArgumentNullException(nameof(efMetadata));
        if (efMetadata.Type == null)
            throw new ArgumentNullException($"{nameof(efMetadata)}.{nameof(EfMetadata.Type)}");

        //run the subField resolve function to get the lambda expression
        //note that this does not currently support async functions; although it is possible, it
        //  is designed this way because it should be a simple function returning a lambda
        //  expression based on the arguments; nothing more
        var subFieldExpression = resolveExpressionResolver(subContext);

        if (efMetadata.Query) {
            //pass the resolved lambda expression on to CreateSelectQuery to append on the proper select
            //  expression based on the selected child fields
            // -- e.g. subFieldExpression = (category) => category.Products
            subFieldExpression = CreateSelectQuery<TObjectType>(efMetadata.Type, subContext, subFieldExpression);
            // -- e.g. subFieldExpression = (category) => (object)category.Products.Select(product => new Dictionary<string, object>() { { "Id", product.Category.Id } })

            if (efMetadata.Single) {
                subFieldExpression = FirstOrDefault(subFieldExpression);
            }
        } else if (efMetadata.Graph) {
            // first, check if the subFieldExpression ends with a FirstOrDefault call
            if (subFieldExpression.EndsWithFirstOrDefault(out var subFieldExpression3)) {
                // -- e.g. subFieldExpression = (category) => category.Products.FirstOrDefault(x => x.IsBest)
                //remove the FirstOrDefault call or replace it with Where
                // -- e.g. subFieldExpression3 = (category) => category.Products.Where(x => x.IsBest)
                //pass the resolved lambda expression on to CreateSelectQuery to append on the proper select
                subFieldExpression = CreateSelectQuery<TObjectType>(efMetadata.Type!, subContext, subFieldExpression3!);
                // -- e.g. subFieldExpression = (category) => (object)category.Products.Where(x => x.IsBest).Select(product => new Dictionary<string, object>() { { "Id", product.Category.Id } })
                subFieldExpression = FirstOrDefault(subFieldExpression);
                // -- e.g. subFieldExpression = (category) => (object)category.Products.Where(x => x.IsBest).Select(product => new Dictionary<string, object>() { { "Id", product.Category.Id } }).FirstOrDefault()
            } else {
                //pass the resolved lambda expression on to CreateObject to select the proper sub fields
                // -- e.g. subFieldExpression = (product) => product.Category
                subFieldExpression = CreateObject<TObjectType>(efMetadata.Type!, subContext, subFieldExpression);
                // -- e.g. subFieldExpression = (product) => product.Category == null ? null : new Dictionary<string, object>() { { "Id", product.Category.Id } }
            }
        } else {
            // -- e.g. subFieldExpression = (product) => product.Id
        }

        //cast resulting expression to object type
        Expression<Func<TObjectType, object>> subFieldExpressionCast;
        if (subFieldExpression is Expression<Func<TObjectType, object>> subFieldExpression2) {
            //just cast the LambdaExpression appropriately
            subFieldExpressionCast = subFieldExpression2;
        } else if (subFieldExpression.ReturnType == typeof(object)) {
            //although it does return an object, it cannot be cast, so build lambda again as the correct type, using the original body and parameters
            subFieldExpressionCast = Expression.Lambda<Func<TObjectType, object>>(subFieldExpression.Body, subFieldExpression.Parameters);
        } else {
            //needs a conversion to object type
            var cast = Expression.Convert(subFieldExpression.Body, typeof(object));
            subFieldExpressionCast = Expression.Lambda<Func<TObjectType, object>>(cast, subFieldExpression.Parameters);
        }

        // -- e.g. subFieldExpressionCast = (product) => (object)product.Id
        return subFieldExpressionCast;
    }

    /// <summary>
    /// Creates a child field context for resolving nested GraphQL fields.
    /// </summary>
    protected virtual IResolveEfFieldContext<TDbContext, object?> CreateChildEfFieldContext(
        IResolveEfFieldContext<TDbContext> context,
        GraphQLField field,
        FieldType type,
        IObjectGraphType parentType,
        IDictionary<string, ArgumentValue> arguments,
        IEnumerable<object> path)
    {
        return new ResolveEfChildContext<TDbContext>(context, field, type, parentType, arguments, path);

    }

    /// <summary>
    /// Attempts to retrieve an object graph type from the provided graph type, handling union and interface types.
    /// </summary>
    protected virtual bool TryGetObjectGraphType<TObjectType>(IGraphType graphType, out IObjectGraphType complexGraph)
        where TObjectType : class
    {
        // strips non-null and list wrappers from the graph type
        graphType = graphType.GetNamedType();
        if (graphType is IAbstractGraphType abstractGraphType) // union or interface
        {
            foreach (var memberType in abstractGraphType.PossibleTypes) {
                if (memberType is ObjectGraphType<EfSource<TObjectType>>) {
                    complexGraph = memberType;
                    return true;
                }
            }
        }
        complexGraph = (graphType as ObjectGraphType<EfSource<TObjectType>>)!;
        return complexGraph != null;
    }

}
