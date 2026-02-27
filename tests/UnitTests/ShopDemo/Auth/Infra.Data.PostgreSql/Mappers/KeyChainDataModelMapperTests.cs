using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class KeyChainDataModelMapperTests : TestBase
{
    public KeyChainDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void KeyChainDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking KeyChainDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(KeyChainDataModelMapper);

        // Assert
        LogAssert("Verifying KeyChainDataModelMapper inherits from DataModelMapperBase<KeyChainDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<KeyChainDataModel>));
    }

    [Fact]
    public void KeyChainDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking KeyChainDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(KeyChainDataModelMapper);

        // Assert
        LogAssert("Verifying KeyChainDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper instance");

        // Act
        LogAct("Constructing KeyChainDataModelMapper");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading TableSchema");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthKeyChains()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify table name");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading TableName");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_key_chains'");
        mapper.TableName.ShouldBe("auth_key_chains");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading ColumnMapDictionary");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapKeyIdColumn()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify KeyId column mapping");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading ColumnMapDictionary");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying KeyId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("KeyId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapPublicKeyColumn()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify PublicKey column mapping");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading ColumnMapDictionary");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying PublicKey column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("PublicKey");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapEncryptedSharedSecretColumn()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify EncryptedSharedSecret column mapping");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading ColumnMapDictionary");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying EncryptedSharedSecret column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("EncryptedSharedSecret");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading ColumnMapDictionary");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating KeyChainDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing KeyChainDataModelMapper and reading ColumnMapDictionary");
        var mapper = new KeyChainDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }
}
