using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ServiceClientScopeDataModelMapperTests : TestBase
{
    public ServiceClientScopeDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ServiceClientScopeDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ServiceClientScopeDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ServiceClientScopeDataModelMapper);

        // Assert
        LogAssert("Verifying ServiceClientScopeDataModelMapper inherits from DataModelMapperBase<ServiceClientScopeDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ServiceClientScopeDataModel>));
    }

    [Fact]
    public void ServiceClientScopeDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ServiceClientScopeDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ServiceClientScopeDataModelMapper);

        // Assert
        LogAssert("Verifying ServiceClientScopeDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModelMapper instance");

        // Act
        LogAct("Constructing ServiceClientScopeDataModelMapper");
        var mapper = new ServiceClientScopeDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ServiceClientScopeDataModelMapper and reading TableSchema");
        var mapper = new ServiceClientScopeDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthServiceClientScopes()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ServiceClientScopeDataModelMapper and reading TableName");
        var mapper = new ServiceClientScopeDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_service_client_scopes'");
        mapper.TableName.ShouldBe("auth_service_client_scopes");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapServiceClientIdColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModelMapper to verify ServiceClientId column mapping");

        // Act
        LogAct("Constructing ServiceClientScopeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientScopeDataModelMapper();

        // Assert
        LogAssert("Verifying ServiceClientId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ServiceClientId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapScopeColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModelMapper to verify Scope column mapping");

        // Act
        LogAct("Constructing ServiceClientScopeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientScopeDataModelMapper();

        // Assert
        LogAssert("Verifying Scope column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Scope");
    }
}
