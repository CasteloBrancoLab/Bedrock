using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class TokenExchangeDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public TokenExchangeDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdCorrectly()
    {
        // Arrange
        LogArrange("Creating TokenExchange entity with known userId");
        var expectedUserId = Guid.NewGuid();
        var entity = CreateTestEntityWithUserId(expectedUserId);

        // Act
        LogAct("Creating TokenExchangeDataModel from TokenExchange entity");
        var dataModel = TokenExchangeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying UserId mapping");
        dataModel.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapSubjectTokenJtiCorrectly()
    {
        // Arrange
        LogArrange("Creating TokenExchange entity with known subjectTokenJti");
        string expectedSubjectTokenJti = Guid.NewGuid().ToString();
        var entity = CreateTestEntity(subjectTokenJti: expectedSubjectTokenJti);

        // Act
        LogAct("Creating TokenExchangeDataModel from TokenExchange entity");
        var dataModel = TokenExchangeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying SubjectTokenJti mapping");
        dataModel.SubjectTokenJti.ShouldBe(expectedSubjectTokenJti);
    }

    [Fact]
    public void Create_ShouldMapRequestedAudienceCorrectly()
    {
        // Arrange
        LogArrange("Creating TokenExchange entity with known requestedAudience");
        string expectedRequestedAudience = "api-service";
        var entity = CreateTestEntity(requestedAudience: expectedRequestedAudience);

        // Act
        LogAct("Creating TokenExchangeDataModel from TokenExchange entity");
        var dataModel = TokenExchangeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RequestedAudience mapping");
        dataModel.RequestedAudience.ShouldBe(expectedRequestedAudience);
    }

    [Fact]
    public void Create_ShouldMapIssuedTokenJtiCorrectly()
    {
        // Arrange
        LogArrange("Creating TokenExchange entity with known issuedTokenJti");
        string expectedIssuedTokenJti = Guid.NewGuid().ToString();
        var entity = CreateTestEntity(issuedTokenJti: expectedIssuedTokenJti);

        // Act
        LogAct("Creating TokenExchangeDataModel from TokenExchange entity");
        var dataModel = TokenExchangeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying IssuedTokenJti mapping");
        dataModel.IssuedTokenJti.ShouldBe(expectedIssuedTokenJti);
    }

    [Fact]
    public void Create_ShouldMapIssuedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating TokenExchange entity with known issuedAt");
        var expectedIssuedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var entity = CreateTestEntity(issuedAt: expectedIssuedAt);

        // Act
        LogAct("Creating TokenExchangeDataModel from TokenExchange entity");
        var dataModel = TokenExchangeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying IssuedAt mapping");
        dataModel.IssuedAt.ShouldBe(expectedIssuedAt);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating TokenExchange entity with known expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var entity = CreateTestEntity(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Creating TokenExchangeDataModel from TokenExchange entity");
        var dataModel = TokenExchangeDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating TokenExchange entity with specific EntityInfo values");
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
        LogAct("Creating TokenExchangeDataModel from TokenExchange entity");
        var dataModel = TokenExchangeDataModelFactory.Create(entity);

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

    private static TokenExchange CreateTestEntity(
        string? subjectTokenJti = null,
        string? requestedAudience = null,
        string? issuedTokenJti = null,
        DateTimeOffset? issuedAt = null,
        DateTimeOffset? expiresAt = null)
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
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                subjectTokenJti ?? Guid.NewGuid().ToString(),
                requestedAudience ?? "api-service",
                issuedTokenJti ?? Guid.NewGuid().ToString(),
                issuedAt ?? DateTimeOffset.UtcNow,
                expiresAt ?? DateTimeOffset.UtcNow.AddHours(1)));
    }

    private static TokenExchange CreateTestEntityWithUserId(Guid userId)
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
                Id.CreateFromExistingInfo(userId),
                Guid.NewGuid().ToString(),
                "api-service",
                Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1)));
    }

    #endregion
}
