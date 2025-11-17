// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.ComponentModel;
using GraphQL;
using GraphQL.DI;
using GraphQL.Linq.GraphApi;
using GraphQL.Types.Relay.DataObjects;

namespace TechMart.Graphs;

public class QueryGraphType : DIObjectGraphBase
{
    private readonly TechMartDbContext _dbContext;

    public QueryGraphType(TechMartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Product queries
    [Description("Retrieves a single product by its unique identifier.")]
    public Task<EfSource<Product>> ProductAsync([Id] int id)
        => _dbContext.Products.Where(x => x.Id == id).ToGraphSingleAsync();

    [Description("Retrieves a paginated list of products with optional filtering by IDs, search text, category IDs, and custom ordering.")]
    public Task<Connection<EfSource<Product>>> ProductsAsync(
        [Id] IEnumerable<int>? ids,
        string? search,
        [Id] IEnumerable<int>? categoryIds,
        [MinMax(1, 1000)] int? first, [MinMax(1, 1000)] int? last, [Id] string? after, [Id] string? before,
        ProductsOrderBy orderBy = ProductsOrderBy.IdAsc)
    {
        var query = _dbContext.Products.Where(x => x.IsActive);

        // Apply search filter(s)
        if (search is not null) {
            query = query.Where(x => x.Name.Contains(search) || x.Description!.Contains(search));
        }
        if (ids is not null) {
            query = query.Where(x => ids.Contains(x.Id));
        }
        if (categoryIds is not null) {
            query = query.Where(x => x.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
        }

        // Apply ordering with tiebreaker on Id
        query = orderBy switch {
            ProductsOrderBy.IdAsc => query.OrderBy(x => x.Id),
            ProductsOrderBy.IdDesc => query.OrderByDescending(x => x.Id),
            ProductsOrderBy.NameAsc => query.OrderBy(x => x.Name).ThenBy(x => x.Id),
            ProductsOrderBy.NameDesc => query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id),
            ProductsOrderBy.PriceAsc => query.OrderBy(x => x.Price).ThenBy(x => x.Id),
            ProductsOrderBy.PriceDesc => query.OrderByDescending(x => x.Price).ThenByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id),
        };

        return query.ToGraphConnectionAsync(first, last, after, before, 100);
    }

    // Customer queries
    [Description("Retrieves a single customer by their unique identifier.")]
    public Task<EfSource<Customer>> CustomerAsync([Id] int id)
        => _dbContext.Customers.Where(x => x.Id == id).ToGraphSingleAsync();

    [Description("Retrieves a paginated list of customers with optional filtering by IDs, search text, and custom ordering.")]
    public Task<Connection<EfSource<Customer>>> CustomersAsync(
        [Id] IEnumerable<int>? ids,
        string? search,
        [MinMax(1, 1000)] int? first, [MinMax(1, 1000)] int? last, [Id] string? after, [Id] string? before,
        CustomersOrderBy orderBy = CustomersOrderBy.IdAsc)
    {
        var query = _dbContext.Customers.AsQueryable();

        // Apply search filter(s)
        if (search is not null) {
            query = query.Where(x => x.FullName.Contains(search) || x.Email.Contains(search));
        }
        if (ids is not null) {
            query = query.Where(x => ids.Contains(x.Id));
        }

        // Apply ordering with tiebreaker on Id
        query = orderBy switch {
            CustomersOrderBy.IdAsc => query.OrderBy(x => x.Id),
            CustomersOrderBy.IdDesc => query.OrderByDescending(x => x.Id),
            CustomersOrderBy.NameAsc => query.OrderBy(x => x.FullName).ThenBy(x => x.Id),
            CustomersOrderBy.NameDesc => query.OrderByDescending(x => x.FullName).ThenByDescending(x => x.Id),
            CustomersOrderBy.EmailAsc => query.OrderBy(x => x.Email).ThenBy(x => x.Id),
            CustomersOrderBy.EmailDesc => query.OrderByDescending(x => x.Email).ThenByDescending(x => x.Id),
            CustomersOrderBy.CreatedAtAsc => query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            CustomersOrderBy.CreatedAtDesc => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id),
        };

