using Bedrock.BuildingBlocks.Core.TimeProviders;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.TimeProviders;

public class CustomTimeProviderTests : TestBase
{
    public CustomTimeProviderTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void DefaultInstance_ShouldReturnSameInstance()
    {
        // Arrange
        LogArrange("Getting DefaultInstance twice");

        // Act
        LogAct("Accessing DefaultInstance property");
        var instance1 = CustomTimeProvider.DefaultInstance;
        var instance2 = CustomTimeProvider.DefaultInstance;

        // Assert
        LogAssert("Verifying same instance is returned");
        instance1.ShouldBeSameAs(instance2);
        LogInfo("DefaultInstance is singleton");
    }

    [Fact]
    public void DefaultInstance_ShouldUseUtcTimeZone()
    {
        // Arrange
        LogArrange("Getting DefaultInstance");

        // Act
        LogAct("Checking LocalTimeZone");
        var instance = CustomTimeProvider.DefaultInstance;

        // Assert
        LogAssert("Verifying LocalTimeZone is UTC");
        instance.LocalTimeZone.ShouldBe(TimeZoneInfo.Utc);
        LogInfo("DefaultInstance uses UTC");
    }

    [Fact]
    public void DefaultInstance_GetUtcNow_ShouldReturnCurrentTime()
    {
        // Arrange
        LogArrange("Getting DefaultInstance");
        var instance = CustomTimeProvider.DefaultInstance;
        var before = DateTimeOffset.UtcNow;

        // Act
        LogAct("Calling GetUtcNow");
        var result = instance.GetUtcNow();
        var after = DateTimeOffset.UtcNow;

        // Assert
        LogAssert("Verifying time is within expected range");
        result.ShouldBeGreaterThanOrEqualTo(before);
        result.ShouldBeLessThanOrEqualTo(after);
        LogInfo("GetUtcNow returned: {0}", result);
    }

    [Fact]
    public void Constructor_WithNullTimeZone_ShouldDefaultToUtc()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with null timezone");

        // Act
        LogAct("Instantiating with null localTimeZone");
        var provider = new CustomTimeProvider(utcNowFunc: null, localTimeZone: null);

