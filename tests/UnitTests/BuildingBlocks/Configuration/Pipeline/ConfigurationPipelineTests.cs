using System.Collections.Concurrent;
using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Configuration.Handlers.Enums;
using Bedrock.BuildingBlocks.Configuration.Pipeline;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration.Pipeline;

#region Test Handlers for Pipeline

public sealed class CountingHandler : ConfigurationHandlerBase
{
    public int GetCallCount { get; private set; }
    public int SetCallCount { get; private set; }

    public CountingHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue)
    {
        GetCallCount++;
        return currentValue;
    }

    public override object? HandleSet(string key, object? currentValue)
    {
        SetCallCount++;
        return currentValue;
    }
}

public sealed class AppendHandler : ConfigurationHandlerBase
{
    private readonly string _suffix;

    public AppendHandler(string suffix) : base(LoadStrategy.AllTime)
    {
        _suffix = suffix;
    }

    public override object? HandleGet(string key, object? currentValue)
    {
        if (currentValue is string str)
        {
            return str + _suffix;
        }

        return currentValue;
    }

    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class PipelineFailingHandler : ConfigurationHandlerBase
{
    public PipelineFailingHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue)
        => throw new InvalidOperationException("Erro no handler");

    public override object? HandleSet(string key, object? currentValue)
        => throw new InvalidOperationException("Erro no handler Set");
}

public sealed class StartupOnlyCountingHandler : ConfigurationHandlerBase
{
    private readonly string _returnValue;
    public int ExecutionCount { get; private set; }

    public StartupOnlyCountingHandler(string returnValue) : base(LoadStrategy.StartupOnly)
    {
        _returnValue = returnValue;
    }

