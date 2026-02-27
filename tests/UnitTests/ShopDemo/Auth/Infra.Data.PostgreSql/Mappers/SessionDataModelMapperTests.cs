using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class SessionDataModelMapperTests : TestBase
{
    public SessionDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void SessionDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking SessionDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(SessionDataModelMapper);

        // Assert
        LogAssert("Verifying SessionDataModelMapper inherits from DataModelMapperBase<SessionDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<SessionDataModel>));
    }

    [Fact]
    public void SessionDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking SessionDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(SessionDataModelMapper);

        // Assert
        LogAssert("Verifying SessionDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper instance");

        // Act
        LogAct("Constructing SessionDataModelMapper");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading TableSchema");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthSessions()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify table name");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading TableName");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_sessions'");
        mapper.TableName.ShouldBe("auth_sessions");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRefreshTokenIdColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify RefreshTokenId column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying RefreshTokenId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RefreshTokenId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapDeviceInfoColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify DeviceInfo column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying DeviceInfo column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("DeviceInfo");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIpAddressColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify IpAddress column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying IpAddress column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IpAddress");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserAgentColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify UserAgent column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying UserAgent column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserAgent");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapLastActivityAtColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify LastActivityAt column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying LastActivityAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("LastActivityAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRevokedAtColumn()
    {
        // Arrange
        LogArrange("Creating SessionDataModelMapper to verify RevokedAt column mapping");

        // Act
        LogAct("Constructing SessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new SessionDataModelMapper();

        // Assert
        LogAssert("Verifying RevokedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RevokedAt");
    }
}
