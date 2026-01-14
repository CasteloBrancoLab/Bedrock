using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.TenantInfos;

public class TenantInfoTests : TestBase
{
    public TenantInfoTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_WithCodeAndName_ShouldPreserveValues()
    {
        // Arrange
        LogArrange("Creating tenant code and name");
        var code = Guid.NewGuid();
        const string name = "Test Tenant";

        // Act
        LogAct("Creating TenantInfo");
        var tenant = TenantInfo.Create(code, name);

        // Assert
        LogAssert("Verifying values are preserved");
        tenant.Code.ShouldBe(code);
        tenant.Name.ShouldBe(name);
        LogInfo("TenantInfo created with Code: {0}, Name: {1}", tenant.Code, tenant.Name ?? "(null)");
    }

    [Fact]
    public void Create_WithCodeOnly_ShouldHaveNullName()
    {
        // Arrange
        LogArrange("Creating tenant code");
        var code = Guid.NewGuid();

        // Act
        LogAct("Creating TenantInfo without name");
        var tenant = TenantInfo.Create(code);

        // Assert
        LogAssert("Verifying name is null");
        tenant.Code.ShouldBe(code);
        tenant.Name.ShouldBeNull();
        LogInfo("TenantInfo created with Code: {0}, Name is null", tenant.Code);
    }

    [Fact]
    public void Create_WithNullName_ShouldPreserveNull()
    {
        // Arrange
        LogArrange("Creating tenant code with explicit null name");
        var code = Guid.NewGuid();

        // Act
        LogAct("Creating TenantInfo with null name");
        var tenant = TenantInfo.Create(code, null);

        // Assert
        LogAssert("Verifying name is null");
        tenant.Name.ShouldBeNull();
        LogInfo("TenantInfo name is null as expected");
    }

    [Fact]
    public void WithName_ShouldReturnNewInstanceWithNewName()
    {
        // Arrange
        LogArrange("Creating TenantInfo with original name");
        var code = Guid.NewGuid();
        var original = TenantInfo.Create(code, "Original Name");

        // Act
        LogAct("Creating new TenantInfo with different name");
        var updated = original.WithName("New Name");

        // Assert
        LogAssert("Verifying new instance has updated name and same code");
        updated.Code.ShouldBe(code);
        updated.Name.ShouldBe("New Name");
        original.Name.ShouldBe("Original Name");
        LogInfo("Original name: {0}, Updated name: {1}", original.Name ?? "(null)", updated.Name ?? "(null)");
    }

    [Fact]
    public void WithName_WithNull_ShouldClearName()
    {
        // Arrange
        LogArrange("Creating TenantInfo with name");
        var code = Guid.NewGuid();
        var original = TenantInfo.Create(code, "Original Name");

        // Act
        LogAct("Setting name to null");
        var updated = original.WithName(null);

        // Assert
        LogAssert("Verifying name is null");
        updated.Code.ShouldBe(code);
        updated.Name.ShouldBeNull();
        LogInfo("Name cleared successfully");
    }

    [Fact]
    public void ToString_WithName_ShouldReturnNameAndCode()
    {
        // Arrange
        LogArrange("Creating TenantInfo with name");
        var code = Guid.NewGuid();
        var tenant = TenantInfo.Create(code, "Test Tenant");

        // Act
        LogAct("Calling ToString");
        var result = tenant.ToString();

        // Assert
        LogAssert("Verifying format is 'Name (Code)'");
        result.ShouldBe($"Test Tenant ({code})");
        LogInfo("ToString result: {0}", result);
    }

    [Fact]
    public void ToString_WithoutName_ShouldReturnCodeOnly()
    {
        // Arrange
        LogArrange("Creating TenantInfo without name");
        var code = Guid.NewGuid();
        var tenant = TenantInfo.Create(code);

        // Act
        LogAct("Calling ToString");
        var result = tenant.ToString();

        // Assert
        LogAssert("Verifying returns only code");
        result.ShouldBe(code.ToString());
        LogInfo("ToString result: {0}", result);
    }

    [Fact]
    public void GetHashCode_ShouldBeBasedOnCode()
    {
        // Arrange
        LogArrange("Creating TenantInfo");
        var code = Guid.NewGuid();
        var tenant = TenantInfo.Create(code, "Test Tenant");

        // Act
        LogAct("Getting hash code");
        var hashCode = tenant.GetHashCode();

        // Assert
        LogAssert("Verifying hash code matches code's hash code");
        hashCode.ShouldBe(code.GetHashCode());
        LogInfo("Hash code: {0}", hashCode);
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating TenantInfo");
        var tenant = TenantInfo.Create(Guid.NewGuid(), "Test");

        // Act
        LogAct("Getting hash code multiple times");
        var hash1 = tenant.GetHashCode();
        var hash2 = tenant.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are consistent");
        hash1.ShouldBe(hash2);
        LogInfo("Hash code is consistent: {0}", hash1);
    }

    [Fact]
    public void Equals_WithSameCode_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two TenantInfos with same code");
        var code = Guid.NewGuid();
        var tenant1 = TenantInfo.Create(code, "Name 1");
        var tenant2 = TenantInfo.Create(code, "Name 2");

