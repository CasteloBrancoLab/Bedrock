using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class UserDataModelMapperTests : TestBase
{
    public UserDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void UserDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking UserDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(UserDataModelMapper);

        // Assert
        LogAssert("Verifying UserDataModelMapper inherits from DataModelMapperBase<UserDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<UserDataModel>));
    }

    [Fact]
    public void UserDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking UserDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(UserDataModelMapper);

        // Assert
        LogAssert("Verifying UserDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating UserDataModelMapper instance");

        // Act
        LogAct("Constructing UserDataModelMapper");
        var mapper = new UserDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldBeProtected()
    {
        // Arrange
        LogArrange("Checking ConfigureInternal method accessibility");

        // Act
        LogAct("Reflecting on ConfigureInternal method");
        var method = typeof(UserDataModelMapper).GetMethod(
            "ConfigureInternal",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Assert
        LogAssert("Verifying ConfigureInternal is a protected method");
        method.ShouldNotBeNull();
        method.IsFamily.ShouldBeTrue();
    }

    [Fact]
    public void MapBinaryImporter_ShouldBePublicOverride()
    {
        // Arrange
        LogArrange("Checking MapBinaryImporter method accessibility");

        // Act
        LogAct("Reflecting on MapBinaryImporter method");
        var method = typeof(UserDataModelMapper).GetMethod(
            "MapBinaryImporter",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        LogAssert("Verifying MapBinaryImporter is a public method");
        method.ShouldNotBeNull();
        method.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating UserDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing UserDataModelMapper and reading TableSchema");
        var mapper = new UserDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthUsers()
    {
        // Arrange
        LogArrange("Creating UserDataModelMapper to verify table name");

        // Act
        LogAct("Constructing UserDataModelMapper and reading TableName");
        var mapper = new UserDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_users'");
        mapper.TableName.ShouldBe("auth_users");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUsernameColumn()
    {
        // Arrange
        LogArrange("Creating UserDataModelMapper to verify Username column mapping");

        // Act
        LogAct("Constructing UserDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserDataModelMapper();

        // Assert
        LogAssert("Verifying Username column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Username");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapEmailColumn()
    {
        // Arrange
        LogArrange("Creating UserDataModelMapper to verify Email column mapping");

        // Act
        LogAct("Constructing UserDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserDataModelMapper();

        // Assert
        LogAssert("Verifying Email column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Email");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapPasswordHashColumn()
    {
        // Arrange
        LogArrange("Creating UserDataModelMapper to verify PasswordHash column mapping");

        // Act
        LogAct("Constructing UserDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserDataModelMapper();

        // Assert
        LogAssert("Verifying PasswordHash column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("PasswordHash");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating UserDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing UserDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }
}
