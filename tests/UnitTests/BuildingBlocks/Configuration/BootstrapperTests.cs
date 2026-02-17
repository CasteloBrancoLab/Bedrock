using Bedrock.BuildingBlocks.Configuration;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration;

/// <summary>Manager simples para testes de DI (construtor com 2 parametros).</summary>
public sealed class DiTestConfigurationManager : ConfigurationManagerBase
{
    public DiTestConfigurationManager(IConfiguration configuration, ILogger<DiTestConfigurationManager> logger)
        : base(configuration, logger)
    {
    }

    protected override void ConfigureInternal(ConfigurationOptions options)
    {
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");
    }
}

public sealed class BootstrapperTests : TestBase
{
    public BootstrapperTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void AddBedrockConfiguration_ShouldRegisterManagerAsSingleton()
    {
        // Arrange
        LogArrange("Configurando service collection com IConfiguration");
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();

        // Act
        LogAct("Chamando AddBedrockConfiguration<DiTestConfigurationManager>");
        services.AddBedrockConfiguration<DiTestConfigurationManager>();

        // Assert
        LogAssert("Verificando que manager esta registrado");
        var provider = services.BuildServiceProvider();
        var manager = provider.GetService<DiTestConfigurationManager>();
        manager.ShouldNotBeNull();
    }

    [Fact]
    public void AddBedrockConfiguration_ShouldResolveSameInstance()
    {
        // Arrange
        LogArrange("Configurando service collection");
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddBedrockConfiguration<DiTestConfigurationManager>();

        // Act
        LogAct("Resolvendo manager duas vezes");
        var provider = services.BuildServiceProvider();
        var manager1 = provider.GetRequiredService<DiTestConfigurationManager>();
        var manager2 = provider.GetRequiredService<DiTestConfigurationManager>();

        // Assert
        LogAssert("Verificando que e a mesma instancia (Singleton)");
        manager1.ShouldBeSameAs(manager2);
    }

    [Fact]
    public void AddBedrockConfiguration_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando AddBedrockConfiguration e verificando encadeamento");
        var result = services.AddBedrockConfiguration<DiTestConfigurationManager>();

        // Assert
        LogAssert("Verificando que retorna a mesma instancia para encadeamento");
        result.ShouldBeSameAs(services);
    }
}
