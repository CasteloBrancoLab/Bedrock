using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Configuration.Handlers.Enums;
using Bedrock.BuildingBlocks.Configuration.Registration;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration.Registration;

#region Test Handlers for Builder

public sealed class BuilderTestHandler : ConfigurationHandlerBase
{
    public BuilderTestHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue) => currentValue;
    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

#endregion

public sealed class ConfigurationHandlerBuilderTests : TestBase
{
    public ConfigurationHandlerBuilderTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void AddHandler_ShouldReturnBuilder()
    {
        // Arrange
        LogArrange("Criando ConfigurationOptions com mapeamento");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Chamando AddHandler");
        var builder = options.AddHandler<BuilderTestHandler>();

        // Assert
        LogAssert("Verificando que builder foi retornado");
        builder.ShouldNotBeNull();
    }

    [Fact]
    public void AtPosition_ShouldSetPosition()
    {
        // Arrange
        LogArrange("Criando builder para handler");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Definindo posicao via AtPosition");
        options.AddHandler<BuilderTestHandler>().AtPosition(5);

        // Assert
        LogAssert("Verificando que pipeline foi construido (posicao e interna)");
        var (getPipeline, _) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeTrue();
    }

    [Fact]
    public void ForGet_ShouldRegisterOnlyInGetPipeline()
    {
        // Arrange
        LogArrange("Criando builder com ForGet");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando handler apenas no pipeline de Get");
        options.AddHandler<BuilderTestHandler>().AtPosition(1).ForGet();

        // Assert
        LogAssert("Verificando que handler esta apenas no Get pipeline");
        var (getPipeline, setPipeline) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeTrue();
        setPipeline.HasEntries.ShouldBeFalse();
    }

    [Fact]
    public void ForSet_ShouldRegisterOnlyInSetPipeline()
    {
        // Arrange
        LogArrange("Criando builder com ForSet");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando handler apenas no pipeline de Set");
        options.AddHandler<BuilderTestHandler>().AtPosition(1).ForSet();

        // Assert
        LogAssert("Verificando que handler esta apenas no Set pipeline");
        var (getPipeline, setPipeline) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeFalse();
        setPipeline.HasEntries.ShouldBeTrue();
    }

    [Fact]
    public void ForBoth_ShouldRegisterInBothPipelines()
    {
        // Arrange
        LogArrange("Criando builder com ForBoth");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando handler em ambos os pipelines");
        options.AddHandler<BuilderTestHandler>().AtPosition(1).ForBoth();

        // Assert
        LogAssert("Verificando que handler esta em ambos os pipelines");
        var (getPipeline, setPipeline) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeTrue();
        setPipeline.HasEntries.ShouldBeTrue();
    }

    [Fact]
    public void DefaultRegistration_ShouldBeInBothPipelines()
    {
        // Arrange
        LogArrange("Criando handler sem especificar pipeline");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando handler sem ForGet/ForSet/ForBoth");
        options.AddHandler<BuilderTestHandler>().AtPosition(1);

        // Assert
        LogAssert("Verificando que default e ambos os pipelines");
        var (getPipeline, setPipeline) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeTrue();
        setPipeline.HasEntries.ShouldBeTrue();
    }

    [Fact]
    public void ToClass_WithMappedType_ShouldSetClassScope()
    {
        // Arrange
        LogArrange("Criando builder com ToClass (P2-9)");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando handler com escopo de classe");
        options.AddHandler<BuilderTestHandler>().AtPosition(1).ToClass<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que handler tem escopo de classe");
        var (getPipeline, _) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeTrue();
    }

    [Fact]
    public void ToClass_WithUnmappedType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Criando builder com ToClass para tipo nao mapeado");
        var options = new ConfigurationOptions();

        // Act & Assert
        LogAct("Tentando ToClass com tipo sem MapSection");
        LogAssert("Verificando que InvalidOperationException e lancada");
        Should.Throw<InvalidOperationException>(() =>
            options.AddHandler<BuilderTestHandler>().AtPosition(1).ToClass<PostgreSqlConfig>());
    }

    [Fact]
    public void ToClassToProperty_ShouldSetPropertyScope()
    {
        // Arrange
        LogArrange("Criando builder com ToClass().ToProperty() (P2-8)");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando handler com escopo de propriedade via expressao type-safe");
        options.AddHandler<BuilderTestHandler>()
            .AtPosition(1)
            .ToClass<PostgreSqlConfig>()
            .ToProperty(c => c.ConnectionString);

        // Assert
        LogAssert("Verificando que handler tem escopo de propriedade");
        var (getPipeline, _) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeTrue();
    }

    [Fact]
    public void DuplicatePositions_InSamePipeline_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Criando dois handlers na mesma posicao (RF-014)");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando dois handlers na posicao 1");
        options.AddHandler<BuilderTestHandler>().AtPosition(1);
        options.AddHandler<BuilderTestHandler>().AtPosition(1);

        // Assert
        LogAssert("Verificando que BuildPipelines rejeita posicoes duplicadas");
        var ex = Should.Throw<InvalidOperationException>(() => options.BuildPipelines());
        ex.Message.ShouldContain("duplicada");
        ex.Message.ShouldContain("1");
    }

    [Fact]
    public void DuplicatePositions_InDifferentPipelines_ShouldBeAllowed()
    {
        // Arrange
        LogArrange("Criando handlers na mesma posicao em pipelines diferentes");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Registrando handler na posicao 1 no Get e posicao 1 no Set");
        options.AddHandler<BuilderTestHandler>().AtPosition(1).ForGet();
        options.AddHandler<BuilderTestHandler>().AtPosition(1).ForSet();

        // Assert
        LogAssert("Verificando que posicoes iguais em pipelines diferentes sao permitidas");
        var (getPipeline, setPipeline) = options.BuildPipelines();
        getPipeline.HasEntries.ShouldBeTrue();
        setPipeline.HasEntries.ShouldBeTrue();
    }

    [Fact]
    public void FluentChaining_ShouldReturnSameBuilder()
    {
        // Arrange
        LogArrange("Criando builder e encadeando metodos");
        var options = new ConfigurationOptions();
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Act
        LogAct("Encadeando AtPosition, WithLoadStrategy, ForBoth");
        var builder = options.AddHandler<BuilderTestHandler>();
        var result = builder
            .AtPosition(1)
            .WithLoadStrategy(LoadStrategy.StartupOnly)
            .ForBoth();

        // Assert
        LogAssert("Verificando que todos retornam o mesmo builder");
        result.ShouldBeSameAs(builder);
    }
}
