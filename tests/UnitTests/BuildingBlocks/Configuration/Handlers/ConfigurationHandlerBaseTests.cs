using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Configuration.Handlers.Enums;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration.Handlers;

#region Test Handlers

public sealed class PassthroughHandler : ConfigurationHandlerBase
{
    public PassthroughHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue) => currentValue;
    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class StartupOnlyHandler : ConfigurationHandlerBase
{
    public StartupOnlyHandler() : base(LoadStrategy.StartupOnly) { }

    public override object? HandleGet(string key, object? currentValue) => currentValue;
    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class LazyHandler : ConfigurationHandlerBase
{
    public LazyHandler() : base(LoadStrategy.LazyStartupOnly) { }

    public override object? HandleGet(string key, object? currentValue) => currentValue;
    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class TransformHandler : ConfigurationHandlerBase
{
    public TransformHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue)
    {
        if (currentValue is string str)
        {
            return str.ToUpperInvariant();
        }

        return currentValue;
    }

    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class ReplaceHandler : ConfigurationHandlerBase
{
    public ReplaceHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue) => "REPLACED";
    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class KeyAwareHandler : ConfigurationHandlerBase
{
    public KeyAwareHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue)
    {
        if (key.EndsWith(":ConnectionString", StringComparison.Ordinal))
        {
            return "vault-resolved-value";
        }

        return currentValue;
    }

    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class FailingHandler : ConfigurationHandlerBase
{
    public FailingHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue)
        => throw new InvalidOperationException("Handler falhou intencionalmente");

    public override object? HandleSet(string key, object? currentValue)
        => throw new InvalidOperationException("Handler falhou intencionalmente");
}

#endregion

public sealed class ConfigurationHandlerBaseTests : TestBase
{
    public ConfigurationHandlerBaseTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Constructor_WithLoadStrategy_ShouldSetProperty()
    {
        // Arrange & Act
        LogArrange("Criando handler com LoadStrategy.StartupOnly");
        LogAct("Verificando propriedade LoadStrategy");
        var handler = new StartupOnlyHandler();

        // Assert
        LogAssert("Verificando que LoadStrategy foi definido");
        handler.LoadStrategy.ShouldBe(LoadStrategy.StartupOnly);
    }

    [Fact]
    public void Constructor_WithAllTimeStrategy_ShouldSetProperty()
    {
        // Arrange & Act
        LogArrange("Criando handler com LoadStrategy.AllTime");
        LogAct("Verificando propriedade LoadStrategy");
        var handler = new PassthroughHandler();

        // Assert
        LogAssert("Verificando que LoadStrategy AllTime foi definido");
        handler.LoadStrategy.ShouldBe(LoadStrategy.AllTime);
    }

    [Fact]
    public void Constructor_WithLazyStartupOnlyStrategy_ShouldSetProperty()
    {
        // Arrange & Act
        LogArrange("Criando handler com LoadStrategy.LazyStartupOnly");
        LogAct("Verificando propriedade LoadStrategy");
        var handler = new LazyHandler();

        // Assert
        LogAssert("Verificando que LoadStrategy LazyStartupOnly foi definido");
        handler.LoadStrategy.ShouldBe(LoadStrategy.LazyStartupOnly);
    }

    [Fact]
    public void HandleGet_Passthrough_ShouldReturnSameValue()
    {
        // Arrange
        LogArrange("Criando handler passthrough");
        var handler = new PassthroughHandler();

        // Act
        LogAct("Chamando HandleGet com valor");
        var result = handler.HandleGet("key", "value");

        // Assert
        LogAssert("Verificando que valor foi repassado");
        result.ShouldBe("value");
    }

    [Fact]
    public void HandleGet_Transform_ShouldTransformValue()
    {
        // Arrange
        LogArrange("Criando handler de transformacao");
        var handler = new TransformHandler();

        // Act
        LogAct("Chamando HandleGet para transformar string para maiuscula");
        var result = handler.HandleGet("key", "hello");

        // Assert
        LogAssert("Verificando que valor foi transformado");
        result.ShouldBe("HELLO");
    }

    [Fact]
    public void HandleGet_Replace_ShouldIgnoreCurrentValueAndReturnNew()
    {
        // Arrange
        LogArrange("Criando handler que substitui valor (RF-013)");
        var handler = new ReplaceHandler();

        // Act
        LogAct("Chamando HandleGet — handler ignora valor atual");
        var result = handler.HandleGet("key", "original");

        // Assert
        LogAssert("Verificando que valor foi substituido");
        result.ShouldBe("REPLACED");
    }

    [Fact]
    public void HandleGet_KeyAware_ShouldDecideBasedOnKey()
    {
        // Arrange
        LogArrange("Criando handler que decide com base na chave");
        var handler = new KeyAwareHandler();

        // Act
        LogAct("Chamando HandleGet com chave que corresponde ao padrao");
        var resultMatch = handler.HandleGet("Persistence:PostgreSql:ConnectionString", "original");
        var resultNoMatch = handler.HandleGet("Persistence:PostgreSql:Port", "5432");

        // Assert
        LogAssert("Verificando que handler toma decisao com base na chave");
        resultMatch.ShouldBe("vault-resolved-value");
        resultNoMatch.ShouldBe("5432");
    }

    [Fact]
    public void HandleGet_WithNullValue_ShouldHandleGracefully()
    {
        // Arrange
        LogArrange("Criando handler passthrough");
        var handler = new PassthroughHandler();

        // Act
        LogAct("Chamando HandleGet com valor null");
        var result = handler.HandleGet("key", null);

        // Assert
        LogAssert("Verificando que null e repassado");
        result.ShouldBeNull();
    }

    [Fact]
    public void HandleSet_Passthrough_ShouldReturnSameValue()
    {
        // Arrange
        LogArrange("Criando handler passthrough");
        var handler = new PassthroughHandler();

        // Act
        LogAct("Chamando HandleSet com valor");
        var result = handler.HandleSet("key", "value");

        // Assert
        LogAssert("Verificando que valor foi repassado no Set");
        result.ShouldBe("value");
    }
}
