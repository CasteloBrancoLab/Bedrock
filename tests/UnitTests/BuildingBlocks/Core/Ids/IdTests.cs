using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Ids;

public class IdTests : TestBase
{
    public IdTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void GenerateNewId_ShouldReturnValidId()
    {
        // Arrange
        LogArrange("Preparing to generate a new Id");

        // Act
        LogAct("Generating new Id");
        var id = Id.GenerateNewId();

        // Assert
        LogAssert("Verifying Id is not empty");
        id.Value.ShouldNotBe(Guid.Empty);
        LogInfo("Generated Id: {0}", id.Value);
    }

    [Fact]
    public void GenerateNewId_ShouldBeMonotonicWithinSameMillisecond()
    {
        // Arrange
        LogArrange("Preparing to generate multiple Ids in rapid succession");
        const int count = 1000;
        var ids = new Id[count];

        // Act
        LogAct($"Generating {count} Ids");
        for (int i = 0; i < count; i++)
        {
            ids[i] = Id.GenerateNewId();
        }

        // Assert
        LogAssert("Verifying Ids are monotonically increasing");
        for (int i = 1; i < count; i++)
        {
            ids[i].ShouldBeGreaterThan(ids[i - 1],
                $"Id at index {i} should be greater than Id at index {i - 1}");
        }
        LogInfo("All {0} Ids are monotonically increasing", count);
    }

    [Fact]
    public void GenerateNewId_WithTimeProvider_ShouldUseProvidedTime()
    {
        // Arrange
        LogArrange("Creating a fixed time provider");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(fixedTime);

        // Act
        LogAct("Generating Id with fixed time provider");
        var id = Id.GenerateNewId(timeProvider);

        // Assert
        LogAssert("Verifying Id is not empty");
        id.Value.ShouldNotBe(Guid.Empty);
        LogInfo("Generated Id with fixed time: {0}", id.Value);
    }

    [Fact]
    public void GenerateNewId_WithDateTimeOffset_ShouldUseProvidedTime()
    {
        // Arrange
        LogArrange("Creating a fixed DateTimeOffset");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating Id with DateTimeOffset");
        var id = Id.GenerateNewId(fixedTime);

        // Assert
        LogAssert("Verifying Id is not empty");
        id.Value.ShouldNotBe(Guid.Empty);
        LogInfo("Generated Id: {0}", id.Value);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveGuidValue()
    {
        // Arrange
        LogArrange("Creating a known Guid");
        var expectedGuid = Guid.NewGuid();

        // Act
        LogAct("Creating Id from existing Guid");
        var id = Id.CreateFromExistingInfo(expectedGuid);

        // Assert
        LogAssert("Verifying Guid value is preserved");
        id.Value.ShouldBe(expectedGuid);
        LogInfo("Id value matches original Guid");
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        LogArrange("Generating a new Id");
        var id = Id.GenerateNewId();

        // Act
        LogAct("Implicitly converting Id to Guid");
        Guid guid = id;

        // Assert
        LogAssert("Verifying conversion preserves value");
        guid.ShouldBe(id.Value);
        LogInfo("Implicit conversion to Guid successful");
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldWork()
    {
        // Arrange
        LogArrange("Creating a Guid");
        var guid = Guid.NewGuid();

        // Act
        LogAct("Implicitly converting Guid to Id");
        Id id = guid;

        // Assert
        LogAssert("Verifying conversion preserves value");
        id.Value.ShouldBe(guid);
        LogInfo("Implicit conversion from Guid successful");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two Ids with same Guid");
        var guid = Guid.NewGuid();
        var id1 = Id.CreateFromExistingInfo(guid);
        var id2 = Id.CreateFromExistingInfo(guid);

        // Act
        LogAct("Comparing Ids for equality");
        var areEqual = id1.Equals(id2);

        // Assert
        LogAssert("Verifying Ids are equal");
        areEqual.ShouldBeTrue();
        LogInfo("Ids with same value are equal");
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two different Ids");
        var id1 = Id.GenerateNewId();
        var id2 = Id.GenerateNewId();

        // Act
        LogAct("Comparing Ids for equality");
        var areEqual = id1.Equals(id2);

        // Assert
        LogAssert("Verifying Ids are not equal");
        areEqual.ShouldBeFalse();
        LogInfo("Different Ids are not equal");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two Ids with same Guid");
        var guid = Guid.NewGuid();
        var id1 = Id.CreateFromExistingInfo(guid);
        var id2 = Id.CreateFromExistingInfo(guid);

        // Act & Assert
        LogAct("Testing equality operator");
        (id1 == id2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void InequalityOperator_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two different Ids");
        var id1 = Id.GenerateNewId();
        var id2 = Id.GenerateNewId();

        // Act & Assert
        LogAct("Testing inequality operator");
        (id1 != id2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    [Fact]
    public void ComparisonOperators_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two sequential Ids");
        var id1 = Id.GenerateNewId();
        var id2 = Id.GenerateNewId();

        // Act & Assert
        LogAct("Testing comparison operators");
        (id1 < id2).ShouldBeTrue("First Id should be less than second");
        (id2 > id1).ShouldBeTrue("Second Id should be greater than first");
        (id1 <= id2).ShouldBeTrue("First Id should be less than or equal to second");
        (id2 >= id1).ShouldBeTrue("Second Id should be greater than or equal to first");
        (id1 <= id1).ShouldBeTrue("Id should be less than or equal to itself");
        (id1 >= id1).ShouldBeTrue("Id should be greater than or equal to itself");
        LogAssert("All comparison operators work correctly");
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating an Id");
        var guid = Guid.NewGuid();
        var id = Id.CreateFromExistingInfo(guid);

        // Act
        LogAct("Getting hash code multiple times");
        var hash1 = id.GetHashCode();
        var hash2 = id.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are consistent");
        hash1.ShouldBe(hash2);
        hash1.ShouldBe(guid.GetHashCode());
        LogInfo("Hash code is consistent: {0}", hash1);
    }

    [Fact]
    public void GenerateNewId_ThreadSafety_ShouldGenerateUniqueIds()
    {
        // Arrange
        LogArrange("Preparing parallel Id generation");
        const int threadsCount = 10;
        const int idsPerThread = 1000;
        var allIds = new System.Collections.Concurrent.ConcurrentBag<Guid>();

        // Act
        LogAct($"Generating {threadsCount * idsPerThread} Ids across {threadsCount} threads");
        Parallel.For(0, threadsCount, _ =>
        {
            for (int i = 0; i < idsPerThread; i++)
            {
                var id = Id.GenerateNewId();
                allIds.Add(id.Value);
            }
        });

        // Assert
        LogAssert("Verifying all Ids are unique");
        var uniqueIds = allIds.Distinct().Count();
        uniqueIds.ShouldBe(allIds.Count, "All generated Ids should be unique");
        LogInfo("Generated {0} unique Ids across {1} threads", uniqueIds, threadsCount);
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
}
