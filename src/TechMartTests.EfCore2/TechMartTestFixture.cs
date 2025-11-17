// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Data.SqlClient;
using System.Net.Http;
using DbHelpers;
using GraphQL;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TechMart.EfCore2;

namespace TechMartTests;

public class TechMartTestFixture : IDisposable
{
    private readonly TestServer _server;
    public IDbCreator DbCreator { get; }
    public HttpClient Client { get; }
    public IServiceProvider Services => _server.Host.Services;

    public TechMartTestFixture()
    {
        // Create a single shared DbCreator instance for all tests
        DbCreator = new DbCreator<SqlConnection>("TechMartEfCore2Tests");

        // Create test server with custom service configuration
        _server = new TestServer(Program.CreateWebHostBuilder([])
            .ConfigureTestServices(services => {
                // Override the IDbCreator registration with our shared instance
                services.AddSingleton<IDbCreator>(DbCreator);
                services.AddGraphQL(b => b.AddSerializer(new GraphQL.SystemTextJson.GraphQLSerializer(options => {
                    options.Converters.Add(new TrimmedDecimalConverter());
                    options.Converters.Add(new TrimmedNullableDecimalConverter());
                })));
            }));

        Client = _server.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
        _server?.Dispose();
        DbCreator?.Dispose();
        GC.SuppressFinalize(this);
    }
}
