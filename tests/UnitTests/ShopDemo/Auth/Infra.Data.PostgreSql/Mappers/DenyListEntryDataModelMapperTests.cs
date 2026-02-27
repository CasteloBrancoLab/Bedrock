using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class DenyListEntryDataModelMapperTests : TestBase
{
    public DenyListEntryDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void DenyListEntryDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking DenyListEntryDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(DenyListEntryDataModelMapper);

        // Assert
        LogAssert("Verifying DenyListEntryDataModelMapper inherits from DataModelMapperBase<DenyListEntryDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<DenyListEntryDataModel>));
    }

    [Fact]
    public void DenyListEntryDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking DenyListEntryDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(DenyListEntryDataModelMapper);

        // Assert
        LogAssert("Verifying DenyListEntryDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModelMapper instance");

        // Act
        LogAct("Constructing DenyListEntryDataModelMapper");
        var mapper = new DenyListEntryDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing DenyListEntryDataModelMapper and reading TableSchema");
        var mapper = new DenyListEntryDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthDenyListEntries()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModelMapper to verify table name");

        // Act
        LogAct("Constructing DenyListEntryDataModelMapper and reading TableName");
        var mapper = new DenyListEntryDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_deny_list_entries'");
        mapper.TableName.ShouldBe("auth_deny_list_entries");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapTypeColumn()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModelMapper to verify Type column mapping");

        // Act
        LogAct("Constructing DenyListEntryDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DenyListEntryDataModelMapper();

        // Assert
        LogAssert("Verifying Type column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Type");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapValueColumn()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModelMapper to verify Value column mapping");

        // Act
        LogAct("Constructing DenyListEntryDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DenyListEntryDataModelMapper();

        // Assert
        LogAssert("Verifying Value column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Value");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing DenyListEntryDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DenyListEntryDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapReasonColumn()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModelMapper to verify Reason column mapping");

        // Act
        LogAct("Constructing DenyListEntryDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DenyListEntryDataModelMapper();

        // Assert
        LogAssert("Verifying Reason column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Reason");
    }
}
