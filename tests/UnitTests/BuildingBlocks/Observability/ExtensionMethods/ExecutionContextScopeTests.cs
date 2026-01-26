using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Observability.ExtensionMethods;

public class ExecutionContextScopeTests : TestBase
{
    private readonly ExecutionContext _executionContext;
    private readonly TenantInfo _tenantInfo;
    private readonly Guid _correlationId;

    public ExecutionContextScopeTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _correlationId = Guid.NewGuid();
        _tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        _executionContext = ExecutionContext.Create(
            correlationId: _correlationId,
            tenantInfo: _tenantInfo,
            executionUser: "test-user",
            executionOrigin: "test-origin",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System
        );
    }

    #region Count Tests

    [Fact]
    public void Count_ShouldReturnSeven()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting Count");
        var count = scope.Count;

        // Assert
        LogAssert("Verifying count is 7");
        count.ShouldBe(7);
        LogInfo("Count returned 7 as expected");
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_Index0_ShouldReturnTimestamp()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting element at index 0");
        var kvp = scope[0];

        // Assert
        LogAssert("Verifying Timestamp");
        kvp.Key.ShouldBe("Timestamp");
        kvp.Value.ShouldBe(_executionContext.Timestamp);
        LogInfo("Index 0 returned Timestamp");
    }

    [Fact]
    public void Indexer_Index1_ShouldReturnCorrelationId()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting element at index 1");
        var kvp = scope[1];

        // Assert
        LogAssert("Verifying CorrelationId");
        kvp.Key.ShouldBe("CorrelationId");
        kvp.Value.ShouldBe(_correlationId);
        LogInfo("Index 1 returned CorrelationId");
    }

    [Fact]
    public void Indexer_Index2_ShouldReturnTenantCode()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting element at index 2");
        var kvp = scope[2];

        // Assert
        LogAssert("Verifying TenantCode");
        kvp.Key.ShouldBe("TenantCode");
        kvp.Value.ShouldBe(_tenantInfo.Code);
        LogInfo("Index 2 returned TenantCode");
    }

    [Fact]
    public void Indexer_Index3_ShouldReturnTenantName()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting element at index 3");
        var kvp = scope[3];

        // Assert
        LogAssert("Verifying TenantName");
        kvp.Key.ShouldBe("TenantName");
        kvp.Value.ShouldBe("Test Tenant");
        LogInfo("Index 3 returned TenantName");
    }

    [Fact]
    public void Indexer_Index4_ShouldReturnExecutionUser()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting element at index 4");
        var kvp = scope[4];

        // Assert
        LogAssert("Verifying ExecutionUser");
        kvp.Key.ShouldBe("ExecutionUser");
        kvp.Value.ShouldBe("test-user");
        LogInfo("Index 4 returned ExecutionUser");
    }

    [Fact]
    public void Indexer_Index5_ShouldReturnExecutionOrigin()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting element at index 5");
        var kvp = scope[5];

        // Assert
        LogAssert("Verifying ExecutionOrigin");
        kvp.Key.ShouldBe("ExecutionOrigin");
        kvp.Value.ShouldBe("test-origin");
        LogInfo("Index 5 returned ExecutionOrigin");
    }

    [Fact]
    public void Indexer_Index6_ShouldReturnBusinessOperationCode()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Getting element at index 6");
        var kvp = scope[6];

        // Assert
        LogAssert("Verifying BusinessOperationCode");
        kvp.Key.ShouldBe("BusinessOperationCode");
        kvp.Value.ShouldBe("TEST_OP");
        LogInfo("Index 6 returned BusinessOperationCode");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(100)]
    public void Indexer_InvalidIndex_ShouldThrowArgumentOutOfRangeException(int index)
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act & Assert
        LogAct($"Getting element at invalid index {index}");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => _ = scope[index]);

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("index");
        exception.ActualValue.ShouldBe(index);
        exception.Message.ShouldContain("Index must be between 0 and 6");
        LogInfo($"ArgumentOutOfRangeException thrown for index {index}");
    }

    #endregion

    #region Enumerator Tests

    [Fact]
    public void GetEnumerator_ShouldEnumerateAllElements()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Enumerating all elements");
        var list = new List<KeyValuePair<string, object?>>();
        foreach (var kvp in scope)
        {
            list.Add(kvp);
        }

        // Assert
        LogAssert("Verifying all 7 elements were enumerated");
        list.Count.ShouldBe(7);
        list[0].Key.ShouldBe("Timestamp");
        list[1].Key.ShouldBe("CorrelationId");
        list[2].Key.ShouldBe("TenantCode");
        list[3].Key.ShouldBe("TenantName");
        list[4].Key.ShouldBe("ExecutionUser");
        list[5].Key.ShouldBe("ExecutionOrigin");
        list[6].Key.ShouldBe("BusinessOperationCode");
        LogInfo("All elements enumerated successfully");
    }

    [Fact]
    public void GetEnumerator_IEnumerableOfT_ShouldEnumerateAllElements()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);
        IEnumerable<KeyValuePair<string, object?>> enumerable = scope;

        // Act
        LogAct("Enumerating via IEnumerable<T>");
        var list = enumerable.ToList();

        // Assert
        LogAssert("Verifying all 7 elements were enumerated");
        list.Count.ShouldBe(7);
        LogInfo("IEnumerable<T> enumeration successful");
    }

    [Fact]
    public void GetEnumerator_IEnumerable_ShouldEnumerateAllElements()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);
        System.Collections.IEnumerable enumerable = scope;

        // Act
        LogAct("Enumerating via IEnumerable");
        var count = 0;
        foreach (var _ in enumerable)
        {
            count++;
        }

        // Assert
        LogAssert("Verifying all 7 elements were enumerated");
        count.ShouldBe(7);
        LogInfo("IEnumerable enumeration successful");
    }

    [Fact]
    public void Enumerator_Reset_ShouldResetToBeginning()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope and enumerator");
        var scope = new ExecutionContextScope(_executionContext);
        var enumerator = scope.GetEnumerator();

        // Act - Move to first element
        LogAct("Moving to first element then resetting");
        enumerator.MoveNext();
        var firstValue = enumerator.Current;
        enumerator.Reset();
        enumerator.MoveNext();
        var valueAfterReset = enumerator.Current;

        // Assert
        LogAssert("Verifying reset returned to beginning");
        firstValue.Key.ShouldBe(valueAfterReset.Key);
        firstValue.Value.ShouldBe(valueAfterReset.Value);
        LogInfo("Reset successfully returned to beginning");
    }

    [Fact]
    public void Enumerator_MoveNext_ReturnsFalseAfterLastElement()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope and enumerator");
        var scope = new ExecutionContextScope(_executionContext);
        var enumerator = scope.GetEnumerator();

        // Act - Move past all elements
        LogAct("Moving past all elements");
        for (int i = 0; i < 7; i++)
        {
            enumerator.MoveNext().ShouldBeTrue();
        }
        var resultAfterLast = enumerator.MoveNext();

        // Assert
        LogAssert("Verifying MoveNext returns false after last element");
        resultAfterLast.ShouldBeFalse();
        LogInfo("MoveNext correctly returned false after last element");
    }

    [Fact]
    public void Enumerator_Current_ViaIEnumerator_ShouldReturnBoxedValue()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope and enumerator");
        var scope = new ExecutionContextScope(_executionContext);
        System.Collections.IEnumerator enumerator = ((System.Collections.IEnumerable)scope).GetEnumerator();

        // Act
        LogAct("Getting Current via IEnumerator interface");
        enumerator.MoveNext();
        var current = enumerator.Current;

        // Assert
        LogAssert("Verifying Current returns boxed KeyValuePair");
        current.ShouldBeOfType<KeyValuePair<string, object?>>();
        LogInfo("IEnumerator.Current returned boxed value correctly");
    }

    [Fact]
    public void Enumerator_Dispose_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope and enumerator");
        var scope = new ExecutionContextScope(_executionContext);
        var enumerator = scope.GetEnumerator();

        // Act & Assert
        LogAct("Calling Dispose on enumerator");
        Should.NotThrow(() => enumerator.Dispose());
        LogInfo("Dispose completed without throwing");
    }

    #endregion

    #region IReadOnlyList<T> Implementation Tests

    [Fact]
    public void Scope_ImplementsIReadOnlyListOfKeyValuePair()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act & Assert
        LogAct("Verifying interface implementation");
        scope.ShouldBeAssignableTo<IReadOnlyList<KeyValuePair<string, object?>>>();
        LogInfo("ExecutionContextScope implements IReadOnlyList<KeyValuePair<string, object?>>");
    }

    [Fact]
    public void Scope_CanBeUsedWithLinq()
    {
        // Arrange
        LogArrange("Creating ExecutionContextScope");
        var scope = new ExecutionContextScope(_executionContext);

        // Act
        LogAct("Using LINQ to query scope");
        var keys = scope.Select(kvp => kvp.Key).ToList();

        // Assert
        LogAssert("Verifying LINQ query result");
        keys.ShouldContain("Timestamp");
        keys.ShouldContain("CorrelationId");
        keys.ShouldContain("ExecutionUser");
        LogInfo("LINQ operations work correctly on ExecutionContextScope");
    }

    #endregion
}
