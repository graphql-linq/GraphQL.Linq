// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Security.Claims;
using GraphQL.Execution;
using GraphQL.Instrumentation;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser.AST;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq;

/// <inheritdoc cref="IResolveEfFieldContext{TDbContext, TSource}"/>
public class ResolveEfFieldContext<TDbContext, TSource> : IResolveEfFieldContext<TDbContext, TSource>
{
    /// <summary>
    /// Base context from which this context was constructed.
    /// </summary>
    protected IResolveFieldContext BaseContext { get; }

    /// <summary>
    /// Initializes a new instance based on a specified base field context, database context, and <see cref="IEfGraphQLService{TDbContext}"/> instance.
    /// </summary>
    public ResolveEfFieldContext(IResolveFieldContext baseContext, IEfGraphQLService<TDbContext> efGraphQLService)
    {
        EfGraphQLService = efGraphQLService ?? throw new ArgumentNullException(nameof(efGraphQLService));
        BaseContext = baseContext ?? throw new ArgumentNullException(nameof(baseContext));
        Source = baseContext is IResolveFieldContext<TSource> baseContextTyped ? baseContextTyped.Source : (TSource)baseContext.Source!;
    }

    /// <inheritdoc/>
    public TDbContext DbContext => field ??= (TDbContext)BaseContext.RequestServices!.GetRequiredService(typeof(TDbContext));

    /// <inheritdoc/>
    public IEfGraphQLService<TDbContext> EfGraphQLService { get; }

    /// <inheritdoc cref="IResolveFieldContext{TSource}.Source"/>
    public TSource Source { get; }

    /// <inheritdoc cref="IResolveFieldContext.FieldAst"/>
    public GraphQLField FieldAst => BaseContext.FieldAst;

    /// <inheritdoc cref="IResolveFieldContext.FieldDefinition"/>
    public FieldType FieldDefinition => BaseContext.FieldDefinition;

    /// <inheritdoc cref="IResolveFieldContext.ParentType"/>
    public IObjectGraphType ParentType => BaseContext.ParentType;

    /// <inheritdoc cref="IResolveFieldContext.Arguments"/>
    public IDictionary<string, ArgumentValue>? Arguments => BaseContext.Arguments;

    /// <inheritdoc cref="IResolveFieldContext.RootValue"/>
    public object? RootValue => BaseContext.RootValue;

    /// <inheritdoc cref="IResolveFieldContext.Source"/>
    object? IResolveFieldContext.Source => BaseContext.Source;

    /// <inheritdoc cref="IResolveFieldContext.Schema"/>
    public ISchema Schema => BaseContext.Schema;

    /// <inheritdoc cref="IResolveFieldContext.Document"/>
    public GraphQLDocument Document => BaseContext.Document;

    /// <inheritdoc cref="IResolveFieldContext.Operation"/>
    public GraphQLOperationDefinition Operation => BaseContext.Operation;

    /// <inheritdoc cref="IResolveFieldContext.Variables"/>
    public Variables Variables => BaseContext.Variables;

    /// <inheritdoc cref="IResolveFieldContext.CancellationToken"/>
    public CancellationToken CancellationToken => BaseContext.CancellationToken;

    /// <inheritdoc cref="IResolveFieldContext.Metrics"/>
    public Metrics Metrics => BaseContext.Metrics;

    /// <inheritdoc cref="IResolveFieldContext.Errors"/>
    public ExecutionErrors Errors => BaseContext.Errors;

    /// <inheritdoc cref="IResolveFieldContext.Path"/>
    public IEnumerable<object> Path => BaseContext.Path;

    /// <inheritdoc cref="IResolveFieldContext.InputExtensions"/>
    public IReadOnlyDictionary<string, object?> InputExtensions => BaseContext.InputExtensions;

    /// <inheritdoc cref="IResolveFieldContext.OutputExtensions"/>
    public IDictionary<string, object?> OutputExtensions => BaseContext.OutputExtensions;

    /// <inheritdoc cref="IProvideUserContext.UserContext"/>
    public IDictionary<string, object?> UserContext => BaseContext.UserContext;

    /// <inheritdoc cref="IResolveFieldContext.ResponsePath"/>
    public IEnumerable<object> ResponsePath => BaseContext.ResponsePath;

    /// <inheritdoc cref="IResolveFieldContext.RequestServices"/>
    public IServiceProvider? RequestServices => BaseContext.RequestServices;

    /// <inheritdoc cref="IResolveFieldContext.Parent"/>
    public IResolveFieldContext? Parent => BaseContext.Parent;

    /// <inheritdoc cref="IResolveFieldContext.SubFields"/>
    public Dictionary<string, (GraphQLField Field, FieldType FieldType)>? SubFields => BaseContext.SubFields;

    /// <inheritdoc cref="IResolveFieldContext.ArrayPool"/>
    public IExecutionArrayPool ArrayPool => BaseContext.ArrayPool;

    /// <inheritdoc cref="IResolveFieldContext.Directives"/>
    public IDictionary<string, DirectiveInfo> Directives => throw new NotImplementedException();

    /// <inheritdoc cref="IResolveFieldContext.User"/>
    public ClaimsPrincipal? User => BaseContext.User;

    /// <inheritdoc cref="IResolveFieldContext.ExecutionContext"/>
    public IExecutionContext ExecutionContext => BaseContext.ExecutionContext;
}
