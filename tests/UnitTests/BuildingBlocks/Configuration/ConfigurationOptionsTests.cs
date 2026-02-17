using Bedrock.BuildingBlocks.Configuration;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration;

public sealed class ConfigurationOptionsTests : TestBase
{
    public ConfigurationOptionsTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void MapSection_ShouldStoreSectionMapping()
    {
        // Arrange
        LogArrange("Criando ConfigurationOptions");
        var options = new ConfigurationOptions();

        // Act
        LogAct("Mapeando secao para tipo");
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Assert
        LogAssert("Verificando que mapeamento foi armazenado");
        var mappings = options.GetSectionMappings();
        mappings.ShouldContainKey(typeof(PostgreSqlConfig));
        mappings[typeof(PostgreSqlConfig)].ShouldBe("Persistence:PostgreSql");
    }

    [Fact]
    public void MapSection_MultipleSections_ShouldStoreAll()
    {
        // Arrange
        LogArrange("Criando ConfigurationOptions com multiplos mapeamentos");
        var options = new ConfigurationOptions();

        // Act
        LogAct("Mapeando multiplas secoes");
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");
        options.MapSection<JwtConfig>("Security:Jwt");

        // Assert
        LogAssert("Verificando que todos os mapeamentos foram armazenados");
        var mappings = options.GetSectionMappings();
        mappings.Count.ShouldBe(2);
        mappings[typeof(PostgreSqlConfig)].ShouldBe("Persistence:PostgreSql");
        mappings[typeof(JwtConfig)].ShouldBe("Security:Jwt");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MapSection_WithNullOrEmptySectionPath_ShouldThrowArgumentException(string? sectionPath)
    {
        // Arrange
        LogArrange("Tentando mapear secao com caminho invalido");
        var options = new ConfigurationOptions();

        // Act & Assert
        LogAct("Chamando MapSection com caminho nulo ou vazio");
        LogAssert("Verificando que ArgumentException e lancada");
        Should.Throw<ArgumentException>(() => options.MapSection<PostgreSqlConfig>(sectionPath!));
    }

    [Fact]
    public void MapSection_ShouldReturnOptionsForChaining()
    {
        // Arrange
        LogArrange("Criando ConfigurationOptions");
        var options = new ConfigurationOptions();

        // Act
        LogAct("Chamando MapSection e verificando encadeamento");
        var result = options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

        // Assert
        LogAssert("Verificando que retorna a mesma instancia para encadeamento");
        result.ShouldBeSameAs(options);
    }

    [Fact]
    public void MapSection_DuplicateType_ShouldOverwrite()
    {
        // Arrange
        LogArrange("Mapeando mesmo tipo duas vezes");
        var options = new ConfigurationOptions();

        // Act
        LogAct("Mapeando PostgreSqlConfig duas vezes com secoes diferentes");
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");
        options.MapSection<PostgreSqlConfig>("Persistence:Pg");

        // Assert
        LogAssert("Verificando que ultimo mapeamento prevalece");
        var mappings = options.GetSectionMappings();
        mappings[typeof(PostgreSqlConfig)].ShouldBe("Persistence:Pg");
    }

    [Fact]
    public void BuildPipelines_WithNoHandlers_ShouldReturnEmptyPipelines()
    {
        // Arrange
        LogArrange("Criando ConfigurationOptions sem handlers");
        var options = new ConfigurationOptions();

        // Act
        LogAct("Chamando BuildPipelines");
        var (getPipeline, setPipeline) = options.BuildPipelines();

        // Assert
        LogAssert("Verificando que pipelines vazios foram criados");
        getPipeline.ShouldNotBeNull();
        setPipeline.ShouldNotBeNull();
    }
}
