using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ServiceClientClaimDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientClaimDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapServiceClientIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaim entity with known serviceClientId");
        var expectedServiceClientId = Guid.NewGuid();
        var entity = CreateTestEntity(expectedServiceClientId, Guid.NewGuid(), (short)1);

        // Act
        LogAct("Creating ServiceClientClaimDataModel from ServiceClientClaim entity");
        var dataModel = ServiceClientClaimDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ServiceClientId mapping");
        dataModel.ServiceClientId.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Create_ShouldMapClaimIdCorrectly()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaim entity with known claimId");
        var expectedClaimId = Guid.NewGuid();
        var entity = CreateTestEntity(Guid.NewGuid(), expectedClaimId, (short)1);

        // Act
        LogAct("Creating ServiceClientClaimDataModel from ServiceClientClaim entity");
        var dataModel = ServiceClientClaimDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ClaimId mapping");
        dataModel.ClaimId.ShouldBe(expectedClaimId);
    }

    [Theory]
    [InlineData((short)1)]
    [InlineData((short)-1)]
    [InlineData((short)0)]
    public void Create_ShouldMapValueCorrectly(short expectedValue)
    {
        // Arrange
        LogArrange($"Creating ServiceClientClaim entity with value {expectedValue}");
        var entity = CreateTestEntity(Guid.NewGuid(), Guid.NewGuid(), expectedValue);

        // Act
        LogAct("Creating ServiceClientClaimDataModel from ServiceClientClaim entity");
        var dataModel = ServiceClientClaimDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Value mapping");
        dataModel.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaim entity with specific EntityInfo values");
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

        var entity = ServiceClientClaim.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientClaimInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                ClaimValue.CreateFromExistingInfo(1)));

        // Act
        LogAct("Creating ServiceClientClaimDataModel from ServiceClientClaim entity");
        var dataModel = ServiceClientClaimDataModelFactory.Create(entity);

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

    private static ServiceClientClaim CreateTestEntity(
        Guid serviceClientId,
        Guid claimId,
        short value)
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

        return ServiceClientClaim.CreateFromExistingInfo(
            new CreateFromExistingInfoServiceClientClaimInput(
                entityInfo,
                Id.CreateFromExistingInfo(serviceClientId),
                Id.CreateFromExistingInfo(claimId),
                ClaimValue.CreateFromExistingInfo(value)));
    }

    #endregion
}
