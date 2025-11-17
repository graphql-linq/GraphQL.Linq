// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Linq.Expressions;
using GraphQL.Linq.Expressions;
using GraphQL.Linq.GraphApi;

namespace Tests;

[TestClass]
public class LambdaBuilderTests
{
    // Test entity classes
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public TestEntity? Parent { get; set; }
        public List<TestChildEntity> Children { get; set; } = new();
        public IEnumerable<TestChildEntity> ChildrenEnumerable => Children;
    }

    private sealed class TestChildEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ParentId { get; set; }
    }

    // =================== Build Method Tests ===================

    [TestMethod]
    public void Build_WithNullFields_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LambdaBuilder.Build<TestEntity>(null!));
    }

    [TestMethod]
    public void Build_WithEmptyFields_ReturnsConstantExpression()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>();

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var efSource = compiled(entity);
        Assert.IsNotNull(efSource);
        Assert.IsEmpty(efSource);
    }

    [TestMethod]
    public void Build_WithSingleField_CreatesValidExpression()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("id", x => x.Id)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity { Id = 42, Name = "Test" };
        var efSource = compiled(entity);
        Assert.HasCount(1, efSource);
        Assert.IsTrue(efSource.ContainsKey("id"));
        Assert.AreEqual(42, efSource["id"]);
    }

    [TestMethod]
    public void Build_WithMultipleFields_CreatesValidExpression()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("id", x => x.Id),
            new("name", x => x.Name),
            new("price", x => x.Price),
            new("isActive", x => x.IsActive)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity {
            Id = 42,
            Name = "Test Product",
            Price = 99.99m,
            IsActive = true
        };
        var efSource = compiled(entity);
        Assert.HasCount(4, efSource);
        Assert.AreEqual(42, efSource["id"]);
        Assert.AreEqual("Test Product", efSource["name"]);
        Assert.AreEqual(99.99m, efSource["price"]);
        Assert.IsTrue((bool)efSource["isActive"]!);
    }

    [TestMethod]
    public void Build_WithNullableField_HandlesNullValue()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("description", x => x.Description!)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity { Id = 1, Name = "Test", Description = null };
        var efSource = compiled(entity);
        Assert.HasCount(1, efSource);
        Assert.IsTrue(efSource.ContainsKey("description"));
        Assert.IsNull(efSource["description"]);
    }

    [TestMethod]
    public void Build_WithNullableField_HandlesNonNullValue()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("description", x => x.Description!)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity { Id = 1, Name = "Test", Description = "A description" };
        var efSource = compiled(entity);
        Assert.HasCount(1, efSource);
        Assert.AreEqual("A description", efSource["description"]);
    }

    [TestMethod]
    public void Build_WithNavigationProperty_CreatesValidExpression()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("parent", x => x.Parent!)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var parent = new TestEntity { Id = 1, Name = "Parent" };
        var entity = new TestEntity { Id = 2, Name = "Child", Parent = parent };
        var efSource = compiled(entity);
        Assert.HasCount(1, efSource);
        Assert.AreSame(parent, efSource["parent"]);
    }

    [TestMethod]
    public void Build_WithDateTimeField_CreatesValidExpression()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("createdAt", x => x.CreatedAt)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var now = DateTime.UtcNow;
        var entity = new TestEntity { Id = 1, Name = "Test", CreatedAt = now };
        var efSource = compiled(entity);
        Assert.HasCount(1, efSource);
        Assert.AreEqual(now, efSource["createdAt"]);
    }

    [TestMethod]
    public void Build_WithComplexExpression_CreatesValidExpression()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("computed", x => x.Price * 1.1m)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity { Id = 1, Name = "Test", Price = 100m };
        var efSource = compiled(entity);
        Assert.HasCount(1, efSource);
        Assert.AreEqual(110m, efSource["computed"]);
    }

    [TestMethod]
    public void Build_WithDuplicateKeys_ThrowsArgumentException()
    {
        // Arrange
        var fields = new List<KeyValuePair<string, Expression<Func<TestEntity, object>>>>
        {
            new("value", x => x.Id),
            new("value", x => x.Name)
        };

        // Act
        var result = LambdaBuilder.Build(fields);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity { Id = 42, Name = "Test" };
        Assert.Throws<ArgumentException>(() => {
            compiled(entity);
        });
    }

    // =================== BuildNavigation Method Tests ===================

    [TestMethod]
    public void BuildNavigation_WithNullMediasSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestChildEntity, EfSource<TestChildEntity>>> mediasExp = x => new EfSource<TestChildEntity>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LambdaBuilder.BuildNavigation<TestEntity, TestChildEntity>(null!, mediasExp));
    }

    [TestMethod]
    public void BuildNavigation_WithNullMediasExp_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, IEnumerable<TestChildEntity>>> mediasSelector = x => x.Children;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            LambdaBuilder.BuildNavigation(mediasSelector, null!));
    }

    [TestMethod]
    public void BuildNavigation_WithValidInputs_CreatesValidExpression()
    {
        // Arrange
        Expression<Func<TestEntity, IEnumerable<TestChildEntity>>> mediasSelector = x => x.Children;
        Expression<Func<TestChildEntity, EfSource<TestChildEntity>>> mediasExp = child => new EfSource<TestChildEntity>
        {
            { "id", child.Id },
            { "name", child.Name }
        };

        // Act
        var result = LambdaBuilder.BuildNavigation(mediasSelector, mediasExp);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity {
            Id = 1,
            Name = "Parent",
            Children = new List<TestChildEntity>
            {
                new() { Id = 10, Name = "Child1", ParentId = 1 },
                new() { Id = 20, Name = "Child2", ParentId = 1 }
            }
        };
        var resultValue = compiled(entity);
        Assert.IsNotNull(resultValue);

        // The result should be an IEnumerable<EfSource<TestChildEntity>>
        var enumerable = resultValue as IEnumerable<EfSource<TestChildEntity>>;
        Assert.IsNotNull(enumerable);
        var list = enumerable.ToList();
        Assert.HasCount(2, list);
        Assert.AreEqual(10, list[0]["id"]);
        Assert.AreEqual("Child1", list[0]["name"]);
        Assert.AreEqual(20, list[1]["id"]);
        Assert.AreEqual("Child2", list[1]["name"]);
    }

    [TestMethod]
    public void BuildNavigation_WithEmptyCollection_ReturnsEmptyEnumerable()
    {
        // Arrange
        Expression<Func<TestEntity, IEnumerable<TestChildEntity>>> mediasSelector = x => x.Children;
        Expression<Func<TestChildEntity, EfSource<TestChildEntity>>> mediasExp = child => new EfSource<TestChildEntity>
        {
            { "id", child.Id }
        };

        // Act
        var result = LambdaBuilder.BuildNavigation(mediasSelector, mediasExp);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity {
            Id = 1,
            Name = "Parent",
            Children = new List<TestChildEntity>()
        };
        var resultValue = compiled(entity);
        var enumerable = resultValue as IEnumerable<EfSource<TestChildEntity>>;
        Assert.IsNotNull(enumerable);
        Assert.IsEmpty(enumerable);
    }

    [TestMethod]
    public void BuildNavigation_WithIEnumerableProperty_CreatesValidExpression()
    {
        // Arrange
        Expression<Func<TestEntity, IEnumerable<TestChildEntity>>> mediasSelector = x => x.ChildrenEnumerable;
        Expression<Func<TestChildEntity, EfSource<TestChildEntity>>> mediasExp = child => new EfSource<TestChildEntity>
        {
            { "id", child.Id },
            { "name", child.Name }
        };

        // Act
        var result = LambdaBuilder.BuildNavigation(mediasSelector, mediasExp);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity {
            Id = 1,
            Name = "Parent",
            Children = new List<TestChildEntity>
            {
                new() { Id = 100, Name = "TestChild", ParentId = 1 }
            }
        };
        var resultValue = compiled(entity);
        var enumerable = resultValue as IEnumerable<EfSource<TestChildEntity>>;
        Assert.IsNotNull(enumerable);
        var list = enumerable.ToList();
        Assert.HasCount(1, list);
        Assert.AreEqual(100, list[0]["id"]);
        Assert.AreEqual("TestChild", list[0]["name"]);
    }

    [TestMethod]
    public void BuildNavigation_WithComplexMediasExp_CreatesValidExpression()
    {
        // Arrange
        Expression<Func<TestEntity, IEnumerable<TestChildEntity>>> mediasSelector = x => x.Children;
        Expression<Func<TestChildEntity, EfSource<TestChildEntity>>> mediasExp = child => new EfSource<TestChildEntity>
        {
            { "id", child.Id },
            { "name", child.Name },
            { "parentId", child.ParentId },
            { "computed", child.Id * 10 }
        };

        // Act
        var result = LambdaBuilder.BuildNavigation(mediasSelector, mediasExp);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity {
            Id = 1,
            Name = "Parent",
            Children = new List<TestChildEntity>
            {
                new() { Id = 5, Name = "Child", ParentId = 1 }
            }
        };
        var resultValue = compiled(entity);
        var enumerable = resultValue as IEnumerable<EfSource<TestChildEntity>>;
        Assert.IsNotNull(enumerable);
        var list = enumerable.ToList();
        Assert.HasCount(1, list);
        Assert.AreEqual(5, list[0]["id"]);
        Assert.AreEqual("Child", list[0]["name"]);
        Assert.AreEqual(1, list[0]["parentId"]);
        Assert.AreEqual(50, list[0]["computed"]);
    }

    [TestMethod]
    public void BuildNavigation_WithWhereClause_CreatesValidExpression()
    {
        // Arrange
        Expression<Func<TestEntity, IEnumerable<TestChildEntity>>> mediasSelector = x => x.Children.Where(c => c.Id > 15);
        Expression<Func<TestChildEntity, EfSource<TestChildEntity>>> mediasExp = child => new EfSource<TestChildEntity>
        {
            { "id", child.Id }
        };

        // Act
        var result = LambdaBuilder.BuildNavigation(mediasSelector, mediasExp);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();
        var entity = new TestEntity {
            Id = 1,
            Name = "Parent",
            Children = new List<TestChildEntity>
            {
                new() { Id = 10, Name = "Child1", ParentId = 1 },
                new() { Id = 20, Name = "Child2", ParentId = 1 },
                new() { Id = 30, Name = "Child3", ParentId = 1 }
            }
        };
        var resultValue = compiled(entity);
        var enumerable = resultValue as IEnumerable<EfSource<TestChildEntity>>;
        Assert.IsNotNull(enumerable);
        var list = enumerable.ToList();
        Assert.HasCount(2, list);
        Assert.AreEqual(20, list[0]["id"]);
        Assert.AreEqual(30, list[1]["id"]);
    }
}
