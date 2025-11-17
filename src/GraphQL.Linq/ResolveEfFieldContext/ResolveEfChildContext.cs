// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Security.Claims;
using GraphQL.Execution;
using GraphQL.Instrumentation;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser.AST;

namespace GraphQL.Linq;

internal class ResolveEfChildContext<TDbContext> : IResolveEfFieldContext<TDbContext, object?>
{
    private readonly IResolveEfFieldContext<TDbContext> _rootContext;

    public ResolveEfChildContext(
        IResolveEfFieldContext<TDbContext> context,
        GraphQLField field,
        FieldType fieldDefinition,
        IObjectGraphType parentType,
        IDictionary<string, ArgumentValue> arguments,
        IEnumerable<object> path)
    {
        _rootContext = context ?? throw new ArgumentNullException(nameof(context));
        FieldAst = field;
        FieldDefinition = fieldDefinition;
        ParentType = parentType;
        Arguments = arguments;
        Path = path;
    }

    public object? Source => null;

    public TDbContext DbContext => _rootContext.DbContext;

    public IEfGraphQLService<TDbContext> EfGraphQLService => _rootContext.EfGraphQLService;

    public GraphQLField FieldAst { get; }

    public FieldType FieldDefinition { get; }

    public IGraphType? ReturnType => FieldDefinition.ResolvedType;

    public IObjectGraphType ParentType { get; }

    public IDictionary<string, ArgumentValue> Arguments { get; }

    public object? RootValue => _rootContext.RootValue;

    public ISchema Schema => _rootContext.Schema;

    public GraphQLDocument Document => _rootContext.Document;

    public GraphQLOperationDefinition Operation => _rootContext.Operation;

    public Variables Variables => _rootContext.Variables;

    public CancellationToken CancellationToken => _rootContext.CancellationToken;

    public Metrics Metrics => _rootContext.Metrics;

    public ExecutionErrors Errors => _rootContext.Errors;

    public IEnumerable<object> Path { get; }

    public IDictionary<string, object?> OutputExtensions => throw new NotImplementedException();

    public Dictionary<string, (GraphQLField Field, FieldType FieldType)> SubFields => throw new NotImplementedException();

    public IDictionary<string, object?> UserContext => _rootContext.UserContext;

    public IEnumerable<object> ResponsePath => throw new NotImplementedException();

    public IServiceProvider? RequestServices => _rootContext.RequestServices;

    public IResolveFieldContext Parent => throw new NotImplementedException();

    public IExecutionArrayPool ArrayPool => throw new NotImplementedException();

    public IDictionary<string, DirectiveInfo> Directives => throw new NotImplementedException();

    public IReadOnlyDictionary<string, object?> InputExtensions => _rootContext.InputExtensions;

    public ClaimsPrincipal? User => _rootContext.User;

    public IExecutionContext ExecutionContext => _rootContext.ExecutionContext;
}
