// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Runtime.InteropServices;
using GraphQL.DI;
using GraphQL.Execution;
using Microsoft.EntityFrameworkCore;
using TestDb;

namespace Tests;

public class QueryTestBase
{
    public Type? QueryType { get; set; }
    public List<Type> MappedGraphTypes { get; } = new();
    public Action<IGraphQLBuilder>? ConfigureGraphQLBuilder { get; set; }
    public Action<IServiceCollection>? ConfigureServices { get; set; }
    public bool ExpectFailure { get; set; }

    private static readonly Type[] _typesToRegister =
    [
        typeof(ObjectGraphType<>),
        typeof(InputObjectGraphType<>),
        typeof(EnumerationGraphType<>),
    ];

    public async Task<string> ExecuteQueryAsync(string query)
    {
        await using var serviceProvider = await GetServiceProviderAsync();

        var executer = serviceProvider.GetRequiredService<IDocumentExecuter<ISchema>>();
        var result = await executer.ExecuteAsync(options => {
            options.Query = query;
            options.RequestServices = serviceProvider;
            options.ThrowOnUnhandledException = !ExpectFailure;
        });

        if (!ExpectFailure)
            Assert.AreEqual(0, result.Errors?.Count ?? 0, "GraphQL query execution had error: " + result.Errors?.FirstOrDefault()?.Message);
        else
            Assert.AreNotEqual(0, result.Errors?.Count ?? 0, "Expected GraphQL query execution to have errors, but none were found.");
        var serializer = serviceProvider.GetRequiredService<IGraphQLTextSerializer>();
        var json = serializer.Serialize(result);
        return json;
    }

    public async Task<string> GetSchemaSDLAsync()
    {
        await using var serviceProvider = await GetServiceProviderAsync();
        var schema = serviceProvider.GetRequiredService<ISchema>();
        return schema.Print(new GraphQL.Utilities.PrintOptions {
            StringComparison = StringComparison.OrdinalIgnoreCase,
        });
    }

    private async Task<ServiceProvider> GetServiceProviderAsync()
    {
        if (QueryType == null)
            throw new InvalidOperationException("QueryType must be set before executing a query.");
        if (!typeof(IObjectGraphType).IsAssignableFrom(QueryType))
            throw new InvalidOperationException("QueryType must implement IObjectGraphType.");
        if (QueryType.IsAbstract || QueryType.IsInterface)
            throw new InvalidOperationException("QueryType cannot be an abstract class or interface.");

        var services = new ServiceCollection();
        services.AddGraphQL(b => {
            b
                .AddSelfActivatingSchema<MySchema>()
                .ConfigureSchema((s, provider) => s.Query = (IObjectGraphType)ActivatorUtilities.GetServiceOrCreateInstance(provider, QueryType))
                .AddSystemTextJson(o => o.WriteIndented = true)
                .AddLinq<SampleDbContext>()
                .AddDI()
                .AddAutoClrMappings(true, false)
                .AddDataLoader()
                .AddExecutionStrategy<SerialExecutionStrategy>(GraphQLParser.AST.OperationType.Query)
                .AddUnhandledExceptionHandler(context => context.ErrorMessage = context.OriginalException.Message);

            b.ConfigureSchema(s => {
                //loop through each specified type
                foreach (var graphType in MappedGraphTypes) {
                    //skip types that are not graph types, or are abstract types/interfaces
                    if (!typeof(IGraphType).IsAssignableFrom(graphType) || graphType.IsAbstract || graphType.IsInterface)
                        continue;

                    //start with the base type
                    var baseType = graphType.BaseType;
                    while (baseType != null) {
                        //look for generic types that match our list above
                        if (baseType.IsConstructedGenericType && _typesToRegister.Contains(baseType.GetGenericTypeDefinition())) {
                            //get the base type
                            var clrType = baseType.GetGenericArguments()[0];

                            //as long as it's not of type 'object', register it
                            if (clrType != typeof(object))
                                s.RegisterTypeMapping(clrType, graphType);

                            //skip to the next type
                            break;
                        }

                        //look up the inheritance chain for a match
                        baseType = baseType.BaseType;
                    }
                }
            });

            ConfigureGraphQLBuilder?.Invoke(b);
        });
        ConfigureServices?.Invoke(services);
        services.AddDbContext<SampleDbContext>(o => o.UseSqlServer(SampleDbContext.GetConnectionString()));
        var serviceProvider = services.BuildServiceProvider();

        try {
            await EnsureDatabaseInitializedAsync(serviceProvider);
        } catch {
            serviceProvider.Dispose();
            throw;
        }

        return serviceProvider;
    }

    private static readonly SemaphoreSlim _dbInitLock = new(1, 1);
    private static bool _dbIsInitialized;

    private static async Task EnsureDatabaseInitializedAsync(IServiceProvider serviceProvider)
    {
        // Fast path: if already initialized, return immediately
        if (_dbIsInitialized)
            return;

        // Acquire lock to ensure only one thread initializes
        await _dbInitLock.WaitAsync();
        try {
            // Double-check after acquiring lock (another thread may have initialized while we waited)
            if (_dbIsInitialized)
                return;

            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            await db.Database.EnsureCreatedAsync();

            _dbIsInitialized = true;
        } finally {
            _dbInitLock.Release();
        }
    }

    private sealed class MySchema(IServiceProvider provider) : Schema(provider)
    {
    }
}
