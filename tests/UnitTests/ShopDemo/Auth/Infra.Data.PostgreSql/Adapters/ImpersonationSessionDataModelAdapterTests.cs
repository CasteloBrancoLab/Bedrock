using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ImpersonationSessionDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ImpersonationSessionDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateOperatorUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel and ImpersonationSession with different OperatorUserIds");
        var dataModel = CreateTestDataModel();
        var expectedOperatorUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedOperatorUserId, Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        ImpersonationSessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying OperatorUserId was updated");
        dataModel.OperatorUserId.ShouldBe(expectedOperatorUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateTargetUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel and ImpersonationSession with different TargetUserIds");
        var dataModel = CreateTestDataModel();
        var expectedTargetUserId = Guid.NewGuid();
        var entity = CreateTestEntity(Guid.NewGuid(), expectedTargetUserId, DateTimeOffset.UtcNow.AddHours(1), ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        ImpersonationSessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying TargetUserId was updated");
        dataModel.TargetUserId.ShouldBe(expectedTargetUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel and ImpersonationSession with different ExpiresAt values");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(1);
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), expectedExpiresAt, ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        ImpersonationSessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel and ImpersonationSession with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)ImpersonationSessionStatus.Active;
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), ImpersonationSessionStatus.Ended, DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        ImpersonationSessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)ImpersonationSessionStatus.Ended);
    }

    [Fact]
    public void Adapt_ShouldUpdateEndedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel and ImpersonationSession with different EndedAt values");
        var dataModel = CreateTestDataModel();
        DateTimeOffset? expectedEndedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), ImpersonationSessionStatus.Ended, expectedEndedAt);

        // Act
        LogAct("Adapting data model from entity");
        ImpersonationSessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying EndedAt was updated");
        dataModel.EndedAt.ShouldBe(expectedEndedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel and ImpersonationSession with different EntityInfo values");
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

        var entity = ImpersonationSession.CreateFromExistingInfo(
            new CreateFromExistingInfoImpersonationSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                DateTimeOffset.UtcNow.AddHours(1),
                ImpersonationSessionStatus.Active,
                null));

        // Act
        LogAct("Adapting data model from entity");
        ImpersonationSessionDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ImpersonationSessionDataModel and ImpersonationSession");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Adapting data model from entity");
        var result = ImpersonationSessionDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ImpersonationSessionDataModel CreateTestDataModel()
    {
        return new ImpersonationSessionDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            OperatorUserId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            Status = (short)ImpersonationSessionStatus.Active,
            EndedAt = null
        };
    }

    private static ImpersonationSession CreateTestEntity(
        Guid operatorUserId,
        Guid targetUserId,
        DateTimeOffset expiresAt,
        ImpersonationSessionStatus status,
        DateTimeOffset? endedAt)
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

        return ImpersonationSession.CreateFromExistingInfo(
            new CreateFromExistingInfoImpersonationSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(operatorUserId),
                Id.CreateFromExistingInfo(targetUserId),
                expiresAt,
                status,
                endedAt));
    }

    #endregion
}
