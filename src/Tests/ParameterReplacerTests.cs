// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Globalization;
using System.Linq.Expressions;
using GraphQL.Linq.Expressions;

namespace Tests;

[TestClass]
public class ParameterReplacerTests
{
    // Test entity classes
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public TestEntity? Parent { get; set; }
        public TestChildEntity? Child { get; set; }
        public List<TestChildEntity> Children { get; set; } = new();
    }

    private sealed class TestChildEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ParentId { get; set; }
        public TestEntity? Parent { get; set; }
    }

    private sealed class Address
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? State { get; set; }
        public string ZipCode { get; set; } = null!;
    }

    private sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Address? PrimaryAddress { get; set; }
        public List<Address> Addresses { get; set; } = new();
    }

    // =================== Replace Method Tests ===================

    [TestMethod]
    public void Replace_WithNullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(TestEntity));
        var newBody = Expression.Constant(42);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ParameterReplacer.Replace(null!, parameter, newBody));
    }

    [TestMethod]
    public void Replace_WithNullOldParameter_ThrowsArgumentNullException()
    {
        // Arrange
        var expression = Expression.Constant(42);
        var newBody = Expression.Constant(100);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            expression.Replace(null!, newBody));
    }

    [TestMethod]
    public void Replace_WithNullNewBody_ThrowsArgumentNullException()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(TestEntity));
        var expression = Expression.Constant(42);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            expression.Replace(parameter, null!));
    }

    [TestMethod]
    public void Replace_WithLambdaExpression_ThrowsInvalidOperationException()
    {
        // Arrange
        Expression<Func<TestEntity, int>> lambda = x => x.Id;
        var parameter = Expression.Parameter(typeof(TestEntity));
        var newBody = Expression.Constant(42);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            lambda.Replace(parameter, newBody));
    }

    [TestMethod]
    public void Replace_SimplePropertyAccess_ReplacesParameter()
    {
        // Arrange
        var oldParameter = Expression.Parameter(typeof(TestEntity), "x");
        var propertyAccess = Expression.Property(oldParameter, nameof(TestEntity.Id));

        var newParameter = Expression.Parameter(typeof(TestEntity), "y");

        // Act
        var result = propertyAccess.Replace(oldParameter, newParameter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<MemberExpression>(result);
        var memberExpr = (MemberExpression)result;
        Assert.AreEqual(newParameter, memberExpr.Expression);
        Assert.AreEqual(nameof(TestEntity.Id), memberExpr.Member.Name);
    }

    [TestMethod]
    public void Replace_NestedPropertyAccess_ReplacesParameter()
    {
        // Arrange
        var oldParameter = Expression.Parameter(typeof(TestEntity), "x");
        var childAccess = Expression.Property(oldParameter, nameof(TestEntity.Child));
        var nameAccess = Expression.Property(childAccess, nameof(TestChildEntity.Name));

        var newParameter = Expression.Parameter(typeof(TestEntity), "y");

        // Act
        var result = nameAccess.Replace(oldParameter, newParameter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<MemberExpression>(result);
        var memberExpr = (MemberExpression)result;
        Assert.AreEqual(nameof(TestChildEntity.Name), memberExpr.Member.Name);

        // Check that the child access was also updated
        var childExpr = memberExpr.Expression as MemberExpression;
        Assert.IsNotNull(childExpr);
        Assert.AreEqual(newParameter, childExpr.Expression);
    }

    [TestMethod]
    public void Replace_BinaryExpression_ReplacesParameter()
    {
        // Arrange
        var oldParameter = Expression.Parameter(typeof(TestEntity), "x");
        var idAccess = Expression.Property(oldParameter, nameof(TestEntity.Id));
        var constant = Expression.Constant(42);
        var comparison = Expression.Equal(idAccess, constant);

        var newParameter = Expression.Parameter(typeof(TestEntity), "y");

        // Act
        var result = comparison.Replace(oldParameter, newParameter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<BinaryExpression>(result);
        var binaryExpr = (BinaryExpression)result;

        var leftMember = binaryExpr.Left as MemberExpression;
        Assert.IsNotNull(leftMember);
        Assert.AreEqual(newParameter, leftMember.Expression);
    }

    [TestMethod]
    public void Replace_MethodCallExpression_ReplacesParameter()
    {
        // Arrange
        var oldParameter = Expression.Parameter(typeof(TestEntity), "x");
        var nameAccess = Expression.Property(oldParameter, nameof(TestEntity.Name));
        var startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!;
        var constant = Expression.Constant("Test");
        var methodCall = Expression.Call(nameAccess, startsWithMethod, constant);

        var newParameter = Expression.Parameter(typeof(TestEntity), "y");

        // Act
        var result = methodCall.Replace(oldParameter, newParameter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<MethodCallExpression>(result);
        var methodExpr = (MethodCallExpression)result;

        var instanceMember = methodExpr.Object as MemberExpression;
        Assert.IsNotNull(instanceMember);
        Assert.AreEqual(newParameter, instanceMember.Expression);
    }

    [TestMethod]
    public void Replace_ConditionalExpression_ReplacesParameter()
    {
        // Arrange
        var oldParameter = Expression.Parameter(typeof(TestEntity), "x");
        var isActiveAccess = Expression.Property(oldParameter, nameof(TestEntity.IsActive));
        var idAccess = Expression.Property(oldParameter, nameof(TestEntity.Id));
        var constant = Expression.Constant(0);
        var conditional = Expression.Condition(isActiveAccess, idAccess, constant);

        var newParameter = Expression.Parameter(typeof(TestEntity), "y");

        // Act
        var result = conditional.Replace(oldParameter, newParameter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<ConditionalExpression>(result);
        var condExpr = (ConditionalExpression)result;

        var testMember = condExpr.Test as MemberExpression;
        Assert.IsNotNull(testMember);
        Assert.AreEqual(newParameter, testMember.Expression);

        var ifTrueMember = condExpr.IfTrue as MemberExpression;
        Assert.IsNotNull(ifTrueMember);
        Assert.AreEqual(newParameter, ifTrueMember.Expression);
    }

    [TestMethod]
    public void Replace_WithDifferentParameter_DoesNotReplace()
    {
        // Arrange
        var parameter1 = Expression.Parameter(typeof(TestEntity), "x");
        var parameter2 = Expression.Parameter(typeof(TestEntity), "y");
        var propertyAccess = Expression.Property(parameter1, nameof(TestEntity.Id));

        var newParameter = Expression.Parameter(typeof(TestEntity), "z");

        // Act - trying to replace parameter2, but expression uses parameter1
        var result = propertyAccess.Replace(parameter2, newParameter);

        // Assert - should remain unchanged
        Assert.IsNotNull(result);
        var memberExpr = result as MemberExpression;
        Assert.IsNotNull(memberExpr);
        Assert.AreEqual(parameter1, memberExpr.Expression); // Still uses parameter1
    }

    [TestMethod]
    public void Replace_ComplexExpression_ReplacesAllOccurrences()
    {
        // Arrange
        var oldParameter = Expression.Parameter(typeof(TestEntity), "x");
        var idAccess = Expression.Property(oldParameter, nameof(TestEntity.Id));
        var priceAccess = Expression.Property(oldParameter, nameof(TestEntity.Price));
        var isActiveAccess = Expression.Property(oldParameter, nameof(TestEntity.IsActive));

        // (x.Id > 10 && x.Price < 100) || x.IsActive
        var idComparison = Expression.GreaterThan(idAccess, Expression.Constant(10));
        var priceComparison = Expression.LessThan(priceAccess, Expression.Constant(100m));
        var andExpr = Expression.AndAlso(idComparison, priceComparison);
        var orExpr = Expression.OrElse(andExpr, isActiveAccess);

        var newParameter = Expression.Parameter(typeof(TestEntity), "y");

        // Act
        var result = orExpr.Replace(oldParameter, newParameter);

        // Assert
        Assert.IsNotNull(result);

        // Verify all parameter references were replaced
        var lambda = Expression.Lambda<Func<TestEntity, bool>>(result, newParameter);
        var compiled = lambda.Compile();

        var entity = new TestEntity { Id = 15, Price = 50m, IsActive = false };
        Assert.IsTrue(compiled(entity)); // Should be true because Id > 10 && Price < 100

        entity = new TestEntity { Id = 5, Price = 150m, IsActive = true };
        Assert.IsTrue(compiled(entity)); // Should be true because IsActive

        entity = new TestEntity { Id = 5, Price = 150m, IsActive = false };
        Assert.IsFalse(compiled(entity)); // Should be false
    }

    // =================== ChainWith Method Tests ===================

    [TestMethod]
    public void ChainWith_WithNullParentExpression_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<Address, string>> childExpression = addr => addr.Street;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ParameterReplacer.ChainWith<Customer, Address, string>(null!, childExpression));
    }

    [TestMethod]
    public void ChainWith_WithNullChildExpression_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = cust => cust.PrimaryAddress!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            parentExpression.ChainWith<Customer, Address, string>(null!));
    }

    [TestMethod]
    public void ChainWith_SimpleChain_CombinesExpressions()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = customer => customer.PrimaryAddress!;
        Expression<Func<Address, string>> childExpression = address => address.Street;

        // Act
        var result = parentExpression.ChainWith(childExpression);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();

        var customer = new Customer {
            Id = 1,
            Name = "John Doe",
            PrimaryAddress = new Address { Street = "123 Main St", City = "Springfield", ZipCode = "12345" }
        };

        var street = compiled(customer);
        Assert.AreEqual("123 Main St", street);
    }

    [TestMethod]
    public void ChainWith_MultipleChains_CombinesExpressions()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = customer => customer.PrimaryAddress!;
        Expression<Func<Address, string>> childExpression1 = address => address.Street;
        Expression<Func<string, int>> childExpression2 = street => street.Length;

        // Act
        var intermediate = parentExpression.ChainWith(childExpression1);
        var result = intermediate.ChainWith(childExpression2);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();

        var customer = new Customer {
            Id = 1,
            Name = "John Doe",
            PrimaryAddress = new Address { Street = "123 Main St", City = "Springfield", ZipCode = "12345" }
        };

        var length = compiled(customer);
        Assert.AreEqual(11, length); // "123 Main St".Length
    }

    [TestMethod]
    public void ChainWith_WithMethodCall_CombinesExpressions()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = customer => customer.PrimaryAddress!;
        Expression<Func<Address, string>> childExpression = address => address.Street.ToUpper(CultureInfo.InvariantCulture);

        // Act
        var result = parentExpression.ChainWith(childExpression);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();

        var customer = new Customer {
            Id = 1,
            Name = "John Doe",
            PrimaryAddress = new Address { Street = "Main Street", City = "Springfield", ZipCode = "12345" }
        };

        var upperStreet = compiled(customer);
        Assert.AreEqual("MAIN STREET", upperStreet);
    }

    [TestMethod]
    public void ChainWith_WithComplexChild_CombinesExpressions()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = customer => customer.PrimaryAddress!;
        Expression<Func<Address, bool>> childExpression = address =>
            address.Street.StartsWith("123") && address.City == "Springfield";

        // Act
        var result = parentExpression.ChainWith(childExpression);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();

        var customer1 = new Customer {
            Id = 1,
            Name = "John Doe",
            PrimaryAddress = new Address { Street = "123 Main St", City = "Springfield", ZipCode = "12345" }
        };
        Assert.IsTrue(compiled(customer1));

        var customer2 = new Customer {
            Id = 2,
            Name = "Jane Doe",
            PrimaryAddress = new Address { Street = "456 Oak Ave", City = "Springfield", ZipCode = "12345" }
        };
        Assert.IsFalse(compiled(customer2));
    }

    // =================== Chain Method Tests ===================

    [TestMethod]
    public void Chain_WithNullParentExpression_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<Address, string>> childExpression = addr => addr.Street;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ParameterReplacer.Chain(null!, childExpression));
    }

    [TestMethod]
    public void Chain_WithNullChildExpression_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = cust => cust.PrimaryAddress!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ParameterReplacer.Chain(parentExpression, null!));
    }

    [TestMethod]
    public void Chain_WithVoidParent_ThrowsArgumentException()
    {
        // Arrange
        var parameter = Expression.Parameter(typeof(Customer));
        var body = Expression.Empty(); // void expression
        var parentExpression = Expression.Lambda(body, parameter);

        Expression<Func<Address, string>> childExpression = addr => addr.Street;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ParameterReplacer.Chain(parentExpression, childExpression));
    }

    [TestMethod]
    public void Chain_WithMismatchedTypes_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<Customer, string>> parentExpression = cust => cust.Name; // Returns string
        Expression<Func<Address, string>> childExpression = addr => addr.Street; // Expects Address

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ParameterReplacer.Chain(parentExpression, childExpression));
    }

    [TestMethod]
    public void Chain_WithMultipleParentParameters_ChainsCorrectly()
    {
        // Arrange
        var param1 = Expression.Parameter(typeof(Customer), "cust");
        var param2 = Expression.Parameter(typeof(int), "index");
        var addressesAccess = Expression.Property(param1, nameof(Customer.Addresses));
        var indexAccess = Expression.Property(addressesAccess, "Item", param2);
        var parentExpression = Expression.Lambda<Func<Customer, int, Address>>(indexAccess, param1, param2);

        Expression<Func<Address, string>> childExpression = addr => addr.City;

        // Act
        var result = ParameterReplacer.Chain(parentExpression, childExpression);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result.Parameters); // Should preserve both parameters

        var compiled = result.Compile();
        var customer = new Customer {
            Id = 1,
            Name = "John Doe",
            Addresses = new List<Address>
            {
                new() { Street = "123 Main St", City = "Springfield", ZipCode = "12345" },
                new() { Street = "456 Oak Ave", City = "Shelbyville", ZipCode = "67890" }
            }
        };

        var city = compiled.DynamicInvoke(customer, 1) as string;
        Assert.AreEqual("Shelbyville", city);
    }

    [TestMethod]
    public void Chain_WithActionChild_ChainsCorrectly()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = cust => cust.PrimaryAddress!;

        var addrParam = Expression.Parameter(typeof(Address));
        var consoleWriteLine = typeof(Console).GetMethod(nameof(Console.WriteLine), new[] { typeof(string) })!;
        var streetAccess = Expression.Property(addrParam, nameof(Address.Street));
        var writeCall = Expression.Call(consoleWriteLine, streetAccess);
        var childExpression = Expression.Lambda<Action<Address>>(writeCall, addrParam);

        // Act
        var result = ParameterReplacer.Chain(parentExpression, childExpression);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(void), result.ReturnType);
        Assert.HasCount(1, result.Parameters);
        Assert.AreEqual(typeof(Customer), result.Parameters[0].Type);
    }

    [TestMethod]
    public void Chain_SimpleChain_CombinesExpressions()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = customer => customer.PrimaryAddress!;
        Expression<Func<Address, string>> childExpression = address => address.Street;

        // Act
        var result = ParameterReplacer.Chain(parentExpression, childExpression);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();

        var customer = new Customer {
            Id = 1,
            Name = "John Doe",
            PrimaryAddress = new Address { Street = "789 Elm St", City = "Capital City", ZipCode = "11111" }
        };

        var street = compiled.DynamicInvoke(customer) as string;
        Assert.AreEqual("789 Elm St", street);
    }

    [TestMethod]
    public void Chain_WithNoParentParameters_ChainsCorrectly()
    {
        // Arrange
        var constantAddress = new Address { Street = "Constant St", City = "Constant City", ZipCode = "00000" };
        Expression<Func<Address>> parentExpression = () => constantAddress;
        Expression<Func<Address, string>> childExpression = address => address.City;

        // Act
        var result = ParameterReplacer.Chain(parentExpression, childExpression);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result.Parameters);

        var compiled = result.Compile();
        var city = compiled.DynamicInvoke() as string;
        Assert.AreEqual("Constant City", city);
    }

    [TestMethod]
    public void Chain_WithComplexExpressions_ChainsCorrectly()
    {
        // Arrange
        Expression<Func<Customer, Address>> parentExpression = customer =>
            customer.Addresses.FirstOrDefault(a => a.City == "Springfield")!;
        Expression<Func<Address, string>> childExpression = address =>
            $"{address.Street}, {address.City}";

        // Act
        var result = ParameterReplacer.Chain(parentExpression, childExpression);

        // Assert
        Assert.IsNotNull(result);
        var compiled = result.Compile();

        var customer = new Customer {
            Id = 1,
            Name = "John Doe",
            Addresses = new List<Address>
            {
                new() { Street = "123 Main St", City = "Springfield", ZipCode = "12345" },
                new() { Street = "456 Oak Ave", City = "Shelbyville", ZipCode = "67890" }
            }
        };

        var fullAddress = compiled.DynamicInvoke(customer) as string;
        Assert.AreEqual("123 Main St, Springfield", fullAddress);
    }
}
