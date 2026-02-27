using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class UserConsentDataModelMapperTests : TestBase
{
    public UserConsentDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void UserConsentDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking UserConsentDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(UserConsentDataModelMapper);

        // Assert
        LogAssert("Verifying UserConsentDataModelMapper inherits from DataModelMapperBase<UserConsentDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<UserConsentDataModel>));
    }

    [Fact]
    public void UserConsentDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking UserConsentDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(UserConsentDataModelMapper);

        // Assert
        LogAssert("Verifying UserConsentDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper instance");

        // Act
        LogAct("Constructing UserConsentDataModelMapper");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading TableSchema");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthUserConsents()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify table name");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading TableName");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_user_consents'");
        mapper.TableName.ShouldBe("auth_user_consents");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapConsentTermIdColumn()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify ConsentTermId column mapping");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying ConsentTermId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ConsentTermId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapAcceptedAtColumn()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify AcceptedAt column mapping");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying AcceptedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("AcceptedAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRevokedAtColumn()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify RevokedAt column mapping");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying RevokedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RevokedAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIpAddressColumn()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModelMapper to verify IpAddress column mapping");

        // Act
        LogAct("Constructing UserConsentDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserConsentDataModelMapper();

        // Assert
        LogAssert("Verifying IpAddress column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IpAddress");
    }
}
