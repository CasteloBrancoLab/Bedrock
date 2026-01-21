using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Core.ExecutionContexts;

public class ExecutionContextAccessorTests : TestBase
{
    private readonly TimeProvider _timeProvider;

    public ExecutionContextAccessorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _timeProvider = TimeProvider.System;
    }

    private ExecutionContext CreateContext()
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            executionUser: "test-user",
            executionOrigin: "test-origin",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: _timeProvider
        );
    }

    [Fact]
    public void Current_Initially_ShouldBeNull()
    {
        // Arrange
        LogArrange("Creating new accessor");
        var accessor = new ExecutionContextAccessor();

        // Act
        LogAct("Getting Current");
        var current = accessor.Current;

        // Assert
        LogAssert("Verifying Current is null");
        current.ShouldBeNull();
        LogInfo("New accessor has null Current");
    }

    [Fact]
    public void SetCurrent_WithValidContext_ShouldSetCurrent()
    {
        // Arrange
        LogArrange("Creating accessor and context");
        var accessor = new ExecutionContextAccessor();
        var context = CreateContext();

        // Act
        LogAct("Setting Current");
        accessor.SetCurrent(context);

        // Assert
        LogAssert("Verifying Current is set");
        accessor.Current.ShouldBe(context);
        accessor.Current.ShouldNotBeNull();
        LogInfo("Current set successfully");
    }

    [Fact]
    public void SetCurrent_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Creating accessor");
        var accessor = new ExecutionContextAccessor();

        // Act & Assert
        LogAct("Setting Current to null");
        var exception = Should.Throw<ArgumentNullException>(() =>
            accessor.SetCurrent(null!));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("context");
        LogInfo("ArgumentNullException thrown for null context");
    }

    [Fact]
    public void SetCurrent_MultipleTimes_ShouldOverwrite()
    {
        // Arrange
        LogArrange("Creating accessor and two contexts");
        var accessor = new ExecutionContextAccessor();
        var context1 = CreateContext();
        var context2 = CreateContext();

        // Act
        LogAct("Setting Current twice");
        accessor.SetCurrent(context1);
        accessor.SetCurrent(context2);

        // Assert
        LogAssert("Verifying Current is the second context");
        accessor.Current.ShouldBe(context2);
        accessor.Current.ShouldNotBe(context1);
        LogInfo("Current was overwritten successfully");
    }

    [Fact]
    public void IExecutionContextAccessor_ShouldBeImplemented()
    {
        // Arrange
        LogArrange("Creating accessor");
        var accessor = new ExecutionContextAccessor();

        // Assert
        LogAssert("Verifying interface implementation");
        accessor.ShouldBeAssignableTo<IExecutionContextAccessor>();
        LogInfo("ExecutionContextAccessor implements IExecutionContextAccessor");
    }

    [Fact]
    public void SetCurrent_ThenAccessCurrent_ShouldReturnSameInstance()
    {
        // Arrange
        LogArrange("Creating accessor and context");
        var accessor = new ExecutionContextAccessor();
        var context = CreateContext();
        context.AddInformationMessage("TEST_MSG", "Test message");

        // Act
        LogAct("Setting and getting Current");
        accessor.SetCurrent(context);
        var retrieved = accessor.Current;

        // Assert
        LogAssert("Verifying same instance returned");
        retrieved.ShouldBeSameAs(context);
        retrieved!.Messages.Count().ShouldBe(1);
        LogInfo("Same instance returned from Current");
    }

    [Fact]
    public void MultipleAccessors_ShouldHaveIndependentContexts()
    {
        // Arrange
        LogArrange("Creating two accessors");
        var accessor1 = new ExecutionContextAccessor();
        var accessor2 = new ExecutionContextAccessor();
        var context1 = CreateContext();
        var context2 = CreateContext();

        // Act
        LogAct("Setting different contexts on each accessor");
        accessor1.SetCurrent(context1);
        accessor2.SetCurrent(context2);

        // Assert
        LogAssert("Verifying independent contexts");
        accessor1.Current.ShouldBe(context1);
        accessor2.Current.ShouldBe(context2);
        accessor1.Current.ShouldNotBe(accessor2.Current);
        LogInfo("Each accessor maintains its own context");
    }

    [Fact]
    public void Current_AfterSettingContext_ShouldReflectContextChanges()
    {
        // Arrange
        LogArrange("Creating accessor and context");
        var accessor = new ExecutionContextAccessor();
        var context = CreateContext();
        accessor.SetCurrent(context);

        // Act
        LogAct("Modifying context through accessor.Current");
        accessor.Current!.AddErrorMessage("ERROR", "An error occurred");

        // Assert
        LogAssert("Verifying changes reflected");
        accessor.Current.HasErrorMessages.ShouldBeTrue();
        accessor.Current.IsFaulted.ShouldBeTrue();
        LogInfo("Context modifications visible through Current");
    }

    [Fact]
    public void UsingInterfaceReference_ShouldWorkCorrectly()
    {
        // Arrange
        LogArrange("Creating accessor via interface");
        IExecutionContextAccessor accessor = new ExecutionContextAccessor();
        var context = CreateContext();

        // Act
        LogAct("Using interface methods");
        accessor.SetCurrent(context);

        // Assert
        LogAssert("Verifying interface usage");
        accessor.Current.ShouldBe(context);
        LogInfo("Interface methods work correctly");
    }

    [Fact]
    public void SetCurrent_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        LogArrange("Creating accessor for concurrent access test");
        var accessor = new ExecutionContextAccessor();
        var contexts = Enumerable.Range(0, 100)
            .Select(_ => CreateContext())
            .ToList();

        // Act
        LogAct("Setting contexts from multiple threads concurrently");
        Parallel.ForEach(contexts, context =>
        {
            accessor.SetCurrent(context);
            _ = accessor.Current;
        });

        // Assert
        LogAssert("Verifying accessor is in valid state after concurrent access");
        accessor.Current.ShouldNotBeNull();
        contexts.ShouldContain(accessor.Current);
        LogInfo("Accessor remained thread-safe during concurrent access");
    }

    [Fact]
    public void Current_ConcurrentReads_ShouldBeThreadSafe()
    {
        // Arrange
        LogArrange("Creating accessor with context for concurrent read test");
        var accessor = new ExecutionContextAccessor();
        var context = CreateContext();
        accessor.SetCurrent(context);

        // Act
        LogAct("Reading Current from multiple threads concurrently");
        var results = new System.Collections.Concurrent.ConcurrentBag<ExecutionContext?>();
        Parallel.For(0, 100, _ =>
        {
            results.Add(accessor.Current);
        });

        // Assert
        LogAssert("Verifying all reads returned the same context");
        results.ShouldAllBe(c => c == context);
        LogInfo("All concurrent reads returned the same context instance");
    }
}