    public override object? HandleGet(string key, object? currentValue)
    {
        ExecutionCount++;
        return _returnValue;
    }

    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class LazyCountingHandler : ConfigurationHandlerBase
{
    private readonly string _returnValue;
    public int ExecutionCount { get; private set; }

    public LazyCountingHandler(string returnValue) : base(LoadStrategy.LazyStartupOnly)
    {
        _returnValue = returnValue;
    }

    public override object? HandleGet(string key, object? currentValue)
    {
        ExecutionCount++;
        return _returnValue;
    }

    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

public sealed class FailOnceHandler : ConfigurationHandlerBase
{
    private bool _hasFailed;

    public FailOnceHandler(LoadStrategy strategy) : base(strategy) { }

    public override object? HandleGet(string key, object? currentValue)
    {
        if (!_hasFailed)
        {
            _hasFailed = true;
            throw new InvalidOperationException("Falha intencional na primeira execucao");
        }

        return "success-after-retry";
    }

    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

#endregion

public sealed class ConfigurationPipelineTests : TestBase
{
    public ConfigurationPipelineTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ExecuteGet_EmptyPipeline_ShouldReturnInitialValue()
    {
        // Arrange
        LogArrange("Criando pipeline vazio");
        var pipeline = new ConfigurationPipeline([]);

        // Act
        LogAct("Executando Get em pipeline vazio");
        var result = pipeline.ExecuteGet("key", "initial");

        // Assert
        LogAssert("Verificando que valor inicial e retornado");
        result.ShouldBe("initial");
    }

    [Fact]
    public void ExecuteGet_SingleHandler_ShouldExecuteHandler()
    {
        // Arrange
        LogArrange("Criando pipeline com handler de append");
        var handler = new AppendHandler("_modified");
        var entries = new List<PipelineEntry>
        {
            new(handler, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get");
        var result = pipeline.ExecuteGet("key", "value");

        // Assert
        LogAssert("Verificando que handler transformou o valor");
        result.ShouldBe("value_modified");
    }

    [Fact]
    public void ExecuteGet_MultipleHandlers_ShouldChainInOrder()
    {
        // Arrange
        LogArrange("Criando pipeline com multiplos handlers em ordem");
        var entries = new List<PipelineEntry>
        {
            new(new AppendHandler("_A"), HandlerScope.Global(), 1),
            new(new AppendHandler("_B"), HandlerScope.Global(), 2),
            new(new AppendHandler("_C"), HandlerScope.Global(), 3)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get — handlers devem encadear");
        var result = pipeline.ExecuteGet("key", "start");

        // Assert
        LogAssert("Verificando que handlers executaram em ordem (P2-2)");
        result.ShouldBe("start_A_B_C");
    }

    [Fact]
    public void ExecuteGet_HandlersOutOfOrder_ShouldSortByPosition()
    {
        // Arrange
        LogArrange("Criando pipeline com handlers fora de ordem");
        var entries = new List<PipelineEntry>
        {
            new(new AppendHandler("_C"), HandlerScope.Global(), 3),
            new(new AppendHandler("_A"), HandlerScope.Global(), 1),
            new(new AppendHandler("_B"), HandlerScope.Global(), 2)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get — pipeline deve ordenar por posicao");
        var result = pipeline.ExecuteGet("key", "start");

        // Assert
        LogAssert("Verificando ordenacao por posicao");
        result.ShouldBe("start_A_B_C");
    }

    [Fact]
    public void ExecuteGet_GlobalScope_ShouldMatchAllKeys()
    {
        // Arrange
        LogArrange("Criando pipeline com handler global");
        var counter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(counter, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get com diferentes chaves (P2-7)");
        pipeline.ExecuteGet("Persistence:PostgreSql:ConnectionString", "val");
        pipeline.ExecuteGet("Security:Jwt:Secret", "val");
        pipeline.ExecuteGet("AnyKey", "val");

        // Assert
        LogAssert("Verificando que handler global executou para todas as chaves");
        counter.GetCallCount.ShouldBe(3);
    }

    [Fact]
    public void ExecuteGet_ClassScope_ShouldMatchOnlySectionKeys()
    {
        // Arrange
        LogArrange("Criando pipeline com handler de escopo de classe (P2-6)");
        var counter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(counter, HandlerScope.ForClass("Persistence:PostgreSql"), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get com chaves dentro e fora da secao");
        pipeline.ExecuteGet("Persistence:PostgreSql:ConnectionString", "val");
        pipeline.ExecuteGet("Persistence:PostgreSql:Port", "val");
        pipeline.ExecuteGet("Security:Jwt:Secret", "val");

        // Assert
        LogAssert("Verificando que handler executou apenas para chaves da secao");
        counter.GetCallCount.ShouldBe(2);
    }

    [Fact]
    public void ExecuteGet_PropertyScope_ShouldMatchOnlyExactKey()
    {
        // Arrange
        LogArrange("Criando pipeline com handler de escopo de propriedade (P2-5)");
        var counter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(counter, HandlerScope.ForProperty("Persistence:PostgreSql:ConnectionString"), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get com chave exata e chaves diferentes");
        pipeline.ExecuteGet("Persistence:PostgreSql:ConnectionString", "val");
        pipeline.ExecuteGet("Persistence:PostgreSql:Port", "val");
        pipeline.ExecuteGet("Security:Jwt:Secret", "val");

        // Assert
        LogAssert("Verificando que handler executou apenas para chave exata");
        counter.GetCallCount.ShouldBe(1);
    }

    [Fact]
    public void ExecuteGet_HandlerNotMatchingScope_ShouldBeSkipped()
    {
        // Arrange
        LogArrange("Criando pipeline com handler que nao corresponde ao escopo");
        var counter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(counter, HandlerScope.ForProperty("Security:Jwt:Secret"), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get com chave que nao corresponde");
        var result = pipeline.ExecuteGet("Persistence:PostgreSql:ConnectionString", "original");

        // Assert
        LogAssert("Verificando que handler foi pulado e valor original retornado");
        counter.GetCallCount.ShouldBe(0);
        result.ShouldBe("original");
    }

    [Fact]
    public void ExecuteGet_MixedScopes_ShouldExecuteAllMatching()
    {
        // Arrange
        LogArrange("Criando pipeline com escopos global, classe e propriedade para mesma chave");
        var globalCounter = new CountingHandler();
        var classCounter = new CountingHandler();
        var propertyCounter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(globalCounter, HandlerScope.Global(), 1),
            new(classCounter, HandlerScope.ForClass("Persistence:PostgreSql"), 2),
            new(propertyCounter, HandlerScope.ForProperty("Persistence:PostgreSql:ConnectionString"), 3)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get — todos os escopos correspondem a chave");
        pipeline.ExecuteGet("Persistence:PostgreSql:ConnectionString", "val");

        // Assert
        LogAssert("Verificando que todos os handlers aplicaveis executaram");
        globalCounter.GetCallCount.ShouldBe(1);
        classCounter.GetCallCount.ShouldBe(1);
        propertyCounter.GetCallCount.ShouldBe(1);
    }

    [Fact]
    public void ExecuteGet_HandlerThrowsException_ShouldPropagateWithContext()
    {
        // Arrange
        LogArrange("Criando pipeline com handler que falha");
        var entries = new List<PipelineEntry>
        {
            new(new PipelineFailingHandler(), HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act & Assert
        LogAct("Executando Get — handler deve falhar");
        LogAssert("Verificando que excecao propaga com contexto do handler");
        var ex = Should.Throw<InvalidOperationException>(() =>
            pipeline.ExecuteGet("key", "value"));
        ex.Message.ShouldContain("PipelineFailingHandler");
        ex.Message.ShouldContain("posicao 1");
        ex.InnerException.ShouldNotBeNull();
    }

    [Fact]
    public void ExecuteGet_ReplaceHandler_ShouldIgnoreInputAndProvideNewValue()
    {
        // Arrange
        LogArrange("Criando pipeline com handler que substitui valor (RF-013)");
        var replaceHandler = new Bedrock.UnitTests.BuildingBlocks.Configuration.Handlers.ReplaceHandler();
        var entries = new List<PipelineEntry>
        {
            new(replaceHandler, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get — handler ignora valor atual");
        var result = pipeline.ExecuteGet("key", "original-value");

        // Assert
        LogAssert("Verificando que handler substituiu o valor");
        result.ShouldBe("REPLACED");
    }

    [Fact]
    public void ExecuteSet_EmptyPipeline_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Criando pipeline vazio para Set");
        var pipeline = new ConfigurationPipeline([]);

        // Act
        LogAct("Executando Set em pipeline vazio");
        var result = pipeline.ExecuteSet("key", "value");

        // Assert
        LogAssert("Verificando que valor e retornado sem alteracao");
        result.ShouldBe("value");
    }

    [Fact]
    public void ExecuteSet_WithHandler_ShouldExecuteHandler()
    {
        // Arrange
        LogArrange("Criando pipeline com handler de Set");
        var counter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(counter, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Set");
        pipeline.ExecuteSet("key", "value");

        // Assert
        LogAssert("Verificando que handler de Set executou");
        counter.SetCallCount.ShouldBe(1);
    }

    [Fact]
    public void ExecuteSet_HandlerThrowsException_ShouldPropagateWithContext()
    {
        // Arrange
        LogArrange("Criando pipeline com handler de Set que falha");
        var entries = new List<PipelineEntry>
        {
            new(new PipelineFailingHandler(), HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act & Assert
        LogAct("Executando Set — handler deve falhar");
        LogAssert("Verificando que excecao propaga com contexto");
        var ex = Should.Throw<InvalidOperationException>(() =>
            pipeline.ExecuteSet("key", "value"));
        ex.Message.ShouldContain("PipelineFailingHandler");
        ex.Message.ShouldContain("Set");
    }

    [Fact]
    public void ExecuteSet_WithScopedHandler_NonMatchingKey_ShouldSkipHandler()
    {
        // Arrange
        LogArrange("Criando pipeline com handler scoped que nao corresponde a chave");
        var counter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(counter, HandlerScope.ForProperty("Other:Section:Property"), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Set com chave diferente do escopo");
        var result = pipeline.ExecuteSet("Persistence:PostgreSql:ConnectionString", "value");

        // Assert
        LogAssert("Verificando que handler nao executou (scope skip)");
        counter.SetCallCount.ShouldBe(0);
        result.ShouldBe("value");
    }

    #region T036: LoadStrategy tests

    [Fact]
    public void ExecuteGet_StartupOnly_ShouldReturnCachedValueAfterInit()
    {
        // Arrange
        LogArrange("Criando pipeline com handler StartupOnly (P3-1)");
        var handler = new StartupOnlyCountingHandler("startup-value");
        var entries = new List<PipelineEntry>
        {
            new(handler, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Inicializando handlers e executando Get duas vezes");
        pipeline.InitializeStartupHandlers(["key"]);
        var result1 = pipeline.ExecuteGet("key", "initial");
        var result2 = pipeline.ExecuteGet("key", "initial");

        // Assert
        LogAssert("Verificando que handler executou apenas uma vez (no init) e cache foi usado");
        result1.ShouldBe("startup-value");
        result2.ShouldBe("startup-value");
        handler.ExecutionCount.ShouldBe(1);
    }

    [Fact]
    public void ExecuteGet_StartupOnly_InitExceptionShouldPropagate()
    {
        // Arrange
        LogArrange("Criando pipeline com handler StartupOnly que falha no init (P3-5)");
        var handler = new FailOnceHandler(LoadStrategy.StartupOnly);
        var entries = new List<PipelineEntry>
        {
            new(handler, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act & Assert
        LogAct("Inicializando handlers — deve falhar (fail-fast)");
        LogAssert("Verificando que excecao propaga sem ser capturada");
        Should.Throw<InvalidOperationException>(() =>
            pipeline.InitializeStartupHandlers(["key"]));
    }

    [Fact]
    public void ExecuteGet_StartupOnly_NewKeyAfterInit_ShouldExecuteAndCache()
    {
        // Arrange
        LogArrange("Criando pipeline com handler StartupOnly e chave nao pre-inicializada");
        var handler = new StartupOnlyCountingHandler("cached");
        var entries = new List<PipelineEntry>
        {
            new(handler, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Inicializando com chave conhecida, depois executando com chave nova");
        pipeline.InitializeStartupHandlers(["known-key"]);
        var result1 = pipeline.ExecuteGet("new-key", "initial");
        var result2 = pipeline.ExecuteGet("new-key", "initial");

        // Assert
        LogAssert("Verificando que chave nova executou uma vez e foi cacheada");
        result1.ShouldBe("cached");
        result2.ShouldBe("cached");
        handler.ExecutionCount.ShouldBe(2); // 1 no init (known-key) + 1 no primeiro Get (new-key)
    }

    [Fact]
    public void ExecuteGet_LazyStartupOnly_ShouldCacheAfterFirstAccess()
    {
        // Arrange
        LogArrange("Criando pipeline com handler LazyStartupOnly (P3-2)");
        var handler = new LazyCountingHandler("lazy-value");
        var entries = new List<PipelineEntry>
        {
            new(handler, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get duas vezes — handler deve executar apenas uma vez");
        var result1 = pipeline.ExecuteGet("key", "initial");
        var result2 = pipeline.ExecuteGet("key", "initial");

        // Assert
        LogAssert("Verificando que handler executou uma vez e cache foi usado");
        result1.ShouldBe("lazy-value");
        result2.ShouldBe("lazy-value");
        handler.ExecutionCount.ShouldBe(1);
    }

    [Fact]
    public void ExecuteGet_LazyStartupOnly_FailureNotCached_RetryWorks()
    {
        // Arrange
        LogArrange("Criando pipeline com handler LazyStartupOnly que falha na primeira vez (edge case 4)");
        var handler = new FailOnceHandler(LoadStrategy.LazyStartupOnly);
        var entries = new List<PipelineEntry>
        {
            new(handler, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get — primeira chamada falha, segunda deve funcionar");
        var firstEx = Should.Throw<InvalidOperationException>(() =>
            pipeline.ExecuteGet("key", "initial"));

        var result = pipeline.ExecuteGet("key", "initial");

        // Assert
        LogAssert("Verificando que falha nao foi cacheada e retry funcionou");
        firstEx.ShouldNotBeNull();
        result.ShouldBe("success-after-retry");
    }

    [Fact]
    public void ExecuteGet_AllTime_ShouldExecuteOnEveryCall()
    {
        // Arrange
        LogArrange("Criando pipeline com handler AllTime (P3-3)");
        var counter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(counter, HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Executando Get tres vezes");
        pipeline.ExecuteGet("key", "val");
        pipeline.ExecuteGet("key", "val");
        pipeline.ExecuteGet("key", "val");

        // Assert
        LogAssert("Verificando que handler executou todas as tres vezes");
        counter.GetCallCount.ShouldBe(3);
    }

    [Fact]
    public void ExecuteGet_MixedStrategies_ShouldRespectEachHandlerStrategy()
    {
        // Arrange
        LogArrange("Criando pipeline com handlers de diferentes estrategias (P3-4)");
        var startupHandler = new StartupOnlyCountingHandler("startup");
        var lazyHandler = new LazyCountingHandler("lazy");
        var allTimeCounter = new CountingHandler();
        var entries = new List<PipelineEntry>
        {
            new(startupHandler, HandlerScope.Global(), 1),
            new(lazyHandler, HandlerScope.Global(), 2),
            new(allTimeCounter, HandlerScope.Global(), 3)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Inicializando e executando Get duas vezes");
        pipeline.InitializeStartupHandlers(["key"]);
        pipeline.ExecuteGet("key", "val");
        pipeline.ExecuteGet("key", "val");

        // Assert
        LogAssert("Verificando que cada handler respeita sua estrategia");
        startupHandler.ExecutionCount.ShouldBe(1); // Apenas no init
        lazyHandler.ExecutionCount.ShouldBe(1); // Apenas no primeiro Get
        allTimeCounter.GetCallCount.ShouldBe(2); // Todas as vezes
    }

    [Fact]
    public void InitializeStartupHandlers_ShouldRespectScope()
    {
        // Arrange
        LogArrange("Criando pipeline com handler StartupOnly escopado para classe");
        var handler = new StartupOnlyCountingHandler("scoped");
        var entries = new List<PipelineEntry>
        {
            new(handler, HandlerScope.ForClass("Persistence:PostgreSql"), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Act
        LogAct("Inicializando com chaves dentro e fora do escopo");
        pipeline.InitializeStartupHandlers([
            "Persistence:PostgreSql:ConnectionString",
            "Persistence:PostgreSql:Port",
            "Security:Jwt:Secret"
        ]);

        // Assert
        LogAssert("Verificando que handler executou apenas para chaves do escopo");
        handler.ExecutionCount.ShouldBe(2); // Apenas PostgreSql keys
    }

    #endregion

    [Fact]
    public void HasEntries_EmptyPipeline_ShouldBeFalse()
    {
        // Arrange & Act
        LogArrange("Criando pipeline vazio");
        LogAct("Verificando HasEntries");
        var pipeline = new ConfigurationPipeline([]);

        // Assert
        LogAssert("Verificando que HasEntries e false");
        pipeline.HasEntries.ShouldBeFalse();
    }

    [Fact]
    public void HasEntries_WithEntries_ShouldBeTrue()
    {
        // Arrange & Act
        LogArrange("Criando pipeline com entradas");
        LogAct("Verificando HasEntries");
        var entries = new List<PipelineEntry>
        {
            new(new CountingHandler(), HandlerScope.Global(), 1)
        };
        var pipeline = new ConfigurationPipeline(entries);

        // Assert
        LogAssert("Verificando que HasEntries e true");
        pipeline.HasEntries.ShouldBeTrue();
    }
}
