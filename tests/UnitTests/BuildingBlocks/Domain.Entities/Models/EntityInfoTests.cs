using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Domain.Entities.Models;

public class EntityInfoTests : TestBase
{
    public EntityInfoTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_ShouldCreateEntityInfoWithAllFields()
    {
        // Arrange
        LogArrange("Creating execution context");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(fixedTime);
        var correlationId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        var tenantInfo = TenantInfo.Create(tenantCode, "Test Tenant");
        var executionContext = ExecutionContext.Create(
            correlationId: correlationId,
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        // Act
        LogAct("Calling RegisterNew");
        var entityInfo = EntityInfo.RegisterNew(
            executionContext,
            tenantInfo: tenantInfo,
            createdBy: "creator.user");

        // Assert
        LogAssert("Verifying Id is generated");
        entityInfo.Id.Value.ShouldNotBe(Guid.Empty);

        LogAssert("Verifying TenantInfo is set");
        entityInfo.TenantInfo.Code.ShouldBe(tenantCode);
        entityInfo.TenantInfo.Name.ShouldBe("Test Tenant");

        LogAssert("Verifying EntityChangeInfo is populated");
        entityInfo.EntityChangeInfo.CreatedAt.ShouldBe(fixedTime);
        entityInfo.EntityChangeInfo.CreatedBy.ShouldBe("creator.user");
        entityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(correlationId);

        LogAssert("Verifying EntityVersion is generated");
        entityInfo.EntityVersion.Value.ShouldNotBe(0L);
    }

    [Fact]
    public void RegisterNew_ShouldGenerateUniqueIds()
    {
        // Arrange
        LogArrange("Creating execution context");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        var executionContext = CreateExecutionContext(timeProvider);

        // Act
        LogAct("Creating multiple EntityInfo");
        var entityInfo1 = EntityInfo.RegisterNew(executionContext, tenantInfo, "user");
        var entityInfo2 = EntityInfo.RegisterNew(executionContext, tenantInfo, "user");

        // Assert
        LogAssert("Verifying Ids are unique");
        entityInfo1.Id.Value.ShouldNotBe(entityInfo2.Id.Value);
        entityInfo1.EntityVersion.Value.ShouldNotBe(entityInfo2.EntityVersion.Value);
    }

    #endregion

    #region CreateFromExistingInfo (EntityChangeInfo overload) Tests

    [Fact]
    public void CreateFromExistingInfo_WithEntityChangeInfo_ShouldPreserveAllValues()
    {
        // Arrange
        LogArrange("Preparing existing values");
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var entityChangeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "CREATE",
            lastChangedAt: new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero),
            lastChangedBy: "modifier",
            lastChangedCorrelationId: Guid.NewGuid(),
            lastChangedExecutionOrigin: "ModSystem",
            lastChangedBusinessOperationCode: "MODIFY");
        var entityVersion = RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow);

        // Act
        LogAct("Calling CreateFromExistingInfo");
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: id,
            tenantInfo: tenantInfo,
            entityChangeInfo: entityChangeInfo,
            entityVersion: entityVersion);

        // Assert
        LogAssert("Verifying all values preserved");
        entityInfo.Id.ShouldBe(id);
        entityInfo.TenantInfo.ShouldBe(tenantInfo);
        entityInfo.EntityChangeInfo.ShouldBe(entityChangeInfo);
        entityInfo.EntityVersion.ShouldBe(entityVersion);
    }

    #endregion

    #region CreateFromExistingInfo (Individual fields overload) Tests

    [Fact]
    public void CreateFromExistingInfo_WithIndividualFields_ShouldPreserveAllValues()
    {
        // Arrange
        LogArrange("Preparing individual field values");
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var createdAt = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var createdCorrelationId = Guid.NewGuid();
        var lastChangedAt = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        var lastChangedCorrelationId = Guid.NewGuid();
        var entityVersion = RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow);

        // Act
        LogAct("Calling CreateFromExistingInfo with individual fields");
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: id,
            tenantInfo: tenantInfo,
            createdAt: createdAt,
            createdBy: "creator.user",
            createdCorrelationId: createdCorrelationId,
            createdExecutionOrigin: "OriginalSystem",
            createdBusinessOperationCode: "ORIG_OP",
            lastChangedAt: lastChangedAt,
            lastChangedBy: "modifier.user",
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: "ModifierSystem",
            lastChangedBusinessOperationCode: "MOD_OP",
            entityVersion: entityVersion);

        // Assert
        LogAssert("Verifying all values preserved");
        entityInfo.Id.ShouldBe(id);
        entityInfo.TenantInfo.ShouldBe(tenantInfo);
        entityInfo.EntityChangeInfo.CreatedAt.ShouldBe(createdAt);
        entityInfo.EntityChangeInfo.CreatedBy.ShouldBe("creator.user");
        entityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(createdCorrelationId);
        entityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe("OriginalSystem");
        entityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe("ORIG_OP");
        entityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(lastChangedAt);
        entityInfo.EntityChangeInfo.LastChangedBy.ShouldBe("modifier.user");
        entityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        entityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe("ModifierSystem");
        entityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe("MOD_OP");
        entityInfo.EntityVersion.ShouldBe(entityVersion);
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullChangeFields_ShouldPreserveNulls()
    {
        // Arrange
        LogArrange("Preparing values with null change fields");
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        var entityVersion = RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow);

        // Act
        LogAct("Calling CreateFromExistingInfo with null change fields");
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: id,
            tenantInfo: tenantInfo,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: entityVersion);

        // Assert
        LogAssert("Verifying null change fields");
        entityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        entityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        entityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    #endregion

    #region RegisterChange Tests

    [Fact]
    public void RegisterChange_ShouldPreserveIdAndTenantAndUpdateChangeInfo()
    {
        // Arrange
        LogArrange("Creating initial EntityInfo");
        var creationTime = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var creationTimeProvider = new FixedTimeProvider(creationTime);
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var creationContext = CreateExecutionContext(creationTimeProvider);

        var original = EntityInfo.RegisterNew(creationContext, tenantInfo, "creator");
        var originalId = original.Id;
        var originalTenant = original.TenantInfo;

        var changeTime = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        var changeTimeProvider = new FixedTimeProvider(changeTime);
        var changeCorrelationId = Guid.NewGuid();
        var changeContext = ExecutionContext.Create(
            correlationId: changeCorrelationId,
            tenantInfo: tenantInfo,
            executionUser: "modifier",
            executionOrigin: "ModifySystem",
            businessOperationCode: "MODIFY_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: changeTimeProvider);

        // Act
        LogAct("Calling RegisterChange");
        var changed = original.RegisterChange(changeContext, changedBy: "modifier.user");

        // Assert
        LogAssert("Verifying Id is preserved");
        changed.Id.ShouldBe(originalId);

        LogAssert("Verifying TenantInfo is preserved");
        changed.TenantInfo.ShouldBe(originalTenant);

        LogAssert("Verifying creation fields preserved");
        changed.EntityChangeInfo.CreatedAt.ShouldBe(creationTime);
        changed.EntityChangeInfo.CreatedBy.ShouldBe("creator");

        LogAssert("Verifying change fields populated");
        changed.EntityChangeInfo.LastChangedAt.ShouldBe(changeTime);
        changed.EntityChangeInfo.LastChangedBy.ShouldBe("modifier.user");
        changed.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(changeCorrelationId);
        changed.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe("ModifySystem");
        changed.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe("MODIFY_OP");

        LogAssert("Verifying EntityVersion is updated");
        changed.EntityVersion.Value.ShouldNotBe(original.EntityVersion.Value);
    }

    [Fact]
    public void RegisterChange_ShouldNotModifyOriginalInstance()
    {
        // Arrange
        LogArrange("Creating original EntityInfo");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        var executionContext = CreateExecutionContext(timeProvider);

        var original = EntityInfo.RegisterNew(executionContext, tenantInfo, "creator");
        var originalVersion = original.EntityVersion;
        var originalLastChangedAt = original.EntityChangeInfo.LastChangedAt;

        var changeTimeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow.AddHours(1));
        var changeContext = CreateExecutionContext(changeTimeProvider);

        // Act
        LogAct("Calling RegisterChange");
        _ = original.RegisterChange(changeContext, changedBy: "modifier");

        // Assert
        LogAssert("Verifying original is unchanged");
        original.EntityVersion.ShouldBe(originalVersion);
        original.EntityChangeInfo.LastChangedAt.ShouldBe(originalLastChangedAt);
    }

    [Fact]
    public void RegisterChange_ShouldGenerateNewVersion()
    {
        // Arrange
        LogArrange("Creating EntityInfo");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        var executionContext = CreateExecutionContext(timeProvider);

        var original = EntityInfo.RegisterNew(executionContext, tenantInfo, "creator");

        var changeTimeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow.AddHours(1));
        var changeContext = CreateExecutionContext(changeTimeProvider);

        // Act
        LogAct("Calling RegisterChange multiple times");
        var changed1 = original.RegisterChange(changeContext, changedBy: "modifier1");
        var changed2 = changed1.RegisterChange(changeContext, changedBy: "modifier2");

        // Assert
        LogAssert("Verifying versions are different");
        original.EntityVersion.Value.ShouldNotBe(changed1.EntityVersion.Value);
        changed1.EntityVersion.Value.ShouldNotBe(changed2.EntityVersion.Value);
        original.EntityVersion.Value.ShouldNotBe(changed2.EntityVersion.Value);
    }

    #endregion

    #region Record Struct Equality Tests

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Creating two EntityInfo with same values");
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        var correlationId = Guid.NewGuid();
        var entityChangeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: correlationId,
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);
        var entityVersion = RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow);

        var info1 = EntityInfo.CreateFromExistingInfo(id, tenantInfo, entityChangeInfo, entityVersion);
        var info2 = EntityInfo.CreateFromExistingInfo(id, tenantInfo, entityChangeInfo, entityVersion);

        // Act & Assert
        LogAct("Comparing for equality");
        info1.ShouldBe(info2);
        (info1 == info2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        LogArrange("Creating two EntityInfo with different Ids");
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        var correlationId = Guid.NewGuid();
        var entityChangeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: correlationId,
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);
        var entityVersion = RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow);

        var info1 = EntityInfo.CreateFromExistingInfo(
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo, entityChangeInfo, entityVersion);
        var info2 = EntityInfo.CreateFromExistingInfo(
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo, entityChangeInfo, entityVersion);

        // Act & Assert
        LogAct("Comparing for inequality");
        info1.ShouldNotBe(info2);
        (info1 != info2).ShouldBeTrue();
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateExecutionContext(TimeProvider timeProvider)
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);
    }

    private class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _fixedTime;

        public FixedTimeProvider(DateTimeOffset fixedTime)
        {
            _fixedTime = fixedTime;
        }

        public override DateTimeOffset GetUtcNow() => _fixedTime;
    }

    #endregion
}
