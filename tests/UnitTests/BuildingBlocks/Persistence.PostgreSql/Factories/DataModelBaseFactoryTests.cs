using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Factories;

public class DataModelBaseFactoryTests : TestBase
{
    public DataModelBaseFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void CreateFromEntity_ShouldMapAllBaseProperties()
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

        TestFactoryEntity entity = new(entityInfo);

        // Act
        LogAct("Creating data model from entity");
        TestFactoryDataModel result = DataModelBaseFactory.Create<TestFactoryDataModel, TestFactoryEntity>(entity);

        // Assert
        LogAssert("Verifying all base properties are mapped");
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
    public void CreateFromEntity_WithNullLastChangedValues_ShouldMapNullValues()
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

        TestFactoryEntity entity = new(entityInfo);

        // Act
        LogAct("Creating data model from entity");
        TestFactoryDataModel result = DataModelBaseFactory.Create<TestFactoryDataModel, TestFactoryEntity>(entity);

        // Assert
        LogAssert("Verifying null values are mapped correctly");
        result.LastChangedBy.ShouldBeNull();
        result.LastChangedAt.ShouldBeNull();
        result.LastChangedExecutionOrigin.ShouldBeNull();
        result.LastChangedCorrelationId.ShouldBeNull();
        result.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void CreateFromExecutionContext_ShouldMapPropertiesFromContext()
    {
        // Arrange
        LogArrange("Creating ExecutionContext");

        Guid tenantCode = Guid.NewGuid();
        Guid correlationId = Guid.NewGuid();
        DateTimeOffset timestamp = new(2025, 1, 15, 10, 30, 0, TimeSpan.Zero);

        Mock<TimeProvider> timeProviderMock = new();
        timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(timestamp);

        ExecutionContext executionContext = ExecutionContext.Create(
            correlationId: correlationId,
            tenantInfo: TenantInfo.Create(tenantCode, "TestTenant"),
            executionUser: "test-user",
            executionOrigin: "API",
            businessOperationCode: "CREATE_ORDER",
            minimumMessageType: MessageType.Information,
            timeProvider: timeProviderMock.Object
        );

        // Act
        LogAct("Creating data model from execution context");
        TestFactoryDataModel result = DataModelBaseFactory.Create<TestFactoryDataModel>(executionContext);

        // Assert
        LogAssert("Verifying properties are mapped from execution context");
        result.TenantCode.ShouldBe(tenantCode);
        result.CreatedBy.ShouldBe("test-user");
        result.CreatedAt.ShouldBe(timestamp);
        result.CreatedCorrelationId.ShouldBe(correlationId);
        result.CreatedExecutionOrigin.ShouldBe("API");
        result.CreatedBusinessOperationCode.ShouldBe("CREATE_ORDER");
        result.LastChangedBy.ShouldBeNull();
        result.LastChangedAt.ShouldBeNull();
        result.LastChangedExecutionOrigin.ShouldBeNull();
        result.LastChangedCorrelationId.ShouldBeNull();
        result.LastChangedBusinessOperationCode.ShouldBeNull();
        result.EntityVersion.ShouldBe(timestamp.Ticks);
    }

    [Fact]
    public void CreateFromExecutionContext_ShouldSetEntityVersionToTimestampTicks()
    {
        // Arrange
        LogArrange("Creating ExecutionContext with specific timestamp");

        DateTimeOffset timestamp = new(2025, 6, 15, 14, 30, 45, TimeSpan.Zero);

        Mock<TimeProvider> timeProviderMock = new();
        timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(timestamp);

        ExecutionContext executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            executionUser: "user",
            executionOrigin: "CLI",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Information,
            timeProvider: timeProviderMock.Object
        );

        // Act
        LogAct("Creating data model from execution context");
        TestFactoryDataModel result = DataModelBaseFactory.Create<TestFactoryDataModel>(executionContext);

        // Assert
        LogAssert("Verifying EntityVersion equals timestamp ticks");
        result.EntityVersion.ShouldBe(timestamp.Ticks);
    }

    [Fact]
    public void CreateFromExecutionContext_ShouldCreateNewInstanceEachTime()
    {
        // Arrange
        LogArrange("Creating ExecutionContext");

        Mock<TimeProvider> timeProviderMock = new();
        timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        ExecutionContext executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            executionUser: "user",
            executionOrigin: "CLI",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Information,
            timeProvider: timeProviderMock.Object
        );

        // Act
        LogAct("Creating two data models from same execution context");
        TestFactoryDataModel result1 = DataModelBaseFactory.Create<TestFactoryDataModel>(executionContext);
        TestFactoryDataModel result2 = DataModelBaseFactory.Create<TestFactoryDataModel>(executionContext);

        // Assert
        LogAssert("Verifying different instances are created");
        result1.ShouldNotBeSameAs(result2);
    }
}

/// <summary>
/// Test entity implementation for factory tests
/// </summary>
internal sealed class TestFactoryEntity : EntityBase
{
    public TestFactoryEntity(EntityInfo entityInfo) : base(entityInfo)
    {
    }

    protected override bool IsValidInternal(ExecutionContext executionContext) => true;

    protected override string CreateMessageCode(string messageSuffix) => $"TestFactoryEntity.{messageSuffix}";
}

/// <summary>
/// Test data model for factory tests
/// </summary>
public class TestFactoryDataModel : DataModelBase
{
}
