using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class TenantFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public TenantFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapNameFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel with specific name");
        string expectedName = Faker.Company.CompanyName();
        var dataModel = CreateTestDataModel(name: expectedName);

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Name mapping");
        entity.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Create_ShouldMapDomainFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel with specific domain");
        string expectedDomain = "test-domain.com";
        var dataModel = CreateTestDataModel(domain: expectedDomain);

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Domain mapping");
        entity.Domain.ShouldBe(expectedDomain);
    }

    [Fact]
    public void Create_ShouldMapSchemaNameFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel with specific schemaName");
        string expectedSchemaName = "custom_schema";
        var dataModel = CreateTestDataModel(schemaName: expectedSchemaName);

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying SchemaName mapping");
        entity.SchemaName.ShouldBe(expectedSchemaName);
    }

    [Theory]
    [InlineData((short)1, TenantStatus.Active)]
    [InlineData((short)2, TenantStatus.Suspended)]
    [InlineData((short)3, TenantStatus.Maintenance)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, TenantStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating TenantDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel(status: statusValue);

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Theory]
    [InlineData((short)1, TenantTier.Basic)]
    [InlineData((short)2, TenantTier.Professional)]
    [InlineData((short)3, TenantTier.Enterprise)]
    public void Create_ShouldMapTierFromDataModel(short tierValue, TenantTier expectedTier)
    {
        // Arrange
        LogArrange($"Creating TenantDataModel with tier value {tierValue}");
        var dataModel = CreateTestDataModel(tier: tierValue);

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Tier mapped to {expectedTier}");
        entity.Tier.ShouldBe(expectedTier);
    }

    [Fact]
    public void Create_ShouldMapDbVersionFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel with specific dbVersion");
        string? expectedDbVersion = "2.0.0";
        var dataModel = CreateTestDataModel();
        dataModel.DbVersion = expectedDbVersion;

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying DbVersion mapping");
        entity.DbVersion.ShouldBe(expectedDbVersion);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel with specific base fields");
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-5);
        long expectedVersion = Faker.Random.Long(1);
        string? expectedLastChangedBy = Faker.Person.FullName;
        var expectedLastChangedAt = DateTimeOffset.UtcNow;
        var expectedLastChangedCorrelationId = Guid.NewGuid();
        string expectedLastChangedExecutionOrigin = "TestOrigin";
        string expectedLastChangedBusinessOperationCode = "TEST_OP";

        var dataModel = new TenantDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            LastChangedBy = expectedLastChangedBy,
            LastChangedAt = expectedLastChangedAt,
            LastChangedCorrelationId = expectedLastChangedCorrelationId,
            LastChangedExecutionOrigin = expectedLastChangedExecutionOrigin,
            LastChangedBusinessOperationCode = expectedLastChangedBusinessOperationCode,
            EntityVersion = expectedVersion,
            Name = "Test Tenant",
            Domain = "example.com",
            SchemaName = "tenant_schema",
            Status = (short)TenantStatus.Active,
            Tier = (short)TenantTier.Basic,
            DbVersion = null
        };

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EntityInfo fields");
        entity.EntityInfo.Id.Value.ShouldBe(expectedId);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        entity.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBe(expectedLastChangedBy);
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(expectedLastChangedAt);
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(expectedLastChangedCorrelationId);
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe(expectedLastChangedExecutionOrigin);
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe(expectedLastChangedBusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullLastChangedFields_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating TenantDataModel with null last-changed fields");
        var dataModel = new TenantDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Name = "Test Tenant",
            Domain = "example.com",
            SchemaName = "tenant_schema",
            Status = (short)TenantStatus.Active,
            Tier = (short)TenantTier.Basic,
            DbVersion = null
        };

        // Act
        LogAct("Creating Tenant from TenantDataModel with nulls");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying nullable fields are null");
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapCreatedCorrelationIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating TenantDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating Tenant from TenantDataModel");
        var entity = TenantFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static TenantDataModel CreateTestDataModel(
        string? name = null,
        string? domain = null,
        string? schemaName = null,
        short? status = null,
        short? tier = null)
    {
        return new TenantDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_TENANT",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Name = name ?? "Test Tenant",
            Domain = domain ?? "example.com",
            SchemaName = schemaName ?? "tenant_schema",
            Status = status ?? (short)TenantStatus.Active,
            Tier = tier ?? (short)TenantTier.Basic,
            DbVersion = null
        };
    }

    #endregion
}
