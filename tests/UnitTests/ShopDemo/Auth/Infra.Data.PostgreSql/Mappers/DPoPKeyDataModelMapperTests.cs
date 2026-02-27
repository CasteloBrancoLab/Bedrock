using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class DPoPKeyDataModelMapperTests : TestBase
{
    public DPoPKeyDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void DPoPKeyDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking DPoPKeyDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(DPoPKeyDataModelMapper);

        // Assert
        LogAssert("Verifying DPoPKeyDataModelMapper inherits from DataModelMapperBase<DPoPKeyDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<DPoPKeyDataModel>));
    }

    [Fact]
    public void DPoPKeyDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking DPoPKeyDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(DPoPKeyDataModelMapper);

        // Assert
        LogAssert("Verifying DPoPKeyDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper instance");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading TableSchema");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthDpopKeys()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify table name");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading TableName");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_dpop_keys'");
        mapper.TableName.ShouldBe("auth_dpop_keys");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapJwkThumbprintColumn()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify JwkThumbprint column mapping");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying JwkThumbprint column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("JwkThumbprint");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapPublicKeyJwkColumn()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify PublicKeyJwk column mapping");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying PublicKeyJwk column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("PublicKeyJwk");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRevokedAtColumn()
    {
        // Arrange
        LogArrange("Creating DPoPKeyDataModelMapper to verify RevokedAt column mapping");

        // Act
        LogAct("Constructing DPoPKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new DPoPKeyDataModelMapper();

        // Assert
        LogAssert("Verifying RevokedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RevokedAt");
    }
}
