using Bedrock.BuildingBlocks.Persistence.PostgreSql.Utils;
using Bedrock.BuildingBlocks.Testing;
using NpgsqlTypes;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Utils;

public class NpgsqlDbTypeUtilsTests : TestBase
{
    public NpgsqlDbTypeUtilsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MapToNpgsqlDbType_WithGuid_ShouldReturnUuid()
    {
        // Arrange
        LogArrange("Testing Guid mapping");
        var type = typeof(Guid);

        // Act
        LogAct("Mapping Guid to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Uuid");
        result.ShouldBe(NpgsqlDbType.Uuid);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithString_ShouldReturnVarchar()
    {
        // Arrange
        LogArrange("Testing string mapping");
        var type = typeof(string);

        // Act
        LogAct("Mapping string to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Varchar");
        result.ShouldBe(NpgsqlDbType.Varchar);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithDateTimeOffset_ShouldReturnTimestampTz()
    {
        // Arrange
        LogArrange("Testing DateTimeOffset mapping");
        var type = typeof(DateTimeOffset);

        // Act
        LogAct("Mapping DateTimeOffset to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is TimestampTz");
        result.ShouldBe(NpgsqlDbType.TimestampTz);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithDateTime_ShouldReturnTimestamp()
    {
        // Arrange
        LogArrange("Testing DateTime mapping");
        var type = typeof(DateTime);

        // Act
        LogAct("Mapping DateTime to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Timestamp");
        result.ShouldBe(NpgsqlDbType.Timestamp);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithBool_ShouldReturnBoolean()
    {
        // Arrange
        LogArrange("Testing bool mapping");
        var type = typeof(bool);

        // Act
        LogAct("Mapping bool to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Boolean");
        result.ShouldBe(NpgsqlDbType.Boolean);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithLong_ShouldReturnBigint()
    {
        // Arrange
        LogArrange("Testing long mapping");
        var type = typeof(long);

        // Act
        LogAct("Mapping long to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Bigint");
        result.ShouldBe(NpgsqlDbType.Bigint);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithInt_ShouldReturnInteger()
    {
        // Arrange
        LogArrange("Testing int mapping");
        var type = typeof(int);

        // Act
        LogAct("Mapping int to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Integer");
        result.ShouldBe(NpgsqlDbType.Integer);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithShort_ShouldReturnSmallint()
    {
        // Arrange
        LogArrange("Testing short mapping");
        var type = typeof(short);

        // Act
        LogAct("Mapping short to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Smallint");
        result.ShouldBe(NpgsqlDbType.Smallint);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithDouble_ShouldReturnDouble()
    {
        // Arrange
        LogArrange("Testing double mapping");
        var type = typeof(double);

        // Act
        LogAct("Mapping double to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Double");
        result.ShouldBe(NpgsqlDbType.Double);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithFloat_ShouldReturnReal()
    {
        // Arrange
        LogArrange("Testing float mapping");
        var type = typeof(float);

        // Act
        LogAct("Mapping float to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Real");
        result.ShouldBe(NpgsqlDbType.Real);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithDecimal_ShouldReturnNumeric()
    {
        // Arrange
        LogArrange("Testing decimal mapping");
        var type = typeof(decimal);

        // Act
        LogAct("Mapping decimal to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Numeric");
        result.ShouldBe(NpgsqlDbType.Numeric);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithNullableGuid_ShouldReturnUuid()
    {
        // Arrange
        LogArrange("Testing nullable Guid mapping");
        var type = typeof(Guid?);

        // Act
        LogAct("Mapping nullable Guid to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Uuid");
        result.ShouldBe(NpgsqlDbType.Uuid);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithNullableDateTimeOffset_ShouldReturnTimestampTz()
    {
        // Arrange
        LogArrange("Testing nullable DateTimeOffset mapping");
        var type = typeof(DateTimeOffset?);

        // Act
        LogAct("Mapping nullable DateTimeOffset to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is TimestampTz");
        result.ShouldBe(NpgsqlDbType.TimestampTz);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithNullableLong_ShouldReturnBigint()
    {
        // Arrange
        LogArrange("Testing nullable long mapping");
        var type = typeof(long?);

        // Act
        LogAct("Mapping nullable long to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Bigint");
        result.ShouldBe(NpgsqlDbType.Bigint);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithByteArray_ShouldReturnBytea()
    {
        // Arrange
        LogArrange("Testing byte[] mapping");
        var type = typeof(byte[]);

        // Act
        LogAct("Mapping byte[] to NpgsqlDbType");
        var result = NpgsqlDbTypeUtils.MapToNpgsqlDbType(type);

        // Assert
        LogAssert("Verifying result is Bytea");
        result.ShouldBe(NpgsqlDbType.Bytea);
    }

    [Fact]
    public void MapToNpgsqlDbType_WithUnsupportedType_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Testing unsupported type mapping");
        var type = typeof(object);

        // Act & Assert
        LogAct("Attempting to map unsupported type");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => NpgsqlDbTypeUtils.MapToNpgsqlDbType(type));
        LogAssert("Verifying exception is thrown");
        exception.ParamName.ShouldBe("type");
        exception.Message.ShouldContain("Type not supported");
    }

    [Fact]
    public void MapToNpgsqlDbType_WithCustomClass_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Testing custom class type mapping");
        var type = typeof(NpgsqlDbTypeUtilsTests);

        // Act & Assert
        LogAct("Attempting to map custom class type");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => NpgsqlDbTypeUtils.MapToNpgsqlDbType(type));
        LogAssert("Verifying exception is thrown");
        exception.ParamName.ShouldBe("type");
    }
}
