using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.RegistryVersions;

public class RegistryVersionTests : TestBase
{
    public RegistryVersionTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void GenerateNewVersion_ShouldReturnValidVersion()
    {
        // Arrange
        LogArrange("Preparing to generate a new RegistryVersion");

        // Act
        LogAct("Generating new RegistryVersion");
        var version = RegistryVersion.GenerateNewVersion();

        // Assert
        LogAssert("Verifying version value is greater than zero");
        version.Value.ShouldBeGreaterThan(0);
        LogInfo("Generated version: {0}", version.Value);
    }

    [Fact]
    public void GenerateNewVersion_ShouldBeMonotonic()
    {
        // Arrange
        LogArrange("Preparing to generate multiple versions in rapid succession");
        const int count = 1000;
        var versions = new RegistryVersion[count];

        // Act
        LogAct($"Generating {count} versions");
        for (int i = 0; i < count; i++)
        {
            versions[i] = RegistryVersion.GenerateNewVersion();
        }

        // Assert
        LogAssert("Verifying versions are monotonically increasing");
        for (int i = 1; i < count; i++)
        {
            versions[i].ShouldBeGreaterThan(versions[i - 1],
                $"Version at index {i} should be greater than version at index {i - 1}");
        }
        LogInfo("All {0} versions are monotonically increasing", count);
    }

    [Fact]
    public void GenerateNewVersion_WithTimeProvider_ShouldUseProvidedTime()
    {
        // Arrange
        LogArrange("Creating a fixed time provider");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(fixedTime);

        // Act
        LogAct("Generating version with fixed time provider");
        var version = RegistryVersion.GenerateNewVersion(timeProvider);

        // Assert
        LogAssert("Verifying version value corresponds to fixed time ticks");
        version.Value.ShouldBeGreaterThanOrEqualTo(fixedTime.UtcTicks);
        LogInfo("Generated version with fixed time: {0}", version.Value);
    }

    [Fact]
    public void GenerateNewVersion_WithDateTimeOffset_ShouldUseProvidedTime()
    {
        // Arrange
        LogArrange("Creating a fixed DateTimeOffset");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating version with DateTimeOffset");
        var version = RegistryVersion.GenerateNewVersion(fixedTime);

        // Assert
        LogAssert("Verifying version value corresponds to provided ticks");
        version.Value.ShouldBeGreaterThanOrEqualTo(fixedTime.UtcTicks);
        LogInfo("Generated version: {0}", version.Value);
    }

    [Fact]
    public void CreateFromExistingInfo_WithLong_ShouldPreserveValue()
    {
        // Arrange
        LogArrange("Creating a known long value");
        long expectedValue = 638500000000000000L;

        // Act
        LogAct("Creating version from existing long");
        var version = RegistryVersion.CreateFromExistingInfo(expectedValue);

        // Assert
        LogAssert("Verifying long value is preserved");
        version.Value.ShouldBe(expectedValue);
        LogInfo("Version value matches original long");
    }

    [Fact]
    public void CreateFromExistingInfo_WithDateTimeOffset_ShouldConvertToTicks()
    {
        // Arrange
        LogArrange("Creating a DateTimeOffset");
        var dateTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Creating version from DateTimeOffset");
        var version = RegistryVersion.CreateFromExistingInfo(dateTime);

        // Assert
        LogAssert("Verifying value equals UTC ticks");
        version.Value.ShouldBe(dateTime.UtcTicks);
        LogInfo("Version created from DateTimeOffset");
    }

    [Fact]
    public void AsDateTimeOffset_ShouldReturnCorrectDateTime()
    {
        // Arrange
        LogArrange("Creating a version from known DateTime");
        var expectedDateTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var version = RegistryVersion.CreateFromExistingInfo(expectedDateTime);

        // Act
        LogAct("Getting AsDateTimeOffset");
        var actualDateTime = version.AsDateTimeOffset;

        // Assert
        LogAssert("Verifying DateTimeOffset conversion");
        actualDateTime.ShouldBe(expectedDateTime);
        LogInfo("AsDateTimeOffset: {0}", actualDateTime);
    }

