// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using System;
using System.Linq.Expressions;
using GraphQL.Linq;
using GraphQL.Types;
using TestDb.Models;

namespace Tests;

[TestClass]
public class TypeHelperTests
{
    // Test class with various property types for testing
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? NullableName { get; set; }
        public int NonNullableInt { get; set; }
        public int? NullableInt { get; set; }
        public decimal Price { get; set; }
        public decimal? NullablePrice { get; set; }
        public bool IsActive { get; set; }
        public bool? NullableIsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public TestEntity? Parent { get; set; }
        public TestEntity Child { get; set; } = null!;

        // Fields for testing field expressions
        public string NonNullableField = null!;
        public string? NullableField;
        public int NonNullableIntField;
        public int? NullableIntField;

        // Methods for testing method expressions
        public string GetName() => Name;
        public string? GetNullableName() => NullableName;
        public int GetId() => Id;
        public int? GetNullableInt() => NullableInt;
    }

    // =================== GetNullable Tests ===================

    [TestMethod]
    public void GetNullable_NonNullableReferenceType_ReturnsFalse()
    {
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var result = TypeHelper.GetNullable<string>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableReferenceType_ReturnsTrue()
    {
        Expression<Func<TestEntity, string?>> expression = x => x.NullableName;
        var result = TypeHelper.GetNullable<string?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NonNullableValueType_ReturnsFalse()
    {
        Expression<Func<TestEntity, int>> expression = x => x.NonNullableInt;
        var result = TypeHelper.GetNullable<int>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableValueType_ReturnsTrue()
    {
        Expression<Func<TestEntity, int?>> expression = x => x.NullableInt;
        var result = TypeHelper.GetNullable<int?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NullableDecimal_ReturnsTrue()
    {
        Expression<Func<TestEntity, decimal?>> expression = x => x.NullablePrice;
        var result = TypeHelper.GetNullable<decimal?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NonNullableDecimal_ReturnsFalse()
    {
        Expression<Func<TestEntity, decimal>> expression = x => x.Price;
        var result = TypeHelper.GetNullable<decimal>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableBool_ReturnsTrue()
    {
        Expression<Func<TestEntity, bool?>> expression = x => x.NullableIsActive;
        var result = TypeHelper.GetNullable<bool?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NonNullableBool_ReturnsFalse()
    {
        Expression<Func<TestEntity, bool>> expression = x => x.IsActive;
        var result = TypeHelper.GetNullable<bool>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableDateTime_ReturnsTrue()
    {
        Expression<Func<TestEntity, DateTime?>> expression = x => x.UpdatedAt;
        var result = TypeHelper.GetNullable<DateTime?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NonNullableDateTime_ReturnsFalse()
    {
        Expression<Func<TestEntity, DateTime>> expression = x => x.CreatedAt;
        var result = TypeHelper.GetNullable<DateTime>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableNavigationProperty_ReturnsTrue()
    {
        Expression<Func<TestEntity, TestEntity?>> expression = x => x.Parent;
        var result = TypeHelper.GetNullable<TestEntity?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NonNullableNavigationProperty_ReturnsFalse()
    {
        Expression<Func<TestEntity, TestEntity>> expression = x => x.Child;
        var result = TypeHelper.GetNullable<TestEntity>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NonMemberExpression_ReturnsFalse()
    {
        Expression<Func<TestEntity, int>> expression = x => 42;
        var result = TypeHelper.GetNullable<int>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NonMemberExpression_NullableValueType_ReturnsTrue()
    {
        Expression<Func<TestEntity, int?>> expression = x => (int?)42;
        var result = TypeHelper.GetNullable<int?>(expression);
        Assert.IsTrue(result);
    }

    // =================== GetGraphType Tests ===================

    [TestMethod]
    public void GetGraphType_WithExplicitType_ReturnsProvidedType()
    {
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var result = TypeHelper.GetGraphType<string>("TestEntity", "name", expression, null, typeof(StringGraphType));
        Assert.AreEqual(typeof(StringGraphType), result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableString_ReturnsNonNullStringGraphType()
    {
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var result = TypeHelper.GetGraphType<string>("TestEntity", "name", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<string>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableString_ReturnsStringGraphType()
    {
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var result = TypeHelper.GetGraphType<string>("TestEntity", "name", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<string>), result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableInt_ReturnsNonNullIntGraphType()
    {
        Expression<Func<TestEntity, int>> expression = x => x.NonNullableInt;
        var result = TypeHelper.GetGraphType<int>("TestEntity", "nonNullableInt", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableInt_ReturnsIntGraphType()
    {
        Expression<Func<TestEntity, int?>> expression = x => x.NullableInt;
        var result = TypeHelper.GetGraphType<int?>("TestEntity", "nullableInt", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<int>), result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableDecimal_ReturnsNonNullDecimalGraphType()
    {
        Expression<Func<TestEntity, decimal>> expression = x => x.Price;
        var result = TypeHelper.GetGraphType<decimal>("TestEntity", "price", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<decimal>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableDecimal_ReturnsDecimalGraphType()
    {
        Expression<Func<TestEntity, decimal?>> expression = x => x.NullablePrice;
        var result = TypeHelper.GetGraphType<decimal?>("TestEntity", "nullablePrice", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<decimal>), result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableBool_ReturnsNonNullBooleanGraphType()
    {
        Expression<Func<TestEntity, bool>> expression = x => x.IsActive;
        var result = TypeHelper.GetGraphType<bool>("TestEntity", "isActive", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<bool>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableBool_ReturnsBooleanGraphType()
    {
        Expression<Func<TestEntity, bool?>> expression = x => x.NullableIsActive;
        var result = TypeHelper.GetGraphType<bool?>("TestEntity", "nullableIsActive", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<bool>), result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableDateTime_ReturnsNonNullDateTimeGraphType()
    {
        Expression<Func<TestEntity, DateTime>> expression = x => x.CreatedAt;
        var result = TypeHelper.GetGraphType<DateTime>("TestEntity", "createdAt", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<DateTime>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableDateTime_ReturnsDateTimeGraphType()
    {
        Expression<Func<TestEntity, DateTime?>> expression = x => x.UpdatedAt;
        var result = TypeHelper.GetGraphType<DateTime?>("TestEntity", "updatedAt", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<DateTime>), result);
    }

    [TestMethod]
    public void GetGraphType_ForceId_NonNullable_ReturnsNonNullIdGraphType()
    {
        Expression<Func<TestEntity, int>> expression = x => x.Id;
        var result = TypeHelper.GetGraphType<int>("TestEntity", "id", expression, false, null, forceId: true);
        Assert.AreEqual(typeof(NonNullGraphType<IdGraphType>), result);
    }

    [TestMethod]
    public void GetGraphType_ForceId_Nullable_ReturnsIdGraphType()
    {
        Expression<Func<TestEntity, int?>> expression = x => x.NullableInt;
        var result = TypeHelper.GetGraphType<int?>("TestEntity", "id", expression, true, null, forceId: true);
        Assert.AreEqual(typeof(IdGraphType), result);
    }

    [TestMethod]
    public void GetGraphType_NullableNotSpecified_DefaultsToNonNull()
    {
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var result = TypeHelper.GetGraphType<string>("TestEntity", "name", expression, null, null);
        // When nullable is null and NRT is disabled or not inferred, it defaults to non-null (false)
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<string>>), result);
    }

    [TestMethod]
    public void GetGraphType_NonMemberExpression_WithNullableValueType_ReturnsIntGraphType()
    {
        Expression<Func<TestEntity, int?>> expression = x => (int?)42;
        var result = TypeHelper.GetGraphType<int?>("TestEntity", "computed", expression, null, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<int>), result);
    }

    [TestMethod]
    public void GetGraphType_NonMemberExpression_WithNonNullableValueType_ReturnsNonNullIntGraphType()
    {
        Expression<Func<TestEntity, int>> expression = x => 42;
        var result = TypeHelper.GetGraphType<int>("TestEntity", "computed", expression, null, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>), result);
    }

    [TestMethod]
    public void GetGraphType_UnsupportedType_DoesNotThrow()
    {
        Expression<Func<TestEntity, TestEntity>> expression = x => x.Child;
        // GetGraphTypeFromType will return a GraphQLClrOutputTypeReference for custom types
        var result = TypeHelper.GetGraphType<TestEntity>("TestEntity", "child", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<TestEntity>>), result);
    }

    [TestMethod]
    public void GetGraphType_ExplicitTypeOverridesNullability()
    {
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var result = TypeHelper.GetGraphType<string>("TestEntity", "name", expression, true, typeof(NonNullGraphType<StringGraphType>));
        // Explicit type should be returned regardless of nullable parameter
        Assert.AreEqual(typeof(NonNullGraphType<StringGraphType>), result);
    }

    [TestMethod]
    public void GetGraphType_ExplicitTypeOverridesForceId()
    {
        Expression<Func<TestEntity, int>> expression = x => x.Id;
        var result = TypeHelper.GetGraphType<int>("TestEntity", "id", expression, false, typeof(IntGraphType), forceId: true);
        // Explicit type should be returned regardless of forceId parameter
        Assert.AreEqual(typeof(IntGraphType), result);
    }

    // =================== Field Expression Tests ===================

    [TestMethod]
    public void GetNullable_NonNullableField_ReturnsFalse()
    {
        Expression<Func<TestEntity, string>> expression = x => x.NonNullableField;
        var result = TypeHelper.GetNullable<string>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableField_ReturnsTrue()
    {
        Expression<Func<TestEntity, string?>> expression = x => x.NullableField;
        var result = TypeHelper.GetNullable<string?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NonNullableIntField_ReturnsFalse()
    {
        Expression<Func<TestEntity, int>> expression = x => x.NonNullableIntField;
        var result = TypeHelper.GetNullable<int>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableIntField_ReturnsTrue()
    {
        Expression<Func<TestEntity, int?>> expression = x => x.NullableIntField;
        var result = TypeHelper.GetNullable<int?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableField_ReturnsNonNullStringGraphType()
    {
        Expression<Func<TestEntity, string>> expression = x => x.NonNullableField;
        var result = TypeHelper.GetGraphType<string>("TestEntity", "nonNullableField", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<string>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableField_ReturnsStringGraphType()
    {
        Expression<Func<TestEntity, string?>> expression = x => x.NullableField;
        var result = TypeHelper.GetGraphType<string?>("TestEntity", "nullableField", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<string>), result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableIntField_ReturnsNonNullIntGraphType()
    {
        Expression<Func<TestEntity, int>> expression = x => x.NonNullableIntField;
        var result = TypeHelper.GetGraphType<int>("TestEntity", "nonNullableIntField", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableIntField_ReturnsIntGraphType()
    {
        Expression<Func<TestEntity, int?>> expression = x => x.NullableIntField;
        var result = TypeHelper.GetGraphType<int?>("TestEntity", "nullableIntField", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<int>), result);
    }

    // =================== Method Expression Tests ===================

    [TestMethod]
    public void GetNullable_NonNullableMethod_ReturnsFalse()
    {
        Expression<Func<TestEntity, string>> expression = x => x.GetName();
        var result = TypeHelper.GetNullable<string>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableMethod_ReturnsTrue()
    {
        Expression<Func<TestEntity, string?>> expression = x => x.GetNullableName();
        var result = TypeHelper.GetNullable<string?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNullable_NonNullableIntMethod_ReturnsFalse()
    {
        Expression<Func<TestEntity, int>> expression = x => x.GetId();
        var result = TypeHelper.GetNullable<int>(expression);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetNullable_NullableIntMethod_ReturnsTrue()
    {
        Expression<Func<TestEntity, int?>> expression = x => x.GetNullableInt();
        var result = TypeHelper.GetNullable<int?>(expression);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableMethod_ReturnsNonNullStringGraphType()
    {
        Expression<Func<TestEntity, string>> expression = x => x.GetName();
        var result = TypeHelper.GetGraphType<string>("TestEntity", "getName", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<string>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableMethod_ReturnsStringGraphType()
    {
        Expression<Func<TestEntity, string?>> expression = x => x.GetNullableName();
        var result = TypeHelper.GetGraphType<string?>("TestEntity", "getNullableName", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<string>), result);
    }

    [TestMethod]
    public void GetGraphType_NonNullableIntMethod_ReturnsNonNullIntGraphType()
    {
        Expression<Func<TestEntity, int>> expression = x => x.GetId();
        var result = TypeHelper.GetGraphType<int>("TestEntity", "getId", expression, false, null);
        Assert.AreEqual(typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>), result);
    }

    [TestMethod]
    public void GetGraphType_NullableIntMethod_ReturnsIntGraphType()
    {
        Expression<Func<TestEntity, int?>> expression = x => x.GetNullableInt();
        var result = TypeHelper.GetGraphType<int?>("TestEntity", "getNullableInt", expression, true, null);
        Assert.AreEqual(typeof(GraphQLClrOutputTypeReference<int>), result);
    }

    [TestMethod]
    public void GetGraphType_MethodWithForceId_ReturnsIdGraphType()
    {
        Expression<Func<TestEntity, int>> expression = x => x.GetId();
        var result = TypeHelper.GetGraphType<int>("TestEntity", "getId", expression, false, null, forceId: true);
        Assert.AreEqual(typeof(NonNullGraphType<IdGraphType>), result);
    }

    [TestMethod]
    public void GetGraphType_FieldWithForceId_ReturnsIdGraphType()
    {
        Expression<Func<TestEntity, int>> expression = x => x.NonNullableIntField;
        var result = TypeHelper.GetGraphType<int>("TestEntity", "nonNullableIntField", expression, false, null, forceId: true);
        Assert.AreEqual(typeof(NonNullGraphType<IdGraphType>), result);
    }
}
