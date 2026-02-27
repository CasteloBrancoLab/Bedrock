using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class RecoveryCodeDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public RecoveryCodeDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModel and RecoveryCode with different UserIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "code-hash", false, null);

        // Act
        LogAct("Adapting data model from entity");
        RecoveryCodeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateCodeHashFromEntity()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModel and RecoveryCode with different CodeHashes");
        var dataModel = CreateTestDataModel();
        string expectedCodeHash = Faker.Random.AlphaNumeric(64);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedCodeHash, false, null);

        // Act
        LogAct("Adapting data model from entity");
        RecoveryCodeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying CodeHash was updated");
        dataModel.CodeHash.ShouldBe(expectedCodeHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateIsUsedFromEntity()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModel and RecoveryCode with different IsUsed values");
        var dataModel = CreateTestDataModel();
        dataModel.IsUsed = false;
        var usedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var entity = CreateTestEntity(Guid.NewGuid(), "code-hash", true, usedAt);

        // Act
        LogAct("Adapting data model from entity");
        RecoveryCodeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IsUsed was updated");
        dataModel.IsUsed.ShouldBeTrue();
    }

    [Fact]
    public void Adapt_ShouldUpdateUsedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModel and RecoveryCode with different UsedAt values");
        var dataModel = CreateTestDataModel();
        var expectedUsedAt = DateTimeOffset.UtcNow.AddMinutes(-25);
        var entity = CreateTestEntity(Guid.NewGuid(), "code-hash", true, expectedUsedAt);

        // Act
        LogAct("Adapting data model from entity");
        RecoveryCodeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UsedAt was updated");
        dataModel.UsedAt.ShouldBe(expectedUsedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateNullUsedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModel with UsedAt and RecoveryCode with null UsedAt");
        var dataModel = CreateTestDataModel();
        dataModel.UsedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var entity = CreateTestEntity(Guid.NewGuid(), "code-hash", false, null);

        // Act
        LogAct("Adapting data model from entity");
        RecoveryCodeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UsedAt was updated to null");
        dataModel.UsedAt.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating RecoveryCodeDataModel and RecoveryCode with different EntityInfo values");
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

        var entity = RecoveryCode.CreateFromExistingInfo(
            new CreateFromExistingInfoRecoveryCodeInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "code-hash",
                false,
                null));

        // Act
        LogAct("Adapting data model from entity");
        RecoveryCodeDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating RecoveryCodeDataModel and RecoveryCode");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "code-hash", false, null);

        // Act
        LogAct("Adapting data model from entity");
        var result = RecoveryCodeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static RecoveryCodeDataModel CreateTestDataModel()
    {
        return new RecoveryCodeDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            CodeHash = "initial-code-hash",
            IsUsed = false,
            UsedAt = null
        };
    }

    private static RecoveryCode CreateTestEntity(
        Guid userId,
        string codeHash,
        bool isUsed,
        DateTimeOffset? usedAt)
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

        return RecoveryCode.CreateFromExistingInfo(
            new CreateFromExistingInfoRecoveryCodeInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                codeHash,
                isUsed,
                usedAt));
    }

    #endregion
}
