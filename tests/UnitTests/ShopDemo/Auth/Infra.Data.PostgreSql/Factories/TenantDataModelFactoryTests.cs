using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Domain.Entities.Tenants.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class TenantDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public TenantDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapNameCorrectly()
    {
        // Arrange
        LogArrange("Creating Tenant entity with known name");
        string expectedName = Faker.Company.CompanyName();
        var entity = CreateTestEntity(name: expectedName);

        // Act
        LogAct("Creating TenantDataModel from Tenant entity");
        var dataModel = TenantDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Name mapping");
        dataModel.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Create_ShouldMapDomainCorrectly()
    {
        // Arrange
        LogArrange("Creating Tenant entity with known domain");
        string expectedDomain = "example.com";
        var entity = CreateTestEntity(domain: expectedDomain);

        // Act
        LogAct("Creating TenantDataModel from Tenant entity");
        var dataModel = TenantDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Domain mapping");
        dataModel.Domain.ShouldBe(expectedDomain);
    }

    [Fact]
    public void Create_ShouldMapSchemaNameCorrectly()
    {
        // Arrange
        LogArrange("Creating Tenant entity with known schemaName");
        string expectedSchemaName = "tenant_schema";
        var entity = CreateTestEntity(schemaName: expectedSchemaName);

        // Act
        LogAct("Creating TenantDataModel from Tenant entity");
        var dataModel = TenantDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying SchemaName mapping");
        dataModel.SchemaName.ShouldBe(expectedSchemaName);
    }

    [Theory]
    [InlineData(TenantStatus.Active, 1)]
    [InlineData(TenantStatus.Suspended, 2)]
    [InlineData(TenantStatus.Maintenance, 3)]
    public void Create_ShouldMapStatusAsShortCorrectly(TenantStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating Tenant entity with status {status}");
        var entity = CreateTestEntity(status: status);

        // Act
        LogAct("Creating TenantDataModel from Tenant entity");
        var dataModel = TenantDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Theory]
    [InlineData(TenantTier.Basic, 1)]
    [InlineData(TenantTier.Professional, 2)]
    [InlineData(TenantTier.Enterprise, 3)]
    public void Create_ShouldMapTierAsShortCorrectly(TenantTier tier, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating Tenant entity with tier {tier}");
        var entity = CreateTestEntity(tier: tier);

        // Act
        LogAct("Creating TenantDataModel from Tenant entity");
        var dataModel = TenantDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Tier mapped to short value {expectedShortValue}");
        dataModel.Tier.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapDbVersionCorrectly()
    {
        // Arrange
        LogArrange("Creating Tenant entity with known dbVersion");
        string? expectedDbVersion = "1.2.3";
        var entity = CreateTestEntity(dbVersion: expectedDbVersion);

        // Act
        LogAct("Creating TenantDataModel from Tenant entity");
        var dataModel = TenantDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying DbVersion mapping");
        dataModel.DbVersion.ShouldBe(expectedDbVersion);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating Tenant entity with specific EntityInfo values");
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
        LogAct("Creating TenantDataModel from Tenant entity");
        var dataModel = TenantDataModelFactory.Create(entity);

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

    private static Tenant CreateTestEntity(
        string? name = null,
        string? domain = null,
        string? schemaName = null,
        TenantStatus status = TenantStatus.Active,
        TenantTier tier = TenantTier.Basic,
        string? dbVersion = null)
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
                schemaName ?? "tenant_schema",
                status,
                tier,
                dbVersion));
    }

    #endregion
}
