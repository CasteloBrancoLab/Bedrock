using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class SessionDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public SessionDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known userId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntityWithUserId(expectedUserId);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapRefreshTokenIdCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known refreshTokenId");
        var expectedRefreshTokenId = Guid.NewGuid();
        var entity = CreateTestEntityWithRefreshTokenId(expectedRefreshTokenId);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RefreshTokenId mapping");
        dataModel.RefreshTokenId.ShouldBe(expectedRefreshTokenId);
    }

    [Fact]
    public void Create_ShouldMapDeviceInfoCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known deviceInfo");
        string? expectedDeviceInfo = "Mozilla/5.0 (Windows NT 10.0)";
        var entity = CreateTestEntity(deviceInfo: expectedDeviceInfo);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying DeviceInfo mapping");
        dataModel.DeviceInfo.ShouldBe(expectedDeviceInfo);
    }

    [Fact]
    public void Create_ShouldMapIpAddressCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known ipAddress");
        string? expectedIpAddress = "192.168.1.1";
        var entity = CreateTestEntity(ipAddress: expectedIpAddress);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying IpAddress mapping");
        dataModel.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Create_ShouldMapUserAgentCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known userAgent");
        string? expectedUserAgent = "TestAgent/1.0";
        var entity = CreateTestEntity(userAgent: expectedUserAgent);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserAgent mapping");
        dataModel.UserAgent.ShouldBe(expectedUserAgent);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var entity = CreateTestEntity(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData(SessionStatus.Active, 1)]
    [InlineData(SessionStatus.Revoked, 2)]
    public void Create_ShouldMapStatusAsShortCorrectly(SessionStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating Session entity with status {status}");
        var entity = CreateTestEntity(status: status);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapLastActivityAtCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known lastActivityAt");
        var expectedLastActivityAt = DateTimeOffset.UtcNow.AddMinutes(-30);
        var entity = CreateTestEntity(lastActivityAt: expectedLastActivityAt);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying LastActivityAt mapping");
        dataModel.LastActivityAt.ShouldBe(expectedLastActivityAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating Session entity with known revokedAt");
        var expectedRevokedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var entity = CreateTestEntity(revokedAt: expectedRevokedAt);

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating Session entity with specific EntityInfo values");
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        string createdBy = Faker.Person.FullName;
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        long entityVersion = Faker.Random.Long(1);
        string? lastChangedBy = Faker.Person.FullName;
        var lastChangedAt = DateTimeOffset.UtcNow;
        var lastChangedCorrelationId = Guid.NewGuid();
        string lastChangedExecutionOrigin = "TestOrigin";
        string lastChangedBusinessOperationCode = "TEST_OP";

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
            tenantInfo: TenantInfo.Create(tenantCode),
            createdAt: createdAt,
            createdBy: createdBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_CREATE",
            lastChangedAt: lastChangedAt,
            lastChangedBy: lastChangedBy,
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: lastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: lastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion));

        var entity = Session.CreateFromExistingInfo(
            new CreateFromExistingInfoSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddHours(8),
                SessionStatus.Active,
                DateTimeOffset.UtcNow,
                null));

        // Act
        LogAct("Creating SessionDataModel from Session entity");
        var dataModel = SessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying base fields from EntityInfo");
        dataModel.Id.ShouldBe(entityId);
        dataModel.TenantCode.ShouldBe(tenantCode);
        dataModel.CreatedBy.ShouldBe(createdBy);
        dataModel.CreatedAt.ShouldBe(createdAt);
        dataModel.EntityVersion.ShouldBe(entityVersion);
        dataModel.LastChangedBy.ShouldBe(lastChangedBy);
        dataModel.LastChangedAt.ShouldBe(lastChangedAt);
        dataModel.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        dataModel.LastChangedExecutionOrigin.ShouldBe(lastChangedExecutionOrigin);
        dataModel.LastChangedBusinessOperationCode.ShouldBe(lastChangedBusinessOperationCode);
    }

    #region Helper Methods

    private static Session CreateTestEntity(
        string? deviceInfo = null,
        string? ipAddress = null,
        string? userAgent = null,
        DateTimeOffset? expiresAt = null,
        SessionStatus status = SessionStatus.Active,
        DateTimeOffset? lastActivityAt = null,
        DateTimeOffset? revokedAt = null)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return Session.CreateFromExistingInfo(
            new CreateFromExistingInfoSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                deviceInfo,
                ipAddress,
                userAgent,
                expiresAt ?? DateTimeOffset.UtcNow.AddHours(8),
                status,
                lastActivityAt ?? DateTimeOffset.UtcNow,
                revokedAt));
    }

    private static Session CreateTestEntityWithUserId(Guid userId)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return Session.CreateFromExistingInfo(
            new CreateFromExistingInfoSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddHours(8),
                SessionStatus.Active,
                DateTimeOffset.UtcNow,
                null));
    }

    private static Session CreateTestEntityWithRefreshTokenId(Guid refreshTokenId)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return Session.CreateFromExistingInfo(
            new CreateFromExistingInfoSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(refreshTokenId),
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddHours(8),
                SessionStatus.Active,
                DateTimeOffset.UtcNow,
                null));
    }

    #endregion
}
