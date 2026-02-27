using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ApiKeyDataModelMapperTests : TestBase
{
    public ApiKeyDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ApiKeyDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ApiKeyDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ApiKeyDataModelMapper);

        // Assert
        LogAssert("Verifying ApiKeyDataModelMapper inherits from DataModelMapperBase<ApiKeyDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ApiKeyDataModel>));
    }

    [Fact]
    public void ApiKeyDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ApiKeyDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ApiKeyDataModelMapper);

        // Assert
        LogAssert("Verifying ApiKeyDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper instance");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading TableSchema");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthApiKeys()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading TableName");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_api_keys'");
        mapper.TableName.ShouldBe("auth_api_keys");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapServiceClientIdColumn()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify ServiceClientId column mapping");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying ServiceClientId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ServiceClientId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapKeyPrefixColumn()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify KeyPrefix column mapping");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying KeyPrefix column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("KeyPrefix");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapKeyHashColumn()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify KeyHash column mapping");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying KeyHash column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("KeyHash");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapLastUsedAtColumn()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify LastUsedAt column mapping");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying LastUsedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("LastUsedAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRevokedAtColumn()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModelMapper to verify RevokedAt column mapping");

        // Act
        LogAct("Constructing ApiKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ApiKeyDataModelMapper();

        // Assert
        LogAssert("Verifying RevokedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RevokedAt");
    }
}
