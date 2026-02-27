using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.MfaSetups;
using ShopDemo.Auth.Domain.Entities.MfaSetups.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class MfaSetupDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public MfaSetupDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating MfaSetup entity with known UserId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "encrypted-secret", false, null);

        // Act
        LogAct("Creating MfaSetupDataModel from MfaSetup entity");
        var dataModel = MfaSetupDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapEncryptedSharedSecretCorrectly()
    {
        // Arrange
        LogArrange("Creating MfaSetup entity with known EncryptedSharedSecret");
        string expectedSecret = Faker.Random.AlphaNumeric(256);
        var entity = CreateTestEntity(Guid.NewGuid(), expectedSecret, false, null);

        // Act
        LogAct("Creating MfaSetupDataModel from MfaSetup entity");
        var dataModel = MfaSetupDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying EncryptedSharedSecret mapping");
        dataModel.EncryptedSharedSecret.ShouldBe(expectedSecret);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldMapIsEnabledCorrectly(bool expectedIsEnabled)
    {
        // Arrange
        LogArrange($"Creating MfaSetup entity with IsEnabled={expectedIsEnabled}");
        var enabledAt = expectedIsEnabled ? DateTimeOffset.UtcNow.AddDays(-1) : (DateTimeOffset?)null;
        var entity = CreateTestEntity(Guid.NewGuid(), "encrypted-secret", expectedIsEnabled, enabledAt);

        // Act
        LogAct("Creating MfaSetupDataModel from MfaSetup entity");
        var dataModel = MfaSetupDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying IsEnabled mapping to {expectedIsEnabled}");
        dataModel.IsEnabled.ShouldBe(expectedIsEnabled);
    }

    [Fact]
    public void Create_ShouldMapEnabledAtCorrectly()
    {
        // Arrange
        LogArrange("Creating MfaSetup entity with known EnabledAt");
        var expectedEnabledAt = DateTimeOffset.UtcNow.AddDays(-5);
        var entity = CreateTestEntity(Guid.NewGuid(), "encrypted-secret", true, expectedEnabledAt);

        // Act
        LogAct("Creating MfaSetupDataModel from MfaSetup entity");
        var dataModel = MfaSetupDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying EnabledAt mapping");
        dataModel.EnabledAt.ShouldBe(expectedEnabledAt);
    }

    [Fact]
    public void Create_ShouldMapNullEnabledAtCorrectly()
    {
        // Arrange
        LogArrange("Creating MfaSetup entity with null EnabledAt");
        var entity = CreateTestEntity(Guid.NewGuid(), "encrypted-secret", false, null);

        // Act
        LogAct("Creating MfaSetupDataModel from MfaSetup entity");
        var dataModel = MfaSetupDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null EnabledAt mapping");
        dataModel.EnabledAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating MfaSetup entity with specific EntityInfo values");
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

        var entity = MfaSetup.CreateFromExistingInfo(
            new CreateFromExistingInfoMfaSetupInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "encrypted-secret",
                false,
                null));

        // Act
        LogAct("Creating MfaSetupDataModel from MfaSetup entity");
        var dataModel = MfaSetupDataModelFactory.Create(entity);

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
