using System.Linq.Expressions;
using Bedrock.BuildingBlocks.Core.Utils;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Utils;

public class ExpressionUtilsTests : TestBase
{
    public ExpressionUtilsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Test Helper Classes

    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public TestEntity? Parent { get; set; }
    }

    private class DerivedEntity : TestEntity
    {
        public string Description { get; set; } = string.Empty;
    }

    #endregion

    #region GetProperty Tests

    [Fact]
    public void GetProperty_NullExpression_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null expression");
        Expression<Func<TestEntity, string>> expression = null!;

        // Act & Assert
        LogAct("Calling GetProperty with null");
        var exception = Should.Throw<ArgumentNullException>(
            () => ExpressionUtils.GetProperty(expression));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("expression");
    }

    [Fact]
    public void GetProperty_StringProperty_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing string property expression");

        // Act
        LogAct("Calling GetProperty");
        var propertyInfo = ExpressionUtils.GetProperty<TestEntity, string>(e => e.Name);

        // Assert
        LogAssert("Verifying property info");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("Name");
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.DeclaringType.ShouldBe(typeof(TestEntity));
    }

    [Fact]
    public void GetProperty_IntProperty_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing int property expression");

        // Act
        LogAct("Calling GetProperty");
        var propertyInfo = ExpressionUtils.GetProperty<TestEntity, int>(e => e.Age);

        // Assert
        LogAssert("Verifying property info");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("Age");
        propertyInfo.PropertyType.ShouldBe(typeof(int));
    }

    [Fact]
    public void GetProperty_DateTimeProperty_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing DateTime property expression");

        // Act
        LogAct("Calling GetProperty");
        var propertyInfo = ExpressionUtils.GetProperty<TestEntity, DateTime>(e => e.BirthDate);

        // Assert
        LogAssert("Verifying property info");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("BirthDate");
        propertyInfo.PropertyType.ShouldBe(typeof(DateTime));
    }

    [Fact]
    public void GetProperty_NullableReferenceProperty_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing nullable reference property expression");

        // Act
        LogAct("Calling GetProperty");
        var propertyInfo = ExpressionUtils.GetProperty<TestEntity, TestEntity?>(e => e.Parent);

        // Assert
        LogAssert("Verifying property info");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("Parent");
        propertyInfo.PropertyType.ShouldBe(typeof(TestEntity));
    }

    [Fact]
    public void GetProperty_WithObjectCast_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing expression with object cast (UnaryExpression)");

        // Act
        LogAct("Calling GetProperty with cast expression");
        var propertyInfo = ExpressionUtils.GetProperty<TestEntity, object>(e => e.Name);

        // Assert
        LogAssert("Verifying property info from unary expression");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("Name");
        propertyInfo.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void GetProperty_ValueTypeWithObjectCast_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing value type expression with object cast (boxing)");

        // Act
        LogAct("Calling GetProperty with boxing cast");
        var propertyInfo = ExpressionUtils.GetProperty<TestEntity, object>(e => e.Age);

        // Assert
        LogAssert("Verifying property info from boxing expression");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("Age");
        propertyInfo.PropertyType.ShouldBe(typeof(int));
    }

    [Fact]
    public void GetProperty_InheritedProperty_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing inherited property expression");

        // Act
        LogAct("Calling GetProperty for inherited property");
        var propertyInfo = ExpressionUtils.GetProperty<DerivedEntity, string>(e => e.Name);

        // Assert
        LogAssert("Verifying property info for inherited property");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("Name");
        propertyInfo.DeclaringType.ShouldBe(typeof(TestEntity));
    }

    [Fact]
    public void GetProperty_DerivedProperty_ShouldReturnPropertyInfo()
    {
        // Arrange
        LogArrange("Preparing derived class own property expression");

        // Act
        LogAct("Calling GetProperty for derived property");
        var propertyInfo = ExpressionUtils.GetProperty<DerivedEntity, string>(e => e.Description);

        // Assert
        LogAssert("Verifying property info for derived property");
        propertyInfo.ShouldNotBeNull();
        propertyInfo.Name.ShouldBe("Description");
        propertyInfo.DeclaringType.ShouldBe(typeof(DerivedEntity));
    }

    [Fact]
    public void GetProperty_ConstantExpression_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing constant expression (not a property selector)");

        // Act & Assert
        LogAct("Calling GetProperty with constant expression");
        var exception = Should.Throw<ArgumentException>(
            () => ExpressionUtils.GetProperty<TestEntity, string>(e => "constant"));

        LogAssert("Verifying exception message");
        exception.Message.ShouldContain("must select a property");
        exception.ParamName.ShouldBe("expression");
    }

    [Fact]
    public void GetProperty_MethodCallExpression_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing method call expression (not a property selector)");

        // Act & Assert
        LogAct("Calling GetProperty with method call expression");
        var exception = Should.Throw<ArgumentException>(
            () => ExpressionUtils.GetProperty<TestEntity, string>(e => e.Name.ToUpper()));

        LogAssert("Verifying exception message");
        exception.Message.ShouldContain("must select a property");
        exception.ParamName.ShouldBe("expression");
    }

    [Fact]
    public void GetProperty_BinaryExpression_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing binary expression (not a property selector)");

        // Act & Assert
        LogAct("Calling GetProperty with binary expression");
        var exception = Should.Throw<ArgumentException>(
            () => ExpressionUtils.GetProperty<TestEntity, int>(e => e.Age + 1));

        LogAssert("Verifying exception message");
        exception.Message.ShouldContain("must select a property");
        exception.ParamName.ShouldBe("expression");
    }

    [Fact]
    public void GetProperty_NewExpression_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing new expression (not a property selector)");

        // Act & Assert
        LogAct("Calling GetProperty with new expression");
        var exception = Should.Throw<ArgumentException>(
            () => ExpressionUtils.GetProperty<TestEntity, object>(e => new { e.Name }));

        LogAssert("Verifying exception message");
        exception.Message.ShouldContain("must select a property");
        exception.ParamName.ShouldBe("expression");
    }

    [Fact]
    public void GetProperty_ParameterExpression_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing parameter expression (returning the entity itself)");

        // Act & Assert
        LogAct("Calling GetProperty with parameter expression");
        var exception = Should.Throw<ArgumentException>(
            () => ExpressionUtils.GetProperty<TestEntity, TestEntity>(e => e));

        LogAssert("Verifying exception message");
        exception.Message.ShouldContain("must select a property");
        exception.ParamName.ShouldBe("expression");
    }

    #endregion
}
