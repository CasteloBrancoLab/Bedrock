using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class PasswordResetTokenDataModelMapperTests : TestBase
{
    public PasswordResetTokenDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void PasswordResetTokenDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking PasswordResetTokenDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(PasswordResetTokenDataModelMapper);

        // Assert
        LogAssert("Verifying PasswordResetTokenDataModelMapper inherits from DataModelMapperBase<PasswordResetTokenDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<PasswordResetTokenDataModel>));
    }

    [Fact]
    public void PasswordResetTokenDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking PasswordResetTokenDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(PasswordResetTokenDataModelMapper);

        // Assert
        LogAssert("Verifying PasswordResetTokenDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper instance");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper and reading TableSchema");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthPasswordResetTokens()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper to verify table name");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper and reading TableName");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_password_reset_tokens'");
        mapper.TableName.ShouldBe("auth_password_reset_tokens");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapTokenHashColumn()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper to verify TokenHash column mapping");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying TokenHash column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("TokenHash");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIsUsedColumn()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper to verify IsUsed column mapping");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying IsUsed column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IsUsed");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUsedAtColumn()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModelMapper to verify UsedAt column mapping");

        // Act
        LogAct("Constructing PasswordResetTokenDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordResetTokenDataModelMapper();

        // Assert
        LogAssert("Verifying UsedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UsedAt");
    }
}
