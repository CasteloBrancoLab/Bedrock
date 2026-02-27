using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class PasswordHistoryDataModelMapperTests : TestBase
{
    public PasswordHistoryDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void PasswordHistoryDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking PasswordHistoryDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(PasswordHistoryDataModelMapper);

        // Assert
        LogAssert("Verifying PasswordHistoryDataModelMapper inherits from DataModelMapperBase<PasswordHistoryDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<PasswordHistoryDataModel>));
    }

    [Fact]
    public void PasswordHistoryDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking PasswordHistoryDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(PasswordHistoryDataModelMapper);

        // Assert
        LogAssert("Verifying PasswordHistoryDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModelMapper instance");

        // Act
        LogAct("Constructing PasswordHistoryDataModelMapper");
        var mapper = new PasswordHistoryDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing PasswordHistoryDataModelMapper and reading TableSchema");
        var mapper = new PasswordHistoryDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthPasswordHistories()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModelMapper to verify table name");

        // Act
        LogAct("Constructing PasswordHistoryDataModelMapper and reading TableName");
        var mapper = new PasswordHistoryDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_password_histories'");
        mapper.TableName.ShouldBe("auth_password_histories");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing PasswordHistoryDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordHistoryDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapPasswordHashColumn()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModelMapper to verify PasswordHash column mapping");

        // Act
        LogAct("Constructing PasswordHistoryDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordHistoryDataModelMapper();

        // Assert
        LogAssert("Verifying PasswordHash column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("PasswordHash");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapChangedAtColumn()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModelMapper to verify ChangedAt column mapping");

        // Act
        LogAct("Constructing PasswordHistoryDataModelMapper and reading ColumnMapDictionary");
        var mapper = new PasswordHistoryDataModelMapper();

        // Assert
        LogAssert("Verifying ChangedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ChangedAt");
    }
}
