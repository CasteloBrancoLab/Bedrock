using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class RecoveryCodeDataModelMapperTests : TestBase
{
    public RecoveryCodeDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void RecoveryCodeDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking RecoveryCodeDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(RecoveryCodeDataModelMapper);

        // Assert
        LogAssert("Verifying RecoveryCodeDataModelMapper inherits from DataModelMapperBase<RecoveryCodeDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<RecoveryCodeDataModel>));
    }

    [Fact]
    public void RecoveryCodeDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking RecoveryCodeDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(RecoveryCodeDataModelMapper);

        // Assert
        LogAssert("Verifying RecoveryCodeDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModelMapper instance");

        // Act
        LogAct("Constructing RecoveryCodeDataModelMapper");
        var mapper = new RecoveryCodeDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing RecoveryCodeDataModelMapper and reading TableSchema");
        var mapper = new RecoveryCodeDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthRecoveryCodes()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModelMapper to verify table name");

        // Act
        LogAct("Constructing RecoveryCodeDataModelMapper and reading TableName");
        var mapper = new RecoveryCodeDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_recovery_codes'");
        mapper.TableName.ShouldBe("auth_recovery_codes");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing RecoveryCodeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RecoveryCodeDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapCodeHashColumn()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModelMapper to verify CodeHash column mapping");

        // Act
        LogAct("Constructing RecoveryCodeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RecoveryCodeDataModelMapper();

        // Assert
        LogAssert("Verifying CodeHash column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("CodeHash");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIsUsedColumn()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModelMapper to verify IsUsed column mapping");

        // Act
        LogAct("Constructing RecoveryCodeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RecoveryCodeDataModelMapper();

        // Assert
        LogAssert("Verifying IsUsed column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IsUsed");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUsedAtColumn()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModelMapper to verify UsedAt column mapping");

        // Act
        LogAct("Constructing RecoveryCodeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RecoveryCodeDataModelMapper();

        // Assert
        LogAssert("Verifying UsedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UsedAt");
    }
}
