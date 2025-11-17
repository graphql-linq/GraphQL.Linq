// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Collections;
using System.Linq.Expressions;
using GraphQL.Linq;
using GraphQL.Linq.GraphApi;
using Microsoft.EntityFrameworkCore;
using TestDb;
using TestDb.Models;

namespace Tests;

[TestClass]
public class EfGraphQLServiceTests
{
    private sealed class TestEfDbPrimaryKeyNamesProvider : IEfDbPrimaryKeyNamesProvider<SampleDbContext>
    {
        public IEnumerable<string> GetPrimaryKeyNames<TSource>()
        {
            throw new NotImplementedException();
        }

        public Expression<Func<TSource, object>> GetDummyExpression<TSource>()
        {
            throw new NotImplementedException();
        }
    }

    private sealed class TestEfGraphQLService : EfGraphQLServiceBase<SampleDbContext>
    {
        public TestEfGraphQLService(int maxParameterizeContainsVariables)
            : this(new TestEfDbPrimaryKeyNamesProvider(), maxParameterizeContainsVariables)
        {
        }

        public TestEfGraphQLService(IEfDbPrimaryKeyNamesProvider<SampleDbContext> provider, int maxParameterizeContainsVariables)
            : base(provider, maxParameterizeContainsVariables)
        {
        }

        public override Task<IList<TReturn>> QueryToListAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<int> QueryCountAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<TReturn?> QuerySingleOrDefaultAsync<TReturn>(IQueryable<TReturn> query, CancellationToken cancellationToken) where TReturn : class
        {
            throw new NotImplementedException();
        }
    }

    private static TestEfGraphQLService CreateService(int maxParameterizeContainsVariables = 10)
    {
        return new TestEfGraphQLService(maxParameterizeContainsVariables);
    }

    private static List<Product> CreateTestProducts(int count)
    {
        var products = new List<Product>();
        for (int i = 1; i <= count; i++) {
            products.Add(new Product {
                Id = i,
                Name = $"Product {i}",
                Description = $"Description {i}",
                Price = 10.0m * i,
                StockQuantity = 100 + i,
                IsActive = true,
                CreatedByUserId = 1
            });
        }
        return products;
    }

    [TestMethod]
    public void CreateWhereInExpression_EmptyCollection_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var products = CreateTestProducts(15).AsQueryable();
        var emptyKeys = new List<int>();

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            emptyKeys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        foreach (var product in products) {
            Assert.IsFalse(compiled(product), $"Product {product.Id} should not match empty collection");
        }
    }

    [TestMethod]
    public void CreateWhereInExpression_SingleItem_UsesOrExpression()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new List<int> { 5 };

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        CollectionAssert.AreEqual(new[] { 5 }, products.Where(compiled).Select(p => p.Id).ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_SmallCollection_UsesOrExpression()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new List<int> { 2, 5, 8, 10 };

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 2, 5, 8, 10 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_ExactlyAtThreshold_UsesOrExpression()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_LargeCollection_UsesContains()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }; // 11 items > threshold of 10

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_VeryLargeCollection_UsesContains()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = Enumerable.Range(1, 15).ToList();

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(Enumerable.Range(1, 15).ToArray(), results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_ICollectionInterface_UsesCount()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new System.Collections.ArrayList { 3, 7, 12 }; // ICollection but not ICollection<T>

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys.Cast<int>());

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 3, 7, 12 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_IReadOnlyCollection_UsesCount()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        IReadOnlyCollection<int> keys = new List<int> { 4, 9, 14 }.AsReadOnly();

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 4, 9, 14 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_EnumerableWithoutCount_UsesContains()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        // Use an IEnumerable that doesn't implement ICollection
        var keys = GetKeysEnumerable(new[] { 2, 6, 11 });

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 2, 6, 11 }, results.ToArray());
    }

    private static IEnumerable<int> GetKeysEnumerable(int[] values)
    {
        foreach (var value in values)
            yield return value;
    }

    [TestMethod]
    public void CreateWhereInExpression_NegativeThreshold_AlwaysUsesContains()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: -1);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new List<int> { 1, 5, 10 }; // Small collection

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 1, 5, 10 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_ZeroThreshold_AlwaysUsesContains()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 0);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new List<int> { 3, 8, 13 };

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 3, 8, 13 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_IListInterface_UsesOrExpression()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        IList<int> keys = new List<int> { 1, 7, 15 };

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 1, 7, 15 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_ArrayType_UsesOrExpression()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new[] { 2, 9, 14 };

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 2, 9, 14 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new List<int> { 100, 200, 300 }; // IDs that don't exist

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).ToList();
        Assert.IsEmpty(results);
    }

    [TestMethod]
    public void CreateWhereInExpression_AllMatch_ReturnsAll()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 20);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = Enumerable.Range(1, 15).ToList();

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        Assert.HasCount(15, results);
        CollectionAssert.AreEqual(Enumerable.Range(1, 15).ToArray(), results.ToArray());
    }

    private sealed class CustomCollection<T> : ICollection, IEnumerable<T>
    {
        private readonly List<T> _items;

        public CustomCollection(IEnumerable<T> items)
        {
            _items = new List<T>(items);
        }

        public int Count => _items.Count;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_items).CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }

    private sealed class CustomReadOnlyCollection<T> : IReadOnlyCollection<T>
    {
        private readonly List<T> _items;

        public CustomReadOnlyCollection(IEnumerable<T> items)
        {
            _items = new List<T>(items);
        }

        public int Count => _items.Count;
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }

    [TestMethod]
    public void CreateWhereInExpression_CustomICollectionWithIEnumerableT_UsesOrExpression()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new CustomCollection<int>(new[] { 2, 5, 8, 11, 14 });

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 2, 5, 8, 11, 14 }, results.ToArray());
    }

    [TestMethod]
    public void CreateWhereInExpression_CustomIReadOnlyCollection_UsesOrExpression()
    {
        // Arrange
        var service = CreateService(maxParameterizeContainsVariables: 10);
        var products = CreateTestProducts(15).AsQueryable();
        var keys = new CustomReadOnlyCollection<int>(new[] { 3, 6, 9, 12, 15 });

        // Act
        var expression = service.CreateWhereInExpression<int, Product>(
            () => null!,
            p => p.Id,
            keys);

        // Compile and test
        var compiled = expression.Compile();

        // Assert
        var results = products.Where(compiled).Select(p => p.Id).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 3, 6, 9, 12, 15 }, results.ToArray());
    }

    [TestMethod]
    public void Constructor_NullProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestEfGraphQLService(null!, 10));
    }
}
