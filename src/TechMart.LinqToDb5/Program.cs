// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using DbHelpers;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types;
using LinqToDB.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TechMart.Graphs;

var builder = WebApplication.CreateBuilder(args);

// Register DbCreator as a singleton
builder.Services.AddSingleton<IDbCreator, DbCreator<SqlConnection>>();

// Configure DbContext with a factory that uses the DbCreator
LinqToDBForEFTools.Initialize();
builder.Services.AddDbContext<TechMartDb.TechMartDbContext>((serviceProvider, options) => {
    var dbCreator = serviceProvider.GetRequiredService<IDbCreator>();
    options.UseSqlServer(dbCreator.ConnectionString);
    options.UseLinqToDB();
});
builder.Services.AddScoped(provider => {
    var dbContext = provider.GetRequiredService<TechMartDb.TechMartDbContext>();
    return dbContext.CreateLinqToDBContext();
});

builder.Services.AddGraphQL(b => b
    .AddSchema<TechMartSchema>()
    .AddSystemTextJson()
    .AddGraphTypes()
    .AddClrTypeMappings()
    .AddDI()
    .AddAutoClrMappings(true, false)
    .AddLinq<TechMartDbContext>(o => o.UseStringSplit = true)
    .AddDataLoader()
    .AddExecutionStrategy<SerialExecutionStrategy>(GraphQLParser.AST.OperationType.Query)
    .AddScopedSubscriptionExecutionStrategy()
    .AddErrorInfoProvider(options => options.ExposeExceptionDetails = true)
);

var app = builder.Build();

var env = app.Environment;
if (env.EnvironmentName == "Development") {
    app.UseDeveloperExceptionPage();
}

// Initialize the database
using (var scope = app.Services.CreateScope()) {
    // Ensure database is created and seeded
    var context = scope.ServiceProvider.GetRequiredService<TechMartDb.TechMartDbContext>();
    context.Database.EnsureCreated();

    // Ensure schema is initialized
    var schema = scope.ServiceProvider.GetRequiredService<ISchema>();
    schema.Initialize();
}

app.UseWebSockets();
app.UseGraphQL("/", o => o.HandleGet = false);
app.MapGet("/sdl", (HttpContext context) => {
    var schema = context.RequestServices.GetRequiredService<ISchema>();
    var sdl = schema.Print(new() { StringComparison = StringComparison.InvariantCultureIgnoreCase });
    context.Response.Headers["Content-Disposition"] = "inline; filename=\"techmart_schema.graphql\"";
    return Results.Text(sdl, "text/text", System.Text.Encoding.UTF8);
});

app.UseGraphQLGraphiQL("/", new GraphQL.Server.Ui.GraphiQL.GraphiQLOptions {
    GraphQLEndPoint = "/",
});

app.Run();

// Make the Program class accessible for WebApplicationFactory
public partial class Program { }