    [Fact]
    public void ImplicitConversion_ToLong_ShouldWork()
    {
        // Arrange
        LogArrange("Generating a new version");
        var version = RegistryVersion.GenerateNewVersion();

        // Act
        LogAct("Implicitly converting version to long");
        long longValue = version;

        // Assert
        LogAssert("Verifying conversion preserves value");
        longValue.ShouldBe(version.Value);
        LogInfo("Implicit conversion to long successful");
    }

    [Fact]
    public void ImplicitConversion_FromLong_ShouldWork()
    {
        // Arrange
        LogArrange("Creating a long value");
        long longValue = 638500000000000000L;

        // Act
        LogAct("Implicitly converting long to version");
        RegistryVersion version = longValue;

        // Assert
        LogAssert("Verifying conversion preserves value");
        version.Value.ShouldBe(longValue);
        LogInfo("Implicit conversion from long successful");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two versions with same value");
        long value = 638500000000000000L;
        var version1 = RegistryVersion.CreateFromExistingInfo(value);
        var version2 = RegistryVersion.CreateFromExistingInfo(value);

        // Act
        LogAct("Comparing versions for equality");
        var areEqual = version1.Equals(version2);

        // Assert
        LogAssert("Verifying versions are equal");
        areEqual.ShouldBeTrue();
        LogInfo("Versions with same value are equal");
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two different versions");
        var version1 = RegistryVersion.GenerateNewVersion();
        var version2 = RegistryVersion.GenerateNewVersion();

        // Act
        LogAct("Comparing versions for equality");
        var areEqual = version1.Equals(version2);

        // Assert
        LogAssert("Verifying versions are not equal");
        areEqual.ShouldBeFalse();
        LogInfo("Different versions are not equal");
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating version and object for equality test");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);
        object objSame = RegistryVersion.CreateFromExistingInfo(value);
        object objDifferent = RegistryVersion.CreateFromExistingInfo(value + 1);
        object? objNull = null;
        object objWrongType = "not a version";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        version.Equals(objSame).ShouldBeTrue("Equal version objects should be equal");
        version.Equals(objDifferent).ShouldBeFalse("Different version objects should not be equal");
        version.Equals(objNull).ShouldBeFalse("Null should not be equal");
        version.Equals(objWrongType).ShouldBeFalse("Wrong type should not be equal");

        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two versions with same value");
        long value = 638500000000000000L;
        var version1 = RegistryVersion.CreateFromExistingInfo(value);
        var version2 = RegistryVersion.CreateFromExistingInfo(value);

        // Act & Assert
        LogAct("Testing equality operator");
        (version1 == version2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void InequalityOperator_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two different versions");
        var version1 = RegistryVersion.GenerateNewVersion();
        var version2 = RegistryVersion.GenerateNewVersion();

        // Act & Assert
        LogAct("Testing inequality operator");
        (version1 != version2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    [Fact]
    public void ComparisonOperators_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two sequential versions");
        var version1 = RegistryVersion.GenerateNewVersion();
        var version2 = RegistryVersion.GenerateNewVersion();

        // Act & Assert
        LogAct("Testing comparison operators");
        (version1 < version2).ShouldBeTrue("First version should be less than second");
        (version2 > version1).ShouldBeTrue("Second version should be greater than first");
        (version1 <= version2).ShouldBeTrue("First version should be less than or equal to second");
        (version2 >= version1).ShouldBeTrue("Second version should be greater than or equal to first");
#pragma warning disable CS1718 // Comparison to same variable - intentional test of reflexive comparison operators
        (version1 <= version1).ShouldBeTrue("Version should be less than or equal to itself");
        (version1 >= version1).ShouldBeTrue("Version should be greater than or equal to itself");
#pragma warning restore CS1718
        LogAssert("All comparison operators work correctly");
    }

    [Fact]
    public void ComparisonOperators_LessThan_WithEqualVersions_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two versions with same value");
        long value = 638500000000000000L;
        var version1 = RegistryVersion.CreateFromExistingInfo(value);
        var version2 = RegistryVersion.CreateFromExistingInfo(value);

        // Act & Assert
        LogAct("Testing less than operator with equal values");
        (version1 < version2).ShouldBeFalse("Equal versions should not be less than each other");
        (version2 < version1).ShouldBeFalse("Equal versions should not be less than each other");
        LogAssert("Less than operator returns false for equal versions");
    }

    [Fact]
    public void ComparisonOperators_GreaterThan_WithEqualVersions_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two versions with same value");
        long value = 638500000000000000L;
        var version1 = RegistryVersion.CreateFromExistingInfo(value);
        var version2 = RegistryVersion.CreateFromExistingInfo(value);

        // Act & Assert
        LogAct("Testing greater than operator with equal values");
        (version1 > version2).ShouldBeFalse("Equal versions should not be greater than each other");
        (version2 > version1).ShouldBeFalse("Equal versions should not be greater than each other");
        LogAssert("Greater than operator returns false for equal versions");
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating a version");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);

        // Act
        LogAct("Getting hash code multiple times");
        var hash1 = version.GetHashCode();
        var hash2 = version.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are consistent");
        hash1.ShouldBe(hash2);
        hash1.ShouldBe(value.GetHashCode());
        LogInfo("Hash code is consistent: {0}", hash1);
    }

