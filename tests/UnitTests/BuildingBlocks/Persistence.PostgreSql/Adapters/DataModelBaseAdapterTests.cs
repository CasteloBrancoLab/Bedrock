using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Adapters;

public class DataModelBaseAdapterTests : TestBase
{
    public DataModelBaseAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldMapAllBaseProperties()
    {
        // Arrange
        LogArrange("Creating entity with EntityInfo");

        Guid id = Guid.NewGuid();
        Guid tenantCode = Guid.NewGuid();
        Guid createdCorrelationId = Guid.NewGuid();
        Guid lastChangedCorrelationId = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        DateTimeOffset lastChangedAt = DateTimeOffset.UtcNow;
        long entityVersion = 12345678L;

        EntityChangeInfo entityChangeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "creator-user",
            createdCorrelationId: createdCorrelationId,
            createdExecutionOrigin: "API",
            createdBusinessOperationCode: "CREATE_ORDER",
            lastChangedAt: lastChangedAt,
            lastChangedBy: "modifier-user",
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: "CLI",
            lastChangedBusinessOperationCode: "UPDATE_ORDER"
        );

        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(id),
            tenantInfo: TenantInfo.Create(tenantCode, "TestTenant"),
            entityChangeInfo: entityChangeInfo,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion)
        );

        TestAdapterEntity entity = new(entityInfo);
        TestAdapterDataModel dataModel = new();

        // Act
        LogAct("Adapting entity to data model");
        TestAdapterDataModel result = DataModelBaseAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying all base properties are mapped");
        result.ShouldBeSameAs(dataModel);
        result.Id.ShouldBe(id);
        result.TenantCode.ShouldBe(tenantCode);
        result.CreatedBy.ShouldBe("creator-user");
        result.CreatedAt.ShouldBe(createdAt);
        result.CreatedCorrelationId.ShouldBe(createdCorrelationId);
        result.CreatedExecutionOrigin.ShouldBe("API");
        result.CreatedBusinessOperationCode.ShouldBe("CREATE_ORDER");
        result.LastChangedBy.ShouldBe("modifier-user");
        result.LastChangedAt.ShouldBe(lastChangedAt);
        result.LastChangedExecutionOrigin.ShouldBe("CLI");
        result.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        result.LastChangedBusinessOperationCode.ShouldBe("UPDATE_ORDER");
        result.EntityVersion.ShouldBe(entityVersion);
    }

    [Fact]
    public void Adapt_WithNullLastChangedValues_ShouldMapNullValues()
    {
        // Arrange
        LogArrange("Creating entity with null LastChanged values");

        Guid id = Guid.NewGuid();
        Guid tenantCode = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        long entityVersion = 999L;

        EntityChangeInfo entityChangeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "creator-user",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "API",
            createdBusinessOperationCode: "CREATE_ORDER",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null
        );

        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(id),
            tenantInfo: TenantInfo.Create(tenantCode),
            entityChangeInfo: entityChangeInfo,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion)
        );

        TestAdapterEntity entity = new(entityInfo);
        TestAdapterDataModel dataModel = new();

        // Act
        LogAct("Adapting entity to data model");
        TestAdapterDataModel result = DataModelBaseAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying null values are mapped correctly");
        result.LastChangedBy.ShouldBeNull();
        result.LastChangedAt.ShouldBeNull();
        result.LastChangedExecutionOrigin.ShouldBeNull();
        result.LastChangedCorrelationId.ShouldBeNull();
        result.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldReturnSameInstance()
    {
        // Arrange
        LogArrange("Creating entity and data model");

        EntityChangeInfo entityChangeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "user",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "API",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null
        );

        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            entityChangeInfo: entityChangeInfo,
            entityVersion: RegistryVersion.CreateFromExistingInfo(1L)
        );

        TestAdapterEntity entity = new(entityInfo);
        TestAdapterDataModel dataModel = new();

        // Act
        LogAct("Adapting entity to data model");
        TestAdapterDataModel result = DataModelBaseAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }
}

/// <summary>
/// Test entity implementation for adapter tests
/// </summary>
internal sealed class TestAdapterEntity : EntityBase
{
    public TestAdapterEntity(EntityInfo entityInfo) : base(entityInfo)
    {
    }

    protected override bool IsValidInternal(ExecutionContext executionContext) => true;

    protected override string CreateMessageCode(string messageSuffix) => $"TestAdapterEntity.{messageSuffix}";
}

/// <summary>
/// Test data model for adapter tests
/// </summary>
public class TestAdapterDataModel : DataModelBase
{
    public string CustomProperty { get; set; } = null!;
}
