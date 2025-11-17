// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Linq.Expressions;
using GraphQL.Linq.Expressions;

namespace Tests;

[TestClass]
public class LinqExpressionsTests
{
    // Test entity classes
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SortOrder { get; set; }
    }

    private static List<TestEntity> GetTestData()
    {
        return new List<TestEntity>
        {
            new() { Id = 1, Name = "Product A", Price = 10.00m, CategoryId = 1, IsActive = true, CreatedAt = new DateTime(2023, 1, 1), SortOrder = 3 },
            new() { Id = 2, Name = "Product B", Price = 20.00m, CategoryId = 2, IsActive = false, CreatedAt = new DateTime(2023, 2, 1), SortOrder = 1 },
            new() { Id = 3, Name = "Product C", Price = 30.00m, CategoryId = 1, IsActive = true, CreatedAt = new DateTime(2023, 3, 1), SortOrder = 2 },
            new() { Id = 4, Name = "Product D", Price = 40.00m, CategoryId = 3, IsActive = true, CreatedAt = new DateTime(2023, 4, 1), SortOrder = 4 },
            new() { Id = 5, Name = "Product E", Price = 50.00m, CategoryId = 2, IsActive = false, CreatedAt = new DateTime(2023, 5, 1), SortOrder = 5 }
        };
    }

    // =================== Where Method Tests ===================

    [TestMethod]
    public void Where_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> whereQuery = x => x.IsActive;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.Where<object, TestEntity>(null!, whereQuery));
    }

    [TestMethod]
    public void Where_WithNullWhereQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => new List<TestEntity>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            sourceQuery.Where<object, TestEntity>(null!));
    }

    [TestMethod]
    public void Where_WithValidPredicate_FiltersResults()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, bool>> whereQuery = x => x.IsActive;

        // Act
        var result = sourceQuery.Where(whereQuery);
        var compiled = result.Compile();
        var filtered = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(3, filtered);
        Assert.IsTrue(filtered.All(x => x.IsActive));
    }

    [TestMethod]
    public void Where_WithComplexPredicate_FiltersResults()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, bool>> whereQuery = x => x.Price > 25.00m && x.IsActive;

        // Act
        var result = sourceQuery.Where(whereQuery);
        var compiled = result.Compile();
        var filtered = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(2, filtered);
        Assert.IsTrue(filtered.All(x => x.Price > 25.00m && x.IsActive));
    }

    [TestMethod]
    public void Where_ChainedMultipleTimes_AppliesAllFilters()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, bool>> whereQuery1 = x => x.IsActive;
        Expression<Func<TestEntity, bool>> whereQuery2 = x => x.Price > 20.00m;

        // Act
        var result = sourceQuery.Where(whereQuery1).Where(whereQuery2);
        var compiled = result.Compile();
        var filtered = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(2, filtered);
        Assert.IsTrue(filtered.All(x => x.IsActive && x.Price > 20.00m));
    }

    // =================== Take Method Tests ===================

    [TestMethod]
    public void Take_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.Take<object, TestEntity>(null!, 5));
    }

    [TestMethod]
    public void Take_WithValidCount_LimitsResults()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Take(3);
        var compiled = result.Compile();
        var limited = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(3, limited);
        Assert.AreEqual(1, limited[0].Id);
        Assert.AreEqual(2, limited[1].Id);
        Assert.AreEqual(3, limited[2].Id);
    }

    [TestMethod]
    public void Take_WithZeroCount_ReturnsEmpty()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Take(0);
        var compiled = result.Compile();
        var limited = compiled(new object()).ToList();

        // Assert
        Assert.IsEmpty(limited);
    }

    [TestMethod]
    public void Take_WithCountGreaterThanSource_ReturnsAll()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Take(100);
        var compiled = result.Compile();
        var limited = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, limited);
    }

    [TestMethod]
    public void Take_AfterWhere_LimitsFilteredResults()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, bool>> whereQuery = x => x.IsActive;

        // Act
        var result = sourceQuery.Where(whereQuery).Take(2);
        var compiled = result.Compile();
        var limited = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(2, limited);
        Assert.IsTrue(limited.All(x => x.IsActive));
    }

    // =================== Skip Method Tests ===================

    [TestMethod]
    public void Skip_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.Skip<object, TestEntity>(null!, 2));
    }

    [TestMethod]
    public void Skip_WithValidCount_SkipsResults()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Skip(2);
        var compiled = result.Compile();
        var skipped = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(3, skipped);
        Assert.AreEqual(3, skipped[0].Id);
        Assert.AreEqual(4, skipped[1].Id);
        Assert.AreEqual(5, skipped[2].Id);
    }

    [TestMethod]
    public void Skip_WithZeroCount_ReturnsAll()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Skip(0);
        var compiled = result.Compile();
        var skipped = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, skipped);
    }

    [TestMethod]
    public void Skip_WithCountGreaterThanSource_ReturnsEmpty()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Skip(100);
        var compiled = result.Compile();
        var skipped = compiled(new object()).ToList();

        // Assert
        Assert.IsEmpty(skipped);
    }

    [TestMethod]
    public void Skip_WithTake_ImplementsPagination()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Skip(1).Take(2);
        var compiled = result.Compile();
        var paginated = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(2, paginated);
        Assert.AreEqual(2, paginated[0].Id);
        Assert.AreEqual(3, paginated[1].Id);
    }

    // =================== Count Method Tests ===================

    [TestMethod]
    public void Count_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.Count<object, TestEntity>(null!));
    }

    [TestMethod]
    public void Count_WithValidSource_ReturnsCount()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;

        // Act
        var result = sourceQuery.Count();
        var compiled = result.Compile();
        var count = compiled(new object());

        // Assert
        Assert.AreEqual(5, count);
    }

    [TestMethod]
    public void Count_WithEmptySource_ReturnsZero()
    {
        // Arrange
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => new List<TestEntity>();

        // Act
        var result = sourceQuery.Count();
        var compiled = result.Compile();
        var count = compiled(new object());

        // Assert
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void Count_AfterWhere_ReturnsFilteredCount()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, bool>> whereQuery = x => x.IsActive;

        // Act
        var result = sourceQuery.Where(whereQuery).Count();
        var compiled = result.Compile();
        var count = compiled(new object());

        // Assert
        Assert.AreEqual(3, count);
    }

    // =================== OrderBy Method Tests ===================

    [TestMethod]
    public void OrderBy_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> orderByQuery = x => x.Name;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.OrderBy<object, TestEntity, string>(null!, orderByQuery));
    }

    [TestMethod]
    public void OrderBy_WithNullOrderByQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => new List<TestEntity>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            sourceQuery.OrderBy<object, TestEntity, string>(null!));
    }

    [TestMethod]
    public void OrderBy_WithStringProperty_SortsAscending()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, string>> orderByQuery = x => x.Name;

        // Act
        var result = sourceQuery.OrderBy(orderByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, sorted);
        Assert.AreEqual("Product A", sorted[0].Name);
        Assert.AreEqual("Product B", sorted[1].Name);
        Assert.AreEqual("Product C", sorted[2].Name);
        Assert.AreEqual("Product D", sorted[3].Name);
        Assert.AreEqual("Product E", sorted[4].Name);
    }

    [TestMethod]
    public void OrderBy_WithNumericProperty_SortsAscending()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, decimal>> orderByQuery = x => x.Price;

        // Act
        var result = sourceQuery.OrderBy(orderByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, sorted);
        Assert.AreEqual(10.00m, sorted[0].Price);
        Assert.AreEqual(20.00m, sorted[1].Price);
        Assert.AreEqual(30.00m, sorted[2].Price);
        Assert.AreEqual(40.00m, sorted[3].Price);
        Assert.AreEqual(50.00m, sorted[4].Price);
    }

    [TestMethod]
    public void OrderBy_WithDateTimeProperty_SortsAscending()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, DateTime>> orderByQuery = x => x.CreatedAt;

        // Act
        var result = sourceQuery.OrderBy(orderByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, sorted);
        Assert.AreEqual(new DateTime(2023, 1, 1), sorted[0].CreatedAt);
        Assert.AreEqual(new DateTime(2023, 5, 1), sorted[4].CreatedAt);
    }

    // =================== OrderByDescending Method Tests ===================

    [TestMethod]
    public void OrderByDescending_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> orderByQuery = x => x.Name;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.OrderByDescending<object, TestEntity, string>(null!, orderByQuery));
    }

    [TestMethod]
    public void OrderByDescending_WithNullOrderByQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => new List<TestEntity>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            sourceQuery.OrderByDescending<object, TestEntity, string>(null!));
    }

    [TestMethod]
    public void OrderByDescending_WithStringProperty_SortsDescending()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, string>> orderByQuery = x => x.Name;

        // Act
        var result = sourceQuery.OrderByDescending(orderByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, sorted);
        Assert.AreEqual("Product E", sorted[0].Name);
        Assert.AreEqual("Product D", sorted[1].Name);
        Assert.AreEqual("Product C", sorted[2].Name);
        Assert.AreEqual("Product B", sorted[3].Name);
        Assert.AreEqual("Product A", sorted[4].Name);
    }

    [TestMethod]
    public void OrderByDescending_WithNumericProperty_SortsDescending()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, decimal>> orderByQuery = x => x.Price;

        // Act
        var result = sourceQuery.OrderByDescending(orderByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, sorted);
        Assert.AreEqual(50.00m, sorted[0].Price);
        Assert.AreEqual(40.00m, sorted[1].Price);
        Assert.AreEqual(30.00m, sorted[2].Price);
        Assert.AreEqual(20.00m, sorted[3].Price);
        Assert.AreEqual(10.00m, sorted[4].Price);
    }

    // =================== ThenBy Method Tests ===================

    [TestMethod]
    public void ThenBy_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, int>> thenByQuery = x => x.SortOrder;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.ThenBy<object, TestEntity, int>(null!, thenByQuery));
    }

    [TestMethod]
    public void ThenBy_WithNullThenByQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => new List<TestEntity>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            sourceQuery.ThenBy<object, TestEntity, int>(null!));
    }

    [TestMethod]
    public void ThenBy_AfterOrderBy_AppliesSecondarySort()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, int>> orderByQuery = x => x.CategoryId;
        Expression<Func<TestEntity, int>> thenByQuery = x => x.SortOrder;

        // Act
        var result = sourceQuery.OrderBy(orderByQuery).ThenBy(thenByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, sorted);
        // Category 1: Products A (SortOrder 3) and C (SortOrder 2)
        Assert.AreEqual(1, sorted[0].CategoryId);
        Assert.AreEqual(2, sorted[0].SortOrder); // Product C
        Assert.AreEqual(1, sorted[1].CategoryId);
        Assert.AreEqual(3, sorted[1].SortOrder); // Product A
        // Category 2: Products B (SortOrder 1) and E (SortOrder 5)
        Assert.AreEqual(2, sorted[2].CategoryId);
        Assert.AreEqual(1, sorted[2].SortOrder); // Product B
        Assert.AreEqual(2, sorted[3].CategoryId);
        Assert.AreEqual(5, sorted[3].SortOrder); // Product E
    }

    // =================== ThenByDescending Method Tests ===================

    [TestMethod]
    public void ThenByDescending_WithNullSourceQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, int>> thenByQuery = x => x.SortOrder;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.ThenByDescending<object, TestEntity, int>(null!, thenByQuery));
    }

    [TestMethod]
    public void ThenByDescending_WithNullThenByQuery_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => new List<TestEntity>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            sourceQuery.ThenByDescending<object, TestEntity, int>(null!));
    }

    [TestMethod]
    public void ThenByDescending_AfterOrderBy_AppliesSecondaryDescendingSort()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, int>> orderByQuery = x => x.CategoryId;
        Expression<Func<TestEntity, int>> thenByQuery = x => x.SortOrder;

        // Act
        var result = sourceQuery.OrderBy(orderByQuery).ThenByDescending(thenByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(5, sorted);
        // Category 1: Products A (SortOrder 3) and C (SortOrder 2)
        Assert.AreEqual(1, sorted[0].CategoryId);
        Assert.AreEqual(3, sorted[0].SortOrder); // Product A
        Assert.AreEqual(1, sorted[1].CategoryId);
        Assert.AreEqual(2, sorted[1].SortOrder); // Product C
        // Category 2: Products B (SortOrder 1) and E (SortOrder 5)
        Assert.AreEqual(2, sorted[2].CategoryId);
        Assert.AreEqual(5, sorted[2].SortOrder); // Product E
        Assert.AreEqual(2, sorted[3].CategoryId);
        Assert.AreEqual(1, sorted[3].SortOrder); // Product B
    }

    // =================== Or Method Tests ===================

    [TestMethod]
    public void Or_WithNullPredicate1_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate2 = x => x.IsActive;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.Or<TestEntity>(null!, predicate2));
    }

    [TestMethod]
    public void Or_WithNullPredicate2_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate1 = x => x.IsActive;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            predicate1.Or(null!));
    }

    [TestMethod]
    public void Or_WithTwoPredicates_CombinesWithLogicalOr()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate1 = x => x.CategoryId == 1;
        Expression<Func<TestEntity, bool>> predicate2 = x => x.Price > 40.00m;

        // Act
        var result = predicate1.Or(predicate2);
        var compiled = result.Compile();
        var data = GetTestData();
        var filtered = data.Where(compiled).ToList();

        // Assert
        // Should include: CategoryId 1 (Products A, C) OR Price > 40 (Product E)
        Assert.HasCount(3, filtered);
        Assert.IsTrue(filtered.Any(x => x.Id == 1)); // Product A
        Assert.IsTrue(filtered.Any(x => x.Id == 3)); // Product C
        Assert.IsTrue(filtered.Any(x => x.Id == 5)); // Product E
    }

    [TestMethod]
    public void Or_WithMultiplePredicates_CombinesAll()
    {
        // Arrange
        var predicates = new List<Expression<Func<TestEntity, bool>>>
        {
            x => x.Id == 1,
            x => x.Id == 3,
            x => x.Id == 5
        };

        // Act
        var result = predicates.Or();
        var compiled = result.Compile();
        var data = GetTestData();
        var filtered = data.Where(compiled).ToList();

        // Assert
        Assert.HasCount(3, filtered);
        Assert.IsTrue(filtered.All(x => x.Id == 1 || x.Id == 3 || x.Id == 5));
    }

    [TestMethod]
    public void Or_WithNullEnumerable_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.Or<TestEntity>(null!));
    }

    // =================== And Method Tests ===================

    [TestMethod]
    public void And_WithNullPredicate1_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate2 = x => x.IsActive;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.And<TestEntity>(null!, predicate2));
    }

    [TestMethod]
    public void And_WithNullPredicate2_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate1 = x => x.IsActive;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            predicate1.And(null!));
    }

    [TestMethod]
    public void And_WithTwoPredicates_CombinesWithLogicalAnd()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate1 = x => x.IsActive;
        Expression<Func<TestEntity, bool>> predicate2 = x => x.Price > 20.00m;

        // Act
        var result = predicate1.And(predicate2);
        var compiled = result.Compile();
        var data = GetTestData();
        var filtered = data.Where(compiled).ToList();

        // Assert
        // Should include: IsActive AND Price > 20 (Products C, D)
        Assert.HasCount(2, filtered);
        Assert.IsTrue(filtered.All(x => x.IsActive && x.Price > 20.00m));
    }

    [TestMethod]
    public void And_WithMultiplePredicates_CombinesAll()
    {
        // Arrange
        var predicates = new List<Expression<Func<TestEntity, bool>>>
        {
            x => x.IsActive,
            x => x.Price > 20.00m,
            x => x.CategoryId == 1
        };

        // Act
        var result = predicates.And();
        var compiled = result.Compile();
        var data = GetTestData();
        var filtered = data.Where(compiled).ToList();

        // Assert
        // Should include: IsActive AND Price > 20 AND CategoryId == 1 (Product C only)
        Assert.HasCount(1, filtered);
        Assert.AreEqual(3, filtered[0].Id);
    }

    [TestMethod]
    public void And_WithNullEnumerable_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.And<TestEntity>(null!));
    }

    // =================== Not Method Tests ===================

    [TestMethod]
    public void Not_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LinqExpressions.Not<TestEntity>(null!));
    }

    [TestMethod]
    public void Not_WithPredicate_NegatesResult()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = x => x.IsActive;

        // Act
        var result = predicate.Not();
        var compiled = result.Compile();
        var data = GetTestData();
        var filtered = data.Where(compiled).ToList();

        // Assert
        // Should include: NOT IsActive (Products B, E)
        Assert.HasCount(2, filtered);
        Assert.IsTrue(filtered.All(x => !x.IsActive));
    }

    [TestMethod]
    public void Not_WithComplexPredicate_NegatesResult()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = x => x.Price > 30.00m && x.IsActive;

        // Act
        var result = predicate.Not();
        var compiled = result.Compile();
        var data = GetTestData();
        var filtered = data.Where(compiled).ToList();

        // Assert
        // Should include: NOT (Price > 30 AND IsActive) = Products A, B, C, E
        Assert.HasCount(4, filtered);
        Assert.IsFalse(filtered.Any(x => x.Price > 30.00m && x.IsActive));
    }

    // =================== EndsWithFirstOrDefault Method Tests ===================

    [TestMethod]
    public void EndsWithFirstOrDefault_WithEnumerableFirstOrDefault_ReturnsTrue()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, TestEntity?>> expression = x => data.Where(e => e.Id == 1).FirstOrDefault();

        // Act
        var result = expression.EndsWithFirstOrDefault(out var modified);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(modified);
        // The modified expression should have FirstOrDefault removed
        var compiled = modified.Compile();
        var enumerable = compiled.DynamicInvoke(new object()) as IEnumerable<TestEntity>;
        Assert.IsNotNull(enumerable);
    }

    [TestMethod]
    public void EndsWithFirstOrDefault_WithEnumerableFirstOrDefaultWithPredicate_ReturnsTrue()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, TestEntity?>> expression = x => data.FirstOrDefault(e => e.Id == 1);

        // Act
        var result = expression.EndsWithFirstOrDefault(out var modified);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(modified);
        // The modified expression should have FirstOrDefault replaced with Where
        var compiled = modified.Compile();
        var enumerable = compiled.DynamicInvoke(new object()) as IEnumerable<TestEntity>;
        Assert.IsNotNull(enumerable);
        var list = enumerable.ToList();
        Assert.HasCount(1, list);
        Assert.AreEqual(1, list[0].Id);
    }

    [TestMethod]
    public void EndsWithFirstOrDefault_WithQueryableFirstOrDefault_ReturnsTrue()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        Expression<Func<object, TestEntity?>> expression = x => data.Where(e => e.Id == 1).FirstOrDefault();

        // Act
        var result = expression.EndsWithFirstOrDefault(out var modified);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(modified);
    }

    [TestMethod]
    public void EndsWithFirstOrDefault_WithQueryableFirstOrDefaultWithPredicate_ReturnsTrue()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        Expression<Func<object, TestEntity?>> expression = x => data.FirstOrDefault(e => e.Id == 1);

        // Act
        var result = expression.EndsWithFirstOrDefault(out var modified);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(modified);
    }

    [TestMethod]
    public void EndsWithFirstOrDefault_WithoutFirstOrDefault_ReturnsFalse()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> expression = x => data.Where(e => e.Id == 1);

        // Act
        var result = expression.EndsWithFirstOrDefault(out var modified);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(modified);
    }

    [TestMethod]
    public void EndsWithFirstOrDefault_WithDifferentMethod_ReturnsFalse()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, TestEntity?>> expression = x => data.Where(e => e.Id == 1).SingleOrDefault();

        // Act
        var result = expression.EndsWithFirstOrDefault(out var modified);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(modified);
    }

    [TestMethod]
    public void EndsWithFirstOrDefault_WithNonMethodCallExpression_ReturnsFalse()
    {
        // Arrange
        Expression<Func<object, TestEntity?>> expression = x => new TestEntity { Id = 1, Name = "Test" };

        // Act
        var result = expression.EndsWithFirstOrDefault(out var modified);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(modified);
    }

    // =================== Complex Chaining Tests ===================

    [TestMethod]
    public void ComplexChain_WhereOrderByTakeSkip_WorksCorrectly()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, bool>> whereQuery = x => x.IsActive;
        Expression<Func<TestEntity, decimal>> orderByQuery = x => x.Price;

        // Act
        var result = sourceQuery
            .Where(whereQuery)
            .OrderBy(orderByQuery)
            .Skip(1)
            .Take(1);
        var compiled = result.Compile();
        var filtered = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(1, filtered);
        Assert.AreEqual(30.00m, filtered[0].Price); // Product C (second active product by price)
    }

    [TestMethod]
    public void ComplexChain_OrderByThenByWithWhere_WorksCorrectly()
    {
        // Arrange
        var data = GetTestData();
        Expression<Func<object, IEnumerable<TestEntity>>> sourceQuery = x => data;
        Expression<Func<TestEntity, int>> orderByQuery = x => x.CategoryId;
        Expression<Func<TestEntity, int>> thenByQuery = x => x.SortOrder;
        Expression<Func<TestEntity, bool>> whereQuery = x => x.CategoryId <= 2;

        // Act
        var result = sourceQuery
            .Where(whereQuery)
            .OrderBy(orderByQuery)
            .ThenBy(thenByQuery);
        var compiled = result.Compile();
        var sorted = compiled(new object()).ToList();

        // Assert
        Assert.HasCount(4, sorted);
        Assert.AreEqual(3, sorted[0].Id); // Product C (Cat 1, Sort 2)
        Assert.AreEqual(1, sorted[1].Id); // Product A (Cat 1, Sort 3)
        Assert.AreEqual(2, sorted[2].Id); // Product B (Cat 2, Sort 1)
        Assert.AreEqual(5, sorted[3].Id); // Product E (Cat 2, Sort 5)
    }

    [TestMethod]
    public void ComplexChain_AndOrNotPredicates_WorksCorrectly()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate1 = x => x.IsActive;
        Expression<Func<TestEntity, bool>> predicate2 = x => x.Price > 25.00m;
        Expression<Func<TestEntity, bool>> predicate3 = x => x.CategoryId == 1;

        // Act - (IsActive AND Price > 25) OR CategoryId == 1
        var combined = predicate1.And(predicate2).Or(predicate3);
        var compiled = combined.Compile();
        var data = GetTestData();
        var filtered = data.Where(compiled).ToList();

        // Assert
        // Should include: (Active AND Price > 25: C, D) OR (CategoryId 1: A, C)
        // Result: A, C, D
        Assert.HasCount(3, filtered);
        Assert.IsTrue(filtered.Any(x => x.Id == 1)); // Product A
        Assert.IsTrue(filtered.Any(x => x.Id == 3)); // Product C
        Assert.IsTrue(filtered.Any(x => x.Id == 4)); // Product D
    }
}
