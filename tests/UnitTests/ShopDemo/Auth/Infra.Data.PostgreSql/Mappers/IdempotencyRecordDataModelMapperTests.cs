using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class IdempotencyRecordDataModelMapperTests : TestBase
{
    public IdempotencyRecordDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void IdempotencyRecordDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking IdempotencyRecordDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(IdempotencyRecordDataModelMapper);

        // Assert
        LogAssert("Verifying IdempotencyRecordDataModelMapper inherits from DataModelMapperBase<IdempotencyRecordDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<IdempotencyRecordDataModel>));
    }

    [Fact]
    public void IdempotencyRecordDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking IdempotencyRecordDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(IdempotencyRecordDataModelMapper);

        // Assert
        LogAssert("Verifying IdempotencyRecordDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper instance");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper and reading TableSchema");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthIdempotencyRecords()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper to verify table name");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper and reading TableName");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_idempotency_records'");
        mapper.TableName.ShouldBe("auth_idempotency_records");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIdempotencyKeyColumn()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper to verify IdempotencyKey column mapping");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper and reading ColumnMapDictionary");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying IdempotencyKey column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IdempotencyKey");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRequestHashColumn()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper to verify RequestHash column mapping");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper and reading ColumnMapDictionary");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying RequestHash column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RequestHash");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapResponseBodyColumn()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper to verify ResponseBody column mapping");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper and reading ColumnMapDictionary");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying ResponseBody column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ResponseBody");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusCodeColumn()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper to verify StatusCode column mapping");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper and reading ColumnMapDictionary");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying StatusCode column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("StatusCode");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing IdempotencyRecordDataModelMapper and reading ColumnMapDictionary");
        var mapper = new IdempotencyRecordDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }
}
