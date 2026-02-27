using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class SessionDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public SessionDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating SessionDataModel and Session with different userIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(userId: expectedUserId);

        // Act
        LogAct("Adapting data model from entity");
        SessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateRefreshTokenIdFromEntity()
    {
        // Arrange
        LogArrange("Creating SessionDataModel and Session with different refreshTokenIds");
        var dataModel = CreateTestDataModel();
        var expectedRefreshTokenId = Guid.NewGuid();
        var entity = CreateTestEntity(refreshTokenId: expectedRefreshTokenId);

        // Act
        LogAct("Adapting data model from entity");
        SessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RefreshTokenId was updated");
        dataModel.RefreshTokenId.ShouldBe(expectedRefreshTokenId);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating SessionDataModel and Session with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)SessionStatus.Active;
        var entity = CreateTestEntity(status: SessionStatus.Revoked);

        // Act
        LogAct("Adapting data model from entity");
        SessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)SessionStatus.Revoked);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating SessionDataModel and Session with different expiresAt values");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var entity = CreateTestEntity(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Adapting data model from entity");
        SessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating SessionDataModel and Session with different EntityInfo values");
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
        LogAct("Adapting data model from entity");
        SessionDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating SessionDataModel and Session");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity();

        // Act
        LogAct("Adapting data model from entity");
        var result = SessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
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

    private static Session CreateTestEntity(
        Guid? userId = null,
        Guid? refreshTokenId = null,
        SessionStatus status = SessionStatus.Active,
        DateTimeOffset? expiresAt = null)
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
                Id.CreateFromExistingInfo(userId ?? Guid.NewGuid()),
                Id.CreateFromExistingInfo(refreshTokenId ?? Guid.NewGuid()),
                null,
                null,
                null,
                expiresAt ?? DateTimeOffset.UtcNow.AddHours(8),
                status,
                DateTimeOffset.UtcNow,
                null));
    }

    #endregion
}
