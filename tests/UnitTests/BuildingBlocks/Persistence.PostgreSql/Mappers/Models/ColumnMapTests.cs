using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Testing;
using NpgsqlTypes;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

public class ColumnMapTests : TestBase
{
    public ColumnMapTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_WithTypeOnly_ShouldSetPropertiesAndInferNpgsqlDbType()
    {
        // Arrange
        LogArrange("Preparing column name and type");
        const string columnName = "test_column";
        Type columnType = typeof(Guid);

        // Act
        LogAct("Creating ColumnMap with type inference");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying properties are set correctly");
        result.ColumnName.ShouldBe(columnName);
        result.Type.ShouldBe(columnType);
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.Uuid);
    }

    [Fact]
    public void Create_WithExplicitNpgsqlDbType_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Preparing column name, type, and explicit NpgsqlDbType");
        const string columnName = "status_column";
        Type columnType = typeof(string);
        NpgsqlDbType explicitDbType = NpgsqlDbType.Text;

        // Act
        LogAct("Creating ColumnMap with explicit NpgsqlDbType");
        ColumnMap result = ColumnMap.Create(columnName, columnType, explicitDbType);

        // Assert
        LogAssert("Verifying all properties are set correctly");
        result.ColumnName.ShouldBe(columnName);
        result.Type.ShouldBe(columnType);
        result.NpgsqlDbType.ShouldBe(explicitDbType);
    }

    [Fact]
    public void Create_WithStringType_ShouldInferVarchar()
    {
        // Arrange
        LogArrange("Preparing string column");
        const string columnName = "name";
        Type columnType = typeof(string);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is Varchar");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.Varchar);
    }

    [Fact]
    public void Create_WithDateTimeOffsetType_ShouldInferTimestampTz()
    {
        // Arrange
        LogArrange("Preparing DateTimeOffset column");
        const string columnName = "created_at";
        Type columnType = typeof(DateTimeOffset);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is TimestampTz");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.TimestampTz);
    }

    [Fact]
    public void Create_WithBoolType_ShouldInferBoolean()
    {
        // Arrange
        LogArrange("Preparing bool column");
        const string columnName = "is_active";
        Type columnType = typeof(bool);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is Boolean");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.Boolean);
    }

    [Fact]
    public void Create_WithLongType_ShouldInferBigint()
    {
        // Arrange
        LogArrange("Preparing long column");
        const string columnName = "entity_version";
        Type columnType = typeof(long);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is Bigint");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.Bigint);
    }

    [Fact]
    public void Create_WithIntType_ShouldInferInteger()
    {
        // Arrange
        LogArrange("Preparing int column");
        const string columnName = "count";
        Type columnType = typeof(int);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is Integer");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.Integer);
    }

    [Fact]
    public void Create_WithDecimalType_ShouldInferNumeric()
    {
        // Arrange
        LogArrange("Preparing decimal column");
        const string columnName = "price";
        Type columnType = typeof(decimal);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is Numeric");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.Numeric);
    }

    [Fact]
    public void Create_WithNullableGuidType_ShouldInferUuid()
    {
        // Arrange
        LogArrange("Preparing nullable Guid column");
        const string columnName = "correlation_id";
        Type columnType = typeof(Guid?);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is Uuid for nullable Guid");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.Uuid);
    }

    [Fact]
    public void Create_WithNullableDateTimeOffsetType_ShouldInferTimestampTz()
    {
        // Arrange
        LogArrange("Preparing nullable DateTimeOffset column");
        const string columnName = "last_changed_at";
        Type columnType = typeof(DateTimeOffset?);

        // Act
        LogAct("Creating ColumnMap");
        ColumnMap result = ColumnMap.Create(columnName, columnType);

        // Assert
        LogAssert("Verifying NpgsqlDbType is TimestampTz for nullable DateTimeOffset");
        result.NpgsqlDbType.ShouldBe(NpgsqlDbType.TimestampTz);
    }
}
