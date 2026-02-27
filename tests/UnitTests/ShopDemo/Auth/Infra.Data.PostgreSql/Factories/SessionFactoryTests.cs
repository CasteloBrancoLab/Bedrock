using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class SessionFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public SessionFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific userId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel();
        dataModel.UserId = expectedUserId;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapRefreshTokenIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific refreshTokenId");
        var expectedRefreshTokenId = Guid.NewGuid();
        var dataModel = CreateTestDataModel();
        dataModel.RefreshTokenId = expectedRefreshTokenId;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RefreshTokenId mapping");
        entity.RefreshTokenId.Value.ShouldBe(expectedRefreshTokenId);
    }

    [Fact]
    public void Create_ShouldMapDeviceInfoFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific deviceInfo");
        string? expectedDeviceInfo = "Mozilla/5.0 (Windows NT 10.0)";
        var dataModel = CreateTestDataModel();
        dataModel.DeviceInfo = expectedDeviceInfo;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying DeviceInfo mapping");
        entity.DeviceInfo.ShouldBe(expectedDeviceInfo);
    }

    [Fact]
    public void Create_ShouldMapIpAddressFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific ipAddress");
        string? expectedIpAddress = "192.168.1.1";
        var dataModel = CreateTestDataModel();
        dataModel.IpAddress = expectedIpAddress;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying IpAddress mapping");
        entity.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Create_ShouldMapUserAgentFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific userAgent");
        string? expectedUserAgent = "TestAgent/1.0";
        var dataModel = CreateTestDataModel();
        dataModel.UserAgent = expectedUserAgent;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserAgent mapping");
        entity.UserAgent.ShouldBe(expectedUserAgent);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var dataModel = CreateTestDataModel();
        dataModel.ExpiresAt = expectedExpiresAt;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData((short)1, SessionStatus.Active)]
    [InlineData((short)2, SessionStatus.Revoked)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, SessionStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating SessionDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel();
        dataModel.Status = statusValue;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapLastActivityAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific lastActivityAt");
        var expectedLastActivityAt = DateTimeOffset.UtcNow.AddMinutes(-30);
        var dataModel = CreateTestDataModel();
        dataModel.LastActivityAt = expectedLastActivityAt;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying LastActivityAt mapping");
        entity.LastActivityAt.ShouldBe(expectedLastActivityAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific revokedAt");
        var expectedRevokedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var dataModel = CreateTestDataModel();
        dataModel.RevokedAt = expectedRevokedAt;

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        entity.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with specific base fields");
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-5);
        long expectedVersion = Faker.Random.Long(1);
        string? expectedLastChangedBy = Faker.Person.FullName;
        var expectedLastChangedAt = DateTimeOffset.UtcNow;
        var expectedLastChangedCorrelationId = Guid.NewGuid();
        string expectedLastChangedExecutionOrigin = "TestOrigin";
        string expectedLastChangedBusinessOperationCode = "TEST_OP";

        var dataModel = new SessionDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            LastChangedBy = expectedLastChangedBy,
            LastChangedAt = expectedLastChangedAt,
            LastChangedCorrelationId = expectedLastChangedCorrelationId,
            LastChangedExecutionOrigin = expectedLastChangedExecutionOrigin,
            LastChangedBusinessOperationCode = expectedLastChangedBusinessOperationCode,
            EntityVersion = expectedVersion,
            UserId = Guid.NewGuid(),
            RefreshTokenId = Guid.NewGuid(),
            DeviceInfo = null,
            IpAddress = null,
            UserAgent = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8),
            Status = (short)SessionStatus.Active,
            LastActivityAt = DateTimeOffset.UtcNow,
            RevokedAt = null
        };

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EntityInfo fields");
        entity.EntityInfo.Id.Value.ShouldBe(expectedId);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        entity.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBe(expectedLastChangedBy);
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(expectedLastChangedAt);
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(expectedLastChangedCorrelationId);
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe(expectedLastChangedExecutionOrigin);
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe(expectedLastChangedBusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullLastChangedFields_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating SessionDataModel with null last-changed fields");
        var dataModel = new SessionDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            RefreshTokenId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8),
            Status = (short)SessionStatus.Active,
            LastActivityAt = DateTimeOffset.UtcNow,
            RevokedAt = null
        };

        // Act
        LogAct("Creating Session from SessionDataModel with nulls");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying nullable fields are null");
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapCreatedCorrelationIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating SessionDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating Session from SessionDataModel");
        var entity = SessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static SessionDataModel CreateTestDataModel()
    {
        return new SessionDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_SESSION",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            RefreshTokenId = Guid.NewGuid(),
            DeviceInfo = null,
            IpAddress = null,
            UserAgent = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8),
            Status = (short)SessionStatus.Active,
            LastActivityAt = DateTimeOffset.UtcNow,
            RevokedAt = null
        };
    }

    #endregion
}
