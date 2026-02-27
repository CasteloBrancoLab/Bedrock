using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class PasswordResetTokenDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public PasswordResetTokenDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel and PasswordResetToken with different UserIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Adapting data model from entity");
        PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateTokenHashFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel and PasswordResetToken with different TokenHashes");
        var dataModel = CreateTestDataModel();
        string expectedTokenHash = Faker.Random.AlphaNumeric(64);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedTokenHash, DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Adapting data model from entity");
        PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying TokenHash was updated");
        dataModel.TokenHash.ShouldBe(expectedTokenHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel and PasswordResetToken with different ExpiresAt values");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(72);
        var entity = CreateTestEntity(Guid.NewGuid(), "token-hash", expectedExpiresAt, false, null);

        // Act
        LogAct("Adapting data model from entity");
        PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateIsUsedFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel and PasswordResetToken with different IsUsed values");
        var dataModel = CreateTestDataModel();
        dataModel.IsUsed = false;
        var usedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var entity = CreateTestEntity(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), true, usedAt);

        // Act
        LogAct("Adapting data model from entity");
        PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IsUsed was updated");
        dataModel.IsUsed.ShouldBeTrue();
    }

    [Fact]
    public void Adapt_ShouldUpdateUsedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel and PasswordResetToken with different UsedAt values");
        var dataModel = CreateTestDataModel();
        var expectedUsedAt = DateTimeOffset.UtcNow.AddMinutes(-20);
        var entity = CreateTestEntity(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), true, expectedUsedAt);

        // Act
        LogAct("Adapting data model from entity");
        PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UsedAt was updated");
        dataModel.UsedAt.ShouldBe(expectedUsedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateNullUsedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel with UsedAt and PasswordResetToken with null UsedAt");
        var dataModel = CreateTestDataModel();
        dataModel.UsedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var entity = CreateTestEntity(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Adapting data model from entity");
        PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UsedAt was updated to null");
        dataModel.UsedAt.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel and PasswordResetToken with different EntityInfo values");
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

        var entity = PasswordResetToken.CreateFromExistingInfo(
            new CreateFromExistingInfoPasswordResetTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "token-hash",
                DateTimeOffset.UtcNow.AddHours(1),
                false,
                null));

        // Act
        LogAct("Adapting data model from entity");
        PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating PasswordResetTokenDataModel and PasswordResetToken");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Adapting data model from entity");
        var result = PasswordResetTokenDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static PasswordResetTokenDataModel CreateTestDataModel()
    {
        return new PasswordResetTokenDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            TokenHash = "initial-token-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            IsUsed = false,
            UsedAt = null
        };
    }

    private static PasswordResetToken CreateTestEntity(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
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

        return PasswordResetToken.CreateFromExistingInfo(
            new CreateFromExistingInfoPasswordResetTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                tokenHash,
                expiresAt,
                isUsed,
                usedAt));
    }

    #endregion
}