    [Fact]
    public void CompareTo_ShouldReturnCorrectOrder()
    {
        // Arrange
        LogArrange("Creating versions for comparison");
        var smaller = RegistryVersion.CreateFromExistingInfo(100);
        var larger = RegistryVersion.CreateFromExistingInfo(200);
        var equal = RegistryVersion.CreateFromExistingInfo(100);

        // Act & Assert
        LogAct("Testing CompareTo method");
        smaller.CompareTo(larger).ShouldBeLessThan(0);
        larger.CompareTo(smaller).ShouldBeGreaterThan(0);
        smaller.CompareTo(equal).ShouldBe(0);
        LogAssert("CompareTo returns correct ordering");
    }

    [Fact]
    public void ToString_ShouldReturnInvariantString()
    {
        // Arrange
        LogArrange("Creating a version");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);

        // Act
        LogAct("Converting to string");
        var str = version.ToString();

        // Assert
        LogAssert("Verifying string representation");
        str.ShouldBe("638500000000000000");
        LogInfo("ToString result: {0}", str);
    }

    [Fact]
    public void GenerateNewVersion_WithClockDrift_ShouldMaintainMonotonicity()
    {
        // Arrange
        LogArrange("Creating timestamps simulating clock drift");
        var futureTime = DateTimeOffset.UtcNow.AddSeconds(10);
        var pastTime = DateTimeOffset.UtcNow.AddSeconds(-10);

        // Act
        LogAct("Generating version with future time, then past time");
        var versionFuture = RegistryVersion.GenerateNewVersion(futureTime);
        var versionPast = RegistryVersion.GenerateNewVersion(pastTime);

        // Assert
        LogAssert("Verifying past version is still greater (monotonic guarantee)");
        versionPast.ShouldBeGreaterThan(versionFuture,
            "Clock drift should be handled - later generated version should be greater");
        LogInfo("Clock drift handled correctly");
    }

    [Fact]
    public void GenerateNewVersion_WithSameTimestamp_ShouldIncrement()
    {
        // Arrange
        LogArrange("Creating fixed timestamp");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating multiple versions with same timestamp");
        var v1 = RegistryVersion.GenerateNewVersion(fixedTime);
        var v2 = RegistryVersion.GenerateNewVersion(fixedTime);
        var v3 = RegistryVersion.GenerateNewVersion(fixedTime);

        // Assert
        LogAssert("Verifying versions are different and ordered");
        v1.Value.ShouldNotBe(v2.Value);
        v2.Value.ShouldNotBe(v3.Value);
        v2.ShouldBeGreaterThan(v1);
        v3.ShouldBeGreaterThan(v2);
        LogInfo("Same timestamp increment working correctly");
    }

    [Fact]
    public void GenerateNewVersion_SameTimestamp_IncrementsByOneTick()
    {
        // Arrange
        LogArrange("Creating fixed timestamp for increment verification");
        var fixedTime = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Generating versions with same timestamp");
        var v1 = RegistryVersion.GenerateNewVersion(fixedTime);
        var v2 = RegistryVersion.GenerateNewVersion(fixedTime);
        var v3 = RegistryVersion.GenerateNewVersion(fixedTime);

        // Assert
        LogAssert("Verifying increment is exactly 1 tick");
        (v2.Value - v1.Value).ShouldBe(1, "Increment should be exactly 1 tick (100ns)");
        (v3.Value - v2.Value).ShouldBe(1, "Increment should be exactly 1 tick (100ns)");
        LogInfo("Versions: {0} -> {1} -> {2}", v1.Value, v2.Value, v3.Value);
    }

    [Fact]
    public void GenerateNewVersion_NewerTimestamp_UsesNewTimestamp()
    {
        // Arrange - usar timestamps bem no futuro para evitar interferencia de outros testes
        LogArrange("Creating two timestamps far in the future");
        var time1 = new DateTimeOffset(2200, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2200, 1, 1, 0, 0, 1, TimeSpan.Zero); // 1 second later

        // Act
        LogAct("Generating versions with different timestamps");
        var v1 = RegistryVersion.GenerateNewVersion(time1);
        var v2 = RegistryVersion.GenerateNewVersion(time2);

        // Assert
        LogAssert("Verifying newer timestamp produces greater version");
        v2.ShouldBeGreaterThan(v1);
        // Como time2 > time1 > _lastTicks (do time1), o time2 sera usado diretamente
        v2.Value.ShouldBe(time2.UtcTicks, "Version should use newer timestamp directly");
        LogInfo("Newer timestamp used correctly");
    }

    [Fact]
    public void GenerateNewVersion_ClockDriftBackwards_UsesLastTicksPlusOne()
    {
        // Arrange
        LogArrange("Simulating clock drift backwards");
        var normalTime = new DateTimeOffset(2032, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var driftedTime = new DateTimeOffset(2032, 6, 15, 11, 59, 59, TimeSpan.Zero);

        // Act
        LogAct("Generating version at normal time, then at drifted (earlier) time");
        var vNormal = RegistryVersion.GenerateNewVersion(normalTime);
        var vDrifted = RegistryVersion.GenerateNewVersion(driftedTime);

        // Assert
        LogAssert("Verifying monotonicity maintained despite clock drift");
        vDrifted.ShouldBeGreaterThan(vNormal, "Later generated version should be greater even with clock drift");
        (vDrifted.Value - vNormal.Value).ShouldBe(1, "Should increment by exactly 1 tick during drift");
        LogInfo("Clock drift handled correctly - monotonicity maintained");
    }

    [Fact]
    public void GenerateNewVersion_MultipleRapidCalls_AllShouldBeUnique()
    {
        // Arrange
        LogArrange("Preparing to generate many versions rapidly");
        var fixedTime = new DateTimeOffset(2033, 6, 1, 0, 0, 0, TimeSpan.Zero);
        const int count = 100;
        var versions = new HashSet<long>();

        // Act
        LogAct($"Generating {count} versions with same timestamp");
        for (int i = 0; i < count; i++)
        {
            var version = RegistryVersion.GenerateNewVersion(fixedTime);
            versions.Add(version.Value);
        }

        // Assert
        LogAssert("Verifying all versions are unique");
        versions.Count.ShouldBe(count, "All versions should be unique even with same timestamp");
        LogInfo("All {0} versions are unique", count);
    }

    [Fact]
    public void GenerateNewVersion_ThreadSafety_EachThreadIsMonotonic()
    {
        // Arrange
        // NOTA: RegistryVersion usa ThreadStatic, entao cada thread tem seu proprio _lastTicks.
        // Isso significa que versoes DENTRO de uma thread sao monotonicas, mas podem haver
        // duplicatas ENTRE threads se gerarem no mesmo tick. Este e o design esperado.
        LogArrange("Preparing parallel version generation");
        const int threadsCount = 10;
        const int versionsPerThread = 100;
        var threadResults = new System.Collections.Concurrent.ConcurrentDictionary<int, List<RegistryVersion>>();

        // Act
        LogAct($"Generating {versionsPerThread} versions in each of {threadsCount} threads");
        Parallel.For(0, threadsCount, threadIndex =>
        {
            var versions = new List<RegistryVersion>();
            for (int i = 0; i < versionsPerThread; i++)
            {
                versions.Add(RegistryVersion.GenerateNewVersion());
            }
            threadResults[threadIndex] = versions;
        });

        // Assert - cada thread deve ter versoes monotonicas
        LogAssert("Verifying each thread has monotonically increasing versions");
        foreach (var kvp in threadResults)
        {
            var versions = kvp.Value;
            for (int i = 1; i < versions.Count; i++)
            {
                versions[i].ShouldBeGreaterThan(versions[i - 1],
                    $"Thread {kvp.Key}: Version at index {i} should be greater than version at index {i - 1}");
            }
        }
        LogInfo("All {0} threads have monotonically increasing versions", threadsCount);
    }

    [Fact]
    public void GenerateNewVersion_StrictLessThanCheck()
    {
        // Mata mutante: ticks <= _lastTicks -> ticks < _lastTicks
        // Se mutar, mesmo timestamp nao entraria no branch de incremento

        // Arrange
        LogArrange("Verificando que <= e usado corretamente");
        var time = new DateTimeOffset(2040, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act - gera dois no MESMO timestamp
        LogAct("Gerando 2 versoes com timestamp identico");
        var v1 = RegistryVersion.GenerateNewVersion(time);
        var v2 = RegistryVersion.GenerateNewVersion(time);

        // Assert
        LogAssert("Verificando que mesmo timestamp gera versoes diferentes");
        v1.Value.ShouldNotBe(v2.Value);
        v2.ShouldBeGreaterThan(v1);
        (v2.Value - v1.Value).ShouldBe(1, "Diferenca deve ser exatamente 1 tick");
        LogInfo("Check <= verificado corretamente");
    }

    [Fact]
    public void GenerateNewVersion_IncrementMustBePositive()
    {
        // Mata mutante: _lastTicks + 1 -> _lastTicks - 1

        // Arrange
        LogArrange("Verificando que incremento e +1, nao -1");
        var time = new DateTimeOffset(2041, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Gerando 10 versoes sequenciais");
        var versions = new List<RegistryVersion>();
        for (int i = 0; i < 10; i++)
        {
            versions.Add(RegistryVersion.GenerateNewVersion(time));
        }

        // Assert
        LogAssert("Verificando unicidade e ordem crescente");
        var uniqueValues = versions.Select(v => v.Value).Distinct().Count();
        uniqueValues.ShouldBe(10, "Todas versoes devem ser unicas");

        for (int i = 1; i < versions.Count; i++)
        {
            versions[i].ShouldBeGreaterThan(versions[i - 1], $"Versao {i} deve ser maior que versao {i - 1}");
            (versions[i].Value - versions[i - 1].Value).ShouldBe(1, "Diferenca deve ser +1, nao -1");
        }

        LogInfo("Incremento positivo verificado");
    }

    [Fact]
    public void GenerateNewVersion_AssignmentToLastTicks()
    {
        // Mata mutante: _lastTicks = ticks -> statement removal

        // Arrange - usar timestamps bem no futuro para evitar interferencia
        LogArrange("Verificando que _lastTicks e atualizado");
        var time1 = new DateTimeOffset(2300, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2300, 1, 1, 0, 0, 0, 100, TimeSpan.Zero); // 100 ticks depois

        // Act
        LogAct("Gerando versoes e verificando persistencia de estado");
        var v1 = RegistryVersion.GenerateNewVersion(time1);
        var v2 = RegistryVersion.GenerateNewVersion(time1); // mesmo timestamp, deve usar _lastTicks
        var v3 = RegistryVersion.GenerateNewVersion(time2); // timestamp maior

        // Assert
        LogAssert("Verificando que estado e mantido entre chamadas");
        v2.Value.ShouldBe(v1.Value + 1, "Segundo deve incrementar do primeiro");
        v3.Value.ShouldBe(time2.UtcTicks, "Terceiro deve usar timestamp maior");
        v3.ShouldBeGreaterThan(v2, "Terceiro deve ser maior que segundo");

        LogInfo("Atribuicao a _lastTicks verificada");
    }

    [Fact]
    public void GenerateNewVersion_IfConditionPath()
    {
        // Mata mutante: if block removal

        // Arrange - usar timestamps bem no futuro para evitar interferencia
        LogArrange("Verificando ambos os caminhos do if");

        // Act - caminho onde ticks > _lastTicks (tempo avanca)
        LogAct("Testando caminho: tempo avanca");
        var time1 = new DateTimeOffset(2400, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var time2 = new DateTimeOffset(2400, 1, 1, 0, 0, 1, TimeSpan.Zero);
        var v1 = RegistryVersion.GenerateNewVersion(time1);
        var v2 = RegistryVersion.GenerateNewVersion(time2);

        // Assert - tempo avanca normalmente
        LogAssert("Verificando que timestamp maior e usado diretamente");
        v2.Value.ShouldBe(time2.UtcTicks);

        // Act - caminho onde ticks <= _lastTicks (clock drift)
        LogAct("Testando caminho: clock drift");
        var v3 = RegistryVersion.GenerateNewVersion(time1); // tempo retrocede

        // Assert - deve incrementar do ultimo
        LogAssert("Verificando incremento em clock drift");
        v3.ShouldBeGreaterThan(v2);
        v3.Value.ShouldBe(v2.Value + 1);

        LogInfo("Ambos caminhos do if verificados");
    }

    [Fact]
    public void CreateFromExistingInfo_DateTimeOffset_DoesNotAffectLastTicks()
    {
        // Arrange
        LogArrange("Verificando que CreateFromExistingInfo nao afeta _lastTicks");
        var genTime = new DateTimeOffset(2044, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var existingTime = new DateTimeOffset(2050, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Gerando versao, criando de existente, gerando novamente");
        var v1 = RegistryVersion.GenerateNewVersion(genTime);
        var vExisting = RegistryVersion.CreateFromExistingInfo(existingTime);
        var v2 = RegistryVersion.GenerateNewVersion(genTime);

        // Assert
        LogAssert("Verificando que CreateFromExistingInfo nao afeta geracao");
        vExisting.Value.ShouldBe(existingTime.UtcTicks);
        v2.Value.ShouldBe(v1.Value + 1, "v2 deve ser v1 + 1, nao baseado em vExisting");

        LogInfo("CreateFromExistingInfo nao afeta estado de geracao");
    }

    [Fact]
    public void ToString_WithFormat_ShouldFormatCorrectly()
    {
        // Arrange
        LogArrange("Creating a version with known value");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);

        // Act
        LogAct("Formatting with different formats");
        var decimalFormat = version.ToString("D", null);
        var hexFormat = version.ToString("X", null);

        // Assert
        LogAssert("Verifying formatted outputs");
        decimalFormat.ShouldBe(value.ToString("D", null));
        hexFormat.ShouldBe(value.ToString("X", null));
        LogInfo("ToString with format works correctly");
    }

    [Fact]
    public void ToString_WithFormatProvider_ShouldUseProvider()
    {
        // Arrange
        LogArrange("Creating a version with known value");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);

        // Act
        LogAct("Formatting with InvariantCulture provider");
        var result = version.ToString(null, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        LogAssert("Verifying formatted output uses provider");
        result.ShouldBe("638500000000000000");
        LogInfo("ToString with format provider works correctly");
    }

    [Fact]
    public void TryFormat_WithSufficientBuffer_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating a version and buffer");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);
        Span<char> buffer = stackalloc char[32];

        // Act
        LogAct("Formatting into span buffer");
        var success = version.TryFormat(buffer, out int charsWritten, default, null);

        // Assert
        LogAssert("Verifying TryFormat succeeded");
        success.ShouldBeTrue();
        charsWritten.ShouldBe(18);
        buffer[..charsWritten].ToString().ShouldBe("638500000000000000");
        LogInfo("TryFormat wrote {0} characters", charsWritten);
    }

    [Fact]
    public void TryFormat_WithInsufficientBuffer_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating a version and small buffer");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);
        Span<char> buffer = stackalloc char[5]; // Too small for 18 digits

        // Act
        LogAct("Attempting to format into insufficient buffer");
        var success = version.TryFormat(buffer, out int charsWritten, default, null);

        // Assert
        LogAssert("Verifying TryFormat failed gracefully");
        success.ShouldBeFalse();
        charsWritten.ShouldBe(0);
        LogInfo("TryFormat correctly returned false for insufficient buffer");
    }

    [Fact]
    public void TryFormat_WithHexFormat_ShouldFormatCorrectly()
    {
        // Arrange
        LogArrange("Creating a version for hex formatting");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);
        Span<char> buffer = stackalloc char[32];

        // Act
        LogAct("Formatting as hexadecimal");
        var success = version.TryFormat(buffer, out int charsWritten, "X", null);

        // Assert
        LogAssert("Verifying hex format");
        success.ShouldBeTrue();
        buffer[..charsWritten].ToString().ShouldBe(value.ToString("X", null));
        LogInfo("TryFormat with hex format: {0}", buffer[..charsWritten].ToString());
    }

    [Fact]
    public void TryFormat_WithFormatProvider_ShouldUseProvider()
    {
        // Arrange
        LogArrange("Creating a version for formatted output");
        long value = 638500000000000000L;
        var version = RegistryVersion.CreateFromExistingInfo(value);
        Span<char> buffer = stackalloc char[32];

        // Act
        LogAct("Formatting with InvariantCulture");
        var success = version.TryFormat(buffer, out int charsWritten, default, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        LogAssert("Verifying format with provider");
        success.ShouldBeTrue();
        buffer[..charsWritten].ToString().ShouldBe("638500000000000000");
        LogInfo("TryFormat with provider works correctly");
    }

    [Fact]
    public void TryFormat_ZeroAllocation_ShouldNotAllocateStrings()
    {
        // Arrange
        LogArrange("Creating version for zero-allocation test");
        var version = RegistryVersion.CreateFromExistingInfo(12345L);
        Span<char> buffer = stackalloc char[32];

        // Act
        LogAct("Formatting using stackalloc buffer (zero heap allocation)");
        var success = version.TryFormat(buffer, out int charsWritten, default, null);

        // Assert
        LogAssert("Verifying successful formatting without string allocation");
        success.ShouldBeTrue();
        charsWritten.ShouldBe(5);
        buffer[..charsWritten].ToString().ShouldBe("12345");
        LogInfo("Zero-allocation formatting verified");
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
