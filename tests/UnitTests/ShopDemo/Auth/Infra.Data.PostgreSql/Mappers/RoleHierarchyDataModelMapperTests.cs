using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class RoleHierarchyDataModelMapperTests : TestBase
{
    public RoleHierarchyDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void RoleHierarchyDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking RoleHierarchyDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(RoleHierarchyDataModelMapper);

        // Assert
        LogAssert("Verifying RoleHierarchyDataModelMapper inherits from DataModelMapperBase<RoleHierarchyDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<RoleHierarchyDataModel>));
    }

    [Fact]
    public void RoleHierarchyDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking RoleHierarchyDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(RoleHierarchyDataModelMapper);

        // Assert
        LogAssert("Verifying RoleHierarchyDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModelMapper instance");

        // Act
        LogAct("Constructing RoleHierarchyDataModelMapper");
        var mapper = new RoleHierarchyDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing RoleHierarchyDataModelMapper and reading TableSchema");
        var mapper = new RoleHierarchyDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthRoleHierarchies()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModelMapper to verify table name");

        // Act
        LogAct("Constructing RoleHierarchyDataModelMapper and reading TableName");
        var mapper = new RoleHierarchyDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_role_hierarchies'");
        mapper.TableName.ShouldBe("auth_role_hierarchies");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRoleIdColumn()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModelMapper to verify RoleId column mapping");

        // Act
        LogAct("Constructing RoleHierarchyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RoleHierarchyDataModelMapper();

        // Assert
        LogAssert("Verifying RoleId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RoleId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapParentRoleIdColumn()
    {
        // Arrange
        LogArrange("Creating RoleHierarchyDataModelMapper to verify ParentRoleId column mapping");

        // Act
        LogAct("Constructing RoleHierarchyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RoleHierarchyDataModelMapper();

        // Assert
        LogAssert("Verifying ParentRoleId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ParentRoleId");
    }
}
