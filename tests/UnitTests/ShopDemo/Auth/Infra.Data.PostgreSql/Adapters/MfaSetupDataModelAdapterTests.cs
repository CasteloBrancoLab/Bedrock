using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.MfaSetups;
using ShopDemo.Auth.Domain.Entities.MfaSetups.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class MfaSetupDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public MfaSetupDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel and MfaSetup with different UserIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "encrypted-secret", false, null);

        // Act
        LogAct("Adapting data model from entity");
        MfaSetupDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateEncryptedSharedSecretFromEntity()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel and MfaSetup with different EncryptedSharedSecrets");
        var dataModel = CreateTestDataModel();
        string expectedSecret = Faker.Random.AlphaNumeric(256);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedSecret, false, null);

        // Act
        LogAct("Adapting data model from entity");
        MfaSetupDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying EncryptedSharedSecret was updated");
        dataModel.EncryptedSharedSecret.ShouldBe(expectedSecret);
    }

    [Fact]
    public void Adapt_ShouldUpdateIsEnabledFromEntity()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel and MfaSetup with different IsEnabled values");
        var dataModel = CreateTestDataModel();
        dataModel.IsEnabled = false;
        var entity = CreateTestEntity(Guid.NewGuid(), "encrypted-secret", true, DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        MfaSetupDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IsEnabled was updated");
        dataModel.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Adapt_ShouldUpdateEnabledAtFromEntity()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel and MfaSetup with different EnabledAt values");
        var dataModel = CreateTestDataModel();
        var expectedEnabledAt = DateTimeOffset.UtcNow.AddDays(-2);
        var entity = CreateTestEntity(Guid.NewGuid(), "encrypted-secret", true, expectedEnabledAt);

        // Act
        LogAct("Adapting data model from entity");
        MfaSetupDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying EnabledAt was updated");
        dataModel.EnabledAt.ShouldBe(expectedEnabledAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateNullEnabledAtFromEntity()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel with EnabledAt and MfaSetup with null EnabledAt");
        var dataModel = CreateTestDataModel();
        dataModel.EnabledAt = DateTimeOffset.UtcNow.AddDays(-5);
        var entity = CreateTestEntity(Guid.NewGuid(), "encrypted-secret", false, null);

        // Act
        LogAct("Adapting data model from entity");
        MfaSetupDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying EnabledAt was updated to null");
        dataModel.EnabledAt.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel and MfaSetup with different EntityInfo values");
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

        var entity = MfaSetup.CreateFromExistingInfo(
            new CreateFromExistingInfoMfaSetupInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "encrypted-secret",
                false,
                null));

        // Act
        LogAct("Adapting data model from entity");
        MfaSetupDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating MfaSetupDataModel and MfaSetup");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "encrypted-secret", false, null);

        // Act
        LogAct("Adapting data model from entity");
        var result = MfaSetupDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static MfaSetupDataModel CreateTestDataModel()
    {
        return new MfaSetupDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            EncryptedSharedSecret = "initial-secret",
            IsEnabled = false,
            EnabledAt = null
        };
    }

    private static MfaSetup CreateTestEntity(
        Guid userId,
        string encryptedSharedSecret,
        bool isEnabled,
        DateTimeOffset? enabledAt)
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

        return MfaSetup.CreateFromExistingInfo(
            new CreateFromExistingInfoMfaSetupInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                encryptedSharedSecret,
                isEnabled,
                enabledAt));
    }

    #endregion
}
