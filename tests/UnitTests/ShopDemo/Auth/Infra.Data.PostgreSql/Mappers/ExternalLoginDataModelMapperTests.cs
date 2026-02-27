using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ExternalLoginDataModelMapperTests : TestBase
{
    public ExternalLoginDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ExternalLoginDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ExternalLoginDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ExternalLoginDataModelMapper);

        // Assert
        LogAssert("Verifying ExternalLoginDataModelMapper inherits from DataModelMapperBase<ExternalLoginDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ExternalLoginDataModel>));
    }

    [Fact]
    public void ExternalLoginDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ExternalLoginDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ExternalLoginDataModelMapper);

        // Assert
        LogAssert("Verifying ExternalLoginDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModelMapper instance");

        // Act
        LogAct("Constructing ExternalLoginDataModelMapper");
        var mapper = new ExternalLoginDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ExternalLoginDataModelMapper and reading TableSchema");
        var mapper = new ExternalLoginDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthExternalLogins()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ExternalLoginDataModelMapper and reading TableName");
        var mapper = new ExternalLoginDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_external_logins'");
        mapper.TableName.ShouldBe("auth_external_logins");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing ExternalLoginDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ExternalLoginDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapProviderColumn()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModelMapper to verify Provider column mapping");

        // Act
        LogAct("Constructing ExternalLoginDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ExternalLoginDataModelMapper();

        // Assert
        LogAssert("Verifying Provider column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Provider");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapProviderUserIdColumn()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModelMapper to verify ProviderUserId column mapping");

        // Act
        LogAct("Constructing ExternalLoginDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ExternalLoginDataModelMapper();

        // Assert
        LogAssert("Verifying ProviderUserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ProviderUserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapEmailColumn()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModelMapper to verify Email column mapping");

        // Act
        LogAct("Constructing ExternalLoginDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ExternalLoginDataModelMapper();

        // Assert
        LogAssert("Verifying Email column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Email");
    }
}
