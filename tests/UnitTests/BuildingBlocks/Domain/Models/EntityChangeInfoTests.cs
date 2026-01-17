using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Domain.Models;

public class EntityChangeInfoTests : TestBase
{
    public EntityChangeInfoTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_ShouldPopulateCreationFields()
    {
        // Arrange
        LogArrange("Creating execution context");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(fixedTime);
        var correlationId = Guid.NewGuid();
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
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
        var changeInfo = EntityChangeInfo.RegisterNew(executionContext, createdBy: "creator.user");

        // Assert
        LogAssert("Verifying creation fields are populated");
        changeInfo.CreatedAt.ShouldBe(fixedTime);
        changeInfo.CreatedBy.ShouldBe("creator.user");
        changeInfo.CreatedCorrelationId.ShouldBe(correlationId);
        changeInfo.CreatedExecutionOrigin.ShouldBe("UnitTest");
        changeInfo.CreatedBusinessOperationCode.ShouldBe("TEST_OP");

        LogAssert("Verifying change fields are null");
        changeInfo.LastChangedAt.ShouldBeNull();
        changeInfo.LastChangedBy.ShouldBeNull();
        changeInfo.LastChangedCorrelationId.ShouldBeNull();
        changeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        changeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldUseTimeProviderForTimestamp()
    {
        // Arrange
        LogArrange("Creating execution context with specific time");
        var expectedTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(expectedTime);
        var executionContext = CreateExecutionContext(timeProvider);

        // Act
        LogAct("Calling RegisterNew");
        var changeInfo = EntityChangeInfo.RegisterNew(executionContext, createdBy: "user");

        // Assert
        LogAssert("Verifying timestamp from TimeProvider");
        changeInfo.CreatedAt.ShouldBe(expectedTime);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveAllValues()
    {
        // Arrange
        LogArrange("Preparing existing values");
        var createdAt = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var createdCorrelationId = Guid.NewGuid();
        var lastChangedAt = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        var lastChangedCorrelationId = Guid.NewGuid();

        // Act
        LogAct("Calling CreateFromExistingInfo");
        var changeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "original.creator",
            createdCorrelationId: createdCorrelationId,
            createdExecutionOrigin: "OriginalSystem",
            createdBusinessOperationCode: "ORIG_OP",
            lastChangedAt: lastChangedAt,
            lastChangedBy: "modifier.user",
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: "ModifierSystem",
            lastChangedBusinessOperationCode: "MOD_OP");

        // Assert
        LogAssert("Verifying all values preserved");
        changeInfo.CreatedAt.ShouldBe(createdAt);
        changeInfo.CreatedBy.ShouldBe("original.creator");
        changeInfo.CreatedCorrelationId.ShouldBe(createdCorrelationId);
        changeInfo.CreatedExecutionOrigin.ShouldBe("OriginalSystem");
        changeInfo.CreatedBusinessOperationCode.ShouldBe("ORIG_OP");
        changeInfo.LastChangedAt.ShouldBe(lastChangedAt);
        changeInfo.LastChangedBy.ShouldBe("modifier.user");
        changeInfo.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        changeInfo.LastChangedExecutionOrigin.ShouldBe("ModifierSystem");
        changeInfo.LastChangedBusinessOperationCode.ShouldBe("MOD_OP");
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullChangeFields_ShouldPreserveNulls()
    {
        // Arrange
        LogArrange("Preparing values with null change fields");
        var createdAt = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var createdCorrelationId = Guid.NewGuid();

        // Act
        LogAct("Calling CreateFromExistingInfo with nulls");
        var changeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "creator",
            createdCorrelationId: createdCorrelationId,
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);

        // Assert
        LogAssert("Verifying null values preserved");
        changeInfo.LastChangedAt.ShouldBeNull();
        changeInfo.LastChangedBy.ShouldBeNull();
        changeInfo.LastChangedCorrelationId.ShouldBeNull();
        changeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        changeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    #endregion

    #region RegisterChange Tests

    [Fact]
    public void RegisterChange_ShouldPreserveCreationFieldsAndPopulateChangeFields()
    {
        // Arrange
        LogArrange("Creating initial EntityChangeInfo");
        var creationTime = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var creationTimeProvider = new FixedTimeProvider(creationTime);
        var creationCorrelationId = Guid.NewGuid();
        var creationContext = ExecutionContext.Create(
            correlationId: creationCorrelationId,
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            executionUser: "creator",
            executionOrigin: "CreateSystem",
            businessOperationCode: "CREATE_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: creationTimeProvider);

        var original = EntityChangeInfo.RegisterNew(creationContext, createdBy: "original.creator");

        var changeTime = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        var changeTimeProvider = new FixedTimeProvider(changeTime);
        var changeCorrelationId = Guid.NewGuid();
        var changeContext = ExecutionContext.Create(
            correlationId: changeCorrelationId,
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            executionUser: "modifier",
            executionOrigin: "ModifySystem",
            businessOperationCode: "MODIFY_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: changeTimeProvider);

        // Act
        LogAct("Calling RegisterChange");
        var changed = original.RegisterChange(changeContext, changedBy: "modifier.user");

        // Assert
        LogAssert("Verifying creation fields preserved");
        changed.CreatedAt.ShouldBe(creationTime);
        changed.CreatedBy.ShouldBe("original.creator");
        changed.CreatedCorrelationId.ShouldBe(creationCorrelationId);
        changed.CreatedExecutionOrigin.ShouldBe("CreateSystem");
        changed.CreatedBusinessOperationCode.ShouldBe("CREATE_OP");

        LogAssert("Verifying change fields populated");
        changed.LastChangedAt.ShouldBe(changeTime);
        changed.LastChangedBy.ShouldBe("modifier.user");
        changed.LastChangedCorrelationId.ShouldBe(changeCorrelationId);
        changed.LastChangedExecutionOrigin.ShouldBe("ModifySystem");
        changed.LastChangedBusinessOperationCode.ShouldBe("MODIFY_OP");
    }

    [Fact]
    public void RegisterChange_ShouldOverwritePreviousChangeFields()
    {
        // Arrange
        LogArrange("Creating EntityChangeInfo with existing change fields");
        var firstChangeTime = new DateTimeOffset(2024, 3, 1, 10, 0, 0, TimeSpan.Zero);
        var firstCorrelationId = Guid.NewGuid();

        var original = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "CREATE",
            lastChangedAt: firstChangeTime,
            lastChangedBy: "first.modifier",
            lastChangedCorrelationId: firstCorrelationId,
            lastChangedExecutionOrigin: "FirstSystem",
            lastChangedBusinessOperationCode: "FIRST_CHANGE");

        var secondChangeTime = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        var secondTimeProvider = new FixedTimeProvider(secondChangeTime);
        var secondCorrelationId = Guid.NewGuid();
        var secondContext = ExecutionContext.Create(
            correlationId: secondCorrelationId,
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            executionUser: "second.modifier",
            executionOrigin: "SecondSystem",
            businessOperationCode: "SECOND_CHANGE",
            minimumMessageType: MessageType.Trace,
            timeProvider: secondTimeProvider);

        // Act
        LogAct("Calling RegisterChange again");
        var changed = original.RegisterChange(secondContext, changedBy: "second.modifier.user");

        // Assert
        LogAssert("Verifying new change fields overwrite old ones");
        changed.LastChangedAt.ShouldBe(secondChangeTime);
        changed.LastChangedBy.ShouldBe("second.modifier.user");
        changed.LastChangedCorrelationId.ShouldBe(secondCorrelationId);
        changed.LastChangedExecutionOrigin.ShouldBe("SecondSystem");
        changed.LastChangedBusinessOperationCode.ShouldBe("SECOND_CHANGE");
    }

    [Fact]
    public void RegisterChange_ShouldNotModifyOriginalInstance()
    {
        // Arrange
        LogArrange("Creating original EntityChangeInfo");
        var creationTime = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var creationTimeProvider = new FixedTimeProvider(creationTime);
        var executionContext = CreateExecutionContext(creationTimeProvider);

        var original = EntityChangeInfo.RegisterNew(executionContext, createdBy: "creator");
        var originalLastChangedAt = original.LastChangedAt;

        var changeTimeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var changeContext = CreateExecutionContext(changeTimeProvider);

        // Act
        LogAct("Calling RegisterChange");
        var _ = original.RegisterChange(changeContext, changedBy: "modifier");

        // Assert
        LogAssert("Verifying original is unchanged");
        original.LastChangedAt.ShouldBe(originalLastChangedAt);
        original.LastChangedBy.ShouldBeNull();
    }

    #endregion

    #region Record Struct Equality Tests

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Creating two EntityChangeInfo with same values");
        var createdAt = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var correlationId = Guid.NewGuid();

        var info1 = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "creator",
            createdCorrelationId: correlationId,
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);

        var info2 = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "creator",
            createdCorrelationId: correlationId,
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);

        // Act & Assert
        LogAct("Comparing for equality");
        info1.ShouldBe(info2);
        (info1 == info2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        LogArrange("Creating two EntityChangeInfo with different values");
        var createdAt = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);

        var info1 = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "creator1",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);

        var info2 = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: createdAt,
            createdBy: "creator2",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);

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
