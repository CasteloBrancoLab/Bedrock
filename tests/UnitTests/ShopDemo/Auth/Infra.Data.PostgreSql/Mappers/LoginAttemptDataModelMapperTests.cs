using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class LoginAttemptDataModelMapperTests : TestBase
{
    public LoginAttemptDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void LoginAttemptDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking LoginAttemptDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(LoginAttemptDataModelMapper);

        // Assert
        LogAssert("Verifying LoginAttemptDataModelMapper inherits from DataModelMapperBase<LoginAttemptDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<LoginAttemptDataModel>));
    }

    [Fact]
    public void LoginAttemptDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking LoginAttemptDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(LoginAttemptDataModelMapper);

        // Assert
        LogAssert("Verifying LoginAttemptDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper instance");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper and reading TableSchema");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthLoginAttempts()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper to verify table name");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper and reading TableName");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_login_attempts'");
        mapper.TableName.ShouldBe("auth_login_attempts");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUsernameColumn()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper to verify Username column mapping");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper and reading ColumnMapDictionary");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying Username column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Username");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIpAddressColumn()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper to verify IpAddress column mapping");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper and reading ColumnMapDictionary");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying IpAddress column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IpAddress");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapAttemptedAtColumn()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper to verify AttemptedAt column mapping");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper and reading ColumnMapDictionary");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying AttemptedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("AttemptedAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIsSuccessfulColumn()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper to verify IsSuccessful column mapping");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper and reading ColumnMapDictionary");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying IsSuccessful column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IsSuccessful");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapFailureReasonColumn()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModelMapper to verify FailureReason column mapping");

        // Act
        LogAct("Constructing LoginAttemptDataModelMapper and reading ColumnMapDictionary");
        var mapper = new LoginAttemptDataModelMapper();

        // Assert
        LogAssert("Verifying FailureReason column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("FailureReason");
    }
}
