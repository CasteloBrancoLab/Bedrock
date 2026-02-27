using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class TokenExchangeDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public TokenExchangeDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUserIdFromEntity()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel and TokenExchange with different userIds");
        var dataModel = CreateTestDataModel();
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntity(userId: expectedUserId);

        // Act
        LogAct("Adapting data model from entity");
        TokenExchangeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying UserId was updated");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateSubjectTokenJtiFromEntity()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel and TokenExchange with different subjectTokenJtis");
        var dataModel = CreateTestDataModel();
        string expectedSubjectTokenJti = Guid.NewGuid().ToString();
        var entity = CreateTestEntity(subjectTokenJti: expectedSubjectTokenJti);

        // Act
        LogAct("Adapting data model from entity");
        TokenExchangeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying SubjectTokenJti was updated");
        dataModel.SubjectTokenJti.ShouldBe(expectedSubjectTokenJti);
    }

    [Fact]
    public void Adapt_ShouldUpdateRequestedAudienceFromEntity()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel and TokenExchange with different requestedAudiences");
        var dataModel = CreateTestDataModel();
        string expectedRequestedAudience = "new-service";
        var entity = CreateTestEntity(requestedAudience: expectedRequestedAudience);

        // Act
        LogAct("Adapting data model from entity");
        TokenExchangeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RequestedAudience was updated");
        dataModel.RequestedAudience.ShouldBe(expectedRequestedAudience);
    }

    [Fact]
    public void Adapt_ShouldUpdateIssuedTokenJtiFromEntity()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel and TokenExchange with different issuedTokenJtis");
        var dataModel = CreateTestDataModel();
        string expectedIssuedTokenJti = Guid.NewGuid().ToString();
        var entity = CreateTestEntity(issuedTokenJti: expectedIssuedTokenJti);

        // Act
        LogAct("Adapting data model from entity");
        TokenExchangeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IssuedTokenJti was updated");
        dataModel.IssuedTokenJti.ShouldBe(expectedIssuedTokenJti);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel and TokenExchange with different EntityInfo values");
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

        var entity = TokenExchange.CreateFromExistingInfo(
            new CreateFromExistingInfoTokenExchangeInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Guid.NewGuid().ToString(),
                "api-service",
                Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1)));

        // Act
        LogAct("Adapting data model from entity");
        TokenExchangeDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating TokenExchangeDataModel and TokenExchange");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity();

        // Act
        LogAct("Adapting data model from entity");
        var result = TokenExchangeDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static TokenExchangeDataModel CreateTestDataModel()
    {
        return new TokenExchangeDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            SubjectTokenJti = Guid.NewGuid().ToString(),
            RequestedAudience = "initial-service",
            IssuedTokenJti = Guid.NewGuid().ToString(),
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
    }

    private static TokenExchange CreateTestEntity(
        Guid? userId = null,
        string? subjectTokenJti = null,
        string? requestedAudience = null,
        string? issuedTokenJti = null)
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

        return TokenExchange.CreateFromExistingInfo(
            new CreateFromExistingInfoTokenExchangeInput(
                entityInfo,
                Id.CreateFromExistingInfo(userId ?? Guid.NewGuid()),
                subjectTokenJti ?? Guid.NewGuid().ToString(),
                requestedAudience ?? "api-service",
                issuedTokenJti ?? Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1)));
    }

    #endregion
}
