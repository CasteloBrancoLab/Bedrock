using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class RecoveryCodeDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public RecoveryCodeDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating RecoveryCode entity with known UserId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "code-hash", false, null);

        // Act
        LogAct("Creating RecoveryCodeDataModel from RecoveryCode entity");
        var dataModel = RecoveryCodeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapCodeHashCorrectly()
    {
        // Arrange
        LogArrange("Creating RecoveryCode entity with known CodeHash");
        string expectedCodeHash = Faker.Random.AlphaNumeric(64);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedCodeHash, false, null);

        // Act
        LogAct("Creating RecoveryCodeDataModel from RecoveryCode entity");
        var dataModel = RecoveryCodeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying CodeHash mapping");
        dataModel.CodeHash.ShouldBe(expectedCodeHash);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldMapIsUsedCorrectly(bool expectedIsUsed)
    {
        // Arrange
        LogArrange($"Creating RecoveryCode entity with IsUsed={expectedIsUsed}");
        var usedAt = expectedIsUsed ? DateTimeOffset.UtcNow.AddMinutes(-10) : (DateTimeOffset?)null;
        var entity = CreateTestEntity(Guid.NewGuid(), "code-hash", expectedIsUsed, usedAt);

        // Act
        LogAct("Creating RecoveryCodeDataModel from RecoveryCode entity");
        var dataModel = RecoveryCodeDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying IsUsed mapping to {expectedIsUsed}");
        dataModel.IsUsed.ShouldBe(expectedIsUsed);
    }

    [Fact]
    public void Create_ShouldMapUsedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating RecoveryCode entity with known UsedAt");
        var expectedUsedAt = DateTimeOffset.UtcNow.AddMinutes(-30);
        var entity = CreateTestEntity(Guid.NewGuid(), "code-hash", true, expectedUsedAt);

        // Act
        LogAct("Creating RecoveryCodeDataModel from RecoveryCode entity");
        var dataModel = RecoveryCodeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UsedAt mapping");
        dataModel.UsedAt.ShouldBe(expectedUsedAt);
    }

    [Fact]
    public void Create_ShouldMapNullUsedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating RecoveryCode entity with null UsedAt");
        var entity = CreateTestEntity(Guid.NewGuid(), "code-hash", false, null);

        // Act
        LogAct("Creating RecoveryCodeDataModel from RecoveryCode entity");
        var dataModel = RecoveryCodeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null UsedAt mapping");
        dataModel.UsedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating RecoveryCode entity with specific EntityInfo values");
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

        var entity = RecoveryCode.CreateFromExistingInfo(
            new CreateFromExistingInfoRecoveryCodeInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "code-hash",
                false,
                null));

        // Act
        LogAct("Creating RecoveryCodeDataModel from RecoveryCode entity");
        var dataModel = RecoveryCodeDataModelFactory.Create(entity);

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
