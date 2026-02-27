using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ServiceClientClaimDataModelMapperTests : TestBase
{
    public ServiceClientClaimDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ServiceClientClaimDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ServiceClientClaimDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ServiceClientClaimDataModelMapper);

        // Assert
        LogAssert("Verifying ServiceClientClaimDataModelMapper inherits from DataModelMapperBase<ServiceClientClaimDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ServiceClientClaimDataModel>));
    }

    [Fact]
    public void ServiceClientClaimDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ServiceClientClaimDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ServiceClientClaimDataModelMapper);

        // Assert
        LogAssert("Verifying ServiceClientClaimDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModelMapper instance");

        // Act
        LogAct("Constructing ServiceClientClaimDataModelMapper");
        var mapper = new ServiceClientClaimDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ServiceClientClaimDataModelMapper and reading TableSchema");
        var mapper = new ServiceClientClaimDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthServiceClientClaims()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ServiceClientClaimDataModelMapper and reading TableName");
        var mapper = new ServiceClientClaimDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_service_client_claims'");
        mapper.TableName.ShouldBe("auth_service_client_claims");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapServiceClientIdColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModelMapper to verify ServiceClientId column mapping");

        // Act
        LogAct("Constructing ServiceClientClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientClaimDataModelMapper();

        // Assert
        LogAssert("Verifying ServiceClientId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ServiceClientId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapClaimIdColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModelMapper to verify ClaimId column mapping");

        // Act
        LogAct("Constructing ServiceClientClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientClaimDataModelMapper();

        // Assert
        LogAssert("Verifying ClaimId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ClaimId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapValueColumn()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModelMapper to verify Value column mapping");

        // Act
        LogAct("Constructing ServiceClientClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ServiceClientClaimDataModelMapper();

        // Assert
        LogAssert("Verifying Value column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Value");
    }
}
