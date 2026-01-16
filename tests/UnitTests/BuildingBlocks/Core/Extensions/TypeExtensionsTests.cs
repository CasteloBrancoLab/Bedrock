using Bedrock.BuildingBlocks.Core.Extensions;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Extensions;

public class TypeExtensionsTests : TestBase
{
    public TypeExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Test Helper Classes

    private static class TestClassWithConstants
    {
        public const string Constant1 = "Value1";
        public const string Constant2 = "Value2";
        public const string Constant3 = "Value3";
    }

    private static class TestClassWithMixedMembers
    {
        public const string StringConstant = "StringValue";
        public const int IntConstant = 42;
        public static readonly string ReadOnlyField = "ReadOnly";
        public static string StaticField = "Static";
        private const string PrivateConstant = "Private";
    }

    private static class TestClassWithNoConstants
    {
        public static string Field = "NotConstant";
        public static readonly string ReadOnly = "ReadOnly";
    }

    private static class EmptyTestClass
    {
    }

    private class DerivedClassWithConstants : BaseClassWithConstants
    {
        public const string DerivedConstant = "DerivedValue";
    }

    private class BaseClassWithConstants
    {
        public const string BaseConstant = "BaseValue";
    }

    #endregion

    #region GetAllPublicConstantStringValues Tests

    [Fact]
    public void GetAllPublicConstantStringValues_NullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null type");
        Type type = null!;

        // Act & Assert
        LogAct("Calling GetAllPublicConstantStringValues");
        var exception = Should.Throw<ArgumentNullException>(() => type.GetAllPublicConstantStringValues());

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("type");
    }

    [Fact]
    public void GetAllPublicConstantStringValues_TypeWithStringConstants_ShouldReturnAllValues()
    {
        // Arrange
        LogArrange("Preparing type with string constants");
        var type = typeof(TestClassWithConstants);

        // Act
        LogAct("Calling GetAllPublicConstantStringValues");
        var result = type.GetAllPublicConstantStringValues();

        // Assert
        LogAssert("Verifying all constants returned");
        result.Length.ShouldBe(3);
        result.ShouldContain("Value1");
        result.ShouldContain("Value2");
        result.ShouldContain("Value3");
    }

    [Fact]
    public void GetAllPublicConstantStringValues_TypeWithMixedMembers_ShouldReturnOnlyPublicStringConstants()
    {
        // Arrange
        LogArrange("Preparing type with mixed members");
        var type = typeof(TestClassWithMixedMembers);

        // Act
        LogAct("Calling GetAllPublicConstantStringValues");
        var result = type.GetAllPublicConstantStringValues();

        // Assert
        LogAssert("Verifying only public string constants returned");
        result.Length.ShouldBe(1);
        result.ShouldContain("StringValue");
        result.ShouldNotContain("ReadOnly");
        result.ShouldNotContain("Static");
        result.ShouldNotContain("Private");
    }

    [Fact]
    public void GetAllPublicConstantStringValues_TypeWithNoConstants_ShouldReturnEmptyArray()
    {
        // Arrange
        LogArrange("Preparing type with no constants");
        var type = typeof(TestClassWithNoConstants);

        // Act
        LogAct("Calling GetAllPublicConstantStringValues");
        var result = type.GetAllPublicConstantStringValues();

        // Assert
        LogAssert("Verifying empty array returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetAllPublicConstantStringValues_EmptyType_ShouldReturnEmptyArray()
    {
        // Arrange
        LogArrange("Preparing empty type");
        var type = typeof(EmptyTestClass);

        // Act
        LogAct("Calling GetAllPublicConstantStringValues");
        var result = type.GetAllPublicConstantStringValues();

        // Assert
        LogAssert("Verifying empty array returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetAllPublicConstantStringValues_DerivedClass_ShouldIncludeBaseConstants()
    {
        // Arrange
        LogArrange("Preparing derived class");
        var type = typeof(DerivedClassWithConstants);

        // Act
        LogAct("Calling GetAllPublicConstantStringValues");
        var result = type.GetAllPublicConstantStringValues();

        // Assert
        LogAssert("Verifying both base and derived constants returned");
        result.Length.ShouldBe(2);
        result.ShouldContain("BaseValue");
        result.ShouldContain("DerivedValue");
    }

    [Fact]
    public void GetAllPublicConstantStringValues_BaseClass_ShouldReturnOnlyOwnConstants()
    {
        // Arrange
        LogArrange("Preparing base class");
        var type = typeof(BaseClassWithConstants);

        // Act
        LogAct("Calling GetAllPublicConstantStringValues");
        var result = type.GetAllPublicConstantStringValues();

        // Assert
        LogAssert("Verifying only base constant returned");
        result.Length.ShouldBe(1);
        result.ShouldContain("BaseValue");
    }

    [Fact]
    public void GetAllPublicConstantStringValues_BuiltInType_ShouldReturnEmptyArray()
    {
        // Arrange
        LogArrange("Preparing built-in type");
        var type = typeof(int);

        // Act
        LogAct("Calling GetAllPublicConstantStringValues");
        var result = type.GetAllPublicConstantStringValues();

        // Assert
        LogAssert("Verifying empty array returned");
        result.ShouldBeEmpty();
    }

    #endregion
}
