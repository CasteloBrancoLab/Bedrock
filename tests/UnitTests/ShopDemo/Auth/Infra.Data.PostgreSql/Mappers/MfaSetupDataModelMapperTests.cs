using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class MfaSetupDataModelMapperTests : TestBase
{
    public MfaSetupDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MfaSetupDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking MfaSetupDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(MfaSetupDataModelMapper);

        // Assert
        LogAssert("Verifying MfaSetupDataModelMapper inherits from DataModelMapperBase<MfaSetupDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<MfaSetupDataModel>));
    }

    [Fact]
    public void MfaSetupDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking MfaSetupDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(MfaSetupDataModelMapper);

        // Assert
        LogAssert("Verifying MfaSetupDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModelMapper instance");

        // Act
        LogAct("Constructing MfaSetupDataModelMapper");
        var mapper = new MfaSetupDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing MfaSetupDataModelMapper and reading TableSchema");
        var mapper = new MfaSetupDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthMfaSetups()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModelMapper to verify table name");

        // Act
        LogAct("Constructing MfaSetupDataModelMapper and reading TableName");
        var mapper = new MfaSetupDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_mfa_setups'");
        mapper.TableName.ShouldBe("auth_mfa_setups");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing MfaSetupDataModelMapper and reading ColumnMapDictionary");
        var mapper = new MfaSetupDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapEncryptedSharedSecretColumn()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModelMapper to verify EncryptedSharedSecret column mapping");

        // Act
        LogAct("Constructing MfaSetupDataModelMapper and reading ColumnMapDictionary");
        var mapper = new MfaSetupDataModelMapper();

        // Assert
        LogAssert("Verifying EncryptedSharedSecret column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("EncryptedSharedSecret");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIsEnabledColumn()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModelMapper to verify IsEnabled column mapping");

        // Act
        LogAct("Constructing MfaSetupDataModelMapper and reading ColumnMapDictionary");
        var mapper = new MfaSetupDataModelMapper();

        // Assert
        LogAssert("Verifying IsEnabled column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IsEnabled");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapEnabledAtColumn()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModelMapper to verify EnabledAt column mapping");

        // Act
        LogAct("Constructing MfaSetupDataModelMapper and reading ColumnMapDictionary");
        var mapper = new MfaSetupDataModelMapper();

        // Assert
        LogAssert("Verifying EnabledAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("EnabledAt");
    }
}
