# GraphQL.Linq

[![License](https://img.shields.io/badge/license-Commercial%20%2B%20Free%20Tier-blueviolet)](LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq)](https://www.nuget.org/packages/GraphQL.Linq/)
[![Nuget](https://img.shields.io/nuget/dt/GraphQL.Linq)](https://www.nuget.org/packages/GraphQL.Linq)
[![Coverage](https://codecov.io/gh/graphql-linq/GraphQL.Linq/branch/master/graph/badge.svg)](https://codecov.io/gh/graphql-linq/GraphQL.Linq)

Seamlessly integrate [GraphQL.NET](https://github.com/graphql-dotnet/graphql-dotnet) with [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) or [LinqToDB](https://linq2db.github.io/) by translating GraphQL queries directly into optimized LINQ expressions for efficient database access.

## ✨ Features

- **Direct LINQ Translation**: Translates GraphQL queries directly into LINQ expressions for your ORM, which then converts them to optimized SQL queries. This enables faster data retrieval and efficient use of database indexes.
- **Pagination Support**: Relay-style cursor-based pagination with connection types
- **Data Loader Integration**: Automatic batching and caching to prevent N+1 query problems
- **Navigation Properties**: Seamless handling of entity relationships and nested queries
- **Type Safety**: Strongly-typed graph definitions with compile-time checking
- **Multiple Database Support**: Works with [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) and [LinqToDB](https://linq2db.github.io/)

## 📦 Installation

The base package [`GraphQL.Linq`](https://www.nuget.org/packages/GraphQL.Linq/) provides core functionality for integrating GraphQL with IQueryable data sources. Additional packages are required depending on your ORM:

| Package | ORM Support | NuGet |
|---------|-------------|-------|
| [`GraphQL.Linq`](https://www.nuget.org/packages/GraphQL.Linq/) | Base package (required) | [![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq)](https://www.nuget.org/packages/GraphQL.Linq/) |
| [`GraphQL.Linq.EntityFrameworkCore2`](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore2/) | Entity Framework Core 2.x | [![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq.EntityFrameworkCore2)](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore2/) |
| [`GraphQL.Linq.EntityFrameworkCore3`](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore3/) | Entity Framework Core 3.x | [![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq.EntityFrameworkCore3)](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore3/) |
| [`GraphQL.Linq.EntityFrameworkCore6`](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore6/) | Entity Framework Core 6.x | [![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq.EntityFrameworkCore6)](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore6/) |
| [`GraphQL.Linq.EntityFrameworkCore8`](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore8/) | Entity Framework Core 8.x | [![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq.EntityFrameworkCore8)](https://www.nuget.org/packages/GraphQL.Linq.EntityFrameworkCore8/) |
| [`GraphQL.Linq.LinqToDb`](https://www.nuget.org/packages/GraphQL.Linq.LinqToDb/) | LinqToDB 3.1.2 - 5.x | [![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq.LinqToDb)](https://www.nuget.org/packages/GraphQL.Linq.LinqToDb/) |
| [`GraphQL.Linq.LinqToDb6`](https://www.nuget.org/packages/GraphQL.Linq.LinqToDb6/) | LinqToDB 6.x | [![NuGet](https://img.shields.io/nuget/v/GraphQL.Linq.LinqToDb6)](https://www.nuget.org/packages/GraphQL.Linq.LinqToDb6/) |

## 📋 Prerequisites

Before using GraphQL.Linq, ensure you have:

- **[GraphQL.NET](https://www.nuget.org/packages/GraphQL/) 8.7.0+** - The underlying GraphQL server library ([documentation](https://graphql-dotnet.github.io/))
- **[Entity Framework Core](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/)** or **[LinqToDB](https://www.nuget.org/packages/linq2db/)** - Already configured in your application with a working DbContext
- **Basic GraphQL Knowledge** - Familiarity with GraphQL concepts (queries, mutations, types, fields)
- **LINQ Experience** - Understanding of LINQ expressions and IQueryable

### Recommended Packages

While not strictly required, these packages significantly simplify setup:

- [`GraphQL.Server.Transports.AspNetCore`](https://www.nuget.org/packages/GraphQL.Server.Transports.AspNetCore/) - ASP.NET Core integration for GraphQL
- [`GraphQL.SystemTextJson`](https://www.nuget.org/packages/GraphQL.SystemTextJson/) - JSON serialization support
- [`Shane32.GraphQL.DI`](https://github.com/Shane32/GraphQL.DI) - Simplified dependency injection and type-first schema support
- [`AutoMapper`](https://www.nuget.org/packages/AutoMapper/) - Object-to-object mapping for input models (used in mutation examples)

## 🧠 Core Concepts

Understanding these core concepts will help you work effectively with GraphQL.Linq:

### `EfSource<T>`

`EfSource<T>` is a dictionary-like container that holds the field values returned from a database query. It serves as a bridge between your entity classes and GraphQL types, enabling GraphQL.NET to map CLR types to graph types. When a query method returns `EfSource<Person>`, it contains only the fields that were selected by the GraphQL query, making data transfer efficient.

### Graph Types vs Entity Classes

GraphQL.Linq uses a mapping approach:

- **Entity Classes** - Your database models (e.g., `Person`, `Order`)
- **Graph Types** - GraphQL type definitions that expose your entities (e.g., `PersonGraph`, `OrderGraph`)

Graph types inherit from `EfObjectGraphType<TDbContext, TEntity>` and define which fields are exposed to GraphQL clients and how they're resolved. This separation allows you to:

- Expose only certain fields from your entities
- Add computed fields that don't exist in the database
- Customize field names and types for the GraphQL schema
- Control access and authorization per field

### Query Translation

GraphQL.Linq translates GraphQL queries into LINQ expressions, which your ORM (Entity Framework Core or LinqToDB) then converts to SQL:

```text
GraphQL Query → LINQ Expression → SQL Query → Database
```

This translation happens automatically for anything that can be represented by LINQ including:

- Field selections (SELECT clauses)
- Filtering (WHERE clauses)
- Sorting (ORDER BY clauses)
- Pagination (LIMIT/OFFSET)
- Navigation properties (JOINs)

### Data Loaders and N+1 Prevention

Data loaders batch database queries to prevent the N+1 query problem. When you use methods like `DelayLoadList` or `DelayLoadEntry`, GraphQL.Linq:

1. Collects all requests for related entities across multiple parent objects
2. Batches them into a single database query

For example, if you query 10 people and each person has a company, instead of making 11 queries (1 for people + 10 for companies), GraphQL.Linq makes just 2 queries (1 for people + 1 batched query for all companies).

## ⚡ Quick Start

Here's a minimal example to get you started quickly:

```csharp
// 1. Entity class (your database model)
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// 2. Graph type (exposes entity to GraphQL)
public class PersonGraph : EfObjectGraphType<MyDbContext, Person>
{
    public PersonGraph()
    {
        EfIdField(x => x.Id);
        EfField(x => x.Name);
    }
}

// 3. Query class (defines available queries)
public class Query : DIObjectGraphBase
{
    private readonly MyDbContext _dbContext;

    public MyQuery(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EfSource<Person>> PersonAsync([Id] int id)
        => _dbContext.People.ToGraphSingleAsync(x => x.Id == id);;
}

// 4. Schema class
public sealed class MySchema : Schema
{
    public MySchema(IServiceProvider services) : base(services)
    {
        Query = services.GetRequiredService<DIObjectGraphType<Query>>();
    }
}

// 5. Register services in Program.cs
services.AddDbContext<MyDbContext>(options => options.UseSqlServer(connectionString));
services.AddGraphQL(b => b
    .AddSchema<MySchema>()
    .AddSystemTextJson()
    .AddGraphTypes()
    .AddClrTypeMappings()
    .AddDI()
    .AddLinq<MyDbContext>()
    .AddExecutionStrategy<SerialExecutionStrategy>(GraphQLParser.AST.OperationType.Query)
);
```

**GraphQL Query:**

```graphql
query {
  person(id: 1) {
    id
    name
  }
}
```

**Response:**

```json
{
  "data": {
    "person": {
      "id": "1",
      "name": "John Doe"
    }
  }
}
```

For a complete setup guide, continue to the [Getting Started](#-getting-started) section below.

## 🚀 Getting Started

This guide assumes you have already configured Entity Framework Core or LinqToDB in your application following their standard setup procedures. It also assumes the use of the [`Shane32.GraphQL.DI`](https://github.com/Shane32/GraphQL.DI) package, which is recommended but not required for simplified dependency injection configuration.

### Configure Services

First, ensure your DbContext is registered as a scoped service in your dependency injection container. Then register GraphQL.Linq with your dependency injection container using the `AddLinq<TDbContext>` method, which requires your DbContext type (must inherit from `DbContext` for Entity Framework Core or `IDataContext` for LinqToDB).

**Important:** GraphQL.Linq requires `SerialExecutionStrategy` for queries because it uses scoped services (like DbContext) that are not thread-safe. This ensures all field resolvers execute sequentially rather than in parallel.

```csharp
// Register your DbContext as a scoped service
services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure GraphQL with GraphQL.Linq
services.AddGraphQL(b => b
    // Configure user schema
    .AddSchema<MySchema>()
    // Configure GraphQL serializer
    .AddSystemTextJson()
    // Recommended: Automatically register graph types from assembly
    .AddGraphTypes()
    // Required: Add CLR type mappings based on graph types from assembly
    .AddClrTypeMappings()
    // Recommended: Add DI support for type-first graphs via DIObjectGraphBase
    .AddDI()
    // Recommended: Add automatic input type mappings when no match found within assembly
    .AddAutoClrMappings(true, false)
    // Register GraphQL.Linq with your DbContext
    .AddLinq<MyDbContext>()
    // Required: Add data loader support for batching database queries
    .AddDataLoader()
    // Required: Configure serial execution strategy for queries
    // This prevents threading issues with scoped services like DbContext
    .AddExecutionStrategy<SerialExecutionStrategy>(GraphQLParser.AST.OperationType.Query)
    // Required if subscriptions are in use: Configure scoped serial execution strategy
    .AddScopedSubscriptionExecutionStrategy()

    // Recommended: Configure autorization, error info provider, and exception handler
    // Consider: Add document listener to wrap each GraphQL request within a transaction
);
```

**Note:** If you are using multiple database contexts in your application, call the non-generic `.AddLinq()` method instead of `.AddLinq<MyDbContext>()`.

### Configure Root Query Type

Create your schema, root query type, and mutation type using the DI type-first approach with `DIObjectGraphType` and `DIObjectGraphBase`.

**Note:** `DIObjectGraphBase` implements `IResolveFieldContext`, which provides access to GraphQL.Linq extension methods like `QuerySingleAsync` and `QueryConnectionAsync`. The `DIObjectGraphType<T>` wrapper exposes the methods from your class as GraphQL fields.

```csharp
public sealed class MySchema : Schema
{
    public MySchema(IServiceProvider services) : base(services)
    {
        Query = services.GetRequiredService<QueryType>();
        Mutation = services.GetRequiredService<MutationType>();  // optional; see below
    }
}

public sealed class QueryType : DIObjectGraphType<Query>
{
    // Optional: add constructor with traditional GraphQL.NET code-first
    //   initialization for legacy code
}

public sealed class Query : DIObjectGraphBase
{
    private readonly MyDbContext _db;

    public Query(MyDbContext db)
    {
        _db = db;
    }

    public string Test(string name) => $"Hello, {name}!";
}
```

### Add Query Methods

Add methods to your query class to expose data. These methods return `EfSource<T>` or `Connection<EfSource<T>>`, which contain the field values selected by the GraphQL query. Use type-first attributes on parameters for more control over the generated GraphQL parameter, such as `[Id]` for identifier parameters.

You can use additional type-first attributes declared on the method as needed, such as:

- `[Name("customName")]` - Change the field name in the GraphQL schema
- `[Scoped]` - Mark fields as scoped if the schema is configured for parallel execution
- `[Authorize("policyName")]` - Configure authorization policies
- And other attributes provided by GraphQL.NET for metadata, deprecation, descriptions, etc.

#### Single Entity Query

Query for a single entity by its primary key. The returned `EfSource<Person>` contains only the fields requested in the GraphQL query.

Use `ToGraphSingleAsync` when the entity must exist (returns a GraphQL execution error if not found), or `ToGraphSingleOrDefaultAsync` if `null` may be returned:

```csharp
// Query for a single person by ID - client error if not found
public Task<EfSource<Person>> PersonAsync([Id] int id)
    => _db.People.ToGraphSingleAsync(x => x.Id == id);

// Query for a single person by ID - returns null if not found
public Task<EfSource<Person>?> PersonAsync([Id] int id)
    => _db.People.ToGraphSingleOrDefaultAsync(x => x.Id == id);
```

#### Connection Query with Pagination

Query for a paginated collection with sorting using Relay-style cursor-based pagination. The `Connection<EfSource<Person>>` type provides some or all of the following, depending on the GraphQL request:

- `edges` - Array of edge objects containing `node` (the entity) and `cursor` (pagination cursor)
- `nodes` - Direct array of entities (shortcut for `edges[].node`)
- `pageInfo` - Pagination metadata (`hasNextPage`, `hasPreviousPage`, `startCursor`, `endCursor`)
- `totalCount` - Total number of items (optional, can be expensive for large datasets)

**Pagination Parameters:**

- `first` - Number of items to return from the start
- `last` - Number of items to return from the end
- `after` - Cursor to start after (for forward pagination)
- `before` - Cursor to end before (for backward pagination)

**Important:** Sorting must be deterministic (always produce the same order) to ensure proper cursor-based pagination. Always include a unique field like `Id` as a tiebreaker.

```csharp
// Query for a connection of people with pagination, sorting, and filtering
public Task<Connection<EfSource<Person>>> PeopleAsync(
    int? first,
    int? last,
    [Id] string? before,
    [Id] string? after,
    SortOrder sortOrder = SortOrder.Name,
    string? nameContains = null,
    bool? active = null)
{
    // Start with base query
    var query = _db.People.AsQueryable();
    
    // Apply optional filters
    if (!string.IsNullOrEmpty(nameContains))
        query = query.Where(x => x.Name.Contains(nameContains));
    
    if (active == true)
        query = query.Where(x => x.Active);
    else if (active == false)
        query = query.Where(x => !x.Active);
    
    // Apply deterministic sorting - always include a unique field (like Id) as a tiebreaker
    query = sortOrder switch {
        SortOrder.Name => query.OrderBy(x => x.Name).ThenBy(x => x.Id),
        SortOrder.Id => query.OrderBy(x => x.Id),
        _ => throw new ArgumentOutOfRangeException(nameof(sortOrder)),
    };
    
    // ToGraphConnectionAsync parameters: first, last, after, before, defaultPageSize
    return query.ToGraphConnectionAsync(first, last, after, before, defaultPageSize: 100);
}

public enum SortOrder
{
    Name,
    Id
}
```

### Add Mutations (Optional)

Mutations follow the same pattern as queries. Create a `MutationType` and `Mutation` class, then add methods that modify data and return the updated entity:

```csharp
public sealed class MutationType : DIObjectGraphType<Mutation> { }

public sealed class Mutation : DIObjectGraphBase
{
    private readonly MyDbContext _db;
    private readonly IMapper _mapper;

    public Mutation(MyDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [Authorize("AddPerson")]
    public async Task<EfSource<Person>> AddPersonAsync(
        PersonInputModel input,
        CancellationToken cancellationToken)
    {
        // Validate this is an add operation, not edit
        if (input.Id != 0)
            throw new ExecutionError("Cannot add an entity with a specified id");
        
        // Use AutoMapper to map the input model to the entity object
        var person = _mapper.Map<PersonInputModel, Person>(input);

        // Add the person and save to the database
        _db.Add(person);
        await _db.SaveChangesAsync(cancellationToken);

        // Return the newly created person using ToGraphSingleAsync
        return await _db.People.ToGraphSingleAsync(x => x.Id == person.Id);
    }
}

public class PersonInputModel
{
    [Id]
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Active { get; set; }
}
```

### Advanced: Without Shane32.GraphQL.DI

You can also define your Query or Mutation types without Shane32.GraphQL.DI with typical type-first or code-first coding patterns:

```csharp
// Type-first - via type AutoRegisteringObjectGraphType<Query>
public sealed class Query
{
    // Methods on root types must be static
    // Services must be injected through method arguments

    public static Task<EfSource<Person>> PersonAsync([FromServices] MyDbContext db, [Id] int id)
        => db.People.ToGraphSingleAsync(x => x.Id == id);

    public static Task<EfSource<Person>?> PersonAsync([FromServices] MyDbContext db, [Id] int id)
        => db.People.ToGraphSingleOrDefaultAsync(x => x.Id == id);
}

// Code-first - inherit from QueryGraphType
public sealed class Query : QueryGraphType<MyDbContext>
{
    public Query(IEfGraphQLService<MyDbContext> efGraphQLService) : base(efGraphQLService)
    {
        //use EfSingleField, EfQueryField, and EfQueryConnectionField to map root query functions
        EfSingleField("Person", context => context.DbContext.People, addIdArgument: true);
        EfQueryField("Roles", context => context.DbContext.Roles);
        EfQueryConnectionField("People", context => context.DbContext.People.Where(x => !x.Deleted));
    }
}
```

## 📚 Defining Graphs

### Basic Field Mapping

Create graph types for your entities using `EfObjectGraphType`. Use `EfIdField` to mark identifier fields with the ID GraphQL type. You can optionally specify a custom field name as the first parameter, and the nullable parameter indicates if the field is nullable (defaults to `false`):

```csharp
public class PersonGraph : EfObjectGraphType<MyDbContext, Person>
{
    public PersonGraph()
    {
        EfIdField(x => x.Id);
        EfField(x => x.Name);
        EfField("isActive", x => x.Active);  // Custom field name
        EfField(x => x.MiddleName, true);    // Nullable field
    }
}
```

### Computed Fields

You can use any valid LINQ expression in `EfField`. These expressions will be translated to SQL queries by your ORM:

```csharp
// Computed field - translates to SQL aggregate query
EfField("totalSales", x => x.Orders.Sum(o => o.Total));

// Conditional expression - translates to SQL CASE statement
EfField("status", x => x.Active ? "Active" : "Inactive");

// String concatenation - translates to SQL string operations
EfField("fullName", x => x.FirstName + " " + x.LastName);
```

### Client-Side Processing

For operations that cannot be translated to SQL, select the fields required, then use `ThenResolve` to perform client-side processing after the database query:

```csharp
// Simple client-side string concatenation
EfField("fullName", x => new { x.FirstName, x.LastName })
    .ThenResolve(val => val.FirstName + " " + val.LastName);

// Complex object transformation - decode bit flags into a structured object
EfField(x => x.FontStyle)
    .ThenResolve(fs => new FontStyle {
        Bold = (fs & 1) != 0,
        Italic = (fs & 2) != 0,
        Underline = (fs & 4) != 0,
        Strikeout = (fs & 8) != 0
    });
```

### Advanced: Context-Aware Fields

For advanced scenarios where you need access to GraphQL field arguments or other context information within your LINQ expression, use `EfFieldFromContext`. This method provides access to the full field resolution context, allowing you to build dynamic queries based on arguments:

```csharp
// Field that uses GraphQL arguments in the LINQ expression
EfFieldFromContext(
    "salesCount",
    context => {
        var status = context.GetArgument<SaleStatus>("status");
        return person => context.DbContext.Sales
            .Where(sale => sale.CustomerId == person.Id && sale.Status == status)
            .Count();
    })
    .Argument<SaleStatus>("status");
```

### Navigation Properties

Navigation properties allow you to expose related entities in your GraphQL schema. There are several approaches depending on your needs, as listed below.

### One-to-One or Many-to-One Navigation Properties

For one-to-one or many-to-one relationships, use `EfNavigationField`:

```csharp
// Direct navigation - loads the related entity directly within the same SQL query
EfNavigationField(x => x.Company);
```

Use `EfNavigationFieldLink` when your ORM does not properly recognize the navigation field during conversion to SQL or when the entity classes do not have navigation properties:

```csharp
// Direct navigation - loads the related entity directly within the same SQL query
EfNavigationFieldLink("company", (db, x) => db.Companies.Where(y => y.Id == x.CompanyId));
```

Alternatively, use `DelayLoadEntry` to load the entity via a data loader:

```csharp
// Custom loading logic with data loader
EfField("company", person => person.CompanyId)
    .DelayLoadEntry(ctx => ctx.DbContext.Companies, company => company.Id);
```

### One-to-Many Navigation Properties (Collections)

For one-to-many relationships, you can use `EfNavigationListField`:

```csharp
// Direct list navigation - loads all related entities
EfNavigationListField(person => person.Orders);
```

`EfNavigationListField` should only be used for short result sets, as it will be selected within the main SQL query and produce Cartesian product results (multiplying rows), unless your ORM provider performs query splitting automatically. For larger collections, use `DelayLoadList` instead, which loads the related entities via a data loader:

```csharp
// Custom filtered list with data loader
EfField("Orders", person => person.Id)
    .DelayLoadList(
        ctx => ctx.DbContext.Orders.Where(x => x.Deleted == false),
        order => order.PersonId
    );
```

### Many-to-Many Navigation Properties (Collections)

For many-to-many relationships, see the one-to-many documentation above.  You can apply a item selector to the data loader to omit it from the graphs:

```csharp
// Custom filtered list with data loader
EfField("Categories", product => product.Id)
    .DelayLoadList(
        ctx => ctx.DbContext.ProductCategories,
        productCategory => productCategory.ProductId,
        productCategory => productCategory.Category
    );
```

### Custom Resolvers Outside EfObjectGraphType

While `EfObjectGraphType` provides convenient methods for defining fields on dynamically generated graph objects, you may need to add custom resolvers in other parts of your schema (such as in code-first graph types or type-first classes). GraphQL.Linq provides extension methods that work with any `IQueryable` source, such as `ToGraphSingleAsync` shown above.

For optimal performance, use the data loader methods such as `ToGraphSingleDelayed`, `ToGraphSingleOrDefaultDelayed` and `ToGraphDelayed` instead of the basic query methods. These methods automatically batch database queries across multiple parent objects, preventing N+1 query problems.

```csharp
// Type-first graph type
public class Person
{
    [Id]
    public int Id { get; set; }
    public string Name { get; set; }
    [Id]
    public int CompanyId { get; set; }

    // Single entity via data loader
    public IDataLoaderResult<EfSource<Company>> Company([FromServices] MyDbContext db)
        => db.Companies.ToGraphSingleDelayed(CompanyId, company => company.Id);

    // List of entities via data loader
    public IDataLoaderResult<IEnumerable<EfSource<Order>>> Orders(EfSource<Person> person)
        => _db.Orders.ToGraphDelayed(Id, order => order.PersonId);
}

// Code-first graph type
public class PersonGraphType : ObjectGraphType<Person>
{
    public PersonGraphType()
    {
        Field("id", x => x.Id, type: typeof(IdGraphType));
        Field("name", x => x.Name);
        Field("companyId", x => x.CompanyId, type: typeof(IdGraphType));

        // Single entity via data loader
        Field<EfSource<Company>>("company")
            .ResolveAsync(ctx => {
                var db = ctx.RequestServices!.GetRequiredService<MyDbContext>();
                return db.Companies.ToGraphSingleDelayed(ctx.Source.CompanyId, company => company.Id);
            });

        // List of entities via data loader
        Field<IEnumerable<EfSource<Order>>>("orders")
            .ResolveAsync(ctx => {
                var db = ctx.RequestServices!.GetRequiredService<MyDbContext>();
                return db.Orders.ToGraphDelayed(ctx.Source.Id, order => order.PersonId);
            });
    }
}
```

## 📖 Quick Reference

Resolver methods:

```csharp
// Single entity queries
query.ToGraphSingleAsync();
query.ToGraphSingleOrDefaultAsync();
db.People.ToGraphSingleAsync(x => x.Id == id);
db.People.ToGraphSingleOrDefaultAsync(x => x.Id == id);
db.Companies.ToGraphSingleDelayed(companyId, company => company.Id);
db.Companies.ToGraphSingleOrDefaultDelayed(companyId, company => company.Id);

// List entity queries
query.ToGraphAsync();
db.Orders.ToGraphDelayed(id, order => order.PersonId);
db.ProductCategories.ToGraphDelayed(productId, pc => pc.ProductId, pc => pc.Category);

// Connection queries with pagination
query.ToGraphConnectionAsync(first, last, after, before, defaultPageSize: 100);
```

EfObjectGraphType methods:

```csharp
// Basic field mapping
EfIdField(x => x.Id);
EfField(x => x.Name);
EfField("isActive", x => x.Active);
EfField(x => x.MiddleName, true);
EfField(x => x.ElapsedTime, type: typeof(NonNullGraphType<TimeSpanMillisecondsGraphType>))

// Computed fields
EfField("totalSales", x => x.Orders.Sum(o => o.Total));
EfField("status", x => x.Active ? "Active" : "Inactive");

// Client-side processing
EfField("fullName", x => new { x.FirstName, x.LastName })
    .ThenResolve(val => val.FirstName + " " + val.LastName);

// Context-aware fields (advanced)
EfFieldFromContext("salesCount",
    context => {
        var status = context.GetArgument<SaleStatus>("status");
        return person => context.DbContext.Sales
            .Where(sale => sale.CustomerId == person.Id && sale.Status == status)
            .Count();
    })
    .Argument<SaleStatus>("status");

// Navigation properties (one-to-one/many-to-one)
EfNavigationField(x => x.Company);
EfNavigationFieldLink("company", (db, x) => db.Companies.Where(y => y.Id == x.CompanyId));
EfField("company", person => person.CompanyId)
    .DelayLoadEntry(
        ctx => ctx.DbContext.Companies,
        company => company.Id);

// Navigation properties (one-to-many)
EfNavigationListField(person => person.Orders);
EfField("Orders", person => person.Id)
    .DelayLoadList(
        ctx => ctx.DbContext.Orders.Where(x => x.Deleted == false),
        order => order.PersonId);

// Navigation properties (many-to-many)
EfField("Categories", product => product.Id)
    .DelayLoadList(
        ctx => ctx.DbContext.ProductCategories,
        productCategory => productCategory.ProductId,
        productCategory => productCategory.Category);
```

## 🔧 Advanced Features

### GraphQL.Linq.Expressions Namespace

The `GraphQL.Linq.Expressions` namespace provides low-level utilities for building and manipulating LINQ expressions programmatically, enabling advanced query composition scenarios.

#### Extension Methods

| Method | Description |
|--------|-------------|
| `Where` | Applies a Where clause to filter elements in a collection |
| `Take` / `Skip` | Limits or skips a specified number of elements |
| `Count` | Counts the number of elements in a collection |
| `OrderBy` / `OrderByDescending` | Sorts elements in ascending or descending order |
| `ThenBy` / `ThenByDescending` | Performs a secondary sort in ascending or descending order |
| `Or` / `And` / `Not` | Combines or negates boolean predicate expressions using logical operators |
| `EndsWithFirstOrDefault` | Determines if a lambda expression ends with FirstOrDefault and removes or converts it to a Where clause |
| `Replace` | Replaces a parameter expression within an expression tree with a new expression |
| `ChainWith` | Chains two lambda expressions together |

#### Static Methods

| Method | Description |
|--------|-------------|
| `ParameterReplacer.Chain` | Chains two lambda expressions together (non-generic version) |
| `LambdaBuilder.Build` | Builds a lambda expression to map an object to an EfSource instance |
| `LambdaBuilder.BuildNavigation` | Builds a lambda expression for navigating and transforming a collection |

These methods are primarily used internally by GraphQL.Linq but can be leveraged for custom query building scenarios where you need fine-grained control over expression tree construction.

## ⚠️ Known Limitations

GraphQL.Linq has the following known limitations:

### Connection Query Restrictions

- **Items and Edges Cannot Be Selected Together**: When querying a connection, you cannot request both `items` and `edges` (or `nodes`) in the same query. Choose one approach:
  - Use `items` for a simple list of entities
  - Use `edges` (with `node` and `cursor`) for cursor-based pagination with metadata
  
- **Backward Pagination Requires Cursor**: The `last` parameter for backward pagination must be used with the `before` cursor parameter. Querying with `last` alone (without `before`) is not supported.

- **After and Before Cannot Be Used Together**: Supplying both `after` and `before` cursor parameters in the same query is not supported. Use one or the other:
  - Use `after` with `first` for forward pagination
  - Use `before` with `last` for backward pagination

- **Index-Based Pagination**: Connection pagination currently uses index-based pagination with `Skip` and `Take` operations rather than true cursor-based pagination. This means:
  - Cursors are essentially row indices
  - Changes to the underlying dataset between queries may affect pagination consistency
  - Performance may degrade for very large offsets

- **Graph Type Cannot Be Specified for Connections**: When defining connection fields using methods like `EfNavigationConnectionField`, you cannot specify a custom graph type parameter. The graph type is automatically inferred from the entity type. Attempting to pass a `graphType` parameter will throw a `NotSupportedException`.

These limitations are planned to be addressed in future releases.

## 🚀 CI Builds

The NuGet feed contains only **major/stable** releases. If you want the latest functions and features, you can use the CI builds [via Github packages](https://github.com/graphql-linq/GraphQL.Linq/packages).

_(More information on how to use Github Packages in Nuget Package Manager can be [found here](https://samlearnsazure.blog/2021/08/08/consuming-a-nuget-package-from-github-packages/).)_

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

GraphQL.Linq is developed and maintained by [Shane32](https://github.com/Shane32).

Copyright © 2025 American Community Developers, Inc. All rights reserved.

This software is distributed under a tiered licensing model:

- **Limited-Use License**: Free for entities with annual revenue under USD 1,000,000
- **Commercial License**: Available for larger organizations
- **Consultant License**: Available for consultants and service providers

See the [LICENSE.txt](LICENSE.txt) file for complete license terms and conditions.

## 🙏 Credits

Glory to Jehovah, Lord of Lords and King of Kings, creator of Heaven and Earth, who through his Son Jesus Christ, has redeemed me to become a child of God. -[Shane32](https://github.com/Shane32)
