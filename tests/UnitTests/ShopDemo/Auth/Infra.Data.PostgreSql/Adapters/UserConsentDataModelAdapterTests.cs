using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class UserConsentDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public UserConsentDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent with different userIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntityWithUserId(expectedUserId);

        // Act
        LogAct("Adapting data model from entity");
        UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateConsentTermIdFromEntity()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent with different consentTermIds");
        var dataModel = CreateTestDataModel();
        var expectedConsentTermId = Guid.NewGuid();
        var entity = CreateTestEntityWithConsentTermId(expectedConsentTermId);

        // Act
        LogAct("Adapting data model from entity");
        UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ConsentTermId was updated");
        dataModel.ConsentTermId.ShouldBe(expectedConsentTermId);
    }

    [Fact]
    public void Adapt_ShouldUpdateAcceptedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent with different acceptedAt values");
        var dataModel = CreateTestDataModel();
        var expectedAcceptedAt = DateTimeOffset.UtcNow.AddDays(-5);
        var entity = CreateTestEntity(acceptedAt: expectedAcceptedAt);

        // Act
        LogAct("Adapting data model from entity");
        UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying AcceptedAt was updated");
        dataModel.AcceptedAt.ShouldBe(expectedAcceptedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)UserConsentStatus.Active;
        var entity = CreateTestEntity(status: UserConsentStatus.Revoked);

        // Act
        LogAct("Adapting data model from entity");
        UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)UserConsentStatus.Revoked);
    }

    [Fact]
    public void Adapt_ShouldUpdateRevokedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent with different revokedAt values");
        var dataModel = CreateTestDataModel();
        var expectedRevokedAt = DateTimeOffset.UtcNow.AddHours(-3);
        var entity = CreateTestEntity(revokedAt: expectedRevokedAt);

        // Act
        LogAct("Adapting data model from entity");
        UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RevokedAt was updated");
        dataModel.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateIpAddressFromEntity()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent with different ipAddresses");
        var dataModel = CreateTestDataModel();
        string? expectedIpAddress = "172.16.0.1";
        var entity = CreateTestEntity(ipAddress: expectedIpAddress);

        // Act
        LogAct("Adapting data model from entity");
        UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IpAddress was updated");
        dataModel.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent with different EntityInfo values");
        var dataModel = CreateTestDataModel();
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
        long expectedVersion = Faker.Random.Long(1);

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(expectedId),
            tenantInfo: TenantInfo.Create(expectedTenantCode),
            createdAt: expectedCreatedAt,
            createdBy: expectedCreatedBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(expectedVersion));

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
        LogAct("Adapting data model from entity");
        UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying base fields were updated from EntityInfo");
        dataModel.Id.ShouldBe(expectedId);
        dataModel.TenantCode.ShouldBe(expectedTenantCode);
        dataModel.CreatedBy.ShouldBe(expectedCreatedBy);
        dataModel.CreatedAt.ShouldBe(expectedCreatedAt);
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Adapt_ShouldReturnTheSameDataModelInstance()
    {
        // Arrange
        LogArrange("Creating UserConsentDataModel and UserConsent");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity();

        // Act
        LogAct("Adapting data model from entity");
        var result = UserConsentDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static UserConsentDataModel CreateTestDataModel()
    {
        return new UserConsentDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            ConsentTermId = Guid.NewGuid(),
            AcceptedAt = DateTimeOffset.UtcNow,
            Status = (short)UserConsentStatus.Active,
            RevokedAt = null,
            IpAddress = null
        };
    }

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
