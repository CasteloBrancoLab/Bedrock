using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Application.Factories.Messages;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Application.Factories.Messages;

public class AuthMessageMetadataFactoryTests : TestBase
{
    public AuthMessageMetadataFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void Create_WithValidInputs_ShouldReturnMetadataWithCorrectFields()
    {
        // Arrange
        LogArrange("Creating execution context and time provider");
        var executionContext = CreateTestExecutionContext();
        var timeProvider = TimeProvider.System;

        // Act
        LogAct("Creating MessageMetadata via factory");
        var metadata = AuthMessageMetadataFactory.Create(executionContext, timeProvider);

        // Assert
        LogAssert("Verifying metadata fields match execution context");
        metadata.MessageId.ShouldNotBe(Guid.Empty);
        metadata.CorrelationId.ShouldBe(executionContext.CorrelationId);
        metadata.TenantCode.ShouldBe(executionContext.TenantInfo.Code);
        metadata.ExecutionUser.ShouldBe(executionContext.ExecutionUser);
        metadata.ExecutionOrigin.ShouldBe(executionContext.ExecutionOrigin);
        metadata.BusinessOperationCode.ShouldBe(executionContext.BusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullExecutionContext_ShouldThrow()
    {
        // Act & Assert
        LogAct("Calling Create with null execution context");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            AuthMessageMetadataFactory.Create(null!, TimeProvider.System));
    }

    [Fact]
    public void Create_WithNullTimeProvider_ShouldThrow()
    {
        // Act & Assert
        LogAct("Calling Create with null time provider");
        LogAssert("Verifying ArgumentNullException is thrown");
        var executionContext = CreateTestExecutionContext();
        Should.Throw<ArgumentNullException>(() =>
            AuthMessageMetadataFactory.Create(executionContext, null!));
    }

    [Fact]
    public void Create_ShouldUseTimeProviderForTimestamp()
    {
        // Arrange
        LogArrange("Creating execution context with fixed time provider");
        var executionContext = CreateTestExecutionContext();
        var fixedTime = new DateTimeOffset(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(fixedTime);

        // Act
        LogAct("Creating MessageMetadata with fixed time provider");
        var metadata = AuthMessageMetadataFactory.Create(executionContext, fakeTimeProvider);

        // Assert
        LogAssert("Verifying timestamp comes from time provider");
        metadata.Timestamp.ShouldBe(fixedTime);
    }

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private sealed class FakeTimeProvider(DateTimeOffset fixedTime) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedTime;
    }
}
