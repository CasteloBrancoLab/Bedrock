using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ServiceClientDataModelMapperTests : TestBase
{
    public ServiceClientDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ServiceClientDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ServiceClientDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ServiceClientDataModelMapper);

        // Assert
        LogAssert("Verifying ServiceClientDataModelMapper inherits from DataModelMapperBase<ServiceClientDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ServiceClientDataModel>));
    }

    [Fact]
    public void ServiceClientDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ServiceClientDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ServiceClientDataModelMapper);

        // Assert
        LogAssert("Verifying ServiceClientDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper instance");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading TableSchema");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthServiceClients()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading TableName");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_service_clients'");
        mapper.TableName.ShouldBe("auth_service_clients");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapClientIdColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify ClientId column mapping");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying ClientId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ClientId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapClientSecretHashColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify ClientSecretHash column mapping");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying ClientSecretHash column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ClientSecretHash");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapNameColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify Name column mapping");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying Name column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Name");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapCreatedByUserIdColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify CreatedByUserId column mapping");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying CreatedByUserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("CreatedByUserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRevokedAtColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModelMapper to verify RevokedAt column mapping");

        // Act
        LogAct("Constructing ServiceClientDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientDataModelMapper();

        // Assert
        LogAssert("Verifying RevokedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RevokedAt");
    }
}
