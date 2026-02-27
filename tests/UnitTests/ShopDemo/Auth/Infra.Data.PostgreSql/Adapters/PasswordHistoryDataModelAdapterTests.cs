using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Domain.Entities.PasswordHistories.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class PasswordHistoryDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public PasswordHistoryDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel and PasswordHistory with different UserIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "hashed-password", DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        PasswordHistoryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdatePasswordHashFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel and PasswordHistory with different PasswordHashes");
        var dataModel = CreateTestDataModel();
        string expectedPasswordHash = Faker.Random.AlphaNumeric(128);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedPasswordHash, DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        PasswordHistoryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying PasswordHash was updated");
        dataModel.PasswordHash.ShouldBe(expectedPasswordHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateChangedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel and PasswordHistory with different ChangedAt values");
        var dataModel = CreateTestDataModel();
        var expectedChangedAt = DateTimeOffset.UtcNow.AddDays(-14);
        var entity = CreateTestEntity(Guid.NewGuid(), "hashed-password", expectedChangedAt);

        // Act
        LogAct("Adapting data model from entity");
        PasswordHistoryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ChangedAt was updated");
        dataModel.ChangedAt.ShouldBe(expectedChangedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel and PasswordHistory with different EntityInfo values");
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

        var entity = PasswordHistory.CreateFromExistingInfo(
            new CreateFromExistingInfoPasswordHistoryInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "hashed-password",
                DateTimeOffset.UtcNow));

        // Act
        LogAct("Adapting data model from entity");
        PasswordHistoryDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating PasswordHistoryDataModel and PasswordHistory");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "hashed-password", DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        var result = PasswordHistoryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static PasswordHistoryDataModel CreateTestDataModel()
    {
        return new PasswordHistoryDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            PasswordHash = "initial-hash",
            ChangedAt = DateTimeOffset.UtcNow.AddDays(-30)
        };
    }

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
