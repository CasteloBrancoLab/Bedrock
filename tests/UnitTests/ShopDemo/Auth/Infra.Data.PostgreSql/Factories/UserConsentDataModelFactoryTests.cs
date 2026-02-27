using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class UserConsentDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public UserConsentDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with known userId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntityWithUserId(expectedUserId);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapConsentTermIdCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with known consentTermId");
        var expectedConsentTermId = Guid.NewGuid();
        var entity = CreateTestEntityWithConsentTermId(expectedConsentTermId);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ConsentTermId mapping");
        dataModel.ConsentTermId.ShouldBe(expectedConsentTermId);
    }

    [Fact]
    public void Create_ShouldMapAcceptedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with known acceptedAt");
        var expectedAcceptedAt = DateTimeOffset.UtcNow.AddDays(-3);
        var entity = CreateTestEntity(acceptedAt: expectedAcceptedAt);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying AcceptedAt mapping");
        dataModel.AcceptedAt.ShouldBe(expectedAcceptedAt);
    }

    [Theory]
    [InlineData((short)1, UserConsentStatus.Active)]
    [InlineData((short)2, UserConsentStatus.Revoked)]
    public void Create_ShouldMapStatusCorrectly(short expectedStatusValue, UserConsentStatus status)
    {
        // Arrange
        LogArrange($"Creating UserConsent entity with status {status}");
        var entity = CreateTestEntity(status: status);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatusValue}");
        dataModel.Status.ShouldBe(expectedStatusValue);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with known revokedAt");
        var expectedRevokedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var entity = CreateTestEntity(revokedAt: expectedRevokedAt);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_WithNullRevokedAt_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with null revokedAt");
        var entity = CreateTestEntity(revokedAt: null);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        dataModel.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapIpAddressCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with known ipAddress");
        string? expectedIpAddress = "192.168.1.1";
        var entity = CreateTestEntity(ipAddress: expectedIpAddress);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying IpAddress mapping");
        dataModel.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Create_WithNullIpAddress_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with null ipAddress");
        var entity = CreateTestEntity(ipAddress: null);

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying IpAddress is null");
        dataModel.IpAddress.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating UserConsent entity with specific EntityInfo values");
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

        var entity = UserConsent.CreateFromExistingInfo(
            new CreateFromExistingInfoUserConsentInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                DateTimeOffset.UtcNow,
                UserConsentStatus.Active,
                null,
                null));

        // Act
        LogAct("Creating UserConsentDataModel from UserConsent entity");
        var dataModel = UserConsentDataModelFactory.Create(entity);

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

    private static UserConsent CreateTestEntity(
        UserConsentStatus status = UserConsentStatus.Active,
        DateTimeOffset? acceptedAt = null,
        DateTimeOffset? revokedAt = null,
        string? ipAddress = null)
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

        return UserConsent.CreateFromExistingInfo(
            new CreateFromExistingInfoUserConsentInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                acceptedAt ?? DateTimeOffset.UtcNow,
                status,
                revokedAt,
                ipAddress));
    }

    private static UserConsent CreateTestEntityWithUserId(Guid userId)
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

        return UserConsent.CreateFromExistingInfo(
            new CreateFromExistingInfoUserConsentInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                DateTimeOffset.UtcNow,
                UserConsentStatus.Active,
                null,
                null));
    }

    private static UserConsent CreateTestEntityWithConsentTermId(Guid consentTermId)
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

        return UserConsent.CreateFromExistingInfo(
            new CreateFromExistingInfoUserConsentInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(consentTermId),
                DateTimeOffset.UtcNow,
                UserConsentStatus.Active,
                null,
                null));
    }

    #endregion
}
