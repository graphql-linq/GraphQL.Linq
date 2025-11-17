// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Data.SqlClient;
using DbHelpers;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Linq.EntityFrameworkCore2;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using TechMart.Graphs;

namespace TechMart.EfCore2;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register DbCreator as a singleton
        services.AddSingleton<IDbCreator, DbCreator<SqlConnection>>();

        // Configure DbContext with a factory that uses the DbCreator
        services.AddDbContext<TechMartDbContext>((serviceProvider, options) => {
            var dbCreator = serviceProvider.GetRequiredService<IDbCreator>();
            options.UseSqlServer(dbCreator.ConnectionString);
        });

        services.AddGraphQL(b => b
            .AddSchema<TechMartSchema>()
            .AddSystemTextJson()
            .AddGraphTypes()
            .AddClrTypeMappings()
            .AddDI()
            .AddAutoClrMappings(true, false)
            .AddLinq<TechMartDbContext>()
            .AddDataLoader()
            .AddExecutionStrategy<SerialExecutionStrategy>(GraphQLParser.AST.OperationType.Query)
            .AddScopedSubscriptionExecutionStrategy()
            .AddErrorInfoProvider(options => options.ExposeExceptionDetails = true)
        );

        services.AddHostApplicationLifetime();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }

        // Initialize the database
        using (var scope = app.ApplicationServices.CreateScope()) {
            // Ensure database is created and seeded
            var context = scope.ServiceProvider.GetRequiredService<TechMartDbContext>();
            context.Database.EnsureCreated();

            // Ensure schema is initialized
            var schema = scope.ServiceProvider.GetRequiredService<ISchema>();
            schema.Initialize();
        }

        app.UseWebSockets();
        app.UseGraphQL("/", o => o.HandleGet = false);
        app.Map("/sdl", app => app.Use(next => context => {
            if (context.Request.Method != "GET" || context.WebSockets.IsWebSocketRequest)
                return next(context);
            var schema = context.RequestServices.GetRequiredService<ISchema>();
            var sdl = schema.Print(new() { StringComparison = StringComparison.InvariantCultureIgnoreCase });
            context.Response.ContentType = "text/text";
            context.Response.Headers.Add("Content-Disposition", "inline; filename=\"techmart_schema.graphql\"");
            return context.Response.WriteAsync(sdl, context.RequestAborted);
        }));

        app.UseGraphQLGraphiQL("/", new GraphQL.Server.Ui.GraphiQL.GraphiQLOptions {
            GraphQLEndPoint = "/",
        });
    }
}
