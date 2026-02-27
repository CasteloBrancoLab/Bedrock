using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ClaimDependencyDataModelMapperTests : TestBase
{
    public ClaimDependencyDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ClaimDependencyDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ClaimDependencyDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ClaimDependencyDataModelMapper);

        // Assert
        LogAssert("Verifying ClaimDependencyDataModelMapper inherits from DataModelMapperBase<ClaimDependencyDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ClaimDependencyDataModel>));
    }

    [Fact]
    public void ClaimDependencyDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ClaimDependencyDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ClaimDependencyDataModelMapper);

        // Assert
        LogAssert("Verifying ClaimDependencyDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModelMapper instance");

        // Act
        LogAct("Constructing ClaimDependencyDataModelMapper");
        var mapper = new ClaimDependencyDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ClaimDependencyDataModelMapper and reading TableSchema");
        var mapper = new ClaimDependencyDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthClaimDependencies()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ClaimDependencyDataModelMapper and reading TableName");
        var mapper = new ClaimDependencyDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_claim_dependencies'");
        mapper.TableName.ShouldBe("auth_claim_dependencies");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapClaimIdColumn()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModelMapper to verify ClaimId column mapping");

        // Act
        LogAct("Constructing ClaimDependencyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ClaimDependencyDataModelMapper();

        // Assert
        LogAssert("Verifying ClaimId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ClaimId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapDependsOnClaimIdColumn()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModelMapper to verify DependsOnClaimId column mapping");

        // Act
        LogAct("Constructing ClaimDependencyDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ClaimDependencyDataModelMapper();

        // Assert
        LogAssert("Verifying DependsOnClaimId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("DependsOnClaimId");
    }
}