        // Act
        LogAct("Comparing TenantInfos for equality");
        var areEqual = tenant1.Equals(tenant2);

        // Assert
        LogAssert("Verifying TenantInfos are equal by code");
        areEqual.ShouldBeTrue();
        LogInfo("TenantInfos with same code are equal regardless of name");
    }

    [Fact]
    public void Equals_WithDifferentCode_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two TenantInfos with different codes");
        var tenant1 = TenantInfo.Create(Guid.NewGuid(), "Same Name");
        var tenant2 = TenantInfo.Create(Guid.NewGuid(), "Same Name");

        // Act
        LogAct("Comparing TenantInfos for equality");
        var areEqual = tenant1.Equals(tenant2);

        // Assert
        LogAssert("Verifying TenantInfos are not equal");
        areEqual.ShouldBeFalse();
        LogInfo("TenantInfos with different codes are not equal");
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating TenantInfo and object for equality test");
        var code = Guid.NewGuid();
        var tenant = TenantInfo.Create(code, "Test");
        object objSame = TenantInfo.Create(code, "Other Name");
        object objDifferent = TenantInfo.Create(Guid.NewGuid(), "Test");
        object? objNull = null;
        object objWrongType = "not a TenantInfo";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        tenant.Equals(objSame).ShouldBeTrue("Equal TenantInfo objects should be equal");
        tenant.Equals(objDifferent).ShouldBeFalse("Different TenantInfo objects should not be equal");
        tenant.Equals(objNull).ShouldBeFalse("Null should not be equal");
        tenant.Equals(objWrongType).ShouldBeFalse("Wrong type should not be equal");

        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_WithSameCode_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two TenantInfos with same code");
        var code = Guid.NewGuid();
        var tenant1 = TenantInfo.Create(code, "Name 1");
        var tenant2 = TenantInfo.Create(code, "Name 2");

        // Act & Assert
        LogAct("Testing equality operator");
        (tenant1 == tenant2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void EqualityOperator_WithDifferentCode_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two TenantInfos with different codes");
        var tenant1 = TenantInfo.Create(Guid.NewGuid(), "Test");
        var tenant2 = TenantInfo.Create(Guid.NewGuid(), "Test");

        // Act & Assert
        LogAct("Testing equality operator");
        (tenant1 == tenant2).ShouldBeFalse();
        LogAssert("Equality operator correctly identifies different tenants");
    }

    [Fact]
    public void InequalityOperator_WithDifferentCode_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two TenantInfos with different codes");
        var tenant1 = TenantInfo.Create(Guid.NewGuid(), "Test");
        var tenant2 = TenantInfo.Create(Guid.NewGuid(), "Test");

        // Act & Assert
        LogAct("Testing inequality operator");
        (tenant1 != tenant2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    [Fact]
    public void InequalityOperator_WithSameCode_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two TenantInfos with same code");
        var code = Guid.NewGuid();
        var tenant1 = TenantInfo.Create(code, "Name 1");
        var tenant2 = TenantInfo.Create(code, "Name 2");

        // Act & Assert
        LogAct("Testing inequality operator with same code");
        (tenant1 != tenant2).ShouldBeFalse();
        LogAssert("Inequality operator returns false for equal codes");
    }

    [Fact]
    public void DefaultStruct_ShouldHaveEmptyGuidAndNullName()
    {
        // Arrange & Act
        LogArrange("Creating default TenantInfo struct");
        var tenant = default(TenantInfo);

        // Assert
        LogAssert("Verifying default values");
        tenant.Code.ShouldBe(Guid.Empty);
        tenant.Name.ShouldBeNull();
        LogInfo("Default TenantInfo has Code: {0}, Name: null", tenant.Code);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldWork()
    {
        // Arrange
        LogArrange("Creating TenantInfo with empty Guid");

        // Act
        LogAct("Creating TenantInfo");
        var tenant = TenantInfo.Create(Guid.Empty, "Test");

        // Assert
        LogAssert("Verifying values");
        tenant.Code.ShouldBe(Guid.Empty);
        tenant.Name.ShouldBe("Test");
        LogInfo("TenantInfo with empty Guid created successfully");
    }

    [Fact]
    public void GetHashCode_ForSameCode_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Creating two TenantInfos with same code but different names");
        var code = Guid.NewGuid();
        var tenant1 = TenantInfo.Create(code, "Name 1");
        var tenant2 = TenantInfo.Create(code, "Name 2");

        // Act
        LogAct("Getting hash codes");
        var hash1 = tenant1.GetHashCode();
        var hash2 = tenant2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal for same code");
        hash1.ShouldBe(hash2);
        LogInfo("Hash codes match: {0} == {1}", hash1, hash2);
    }

    [Fact]
    public void WithName_ShouldPreserveCode()
    {
        // Arrange
        LogArrange("Creating TenantInfo");
        var code = Guid.NewGuid();
        var original = TenantInfo.Create(code, "Original");

        // Act
        LogAct("Changing name via WithName");
        var updated = original.WithName("Updated");

        // Assert
        LogAssert("Verifying code is preserved");
        updated.Code.ShouldBe(code);
        LogInfo("Code preserved after WithName: {0}", updated.Code);
    }
}
