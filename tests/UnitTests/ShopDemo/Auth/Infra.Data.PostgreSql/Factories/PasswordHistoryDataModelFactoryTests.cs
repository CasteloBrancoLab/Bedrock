using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Domain.Entities.PasswordHistories.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class PasswordHistoryDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public PasswordHistoryDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating PasswordHistory entity with known UserId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "hashed-password", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating PasswordHistoryDataModel from PasswordHistory entity");
        var dataModel = PasswordHistoryDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapPasswordHashCorrectly()
    {
        // Arrange
        LogArrange("Creating PasswordHistory entity with known PasswordHash");
        string expectedPasswordHash = Faker.Random.AlphaNumeric(128);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedPasswordHash, DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating PasswordHistoryDataModel from PasswordHistory entity");
        var dataModel = PasswordHistoryDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying PasswordHash mapping");
        dataModel.PasswordHash.ShouldBe(expectedPasswordHash);
    }

    [Fact]
    public void Create_ShouldMapChangedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating PasswordHistory entity with known ChangedAt");
        var expectedChangedAt = DateTimeOffset.UtcNow.AddDays(-7);
        var entity = CreateTestEntity(Guid.NewGuid(), "hashed-password", expectedChangedAt);

        // Act
        LogAct("Creating PasswordHistoryDataModel from PasswordHistory entity");
        var dataModel = PasswordHistoryDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ChangedAt mapping");
        dataModel.ChangedAt.ShouldBe(expectedChangedAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating PasswordHistory entity with specific EntityInfo values");
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

        var entity = PasswordHistory.CreateFromExistingInfo(
            new CreateFromExistingInfoPasswordHistoryInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "hashed-password",
                DateTimeOffset.UtcNow));

        // Act
        LogAct("Creating PasswordHistoryDataModel from PasswordHistory entity");
        var dataModel = PasswordHistoryDataModelFactory.Create(entity);

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

    private static PasswordHistory CreateTestEntity(
        Guid userId,
        string passwordHash,
        DateTimeOffset changedAt)
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

        return PasswordHistory.CreateFromExistingInfo(
            new CreateFromExistingInfoPasswordHistoryInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                passwordHash,
                changedAt));
    }

    #endregion
}
