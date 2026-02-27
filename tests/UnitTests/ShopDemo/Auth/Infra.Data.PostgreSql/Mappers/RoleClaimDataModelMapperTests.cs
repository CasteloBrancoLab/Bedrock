using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class RoleClaimDataModelMapperTests : TestBase
{
    public RoleClaimDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void RoleClaimDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking RoleClaimDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(RoleClaimDataModelMapper);

        // Assert
        LogAssert("Verifying RoleClaimDataModelMapper inherits from DataModelMapperBase<RoleClaimDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<RoleClaimDataModel>));
    }

    [Fact]
    public void RoleClaimDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking RoleClaimDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(RoleClaimDataModelMapper);

        // Assert
        LogAssert("Verifying RoleClaimDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModelMapper instance");

        // Act
        LogAct("Constructing RoleClaimDataModelMapper");
        var mapper = new RoleClaimDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing RoleClaimDataModelMapper and reading TableSchema");
        var mapper = new RoleClaimDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthRoleClaims()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModelMapper to verify table name");

        // Act
        LogAct("Constructing RoleClaimDataModelMapper and reading TableName");
        var mapper = new RoleClaimDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_role_claims'");
        mapper.TableName.ShouldBe("auth_role_claims");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRoleIdColumn()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModelMapper to verify RoleId column mapping");

        // Act
        LogAct("Constructing RoleClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RoleClaimDataModelMapper();

        // Assert
        LogAssert("Verifying RoleId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RoleId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapClaimIdColumn()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModelMapper to verify ClaimId column mapping");

        // Act
        LogAct("Constructing RoleClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RoleClaimDataModelMapper();

        // Assert
        LogAssert("Verifying ClaimId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ClaimId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapValueColumn()
    {
        // Arrange
        LogArrange("Creating RoleClaimDataModelMapper to verify Value column mapping");

        // Act
        LogAct("Constructing RoleClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new RoleClaimDataModelMapper();

        // Assert
        LogAssert("Verifying Value column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Value");
    }
}
