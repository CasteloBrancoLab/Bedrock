using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class SigningKeyDataModelMapperTests : TestBase
{
    public SigningKeyDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void SigningKeyDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking SigningKeyDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(SigningKeyDataModelMapper);

        // Assert
        LogAssert("Verifying SigningKeyDataModelMapper inherits from DataModelMapperBase<SigningKeyDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<SigningKeyDataModel>));
    }

    [Fact]
    public void SigningKeyDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking SigningKeyDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(SigningKeyDataModelMapper);

        // Assert
        LogAssert("Verifying SigningKeyDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper instance");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading TableSchema");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthSigningKeys()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify table name");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading TableName");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_signing_keys'");
        mapper.TableName.ShouldBe("auth_signing_keys");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapKidColumn()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify Kid column mapping");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying Kid column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Kid");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapAlgorithmColumn()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify Algorithm column mapping");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying Algorithm column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Algorithm");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapPublicKeyColumn()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify PublicKey column mapping");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying PublicKey column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("PublicKey");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapEncryptedPrivateKeyColumn()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify EncryptedPrivateKey column mapping");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying EncryptedPrivateKey column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("EncryptedPrivateKey");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRotatedAtColumn()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify RotatedAt column mapping");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying RotatedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RotatedAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing SigningKeyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SigningKeyDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }
}
