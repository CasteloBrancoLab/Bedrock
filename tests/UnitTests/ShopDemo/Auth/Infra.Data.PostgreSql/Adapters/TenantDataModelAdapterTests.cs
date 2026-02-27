using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Domain.Entities.Tenants.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class TenantDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public TenantDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateNameFromEntity()
    {
        // Arrange
        LogArrange("Creating TenantDataModel and Tenant with different names");
        var dataModel = CreateTestDataModel();
        string expectedName = Faker.Company.CompanyName();
        var entity = CreateTestEntity(name: expectedName);

        // Act
        LogAct("Adapting data model from entity");
        TenantDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Name was updated");
        dataModel.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Adapt_ShouldUpdateDomainFromEntity()
    {
        // Arrange
        LogArrange("Creating TenantDataModel and Tenant with different domains");
        var dataModel = CreateTestDataModel();
        string expectedDomain = "new-domain.com";
        var entity = CreateTestEntity(domain: expectedDomain);

        // Act
        LogAct("Adapting data model from entity");
        TenantDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Domain was updated");
        dataModel.Domain.ShouldBe(expectedDomain);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating TenantDataModel and Tenant with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)TenantStatus.Active;
        var entity = CreateTestEntity(status: TenantStatus.Suspended);

        // Act
        LogAct("Adapting data model from entity");
        TenantDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)TenantStatus.Suspended);
    }

    [Fact]
    public void Adapt_ShouldUpdateTierFromEntity()
    {
        // Arrange
        LogArrange("Creating TenantDataModel and Tenant with different tiers");
        var dataModel = CreateTestDataModel();
        dataModel.Tier = (short)TenantTier.Basic;
        var entity = CreateTestEntity(tier: TenantTier.Enterprise);

        // Act
        LogAct("Adapting data model from entity");
        TenantDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Tier was updated");
        dataModel.Tier.ShouldBe((short)TenantTier.Enterprise);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating TenantDataModel and Tenant with different EntityInfo values");
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

        var entity = Tenant.CreateFromExistingInfo(
            new CreateFromExistingInfoTenantInput(
                entityInfo,
                "Test Tenant",
                "example.com",
                "tenant_schema",
                TenantStatus.Active,
                TenantTier.Basic,
                null));

        // Act
        LogAct("Adapting data model from entity");
        TenantDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating TenantDataModel and Tenant");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity();

        // Act
        LogAct("Adapting data model from entity");
        var result = TenantDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static TenantDataModel CreateTestDataModel()
    {
        return new TenantDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            Name = "Initial Tenant",
            Domain = "initial.com",
            SchemaName = "initial_schema",
            Status = (short)TenantStatus.Active,
            Tier = (short)TenantTier.Basic,
            DbVersion = null
        };
    }

    private static Tenant CreateTestEntity(
        string? name = null,
        string? domain = null,
        TenantStatus status = TenantStatus.Active,
        TenantTier tier = TenantTier.Basic)
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

        return Tenant.CreateFromExistingInfo(
            new CreateFromExistingInfoTenantInput(
                entityInfo,
                name ?? "Test Tenant",
                domain ?? "example.com",
                "tenant_schema",
                status,
                tier,
                null));
    }

    #endregion
}
