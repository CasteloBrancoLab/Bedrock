using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;
using ShopDemo.Auth.Domain.Entities.ExternalLogins.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ExternalLoginDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ExternalLoginDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel and ExternalLogin with different UserIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedUserId, "google", "user-123", null);

        // Act
        LogAct("Adapting data model from entity");
        ExternalLoginDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateProviderFromEntity()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel and ExternalLogin with different Providers");
        var dataModel = CreateTestDataModel();
        string expectedProvider = "microsoft";
        var entity = CreateTestEntity(Guid.NewGuid(), expectedProvider, "user-123", null);

        // Act
        LogAct("Adapting data model from entity");
        ExternalLoginDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Provider was updated");
        dataModel.Provider.ShouldBe(expectedProvider);
    }

    [Fact]
    public void Adapt_ShouldUpdateProviderUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel and ExternalLogin with different ProviderUserIds");
        var dataModel = CreateTestDataModel();
        string expectedProviderUserId = Faker.Random.AlphaNumeric(20);
        var entity = CreateTestEntity(Guid.NewGuid(), "google", expectedProviderUserId, null);

        // Act
        LogAct("Adapting data model from entity");
        ExternalLoginDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ProviderUserId was updated");
        dataModel.ProviderUserId.ShouldBe(expectedProviderUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateEmailFromEntity()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel and ExternalLogin with different Emails");
        var dataModel = CreateTestDataModel();
        string expectedEmail = Faker.Internet.Email();
        var entity = CreateTestEntity(Guid.NewGuid(), "google", "user-123", expectedEmail);

        // Act
        LogAct("Adapting data model from entity");
        ExternalLoginDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Email was updated");
        dataModel.Email.ShouldBe(expectedEmail);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel and ExternalLogin with different EntityInfo values");
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

        var entity = ExternalLogin.CreateFromExistingInfo(
            new CreateFromExistingInfoExternalLoginInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                LoginProvider.CreateNew("google"),
                "user-123",
                null));

        // Act
        LogAct("Adapting data model from entity");
        ExternalLoginDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ExternalLoginDataModel and ExternalLogin");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(Guid.NewGuid(), "google", "user-123", null);

        // Act
        LogAct("Adapting data model from entity");
        var result = ExternalLoginDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ExternalLoginDataModel CreateTestDataModel()
    {
        return new ExternalLoginDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            Provider = "initial-provider",
            ProviderUserId = "initial-provider-user",
            Email = null
        };
    }

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
