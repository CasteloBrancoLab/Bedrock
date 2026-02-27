using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class UserRoleDataModelMapperTests : TestBase
{
    public UserRoleDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void UserRoleDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking UserRoleDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(UserRoleDataModelMapper);

        // Assert
        LogAssert("Verifying UserRoleDataModelMapper inherits from DataModelMapperBase<UserRoleDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<UserRoleDataModel>));
    }

    [Fact]
    public void UserRoleDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking UserRoleDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(UserRoleDataModelMapper);

        // Assert
        LogAssert("Verifying UserRoleDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating UserRoleDataModelMapper instance");

        // Act
        LogAct("Constructing UserRoleDataModelMapper");
        var mapper = new UserRoleDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating UserRoleDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing UserRoleDataModelMapper and reading TableSchema");
        var mapper = new UserRoleDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthUserRoles()
    {
        // Arrange
        LogArrange("Creating UserRoleDataModelMapper to verify table name");

        // Act
        LogAct("Constructing UserRoleDataModelMapper and reading TableName");
        var mapper = new UserRoleDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_user_roles'");
        mapper.TableName.ShouldBe("auth_user_roles");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating UserRoleDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing UserRoleDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserRoleDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRoleIdColumn()
    {
        // Arrange
        LogArrange("Creating UserRoleDataModelMapper to verify RoleId column mapping");

        // Act
        LogAct("Constructing UserRoleDataModelMapper and reading ColumnMapDictionary");
        var mapper = new UserRoleDataModelMapper();

        // Assert
        LogAssert("Verifying RoleId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RoleId");
    }
}
