using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ClaimDataModelMapperTests : TestBase
{
    public ClaimDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ClaimDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ClaimDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ClaimDataModelMapper);

        // Assert
        LogAssert("Verifying ClaimDataModelMapper inherits from DataModelMapperBase<ClaimDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ClaimDataModel>));
    }

    [Fact]
    public void ClaimDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ClaimDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ClaimDataModelMapper);

        // Assert
        LogAssert("Verifying ClaimDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ClaimDataModelMapper instance");

        // Act
        LogAct("Constructing ClaimDataModelMapper");
        var mapper = new ClaimDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ClaimDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ClaimDataModelMapper and reading TableSchema");
        var mapper = new ClaimDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthClaims()
    {
        // Arrange
        LogArrange("Creating ClaimDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ClaimDataModelMapper and reading TableName");
        var mapper = new ClaimDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_claims'");
        mapper.TableName.ShouldBe("auth_claims");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapNameColumn()
    {
        // Arrange
        LogArrange("Creating ClaimDataModelMapper to verify Name column mapping");

        // Act
        LogAct("Constructing ClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ClaimDataModelMapper();

        // Assert
        LogAssert("Verifying Name column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Name");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapDescriptionColumn()
    {
        // Arrange
        LogArrange("Creating ClaimDataModelMapper to verify Description column mapping");

        // Act
        LogAct("Constructing ClaimDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ClaimDataModelMapper();

        // Assert
        LogAssert("Verifying Description column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Description");
    }
}
