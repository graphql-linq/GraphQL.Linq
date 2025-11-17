// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Net.Http;
using System.Text;
using System.Text.Json;
using GraphQL.Linq.GraphApi;
using Microsoft.Extensions.DependencyInjection;

namespace TechMartTests;

[TestClass]
public class TechMartTests
{
    private static HttpClient _client = null!;
    private static TechMartTestFixture _fixture = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        _fixture = new TechMartTestFixture();
        _client = _fixture.Client;
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _fixture.Dispose();
    }

    [TestMethod]
    public async Task CustomersQuery()
    {
        // Arrange
        var query = """
            {
              customers(first: 3) {
                totalCount
                edges {
                  cursor
                  node {
                    id
                    fullName
                    email
                    phoneNumber
                    createdAt
                  }
                }
                pageInfo {
                  startCursor
                  endCursor
                  hasPreviousPage
                  hasNextPage
                }
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task GetSDL()
    {
        // Act
        var response = await _client.GetAsync("/sdl");
        response.EnsureSuccessStatusCode();

        var sdl = await response.Content.ReadAsStringAsync();

        // Assert
        sdl.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task CategoriesQuery()
    {
        // Arrange
        var query = """
            {
              categories(first: 5) {
                totalCount
                edges {
                  cursor
                  node {
                    id
                    name
                    description
                    isActive
                  }
                }
                pageInfo {
                  startCursor
                  endCursor
                  hasPreviousPage
                  hasNextPage
                }
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task CategoryQuery()
    {
        // Arrange
        var query = """
            {
              category(id: "1") {
                id
                name
                description
                isActive
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task CustomerQuery()
    {
        // Arrange
        var query = """
            {
              customer(id: "1") {
                id
                fullName
                email
                phoneNumber
                createdAt
                loyaltyPoints
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task OrdersQuery()
    {
        // Arrange
        var query = """
            {
              orders(first: 3) {
                totalCount
                edges {
                  cursor
                  node {
                    id
                    orderNumber
                    orderDate
                    shippedDate
                    total
                    orderStatus
                    customerId
                    customer {
                      id
                      fullName
                      email
                    }
                    orderItems {
                      id
                      quantity
                      unitPrice
                      total
                      productId
                      product {
                        id
                        name
                        price
                      }
                    }
                  }
                }
                pageInfo {
                  startCursor
                  endCursor
                  hasPreviousPage
                  hasNextPage
                }
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task OrderQuery()
    {
        // Arrange
        var query = """
            {
              order(id: "1") {
                id
                orderNumber
                orderDate
                shippedDate
                total
                orderStatus
                customerId
                customer {
                  id
                  fullName
                  email
                }
                orderItems {
                  id
                  quantity
                  unitPrice
                  total
                  productId
                  product {
                    id
                    name
                    price
                  }
                }
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ProductsQuery()
    {
        // Arrange
        var query = """
            {
              products(first: 5, categoryIds: "3") {
                totalCount
                edges {
                  cursor
                  node {
                    id
                    name
                    description
                    price
                    stockQuantity
                    categories {
                      id
                      name
                      description
                    }
                  }
                }
                pageInfo {
                  startCursor
                  endCursor
                  hasPreviousPage
                  hasNextPage
                }
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task ProductQuery()
    {
        // Arrange
        var query = """
            {
              product(id: "1") {
                id
                name
                description
                price
                stockQuantity
                categories {
                  id
                  name
                  description
                  isActive
                }
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task UsersQuery()
    {
        // Arrange
        var query = """
            {
              users(first: 5) {
                totalCount
                edges {
                  cursor
                  node {
                    id
                    username
                    email
                    role
                    isActive
                  }
                }
                pageInfo {
                  startCursor
                  endCursor
                  hasPreviousPage
                  hasNextPage
                }
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

    [TestMethod]
    public async Task UserQuery()
    {
        // Arrange
        var query = """
            {
              user(id: "1") {
                id
                username
                email
                role
                isActive
              }
            }
            """;

        // Act
        var result = await ExecuteGraphQLQueryAsync(query);

        // Assert
        result.ShouldMatchApproved();
    }

#if !LINQTODB5 && !LINQTODB6
    [TestMethod]
    public void VerifyPrimaryKeyNamesProvider()
    {
        var provider = _fixture.Services.GetRequiredService<IEfDbPrimaryKeyNamesProvider<TechMartDbContext>>();
        var key = provider.GetPrimaryKeyNames<Customer>().Single();
        Assert.AreEqual("Id", key);
    }
#else
    [TestMethod]
    public void VerifyPrimaryKeyNamesProvider()
    {
        var provider = _fixture.Services.GetRequiredService<IEfDbPrimaryKeyNamesProvider<TechMartDbContext>>();
        var key = provider.GetPrimaryKeyNames<TestClass>().Single();
        Assert.AreEqual(nameof(TestClass.SampleIdField), key);
    }

    private sealed class TestClass
    {
        [LinqToDB.Mapping.PrimaryKey]
        public int SampleIdField { get; set; }
    }
#endif

    [TestMethod]
    public void VerifyContextTypeProvider()
    {
        using var scope = _fixture.Services.CreateScope();
        var provider = new EfDbContextTypeProvider();
        var db = scope.ServiceProvider.GetRequiredService<TechMartDbContext>();
        var contextType = provider.GetDbContextType(db.Customers);
        Assert.AreEqual(db.GetType(), contextType);
        var contextType2 = provider.GetDbContextType(db.Customers.Where(x => x.Id == 123));
        Assert.AreEqual(db.GetType(), contextType2);
    }

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

    private static async Task<string> ExecuteGraphQLQueryAsync(string query)
    {
        var requestBody = new { query };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        try {
            return JsonSerializer.Serialize(
                JsonSerializer.Deserialize<JsonElement>(responseContent),
                _jsonOptions);
        } catch {
        }
        return responseContent;
    }

}