        // Assert
        LogAssert("Verifying LocalTimeZone defaults to UTC");
        provider.LocalTimeZone.ShouldBe(TimeZoneInfo.Utc);
        LogInfo("Null timezone defaults to UTC");
    }

    [Fact]
    public void Constructor_WithCustomTimeZone_ShouldUseProvided()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with custom timezone");
        var customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        // Act
        LogAct("Instantiating with custom localTimeZone");
        var provider = new CustomTimeProvider(utcNowFunc: null, localTimeZone: customTimeZone);

        // Assert
        LogAssert("Verifying LocalTimeZone is custom value");
        provider.LocalTimeZone.ShouldBe(customTimeZone);
        LogInfo("Custom timezone used: {0}", customTimeZone.Id);
    }

    [Fact]
    public void GetUtcNow_WithNullFunc_ShouldReturnSystemTime()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with null utcNowFunc");
        var provider = new CustomTimeProvider(utcNowFunc: null, localTimeZone: null);
        var before = DateTimeOffset.UtcNow;

        // Act
        LogAct("Calling GetUtcNow");
        var result = provider.GetUtcNow();
        var after = DateTimeOffset.UtcNow;

        // Assert
        LogAssert("Verifying system time is returned");
        result.ShouldBeGreaterThanOrEqualTo(before);
        result.ShouldBeLessThanOrEqualTo(after);
        LogInfo("System time returned: {0}", result);
    }

    [Fact]
    public void GetUtcNow_WithCustomFunc_ShouldReturnFuncResult()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with fixed time function");
        var fixedTime = new DateTimeOffset(2025, 6, 15, 12, 30, 45, TimeSpan.Zero);
        var provider = new CustomTimeProvider(
            utcNowFunc: _ => fixedTime,
            localTimeZone: null);

        // Act
        LogAct("Calling GetUtcNow");
        var result = provider.GetUtcNow();

        // Assert
        LogAssert("Verifying fixed time is returned");
        result.ShouldBe(fixedTime);
        LogInfo("Fixed time returned: {0}", result);
    }

    [Fact]
    public void GetUtcNow_WithCustomFunc_ShouldPassLocalTimeZone()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with function that uses timezone");
        TimeZoneInfo? receivedTimeZone = null;
        var fixedTime = new DateTimeOffset(2025, 6, 15, 12, 30, 45, TimeSpan.Zero);
        var customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        var provider = new CustomTimeProvider(
            utcNowFunc: tz =>
            {
                receivedTimeZone = tz;
                return fixedTime;
            },
            localTimeZone: customTimeZone);

        // Act
        LogAct("Calling GetUtcNow");
        var result = provider.GetUtcNow();

        // Assert
        LogAssert("Verifying LocalTimeZone was passed to function");
        receivedTimeZone.ShouldBe(customTimeZone);
        result.ShouldBe(fixedTime);
        LogInfo("LocalTimeZone passed correctly: {0}", receivedTimeZone?.Id);
    }

    [Fact]
    public void GetUtcNow_CalledMultipleTimes_ShouldCallFuncEachTime()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with counter function");
        var callCount = 0;
        var baseTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var provider = new CustomTimeProvider(
            utcNowFunc: _ =>
            {
                callCount++;
                return baseTime.AddSeconds(callCount);
            },
            localTimeZone: null);

        // Act
        LogAct("Calling GetUtcNow three times");
        var result1 = provider.GetUtcNow();
        var result2 = provider.GetUtcNow();
        var result3 = provider.GetUtcNow();

        // Assert
        LogAssert("Verifying function was called each time");
        callCount.ShouldBe(3);
        result1.ShouldBe(baseTime.AddSeconds(1));
        result2.ShouldBe(baseTime.AddSeconds(2));
        result3.ShouldBe(baseTime.AddSeconds(3));
        LogInfo("Function called {0} times with incrementing results", callCount);
    }

    [Fact]
    public void GetUtcNow_WithCustomFunc_ShouldNotCallSystemTime()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with past time function");
        var pastTime = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var provider = new CustomTimeProvider(
            utcNowFunc: _ => pastTime,
            localTimeZone: null);

        // Act
        LogAct("Calling GetUtcNow");
        var result = provider.GetUtcNow();

        // Assert
        LogAssert("Verifying past time is returned (not system time)");
        result.ShouldBe(pastTime);
        result.ShouldBeLessThan(DateTimeOffset.UtcNow);
        LogInfo("Past time returned correctly: {0}", result);
    }

    [Fact]
    public void LocalTimeZone_ShouldBeReadOnly()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with specific timezone");
        var customTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        var provider = new CustomTimeProvider(utcNowFunc: null, localTimeZone: customTimeZone);

        // Act
        LogAct("Reading LocalTimeZone multiple times");
        var tz1 = provider.LocalTimeZone;
        var tz2 = provider.LocalTimeZone;

        // Assert
        LogAssert("Verifying LocalTimeZone is consistent");
        tz1.ShouldBe(customTimeZone);
        tz2.ShouldBe(customTimeZone);
        tz1.ShouldBeSameAs(tz2);
        LogInfo("LocalTimeZone is consistent: {0}", tz1.Id);
    }

    [Fact]
    public void Constructor_WithUtcTimeZone_ShouldWork()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with explicit UTC");

        // Act
        LogAct("Instantiating with TimeZoneInfo.Utc");
        var provider = new CustomTimeProvider(utcNowFunc: null, localTimeZone: TimeZoneInfo.Utc);

        // Assert
        LogAssert("Verifying UTC is used");
        provider.LocalTimeZone.ShouldBe(TimeZoneInfo.Utc);
        provider.LocalTimeZone.Id.ShouldBe("UTC");
        LogInfo("Explicit UTC works correctly");
    }

    [Fact]
    public void GetUtcNow_FuncReturnsNonUtcOffset_ShouldReturnAsIs()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider with non-UTC offset time");
        var nonUtcTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.FromHours(-3));
        var provider = new CustomTimeProvider(
            utcNowFunc: _ => nonUtcTime,
            localTimeZone: null);

        // Act
        LogAct("Calling GetUtcNow");
        var result = provider.GetUtcNow();

        // Assert
        LogAssert("Verifying non-UTC offset is preserved");
        result.ShouldBe(nonUtcTime);
        result.Offset.ShouldBe(TimeSpan.FromHours(-3));
        LogInfo("Non-UTC offset preserved: {0}", result);
    }

    [Fact]
    public void InheritsFromTimeProvider_ShouldBeAssignable()
    {
        // Arrange
        LogArrange("Creating CustomTimeProvider");

        // Act
        LogAct("Assigning to TimeProvider variable");
        TimeProvider provider = new CustomTimeProvider(utcNowFunc: null, localTimeZone: null);

        // Assert
        LogAssert("Verifying inheritance");
        provider.ShouldBeOfType<CustomTimeProvider>();
        provider.ShouldBeAssignableTo<TimeProvider>();
        LogInfo("CustomTimeProvider inherits from TimeProvider");
    }

    [Fact]
    public void DefaultInstance_GetUtcNow_CalledMultipleTimes_ShouldReturnIncreasingTime()
    {
        // Arrange
        LogArrange("Getting DefaultInstance");
        var instance = CustomTimeProvider.DefaultInstance;

        // Act
        LogAct("Calling GetUtcNow multiple times with delay");
        var time1 = instance.GetUtcNow();
        Thread.Sleep(10);
        var time2 = instance.GetUtcNow();

        // Assert
        LogAssert("Verifying time increases");
        time2.ShouldBeGreaterThan(time1);
        LogInfo("Time increased from {0} to {1}", time1, time2);
    }

    [Fact]
    public void GetUtcNow_IfBranch_FuncNotNull_ShouldReturnFuncResult()
    {
        // Mata mutante: if (_utcNowFunc != null) -> if (false) ou block removal

        // Arrange
        LogArrange("Verificando branch quando utcNowFunc nao e null");
        var specificTime = new DateTimeOffset(2099, 12, 31, 23, 59, 59, TimeSpan.Zero);
        var provider = new CustomTimeProvider(
            utcNowFunc: _ => specificTime,
            localTimeZone: null);

        // Act
        LogAct("Chamando GetUtcNow");
        var result = provider.GetUtcNow();

        // Assert
        LogAssert("Verificando que funcao customizada foi usada");
        result.ShouldBe(specificTime);
        // Se mutante remover o if ou trocar para false, retornaria DateTimeOffset.UtcNow
        // que seria diferente de specificTime (ano 2099)
        result.Year.ShouldBe(2099);
        LogInfo("Branch if funcionando: retornou {0}", result);
    }

    [Fact]
    public void GetUtcNow_ElseBranch_FuncIsNull_ShouldReturnSystemTime()
    {
        // Mata mutante: return DateTimeOffset.UtcNow -> outros valores

        // Arrange
        LogArrange("Verificando branch else quando utcNowFunc e null");
        var provider = new CustomTimeProvider(utcNowFunc: null, localTimeZone: null);
        var before = DateTimeOffset.UtcNow;

        // Act
        LogAct("Chamando GetUtcNow");
        var result = provider.GetUtcNow();
        var after = DateTimeOffset.UtcNow;

        // Assert
        LogAssert("Verificando que tempo do sistema foi usado");
        result.ShouldBeGreaterThanOrEqualTo(before);
        result.ShouldBeLessThanOrEqualTo(after);
        // O resultado deve estar muito proximo do tempo atual
        (after - before).TotalSeconds.ShouldBeLessThan(1);
        LogInfo("Branch else funcionando: retornou tempo do sistema");
    }

    [Fact]
    public void Constructor_LocalTimeZone_NullCoalescing_ShouldWork()
    {
        // Mata mutante: localTimeZone ?? TimeZoneInfo.Utc -> localTimeZone

        // Arrange
        LogArrange("Verificando null coalescing do LocalTimeZone");

        // Act - com null
        LogAct("Criando com localTimeZone null");
        var providerWithNull = new CustomTimeProvider(utcNowFunc: null, localTimeZone: null);

        // Act - com valor
        LogAct("Criando com localTimeZone especifico");
        var specificTz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        var providerWithValue = new CustomTimeProvider(utcNowFunc: null, localTimeZone: specificTz);

        // Assert
        LogAssert("Verificando resultados");
        providerWithNull.LocalTimeZone.ShouldBe(TimeZoneInfo.Utc);
        providerWithNull.LocalTimeZone.ShouldNotBeNull();
        providerWithValue.LocalTimeZone.ShouldBe(specificTz);
        providerWithValue.LocalTimeZone.ShouldNotBe(TimeZoneInfo.Utc);
        LogInfo("Null coalescing funcionando corretamente");
    }

    [Fact]
    public void GetUtcNow_FuncReceivesCorrectLocalTimeZone_NotNull()
    {
        // Mata mutante: _utcNowFunc(LocalTimeZone) -> _utcNowFunc(null)

        // Arrange
        LogArrange("Verificando que LocalTimeZone e passado para funcao");
        TimeZoneInfo? capturedTz = null;
        var customTz = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
        var fixedTime = DateTimeOffset.UtcNow;

        var provider = new CustomTimeProvider(
            utcNowFunc: tz =>
            {
                capturedTz = tz;
                return fixedTime;
            },
            localTimeZone: customTz);

        // Act
        LogAct("Chamando GetUtcNow");
        provider.GetUtcNow();

        // Assert
        LogAssert("Verificando que timezone foi passado");
        capturedTz.ShouldNotBeNull();
        capturedTz.ShouldBe(customTz);
        capturedTz!.Id.ShouldBe("Mountain Standard Time");
        LogInfo("LocalTimeZone passado corretamente: {0}", capturedTz.Id);
    }
}
