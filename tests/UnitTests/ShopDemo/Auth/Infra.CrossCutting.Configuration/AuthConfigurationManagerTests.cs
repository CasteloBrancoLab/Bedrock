using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Infra.CrossCutting.Configuration;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.CrossCutting.Configuration;

public class AuthConfigurationManagerTests : TestBase
{
    private readonly Mock<ILogger<AuthConfigurationManager>> _loggerMock;

    public AuthConfigurationManagerTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<AuthConfigurationManager>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Criando configuracao em memoria");
        var configuration = BuildInMemoryConfiguration([]);

        // Act
        LogAct("Criando AuthConfigurationManager");
        var manager = new AuthConfigurationManager(configuration, _loggerMock.Object);

        // Assert
        LogAssert("Verificando que instancia foi criada");
        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparando configuracao nula");

        // Act & Assert
        LogAct("Criando AuthConfigurationManager com configuracao nula");
        LogAssert("Verificando ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new AuthConfigurationManager(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparando logger nulo");
        var configuration = BuildInMemoryConfiguration([]);

        // Act & Assert
        LogAct("Criando AuthConfigurationManager com logger nulo");
        LogAssert("Verificando ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new AuthConfigurationManager(configuration, null!));
    }

    #endregion

    #region ConfigureInternal Tests (Section Mapping)

    [Fact]
    public void Get_AuthDatabaseConfig_ShouldMapToPersistencePostgreSqlSection()
    {
        // Arrange
        LogArrange("Criando configuracao com secao Persistence:PostgreSql");
        var configuration = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "Host=localhost;Database=auth_db",
            ["Persistence:PostgreSql:CommandTimeout"] = "60"
        });
        var manager = new AuthConfigurationManager(configuration, _loggerMock.Object);

        // Act
        LogAct("Obtendo AuthDatabaseConfig via Get");
        var config = manager.Get<AuthDatabaseConfig>();

        // Assert
        LogAssert("Verificando que valores foram mapeados corretamente");
        config.ShouldNotBeNull();
        config.ConnectionString.ShouldBe("Host=localhost;Database=auth_db");
        config.CommandTimeout.ShouldBe(60);
    }

    [Fact]
    public void Get_AuthDatabaseConfig_WithMissingSection_ShouldReturnDefaults()
    {
        // Arrange
        LogArrange("Criando configuracao vazia");
        var configuration = BuildInMemoryConfiguration([]);
        var manager = new AuthConfigurationManager(configuration, _loggerMock.Object);

        // Act
        LogAct("Obtendo AuthDatabaseConfig sem secao configurada");
        var config = manager.Get<AuthDatabaseConfig>();

        // Assert
        LogAssert("Verificando valores padrao quando secao nao existe");
        config.ShouldNotBeNull();
        config.ConnectionString.ShouldBeNull();
        config.CommandTimeout.ShouldBe(0);
    }

    [Fact]
    public void Get_AuthDatabaseConfig_ConnectionString_ShouldReturnSpecificProperty()
    {
        // Arrange
        LogArrange("Criando configuracao com ConnectionString");
        var configuration = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "Host=db.prod;Database=auth"
        });
        var manager = new AuthConfigurationManager(configuration, _loggerMock.Object);

        // Act
        LogAct("Obtendo propriedade ConnectionString via Get com lambda");
        var connectionString = manager.Get<AuthDatabaseConfig, string>(c => c.ConnectionString);

        // Assert
        LogAssert("Verificando valor retornado");
        connectionString.ShouldBe("Host=db.prod;Database=auth");
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void AuthConfigurationManager_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Obtendo tipo AuthConfigurationManager");
        var type = typeof(AuthConfigurationManager);

        // Act
        LogAct("Verificando se classe e sealed");
        var isSealed = type.IsSealed;

        // Assert
        LogAssert("Verificando que classe e sealed");
        isSealed.ShouldBeTrue();
    }

    #endregion

    #region Helper Methods

    private static IConfiguration BuildInMemoryConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    #endregion
}
