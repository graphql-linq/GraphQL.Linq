// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using DbHelpers;
using GraphQL;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace TechMartTests;

public class TechMartTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    public IDbCreator DbCreator { get; }
    public HttpClient Client { get; }
    public IServiceProvider Services => _factory.Services;

    public TechMartTestFixture()
    {
        // Create a single shared DbCreator instance for all tests
#if EFCORE3
        DbCreator = new DbCreator<SqlConnection>("TechMartEfCore3Tests");
#elif EFCORE6
        DbCreator = new DbCreator<SqlConnection>("TechMartEfCore6Tests");
#elif EFCORE8
        DbCreator = new DbCreator<SqlConnection>("TechMartEfCore8Tests");
#elif EFCORE10
        DbCreator = new DbCreator<SqlConnection>("TechMartEfCore10Tests");
#elif LINQTODB5
        DbCreator = new DbCreator<SqlConnection>("TechMartLinqToDb5Tests");
#elif LINQTODB6
        DbCreator = new DbCreator<SqlConnection>("TechMartLinqToDb6Tests");
#endif

        // Create test server with custom service configuration
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => {
                builder.ConfigureServices(services => {
                    // Override the IDbCreator registration with our shared instance
                    services.AddSingleton<IDbCreator>(DbCreator);
                    services.AddGraphQL(b => b.AddSerializer(new GraphQL.SystemTextJson.GraphQLSerializer(options => {
                        options.Converters.Add(new TrimmedDecimalConverter());
                        options.Converters.Add(new TrimmedNullableDecimalConverter());
                    })));
                });
            });

        Client = _factory.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
        _factory?.Dispose();
        DbCreator?.Dispose();
        GC.SuppressFinalize(this);
    }
}
