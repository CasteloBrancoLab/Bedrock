using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class TenantDataModelMapperTests : TestBase
{
    public TenantDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void TenantDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking TenantDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(TenantDataModelMapper);

        // Assert
        LogAssert("Verifying TenantDataModelMapper inherits from DataModelMapperBase<TenantDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<TenantDataModel>));
    }

    [Fact]
    public void TenantDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking TenantDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(TenantDataModelMapper);

        // Assert
        LogAssert("Verifying TenantDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper instance");

        // Act
        LogAct("Constructing TenantDataModelMapper");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading TableSchema");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToTenantLookup()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify table name");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading TableName");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'tenant_lookup'");
        mapper.TableName.ShouldBe("tenant_lookup");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapNameColumn()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify Name column mapping");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying Name column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Name");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapDomainColumn()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify Domain column mapping");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying Domain column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Domain");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapSchemaNameColumn()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify SchemaName column mapping");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying SchemaName column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("SchemaName");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapTierColumn()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify Tier column mapping");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying Tier column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Tier");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapDbVersionColumn()
    {
        // Arrange
        LogArrange("Creating TenantDataModelMapper to verify DbVersion column mapping");

        // Act
        LogAct("Constructing TenantDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TenantDataModelMapper();

        // Assert
        LogAssert("Verifying DbVersion column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("DbVersion");
    }
}