        return query.ToGraphConnectionAsync(first, last, after, before, 100);
    }

    // Order queries
    [Description("Retrieves a single order by its unique identifier.")]
    public Task<EfSource<Order>> OrderAsync([Id] int id)
        => _dbContext.Orders.Where(x => x.Id == id).ToGraphSingleAsync();

    [Description("Retrieves a paginated list of orders with optional filtering by IDs, order number, customer ID, and order statuses.")]
    public Task<Connection<EfSource<Order>>> OrdersAsync(
        [Id] IEnumerable<int>? ids,
        string? orderNumber,
        [Id] int? customerId,
        IEnumerable<OrderStatus>? statuses,
        [MinMax(1, 1000)] int? first, [MinMax(1, 1000)] int? last, [Id] string? after, [Id] string? before)
    {
        var query = _dbContext.Orders.AsQueryable();

        // Apply filters
        if (orderNumber is not null) {
            query = query.Where(x => x.OrderNumber == orderNumber);
        }
        if (customerId is not null) {
            query = query.Where(x => x.CustomerId == customerId);
        }
        if (statuses is not null) {
            query = query.Where(x => statuses.Contains(x.OrderStatus));
        }
        if (ids is not null) {
            query = query.Where(x => ids.Contains(x.Id));
        }

        // Apply ordering with tiebreaker on Id
        query = query.OrderByDescending(x => x.OrderDate).ThenBy(x => x.Id);

        return query.ToGraphConnectionAsync(first, last, after, before, 100);
    }

    // Category queries
    [Description("Retrieves a single category by its unique identifier.")]
    public Task<EfSource<Category>> CategoryAsync([Id] int id)
        => _dbContext.Categories.Where(x => x.Id == id).ToGraphSingleAsync();

    [Description("Retrieves a paginated list of active categories with optional filtering by IDs and search text.")]
    public Task<Connection<EfSource<Category>>> CategoriesAsync(
        [Id] IEnumerable<int>? ids,
        string? search,
        [MinMax(1, 1000)] int? first, [MinMax(1, 1000)] int? last, [Id] string? after, [Id] string? before)
    {
        var query = _dbContext.Categories.Where(x => x.IsActive);

        // Apply filters
        if (search is not null) {
            query = query.Where(x => x.Name.Contains(search) || x.Description!.Contains(search));
        }
        if (ids is not null) {
            query = query.Where(x => ids.Contains(x.Id));
        }

        // Apply ordering with tiebreaker on Id
        query = query.OrderBy(x => x.Name).ThenBy(x => x.Id);

        return query.ToGraphConnectionAsync(first, last, after, before, 100);
    }

    // User queries
    [Description("Retrieves a single user by their unique identifier.")]
    public Task<EfSource<User>> UserAsync([Id] int id)
        => _dbContext.Users.Where(x => x.Id == id).ToGraphSingleAsync();

    [Description("Retrieves a paginated list of active users with optional filtering by IDs, search text, and user roles.")]
    public Task<Connection<EfSource<User>>> UsersAsync(
        [Id] IEnumerable<int>? ids,
        string? search,
        IEnumerable<UserRole>? roles,
        [MinMax(1, 1000)] int? first, [MinMax(1, 1000)] int? last, [Id] string? after, [Id] string? before)
    {
        var query = _dbContext.Users.Where(x => x.IsActive);

        // Apply filters
        if (search is not null) {
            query = query.Where(x => x.Username.Contains(search) || x.Email.Contains(search));
        }
        if (roles is not null) {
            query = query.Where(x => roles.Contains(x.Role));
        }
        if (ids is not null) {
            query = query.Where(x => ids.Contains(x.Id));
        }

        // Apply ordering with tiebreaker on Id
        query = query.OrderBy(x => x.Username).ThenBy(x => x.Id);

        return query.ToGraphConnectionAsync(first, last, after, before, 100);
    }
}
