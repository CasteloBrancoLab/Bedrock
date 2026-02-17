using System.Reflection;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Infra.CrossCutting.Configuration;
using ShopDemo.Auth.Infra.Data.PostgreSql.Migrations;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Migrations;

public class AuthMigrationManagerTests : TestBase
{
    private readonly ILogger<AuthMigrationManager> _logger;
    private readonly AuthConfigurationManager _configurationManager;

    public AuthMigrationManagerTests(ITestOutputHelper output) : base(output)
    {
        var loggerMock = new Mock<ILogger<AuthMigrationManager>>();
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _logger = loggerMock.Object;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:PostgreSql:ConnectionString"] = "Host=localhost;Database=auth_test"
            })
            .Build();

        var configLoggerMock = new Mock<ILogger<AuthConfigurationManager>>();
        _configurationManager = new AuthConfigurationManager(configuration, configLoggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger nulo");

        // Act
        LogAct("Criando AuthMigrationManager com logger nulo");
        var action = () => new AuthMigrationManager(null!, _configurationManager);

        // Assert
        LogAssert("Verificando ArgumentNullException");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullConfigManager_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando config manager nulo");

        // Act
        LogAct("Criando AuthMigrationManager com config manager nulo");
        var action = () => new AuthMigrationManager(_logger, null!);

        // Assert
        LogAssert("Verificando ArgumentNullException");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDeps_ShouldSucceed()
    {
        // Arrange
        LogArrange("Preparando dependencias validas");

        // Act
        LogAct("Criando AuthMigrationManager");
        var manager = new AuthMigrationManager(_logger, _configurationManager);

        // Assert
        LogAssert("Verificando criacao bem-sucedida");
        manager.ShouldNotBeNull();
    }

    [Fact]
    public void CreateServiceProvider_ShouldReturnValidProvider()
    {
        // Arrange
        LogArrange("Criando manager com configuracao valida");
        var manager = new AuthMigrationManager(_logger, _configurationManager);

        // Act
        LogAct("Criando FluentMigrator service provider");
        using var serviceProvider = manager.CreateServiceProvider();

        // Assert
        LogAssert("Verificando service provider valido");
        serviceProvider.ShouldNotBeNull();
    }

    [Fact]
    public void TargetSchema_ShouldReturnPublic()
    {
        // Arrange
        LogArrange("Criando manager com configuracao valida");
        var manager = new AuthMigrationManager(_logger, _configurationManager);

        // Act
        LogAct("Lendo TargetSchema via reflection");
        var targetSchema = typeof(AuthMigrationManager)
            .GetProperty("TargetSchema", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(manager);

        // Assert
        LogAssert("Verificando que TargetSchema e 'public'");
        targetSchema.ShouldBe("public");
    }

    [Fact]
    public async Task MigrateUpAsync_WithNullExecutionContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Criando manager com dependencias validas");
        var manager = new AuthMigrationManager(_logger, _configurationManager);

        // Act
        LogAct("Chamando MigrateUpAsync com contexto nulo");
        var action = () => manager.MigrateUpAsync(null!);

        // Assert
        LogAssert("Verificando ArgumentNullException");
        await action.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MigrateDownAsync_WithNullExecutionContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Criando manager com dependencias validas");
        var manager = new AuthMigrationManager(_logger, _configurationManager);

        // Act
        LogAct("Chamando MigrateDownAsync com contexto nulo");
        var action = () => manager.MigrateDownAsync(null!, 0);

        // Assert
        LogAssert("Verificando ArgumentNullException");
        await action.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetStatusAsync_WithNullExecutionContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Criando manager com dependencias validas");
        var manager = new AuthMigrationManager(_logger, _configurationManager);

        // Act
        LogAct("Chamando GetStatusAsync com contexto nulo");
        var action = () => manager.GetStatusAsync(null!);

        // Assert
        LogAssert("Verificando ArgumentNullException");
        await action.ShouldThrowAsync<ArgumentNullException>();
    }
}
