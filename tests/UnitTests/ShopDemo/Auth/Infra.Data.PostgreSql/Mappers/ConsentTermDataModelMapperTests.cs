using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ConsentTermDataModelMapperTests : TestBase
{
    public ConsentTermDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ConsentTermDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ConsentTermDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ConsentTermDataModelMapper);

        // Assert
        LogAssert("Verifying ConsentTermDataModelMapper inherits from DataModelMapperBase<ConsentTermDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ConsentTermDataModel>));
    }

    [Fact]
    public void ConsentTermDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ConsentTermDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ConsentTermDataModelMapper);

        // Assert
        LogAssert("Verifying ConsentTermDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModelMapper instance");

        // Act
        LogAct("Constructing ConsentTermDataModelMapper");
        var mapper = new ConsentTermDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ConsentTermDataModelMapper and reading TableSchema");
        var mapper = new ConsentTermDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthConsentTerms()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ConsentTermDataModelMapper and reading TableName");
        var mapper = new ConsentTermDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_consent_terms'");
        mapper.TableName.ShouldBe("auth_consent_terms");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapTypeColumn()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModelMapper to verify Type column mapping");

        // Act
        LogAct("Constructing ConsentTermDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ConsentTermDataModelMapper();

        // Assert
        LogAssert("Verifying Type column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Type");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapVersionColumn()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModelMapper to verify Version column mapping");

        // Act
        LogAct("Constructing ConsentTermDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ConsentTermDataModelMapper();

        // Assert
        LogAssert("Verifying Version column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Version");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapContentColumn()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModelMapper to verify Content column mapping");

        // Act
        LogAct("Constructing ConsentTermDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ConsentTermDataModelMapper();

        // Assert
        LogAssert("Verifying Content column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Content");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapPublishedAtColumn()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModelMapper to verify PublishedAt column mapping");

        // Act
        LogAct("Constructing ConsentTermDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ConsentTermDataModelMapper();

        // Assert
        LogAssert("Verifying PublishedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("PublishedAt");
    }
}
