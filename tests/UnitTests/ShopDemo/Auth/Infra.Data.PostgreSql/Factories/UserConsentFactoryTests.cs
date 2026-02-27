using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class UserConsentFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public UserConsentFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with specific userId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel();
        dataModel.UserId = expectedUserId;

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapConsentTermIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with specific consentTermId");
        var expectedConsentTermId = Guid.NewGuid();
        var dataModel = CreateTestDataModel();
        dataModel.ConsentTermId = expectedConsentTermId;

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ConsentTermId mapping");
        entity.ConsentTermId.Value.ShouldBe(expectedConsentTermId);
    }

    [Fact]
    public void Create_ShouldMapAcceptedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with specific acceptedAt");
        var expectedAcceptedAt = DateTimeOffset.UtcNow.AddDays(-7);
        var dataModel = CreateTestDataModel();
        dataModel.AcceptedAt = expectedAcceptedAt;

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying AcceptedAt mapping");
        entity.AcceptedAt.ShouldBe(expectedAcceptedAt);
    }

    [Theory]
    [InlineData((short)1, UserConsentStatus.Active)]
    [InlineData((short)2, UserConsentStatus.Revoked)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, UserConsentStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating UserConsentDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel(status: statusValue);

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with specific revokedAt");
        var expectedRevokedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var dataModel = CreateTestDataModel();
        dataModel.RevokedAt = expectedRevokedAt;

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        entity.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_WithNullRevokedAt_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with null revokedAt");
        var dataModel = CreateTestDataModel();
        dataModel.RevokedAt = null;

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        entity.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapIpAddressFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with specific ipAddress");
        string? expectedIpAddress = "10.0.0.1";
        var dataModel = CreateTestDataModel(ipAddress: expectedIpAddress);

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying IpAddress mapping");
        entity.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Create_WithNullIpAddress_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with null ipAddress");
        var dataModel = CreateTestDataModel(ipAddress: null);

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying IpAddress is null");
        entity.IpAddress.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel with specific base fields");
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

        var dataModel = new UserConsentDataModel
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
            ConsentTermId = Guid.NewGuid(),
            AcceptedAt = DateTimeOffset.UtcNow,
            Status = (short)UserConsentStatus.Active,
            RevokedAt = null,
            IpAddress = null
        };

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

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
        LogArrange("Creating UserConsentDataModel with null last-changed fields");
        var dataModel = new UserConsentDataModel
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
            ConsentTermId = Guid.NewGuid(),
            AcceptedAt = DateTimeOffset.UtcNow,
            Status = (short)UserConsentStatus.Active,
            RevokedAt = null,
            IpAddress = null
        };

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel with nulls");
        var entity = UserConsentFactory.Create(dataModel);

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
        LogArrange("Creating UserConsentDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating UserConsent from UserConsentDataModel");
        var entity = UserConsentFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static UserConsentDataModel CreateTestDataModel(
        short? status = null,
        string? ipAddress = null)
    {
        return new UserConsentDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_USER_CONSENT",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            ConsentTermId = Guid.NewGuid(),
            AcceptedAt = DateTimeOffset.UtcNow,
            Status = status ?? (short)UserConsentStatus.Active,
            RevokedAt = null,
            IpAddress = ipAddress
        };
    }

    #endregion
}
