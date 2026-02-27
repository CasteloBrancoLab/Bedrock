using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;
using ShopDemo.Auth.Domain.Entities.ExternalLogins.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ExternalLoginDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ExternalLoginDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ExternalLogin entity with known UserId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "google", "google-user-123", "user@gmail.com");

        // Act
        LogAct("Creating ExternalLoginDataModel from ExternalLogin entity");
        var dataModel = ExternalLoginDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapProviderValueCorrectly()
    {
        // Arrange
        LogArrange("Creating ExternalLogin entity with known Provider");
        string expectedProvider = "github";
        var entity = CreateTestEntity(Guid.NewGuid(), expectedProvider, "gh-user-456", "user@github.com");

        // Act
        LogAct("Creating ExternalLoginDataModel from ExternalLogin entity");
        var dataModel = ExternalLoginDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Provider mapping");
        dataModel.Provider.ShouldBe(expectedProvider);
    }

    [Fact]
    public void Create_ShouldMapProviderUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ExternalLogin entity with known ProviderUserId");
        string expectedProviderUserId = Faker.Random.AlphaNumeric(20);
        var entity = CreateTestEntity(Guid.NewGuid(), "google", expectedProviderUserId, null);

        // Act
        LogAct("Creating ExternalLoginDataModel from ExternalLogin entity");
        var dataModel = ExternalLoginDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ProviderUserId mapping");
        dataModel.ProviderUserId.ShouldBe(expectedProviderUserId);
    }

    [Fact]
    public void Create_ShouldMapEmailCorrectly()
    {
        // Arrange
        LogArrange("Creating ExternalLogin entity with known Email");
        string expectedEmail = Faker.Internet.Email();
        var entity = CreateTestEntity(Guid.NewGuid(), "google", "user-123", expectedEmail);

        // Act
        LogAct("Creating ExternalLoginDataModel from ExternalLogin entity");
        var dataModel = ExternalLoginDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Email mapping");
        dataModel.Email.ShouldBe(expectedEmail);
    }

    [Fact]
    public void Create_ShouldMapNullEmailCorrectly()
    {
        // Arrange
        LogArrange("Creating ExternalLogin entity with null Email");
        var entity = CreateTestEntity(Guid.NewGuid(), "google", "user-123", null);

        // Act
        LogAct("Creating ExternalLoginDataModel from ExternalLogin entity");
        var dataModel = ExternalLoginDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null Email mapping");
        dataModel.Email.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ExternalLogin entity with specific EntityInfo values");
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

        var entity = ExternalLogin.CreateFromExistingInfo(
            new CreateFromExistingInfoExternalLoginInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                LoginProvider.CreateNew("google"),
                "user-123",
                null));

        // Act
        LogAct("Creating ExternalLoginDataModel from ExternalLogin entity");
        var dataModel = ExternalLoginDataModelFactory.Create(entity);

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

    private static ExternalLogin CreateTestEntity(
        Guid userId,
        string provider,
        string providerUserId,
        string? email)
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

        return ExternalLogin.CreateFromExistingInfo(
            new CreateFromExistingInfoExternalLoginInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId),
                LoginProvider.CreateNew(provider),
                providerUserId,
                email));
    }

    #endregion
}
