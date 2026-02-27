using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class RoleDataModelMapperTests : TestBase
{
    public RoleDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void RoleDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking RoleDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(RoleDataModelMapper);

        // Assert
        LogAssert("Verifying RoleDataModelMapper inherits from DataModelMapperBase<RoleDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<RoleDataModel>));
    }

    [Fact]
    public void RoleDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking RoleDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(RoleDataModelMapper);

        // Assert
        LogAssert("Verifying RoleDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating RoleDataModelMapper instance");

        // Act
        LogAct("Constructing RoleDataModelMapper");
        var mapper = new RoleDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating RoleDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing RoleDataModelMapper and reading TableSchema");
        var mapper = new RoleDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthRoles()
    {
        // Arrange
        LogArrange("Creating RoleDataModelMapper to verify table name");

        // Act
        LogAct("Constructing RoleDataModelMapper and reading TableName");
        var mapper = new RoleDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_roles'");
        mapper.TableName.ShouldBe("auth_roles");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapNameColumn()
    {
        // Arrange
        LogArrange("Creating RoleDataModelMapper to verify Name column mapping");

        // Act
        LogAct("Constructing RoleDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RoleDataModelMapper();

        // Assert
        LogAssert("Verifying Name column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Name");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapDescriptionColumn()
    {
        // Arrange
        LogArrange("Creating RoleDataModelMapper to verify Description column mapping");

        // Act
        LogAct("Constructing RoleDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RoleDataModelMapper();

        // Assert
        LogAssert("Verifying Description column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Description");
    }
}
