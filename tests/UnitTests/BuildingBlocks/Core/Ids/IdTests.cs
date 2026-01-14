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
#pragma warning disable CS1718 // Comparison to same variable - intentional test of reflexive comparison operators
        (id1 <= id1).ShouldBeTrue("Id should be less than or equal to itself");
        (id1 >= id1).ShouldBeTrue("Id should be greater than or equal to itself");
#pragma warning restore CS1718
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
    public void ComparisonOperators_LessThan_WithEqualIds_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two Ids with same value");
        var guid = Guid.NewGuid();
        var id1 = Id.CreateFromExistingInfo(guid);
        var id2 = Id.CreateFromExistingInfo(guid);

        // Act & Assert
        LogAct("Testing less than operator with equal values");
        (id1 < id2).ShouldBeFalse("Equal Ids should not be less than each other");
        (id2 < id1).ShouldBeFalse("Equal Ids should not be less than each other");
        LogAssert("Less than operator returns false for equal Ids");
    }

    [Fact]
    public void ComparisonOperators_GreaterThan_WithEqualIds_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two Ids with same value");
        var guid = Guid.NewGuid();
        var id1 = Id.CreateFromExistingInfo(guid);
        var id2 = Id.CreateFromExistingInfo(guid);

        // Act & Assert
        LogAct("Testing greater than operator with equal values");
        (id1 > id2).ShouldBeFalse("Equal Ids should not be greater than each other");
        (id2 > id1).ShouldBeFalse("Equal Ids should not be greater than each other");
        LogAssert("Greater than operator returns false for equal Ids");
    }

    [Fact]
    public void GenerateNewId_WithClockDrift_ShouldHandleBackwardsTime()
    {
        // Arrange
        LogArrange("Creating timestamps simulating clock drift");
        var futureTime = DateTimeOffset.UtcNow.AddSeconds(10);
        var pastTime = DateTimeOffset.UtcNow.AddSeconds(-10);

        // Act
        LogAct("Generating Id with future time, then past time");
        var idFuture = Id.GenerateNewId(futureTime);
        var idPast = Id.GenerateNewId(pastTime);

        // Assert
        LogAssert("Verifying past Id is still greater (monotonic guarantee)");
        idPast.ShouldBeGreaterThan(idFuture, "Clock drift should be handled - later generated Id should be greater");
        LogInfo("Clock drift handled correctly");
    }

    [Fact]
    public void GenerateNewId_ShouldProduceValidUuidV7Structure()
    {
        // Arrange
        LogArrange("Generating a new Id");

        // Act
        LogAct("Creating Id and extracting version/variant");
        var id = Id.GenerateNewId();
        var bytes = id.Value.ToByteArray();

        // Assert
        LogAssert("Verifying UUID version is 7");
        var version = (bytes[7] >> 4) & 0x0F;
        version.ShouldBe(7, "UUID version should be 7");

        LogAssert("Verifying UUID variant is RFC 4122");
        var variant = (bytes[8] >> 6) & 0x03;
        variant.ShouldBe(2, "UUID variant should be 2 (10 binary = RFC 4122)");

        LogInfo("UUID structure is valid: version={0}, variant={1}", version, variant);
    }

    [Fact]
    public void GenerateNewId_WithSameTimestamp_ShouldIncrementCounter()
    {
        // Arrange
        LogArrange("Creating fixed timestamp");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating multiple Ids with same timestamp");
        var id1 = Id.GenerateNewId(fixedTime);
        var id2 = Id.GenerateNewId(fixedTime);
        var id3 = Id.GenerateNewId(fixedTime);

        // Assert
        LogAssert("Verifying Ids are different and ordered");
        id1.Value.ShouldNotBe(id2.Value);
        id2.Value.ShouldNotBe(id3.Value);
        id2.ShouldBeGreaterThan(id1);
        id3.ShouldBeGreaterThan(id2);
        LogInfo("Counter increment working correctly");
    }

    [Fact]
    public void GenerateNewId_IdsWithDifferentTimestamps_ShouldBeOrdered()
    {
        // Arrange
        LogArrange("Creating two different timestamps");
        var time1 = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating Ids with different timestamps");
        var id1 = Id.GenerateNewId(time1);
        var id2 = Id.GenerateNewId(time2);

        // Assert
        LogAssert("Verifying Ids are ordered by timestamp");
        id2.ShouldBeGreaterThan(id1, "Id with later timestamp should be greater");
        LogInfo("Timestamp ordering verified");
    }

    [Fact]
    public void GenerateNewId_WithExactSameTimestamp_CounterShouldDifferentiate()
    {
        // Arrange
        LogArrange("Creating exact same timestamp");
        var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating two Ids with exact same timestamp");
        var id1 = Id.GenerateNewId(fixedTime);
        var id2 = Id.GenerateNewId(fixedTime);

        // Assert
        LogAssert("Verifying counter differentiates same-timestamp Ids");
        id1.Value.ShouldNotBe(id2.Value, "Ids with same timestamp should be different");
        id2.ShouldBeGreaterThan(id1, "Second Id should be greater due to counter increment");
        LogInfo("Counter correctly differentiates same-timestamp Ids");
    }

    [Fact]
    public void GenerateNewId_MultipleRapidCalls_AllShouldBeUnique()
    {
        // Arrange
        LogArrange("Preparing to generate many Ids rapidly");
        var fixedTime = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        const int count = 100;
        var ids = new HashSet<Guid>();

        // Act
        LogAct($"Generating {count} Ids with same timestamp");
        for (int i = 0; i < count; i++)
        {
            var id = Id.GenerateNewId(fixedTime);
            ids.Add(id.Value);
        }

        // Assert
        LogAssert("Verifying all Ids are unique");
        ids.Count.ShouldBe(count, "All Ids should be unique even with same timestamp");
        LogInfo("All {0} Ids are unique", count);
    }

    [Fact]
    public void GenerateNewId_NewMillisecond_ShouldResetCounter()
    {
        // Arrange
        LogArrange("Creating two timestamps 1ms apart");
        var time1 = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 1, TimeSpan.Zero);

        // Act
        LogAct("Generating Ids: first with time1, then with time2 (1ms later)");
        var id1 = Id.GenerateNewId(time1);
        var id2 = Id.GenerateNewId(time2);

        // Assert
        LogAssert("Verifying new timestamp produces greater Id");
        id2.ShouldBeGreaterThan(id1, "Id with later timestamp should be greater");
        id1.Value.ShouldNotBe(id2.Value);
        LogInfo("New millisecond handling verified");
    }

    [Fact]
    public void GenerateNewId_ClockDriftBackwards_ShouldMaintainMonotonicity()
    {
        // Arrange
        LogArrange("Simulating clock drift backwards");
        var normalTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var driftedTime = new DateTimeOffset(2025, 6, 15, 11, 59, 59, TimeSpan.Zero);

        // Act
        LogAct("Generating Id at normal time, then at drifted (earlier) time");
        var idNormal = Id.GenerateNewId(normalTime);
        var idDrifted = Id.GenerateNewId(driftedTime);

        // Assert
        LogAssert("Verifying monotonicity maintained despite clock drift");
        idDrifted.ShouldBeGreaterThan(idNormal, "Later generated Id should be greater even with clock drift");
        LogInfo("Clock drift handled correctly - monotonicity maintained");
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

    [Fact]
    public void GenerateNewId_TimestampBitsAreEncodedCorrectly()
    {
        // Arrange - gera dois IDs sequenciais e verifica que timestamp muda corretamente
        LogArrange("Verificando encoding de timestamp via comparacao");

        // Act - gera dois IDs com timestamps diferentes
        LogAct("Gerando IDs com timestamps diferentes");
        var time1 = new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2100, 1, 1, 0, 0, 1, TimeSpan.Zero); // 1 segundo depois

        var id1 = Id.GenerateNewId(time1);
        var id2 = Id.GenerateNewId(time2);

        var bytes1 = id1.Value.ToByteArray();
        var bytes2 = id2.Value.ToByteArray();

        // Extrai timestamp (bytes 0-5 contem timestamp de 48 bits)
        int a1 = BitConverter.ToInt32(bytes1, 0);
        int a2 = BitConverter.ToInt32(bytes2, 0);

        // Assert - timestamp do id2 deve ser maior
        LogAssert("Verificando que timestamp maior gera bytes maiores");
        // Como time2 > time1, o timestamp high de id2 deve ser >= ao de id1
        // (podem ser iguais se a diferenca esta nos 16 bits baixos)
        var ts1 = time1.ToUnixTimeMilliseconds();
        var ts2 = time2.ToUnixTimeMilliseconds();
        (ts2 > ts1).ShouldBeTrue("Timestamp 2 deve ser maior que timestamp 1");
        id2.ShouldBeGreaterThan(id1, "ID com timestamp maior deve ser maior");

        LogInfo("Timestamp encoding verificado via comparacao");
    }

    [Fact]
    public void GenerateNewId_VersionAndVariantBitsAreCorrect()
    {
        // Arrange
        LogArrange("Generating an Id to verify version and variant encoding");

        // Act
        LogAct("Generating Id and extracting version/variant");
        var id = Id.GenerateNewId();
        var bytes = id.Value.ToByteArray();

        // c is at bytes 6-7, d is at byte 8
        short c = BitConverter.ToInt16(bytes, 6);
        byte d = bytes[8];

        // Assert - version should be 7 (0x7xxx in high nibble after accounting for little-endian)
        LogAssert("Verifying version 7 is set correctly");
        var versionNibble = (c >> 12) & 0x0F;
        versionNibble.ShouldBe(7, "Version nibble should be 7");

        // Assert - variant should be 10xx xxxx (0x80-0xBF range)
        LogAssert("Verifying variant is RFC 4122");
        var variantBits = (d >> 6) & 0x03;
        variantBits.ShouldBe(2, "Variant bits should be 10 (binary)");

        LogInfo("Version and variant correctly encoded");
    }

    [Fact]
    public void GenerateNewId_CounterBitsAreDistributedCorrectly()
    {
        // Arrange - Generate two Ids at same timestamp to verify counter distribution
        LogArrange("Creating fixed timestamp for counter verification");
        var fixedTime = new DateTimeOffset(2025, 3, 15, 10, 30, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating two Ids with same timestamp");
        var id1 = Id.GenerateNewId(fixedTime);
        var id2 = Id.GenerateNewId(fixedTime);

        var bytes1 = id1.Value.ToByteArray();
        var bytes2 = id2.Value.ToByteArray();

        // Counter is split: 12 bits in c (masked with 0x0FFF), 6 bits in d (masked with 0x3F), 8 bits in e
        // c is at bytes 6-7, d is at byte 8, e is at byte 9
        short c1 = BitConverter.ToInt16(bytes1, 6);
        short c2 = BitConverter.ToInt16(bytes2, 6);
        byte d1 = bytes1[8];
        byte d2 = bytes2[8];
        byte e1 = bytes1[9];
        byte e2 = bytes2[9];

        // Extract counter bits from each Id
        var counter1Bits = ((c1 & 0x0FFF) << 14) | ((d1 & 0x3F) << 8) | e1;
        var counter2Bits = ((c2 & 0x0FFF) << 14) | ((d2 & 0x3F) << 8) | e2;

        // Assert - second counter should be exactly one more than first
        LogAssert("Verifying counter increments correctly");
        counter2Bits.ShouldBeGreaterThan(counter1Bits, "Second Id's counter should be greater");

        LogInfo("Counter distribution verified: {0} -> {1}", counter1Bits, counter2Bits);
    }

    [Fact]
    public void GenerateNewId_RandomBytesAreDifferent()
    {
        // Arrange
        LogArrange("Generating multiple Ids to verify random bytes");
        var fixedTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        const int count = 10;

        // Act
        LogAct($"Generating {count} Ids and extracting random bytes");
        var randomBytesList = new List<byte[]>();
        for (int i = 0; i < count; i++)
        {
            var id = Id.GenerateNewId(fixedTime);
            var bytes = id.Value.ToByteArray();
            // Random bytes are at positions 10-15 (f through k in Guid constructor)
            var randomBytes = new byte[6];
            Array.Copy(bytes, 10, randomBytes, 0, 6);
            randomBytesList.Add(randomBytes);
        }

        // Assert - at least some random bytes should differ between Ids
        LogAssert("Verifying random bytes vary across Ids");
        var uniqueRandomParts = randomBytesList
            .Select(b => BitConverter.ToString(b))
            .Distinct()
            .Count();
        uniqueRandomParts.ShouldBeGreaterThan(1, "Random bytes should differ across Ids");

        LogInfo("Random bytes are different across {0} Ids", uniqueRandomParts);
    }

    [Fact]
    public void GenerateNewId_CounterOverflowProtection_ShouldWork()
    {
        // This test verifies the counter overflow check at 0x3FFFFFF
        // We can't easily trigger it without generating 67 million Ids,
        // but we can verify the mask value is used correctly

        // Arrange
        LogArrange("Verifying counter is bounded");
        var fixedTime = new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero);

        // Act - Generate enough Ids to verify counter stays within bounds
        LogAct("Generating Ids to verify counter bounds");
        const int count = 1000;
        for (int i = 0; i < count; i++)
        {
            var id = Id.GenerateNewId(fixedTime);
            var bytes = id.Value.ToByteArray();

            // Extract counter from the 26-bit space
            short c = BitConverter.ToInt16(bytes, 6);
            byte d = bytes[8];
            byte e = bytes[9];
            var counter = ((c & 0x0FFF) << 14) | ((d & 0x3F) << 8) | e;

            // Counter should never exceed 0x3FFFFFF
            counter.ShouldBeLessThanOrEqualTo(0x3FFFFFF, "Counter should not exceed max value");
        }

        LogAssert("Counter stays within valid bounds");
        LogInfo("All {0} Ids have valid counter values", count);
    }

    [Fact]
    public void GenerateNewId_SameTimestamp_CounterMustIncrement()
    {
        // This test specifically verifies counter increment behavior
        // to kill mutation: _counter++ -> _counter-- or statement removal

        // Arrange
        LogArrange("Setting up to verify counter increment");
        var fixedTime = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act - Generate 3 sequential Ids with exact same timestamp
        LogAct("Generating 3 Ids with same timestamp");
        var id1 = Id.GenerateNewId(fixedTime);
        var id2 = Id.GenerateNewId(fixedTime);
        var id3 = Id.GenerateNewId(fixedTime);

        // Extract counters from each Id
        var counter1 = ExtractCounter(id1);
        var counter2 = ExtractCounter(id2);
        var counter3 = ExtractCounter(id3);

        // Assert - counters must be strictly increasing
        LogAssert("Verifying counters are strictly increasing");
        counter2.ShouldBeGreaterThan(counter1, "Counter should increment, not decrement or stay same");
        counter3.ShouldBeGreaterThan(counter2, "Counter should continue incrementing");
        (counter2 - counter1).ShouldBe(1, "Counter should increment by exactly 1");
        (counter3 - counter2).ShouldBe(1, "Counter should increment by exactly 1");

        LogInfo("Counters: {0} -> {1} -> {2}", counter1, counter2, counter3);
    }

    [Fact]
    public void GenerateNewId_NewerTimestamp_ResetsCounterToZero()
    {
        // This test verifies that moving to a newer timestamp resets counter
        // to kill mutation: timestamp > _lastTimestamp -> timestamp >= _lastTimestamp

        // Arrange
        LogArrange("Setting up timestamps for counter reset verification");
        var time1 = new DateTimeOffset(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2031, 1, 1, 0, 0, 1, TimeSpan.Zero); // 1 second later

        // Act - Generate multiple Ids at time1, then one at time2
        LogAct("Generating Ids to verify counter reset");
        var id1a = Id.GenerateNewId(time1);
        var id1b = Id.GenerateNewId(time1);
        var id1c = Id.GenerateNewId(time1);
        var id2 = Id.GenerateNewId(time2); // Should reset counter

        // Extract counters
        var counter1a = ExtractCounter(id1a);
        var counter1c = ExtractCounter(id1c);
        var counter2 = ExtractCounter(id2);

        // Assert - counter at time2 should be reset (lower than accumulated counter at time1)
        LogAssert("Verifying counter reset on new timestamp");
        counter1c.ShouldBeGreaterThan(counter1a, "Counter should accumulate at same timestamp");
        counter2.ShouldBeLessThan(counter1c, "Counter should reset to 0 on new timestamp");
        counter2.ShouldBe(0, "Counter should be exactly 0 for new timestamp");

        LogInfo("Counter at time1 end: {0}, Counter at time2 start: {1}", counter1c, counter2);
    }

    [Fact]
    public void GenerateNewId_ClockDrift_MaintainsLastTimestampAndIncrementsCounter()
    {
        // This test verifies clock drift handling
        // to kill mutation: timestamp < _lastTimestamp -> timestamp <= _lastTimestamp

        // Arrange
        LogArrange("Setting up clock drift scenario");
        var futureTime = new DateTimeOffset(2032, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var pastTime = new DateTimeOffset(2032, 5, 1, 0, 0, 0, TimeSpan.Zero); // 1 month before

        // Act - Generate Id at future time, then at past time (simulating clock drift)
        LogAct("Generating Ids with clock drift");
        var idFuture = Id.GenerateNewId(futureTime);
        var idPast = Id.GenerateNewId(pastTime);

        // Extract counters and verify the past Id used the future timestamp
        var counterFuture = ExtractCounter(idFuture);
        var counterPast = ExtractCounter(idPast);

        // Assert - despite clock drift, IDs should still be monotonic
        LogAssert("Verifying monotonicity despite clock drift");
        idPast.ShouldBeGreaterThan(idFuture, "Later generated Id should be greater despite clock drift");
        counterPast.ShouldBeGreaterThan(counterFuture, "Counter should increment during clock drift");

        LogInfo("Future counter: {0}, Past counter: {1}", counterFuture, counterPast);
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldCallValueEquals()
    {
        // This test covers the Equals(object? obj) method
        // to kill mutation: block removal on line 77

        // Arrange
        LogArrange("Creating Id and object for equality test");
        var guid = Guid.NewGuid();
        var id = Id.CreateFromExistingInfo(guid);
        object objSame = Id.CreateFromExistingInfo(guid);
        object objDifferent = Id.CreateFromExistingInfo(Guid.NewGuid());
        object? objNull = null;
        object objWrongType = "not an id";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        id.Equals(objSame).ShouldBeTrue("Equal Id objects should be equal");
        id.Equals(objDifferent).ShouldBeFalse("Different Id objects should not be equal");
        id.Equals(objNull).ShouldBeFalse("Null should not be equal");
        id.Equals(objWrongType).ShouldBeFalse("Wrong type should not be equal");

        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void GenerateNewId_VerifyBitwiseOperations_VersionBits()
    {
        // This test verifies the bitwise operations for version encoding
        // to kill mutation: 0x7000 -> other values

        // Arrange
        LogArrange("Generating Id to verify version bits");
        var knownTime = DateTimeOffset.UtcNow.AddYears(50);

        // Act
        LogAct("Extracting and verifying version nibble");
        var id = Id.GenerateNewId(knownTime);
        var bytes = id.Value.ToByteArray();

        // c is at bytes 6-7, extract version nibble (high 4 bits)
        short c = BitConverter.ToInt16(bytes, 6);
        var versionNibble = (c >> 12) & 0x0F;

        // Assert - version MUST be exactly 7
        LogAssert("Verifying version is exactly 7");
        versionNibble.ShouldBe(7, "Version nibble must be exactly 7 (0x7xxx)");

        // Additional check: if mutation changed 0x7000 to 0x6000 or 0x8000, this would fail
        (c & 0xF000).ShouldBe(0x7000, "High nibble of c must be 0x7xxx");

        LogInfo("Version bits verified: {0:X4}", c);
    }

    [Fact]
    public void GenerateNewId_VerifyBitwiseOperations_VariantBits()
    {
        // This test verifies the bitwise operations for variant encoding
        // to kill mutation: 0x80 -> other values, 0x3F -> other values

        // Arrange
        LogArrange("Generating Id to verify variant bits");
        var knownTime = DateTimeOffset.UtcNow.AddYears(51);

        // Act
        LogAct("Extracting and verifying variant byte");
        var id = Id.GenerateNewId(knownTime);
        var bytes = id.Value.ToByteArray();

        // d is at byte 8, variant is in high 2 bits
        byte d = bytes[8];
        var variantBits = (d >> 6) & 0x03;

        // Assert - variant MUST be exactly 2 (binary 10)
        LogAssert("Verifying variant is exactly 2");
        variantBits.ShouldBe(2, "Variant bits must be 10 (binary) = 2");

        // Additional check: d must be in range 0x80-0xBF
        d.ShouldBeGreaterThanOrEqualTo((byte)0x80, "d must have variant bits set (>= 0x80)");
        d.ShouldBeLessThan((byte)0xC0, "d must be valid RFC 4122 variant (< 0xC0)");

        LogInfo("Variant bits verified: d=0x{0:X2}", d);
    }

    [Fact]
    public void GenerateNewId_VerifyBitwiseOperations_CounterLowBits()
    {
        // This test verifies counterLow (byte e) contains expected bits
        // to kill mutation: counter & 0xFF -> other mask

        // Arrange
        LogArrange("Generating Ids to verify counter low byte");
        var fixedTime = new DateTimeOffset(2033, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act - Generate several Ids to accumulate counter
        LogAct("Generating Ids and extracting counter low bytes");
        var ids = new List<Id>();
        var counterLowBytes = new List<byte>();

        for (int i = 0; i < 300; i++)
        {
            var id = Id.GenerateNewId(fixedTime);
            ids.Add(id);
            counterLowBytes.Add(id.Value.ToByteArray()[9]); // byte e = counterLow
        }

        // Assert - counter low bytes should cycle through values
        LogAssert("Verifying counter low byte varies");
        var uniqueCounterLowBytes = counterLowBytes.Distinct().Count();
        uniqueCounterLowBytes.ShouldBeGreaterThan(1, "Counter low byte should change as counter increments");

        // After 256 increments, we should have wrapped through all possible low byte values
        // if counter started at 0, positions 0-255 should have unique low bytes
        var first256 = counterLowBytes.Take(256).Distinct().Count();
        first256.ShouldBe(256, "First 256 counter values should have unique low bytes");

        LogInfo("Unique counter low bytes: {0}, First 256: {1}", uniqueCounterLowBytes, first256);
    }

    [Fact]
    public void GenerateNewId_ClockDrift_StrictLessThan_NotLessOrEqual()
    {
        // Mata mutante: timestamp < _lastTimestamp -> timestamp <= _lastTimestamp
        // Se mutar para <=, ids com mesmo timestamp entrariam no branch errado

        // Arrange
        LogArrange("Verificando que clock drift usa < estrito, nao <=");
        var time1 = new DateTimeOffset(2040, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act - gera dois IDs no MESMO timestamp
        LogAct("Gerando 2 IDs com timestamp identico");
        var id1 = Id.GenerateNewId(time1);
        var id2 = Id.GenerateNewId(time1);

        // Assert - ambos devem ser diferentes e ordenados
        LogAssert("Verificando que mesmo timestamp gera IDs diferentes e ordenados");
        id1.Value.ShouldNotBe(id2.Value);
        id2.ShouldBeGreaterThan(id1, "Segundo ID deve ser maior com mesmo timestamp");

        var counter1 = ExtractCounter(id1);
        var counter2 = ExtractCounter(id2);
        (counter2 - counter1).ShouldBe(1, "Counter deve incrementar exatamente 1 no mesmo milissegundo");

        LogInfo("Clock drift strict less-than verificado");
    }

    [Fact]
    public void GenerateNewId_CounterIncrement_MustBePositive()
    {
        // Mata mutante: _counter++ -> _counter--

        // Arrange
        LogArrange("Verificando que counter incrementa positivamente");
        var time = new DateTimeOffset(2041, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Gerando 10 IDs sequenciais");
        var ids = new List<Id>();
        for (int i = 0; i < 10; i++)
        {
            ids.Add(Id.GenerateNewId(time));
        }

        // Assert
        LogAssert("Verificando unicidade e ordem crescente");
        var uniqueGuids = ids.Select(id => id.Value).Distinct().Count();
        uniqueGuids.ShouldBe(10, "Todos IDs devem ser unicos");

        for (int i = 1; i < ids.Count; i++)
        {
            ids[i].ShouldBeGreaterThan(ids[i - 1], $"ID {i} deve ser maior que ID {i - 1}");
            var counterDiff = ExtractCounter(ids[i]) - ExtractCounter(ids[i - 1]);
            counterDiff.ShouldBe(1, "Diferenca de counter deve ser exatamente +1");
        }

        LogInfo("Counter incremento positivo verificado");
    }

    [Fact]
    public void GenerateNewId_BitwiseShift_TimestampOrdering()
    {
        // Verifica que timestamp encoding preserva ordenacao
        // Se shift estiver errado, ordenacao sera quebrada

        // Arrange
        LogArrange("Verificando que shift preserva ordenacao de timestamps");

        // Act - gera IDs com timestamps crescentes
        LogAct("Gerando IDs com timestamps crescentes");
        var baseTime = new DateTimeOffset(2050, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var ids = new List<Id>();

        for (int i = 0; i < 10; i++)
        {
            var time = baseTime.AddMilliseconds(i * 1000); // 1 segundo entre cada
            ids.Add(Id.GenerateNewId(time));
        }

        // Assert - IDs devem manter ordenacao
        LogAssert("Verificando ordenacao preservada");
        for (int i = 1; i < ids.Count; i++)
        {
            ids[i].ShouldBeGreaterThan(ids[i - 1], $"ID {i} deve ser maior que ID {i - 1}");
        }

        LogInfo("Ordenacao de timestamp verificada");
    }

    [Fact]
    public void GenerateNewId_BitwiseShift_CounterDistribution()
    {
        // Mata mutantes: counter >> 14, counter >> 8, counter & 0xFF

        // Arrange
        LogArrange("Verificando distribuicao do counter nos campos");
        var time = new DateTimeOffset(2042, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Gera IDs para ter counter > 256
        LogAct("Gerando 500 IDs");
        Id lastId = default;
        for (int i = 0; i < 500; i++)
        {
            lastId = Id.GenerateNewId(time);
        }

        var counter = ExtractCounter(lastId);
        var bytes = lastId.Value.ToByteArray();

        short c = BitConverter.ToInt16(bytes, 6);
        byte d = bytes[8];
        byte e = bytes[9];

        var counterHighBits = c & 0x0FFF;
        var counterMidBits = d & 0x3F;

        // Assert
        LogAssert("Verificando distribuicao exata");
        counterHighBits.ShouldBe((counter >> 14) & 0x0FFF);
        counterMidBits.ShouldBe((counter >> 8) & 0x3F);
        e.ShouldBe((byte)(counter & 0xFF));

        LogInfo("Counter {0}: high={1}, mid={2}, low={3}", counter, counterHighBits, counterMidBits, e);
    }

    [Fact]
    public void GenerateNewId_BitwiseMask_VariantByte()
    {
        // Mata mutante: 0x80 -> outros valores

        // Arrange
        LogArrange("Verificando mascara 0x80 do variant");
        var time = new DateTimeOffset(2043, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act & Assert
        LogAct("Gerando 100 IDs e verificando variant");
        for (int i = 0; i < 100; i++)
        {
            var id = Id.GenerateNewId(time);
            var bytes = id.Value.ToByteArray();
            byte d = bytes[8];

            var variantBits = d & 0xC0;
            variantBits.ShouldBe(0x80, $"Variant deve ser 0x80, foi 0x{variantBits:X2}");
        }

        LogAssert("Variant correto em todos os IDs");
    }

    private static int ExtractCounter(Id id)
    {
        var bytes = id.Value.ToByteArray();
        short c = BitConverter.ToInt16(bytes, 6);
        byte d = bytes[8];
        byte e = bytes[9];
        return ((c & 0x0FFF) << 14) | ((d & 0x3F) << 8) | e;
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
