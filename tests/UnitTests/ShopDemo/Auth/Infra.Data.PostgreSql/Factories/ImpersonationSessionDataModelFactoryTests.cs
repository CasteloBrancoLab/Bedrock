using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ImpersonationSessionDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ImpersonationSessionDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapOperatorUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ImpersonationSession entity with known OperatorUserId");
        var expectedOperatorUserId = Guid.NewGuid();
        var entity = CreateTestEntity(
            expectedOperatorUserId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            ImpersonationSessionStatus.Active,
            null);

        // Act
        LogAct("Creating ImpersonationSessionDataModel from ImpersonationSession entity");
        var dataModel = ImpersonationSessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying OperatorUserId mapping");
        dataModel.OperatorUserId.ShouldBe(expectedOperatorUserId);
    }

    [Fact]
    public void Create_ShouldMapTargetUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ImpersonationSession entity with known TargetUserId");
        var expectedTargetUserId = Guid.NewGuid();
        var entity = CreateTestEntity(
            Guid.NewGuid(),
            expectedTargetUserId,
            DateTimeOffset.UtcNow.AddHours(1),
            ImpersonationSessionStatus.Active,
            null);

        // Act
        LogAct("Creating ImpersonationSessionDataModel from ImpersonationSession entity");
        var dataModel = ImpersonationSessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying TargetUserId mapping");
        dataModel.TargetUserId.ShouldBe(expectedTargetUserId);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ImpersonationSession entity with known ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(4);
        var entity = CreateTestEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            expectedExpiresAt,
            ImpersonationSessionStatus.Active,
            null);

        // Act
        LogAct("Creating ImpersonationSessionDataModel from ImpersonationSession entity");
        var dataModel = ImpersonationSessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData(ImpersonationSessionStatus.Active, 1)]
    [InlineData(ImpersonationSessionStatus.Ended, 2)]
    public void Create_ShouldMapStatusAsShortCorrectly(ImpersonationSessionStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating ImpersonationSession entity with status {status}");
        var entity = CreateTestEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            status,
            null);

        // Act
        LogAct("Creating ImpersonationSessionDataModel from ImpersonationSession entity");
        var dataModel = ImpersonationSessionDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapEndedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ImpersonationSession entity with known EndedAt");
        DateTimeOffset? expectedEndedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var entity = CreateTestEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            ImpersonationSessionStatus.Ended,
            expectedEndedAt);

        // Act
        LogAct("Creating ImpersonationSessionDataModel from ImpersonationSession entity");
        var dataModel = ImpersonationSessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying EndedAt mapping");
        dataModel.EndedAt.ShouldBe(expectedEndedAt);
    }

    [Fact]
    public void Create_ShouldMapNullEndedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ImpersonationSession entity with null EndedAt");
        var entity = CreateTestEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            ImpersonationSessionStatus.Active,
            null);

        // Act
        LogAct("Creating ImpersonationSessionDataModel from ImpersonationSession entity");
        var dataModel = ImpersonationSessionDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null EndedAt mapping");
        dataModel.EndedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ImpersonationSession entity with specific EntityInfo values");
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

        var entity = ImpersonationSession.CreateFromExistingInfo(
            new CreateFromExistingInfoImpersonationSessionInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                DateTimeOffset.UtcNow.AddHours(1),
                ImpersonationSessionStatus.Active,
                null));

        // Act
        LogAct("Creating ImpersonationSessionDataModel from ImpersonationSession entity");
        var dataModel = ImpersonationSessionDataModelFactory.Create(entity);

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
