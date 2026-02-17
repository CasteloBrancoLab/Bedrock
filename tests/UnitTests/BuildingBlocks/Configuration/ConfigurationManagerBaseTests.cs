using Bedrock.BuildingBlocks.Configuration;
using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Configuration.Handlers.Enums;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration;

#region Test Support Types

public sealed class PostgreSqlConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Schema { get; set; } = "public";
    public string[] AllowedSchemas { get; set; } = [];
    public int? CommandTimeout { get; set; }
}

public sealed class MySqlConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Port { get; set; }
}

public sealed class JwtConfig
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
    public bool ValidateLifetime { get; set; }
}

public sealed class EmptyConfig
{
    public string? OptionalValue { get; set; }
    public int? NullableInt { get; set; }
    public string[] Tags { get; set; } = [];
}

public sealed class UnmappedConfig
{
    public string Value { get; set; } = string.Empty;
}

public sealed class ExtendedTypesConfig
{
    public long BigNumber { get; set; }
    public double Ratio { get; set; }
    public decimal Price { get; set; }
    public string ReadOnlyComputed => $"computed";
}

public sealed class TestConfigurationManager : ConfigurationManagerBase
{
    [ThreadStatic]
    private static Action<ConfigurationOptions>? PendingConfigureAction;

    public TestConfigurationManager(IConfiguration configuration, ILogger<TestConfigurationManager> logger,
        Action<ConfigurationOptions>? configureAction = null)
        : base(configuration, InitAndReturnLogger(logger, configureAction))
    {
    }

    private static ILogger<TestConfigurationManager> InitAndReturnLogger(
        ILogger<TestConfigurationManager> logger, Action<ConfigurationOptions>? configureAction)
    {
        PendingConfigureAction = configureAction;
        return logger;
    }

    protected override void ConfigureInternal(ConfigurationOptions options)
    {
        // Default mappings
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");
        options.MapSection<MySqlConfig>("Persistence:MySql");
        options.MapSection<JwtConfig>("Security:Jwt");
        options.MapSection<EmptyConfig>("Empty");
        options.MapSection<ExtendedTypesConfig>("Extended");

        PendingConfigureAction?.Invoke(options);
        PendingConfigureAction = null;
    }
}

#endregion

public sealed class ConfigurationManagerBaseTests : TestBase
{
    private readonly Mock<ILogger<TestConfigurationManager>> _loggerMock;

    public ConfigurationManagerBaseTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<TestConfigurationManager>>();
    }

    private static IConfiguration BuildInMemoryConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    #region T018: Constructor and Initialize tests

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Tentando criar manager com configuration nulo");

        // Act & Assert
        LogAct("Chamando construtor com configuration nulo");
        LogAssert("Verificando que ArgumentNullException e lancada");
        Should.Throw<ArgumentNullException>(() =>
            new TestConfigurationManager(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Tentando criar manager com logger nulo");
        var config = BuildInMemoryConfiguration([]);

        // Act & Assert
        LogAct("Chamando construtor com logger nulo");
        LogAssert("Verificando que ArgumentNullException e lancada");
        Should.Throw<ArgumentNullException>(() =>
            new TestConfigurationManager(config, null!));
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldInitializeSuccessfully()
    {
        // Arrange
        LogArrange("Criando manager com dependencias validas");
        var config = BuildInMemoryConfiguration([]);

        // Act
        LogAct("Chamando construtor");
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Assert
        LogAssert("Verificando que manager foi criado com sucesso");
        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Get_WithUnmappedSection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Criando manager e tentando ler secao nao mapeada");
        var config = BuildInMemoryConfiguration([]);
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act & Assert
        LogAct("Chamando Get com tipo nao mapeado");
        LogAssert("Verificando que InvalidOperationException e lancada");
        Should.Throw<InvalidOperationException>(() => manager.Get<UnmappedConfig>());
    }

    #endregion

    #region T019: Get<TSection>() tests

    [Fact]
    public void GetSection_ShouldPopulateAllPropertiesFromConfiguration()
    {
        // Arrange
        LogArrange("Criando configuracao com todas as propriedades de PostgreSqlConfig");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "Host=localhost;Database=mydb",
            ["Persistence:PostgreSql:Port"] = "5432",
            ["Persistence:PostgreSql:Schema"] = "public",
            ["Persistence:PostgreSql:AllowedSchemas:0"] = "public",
            ["Persistence:PostgreSql:AllowedSchemas:1"] = "tenant_a",
            ["Persistence:PostgreSql:CommandTimeout"] = "30"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig>");
        var result = manager.Get<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que todas as propriedades foram populadas");
        result.ConnectionString.ShouldBe("Host=localhost;Database=mydb");
        result.Port.ShouldBe(5432);
        result.Schema.ShouldBe("public");
        result.AllowedSchemas.ShouldBe(new[] { "public", "tenant_a" });
        result.CommandTimeout.ShouldBe(30);
    }

    [Fact]
    public void GetSection_WithArrayProperty_ShouldResolveCorrectly()
    {
        // Arrange
        LogArrange("Criando configuracao com array de strings");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:AllowedSchemas:0"] = "schema1",
            ["Persistence:PostgreSql:AllowedSchemas:1"] = "schema2",
            ["Persistence:PostgreSql:AllowedSchemas:2"] = "schema3"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig>");
        var result = manager.Get<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que array foi resolvido corretamente");
        result.AllowedSchemas.Length.ShouldBe(3);
        result.AllowedSchemas.ShouldBe(new[] { "schema1", "schema2", "schema3" });
    }

    [Fact]
    public void GetSection_WithEmptyArraySource_ShouldReturnEmptyArray()
    {
        // Arrange
        LogArrange("Criando configuracao sem elementos de array");
        var config = BuildInMemoryConfiguration([]);
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig>");
        var result = manager.Get<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que array vazio e retornado, nao null");
        result.AllowedSchemas.ShouldNotBeNull();
        result.AllowedSchemas.ShouldBeEmpty();
    }

    [Fact]
    public void GetSection_WithNullablePropertyMissing_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Criando configuracao sem CommandTimeout");
        var config = BuildInMemoryConfiguration([]);
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig>");
        var result = manager.Get<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que propriedade nullable retorna null");
        result.CommandTimeout.ShouldBeNull();
    }

    [Fact]
    public void GetSection_WithNonNullablePropertyMissing_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Criando configuracao sem Port");
        var config = BuildInMemoryConfiguration([]);
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig>");
        var result = manager.Get<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que propriedade non-nullable retorna default");
        result.Port.ShouldBe(0);
    }

    [Fact]
    public void GetSection_TwoClassesSamePropertyName_ShouldNotCollide()
    {
        // Arrange
        LogArrange("Criando configuracao com ConnectionString em duas classes diferentes");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "Host=pg;Database=pgdb",
            ["Persistence:MySql:ConnectionString"] = "Server=mysql;Database=mysqldb"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get para ambas as classes");
        var pgResult = manager.Get<PostgreSqlConfig>();
        var mysqlResult = manager.Get<MySqlConfig>();

        // Assert
        LogAssert("Verificando que cada classe tem sua propria ConnectionString");
        pgResult.ConnectionString.ShouldBe("Host=pg;Database=pgdb");
        mysqlResult.ConnectionString.ShouldBe("Server=mysql;Database=mysqldb");
    }

    [Fact]
    public void GetSection_WithBooleanProperty_ShouldResolveCorrectly()
    {
        // Arrange
        LogArrange("Criando configuracao com propriedade boolean");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Security:Jwt:ValidateLifetime"] = "true",
            ["Security:Jwt:ExpirationMinutes"] = "60"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<JwtConfig>");
        var result = manager.Get<JwtConfig>();

        // Assert
        LogAssert("Verificando que boolean e int foram resolvidos");
        result.ValidateLifetime.ShouldBeTrue();
        result.ExpirationMinutes.ShouldBe(60);
    }

    [Fact]
    public void GetSection_WithMissingStringProperty_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Criando configuracao vazia para EmptyConfig");
        var config = BuildInMemoryConfiguration([]);
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<EmptyConfig>");
        var result = manager.Get<EmptyConfig>();

        // Assert
        LogAssert("Verificando que string opcional retorna null");
        result.OptionalValue.ShouldBeNull();
        result.NullableInt.ShouldBeNull();
        result.Tags.ShouldNotBeNull();
        result.Tags.ShouldBeEmpty();
    }

    #endregion

    #region T020: Get<TSection, TProperty>() tests

    [Fact]
    public void GetProperty_ShouldReturnSpecificPropertyValue()
    {
        // Arrange
        LogArrange("Criando configuracao com ConnectionString");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "Host=localhost"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig, string> para ConnectionString");
        var result = manager.Get<PostgreSqlConfig, string>(c => c.ConnectionString);

        // Assert
        LogAssert("Verificando valor da propriedade especifica");
        result.ShouldBe("Host=localhost");
    }

    [Fact]
    public void GetProperty_IntProperty_ShouldReturnCorrectValue()
    {
        // Arrange
        LogArrange("Criando configuracao com Port");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:Port"] = "5432"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig, int> para Port");
        var result = manager.Get<PostgreSqlConfig, int>(c => c.Port);

        // Assert
        LogAssert("Verificando valor int da propriedade especifica");
        result.ShouldBe(5432);
    }

    [Fact]
    public void GetProperty_BoolProperty_ShouldReturnCorrectValue()
    {
        // Arrange
        LogArrange("Criando configuracao com ValidateLifetime");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Security:Jwt:ValidateLifetime"] = "true"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<JwtConfig, bool> para ValidateLifetime");
        var result = manager.Get<JwtConfig, bool>(c => c.ValidateLifetime);

        // Assert
        LogAssert("Verificando valor boolean");
        result.ShouldBeTrue();
    }

    [Fact]
    public void GetProperty_NullablePropertyMissing_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Criando configuracao sem CommandTimeout");
        var config = BuildInMemoryConfiguration([]);
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig, int?> para CommandTimeout");
        var result = manager.Get<PostgreSqlConfig, int?>(c => c.CommandTimeout);

        // Assert
        LogAssert("Verificando que nullable retorna default (null -> 0 via ConvertValue)");
        result.ShouldBeNull();
    }

    [Fact]
    public void GetProperty_ArrayProperty_ShouldReturnArray()
    {
        // Arrange
        LogArrange("Criando configuracao com array");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:AllowedSchemas:0"] = "public",
            ["Persistence:PostgreSql:AllowedSchemas:1"] = "admin"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<PostgreSqlConfig, string[]> para AllowedSchemas");
        var result = manager.Get<PostgreSqlConfig, string[]>(c => c.AllowedSchemas);

        // Assert
        LogAssert("Verificando que array e retornado");
        result.ShouldBe(new[] { "public", "admin" });
    }

    [Fact]
    public void GetProperty_CalledTwice_ShouldUseCachedPath()
    {
        // Arrange
        LogArrange("Criando configuracao para testar cache de path");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:Port"] = "5432"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get duas vezes para mesma propriedade");
        var result1 = manager.Get<PostgreSqlConfig, int>(c => c.Port);
        var result2 = manager.Get<PostgreSqlConfig, int>(c => c.Port);

        // Assert
        LogAssert("Verificando que ambas as chamadas retornam o mesmo valor (cache hit)");
        result1.ShouldBe(5432);
        result2.ShouldBe(5432);
    }

    #endregion

    #region T041: Coverage — extended types and edge cases

    [Fact]
    public void GetSection_WithLongProperty_ShouldResolveCorrectly()
    {
        // Arrange
        LogArrange("Criando configuracao com propriedade long");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Extended:BigNumber"] = "9999999999"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<ExtendedTypesConfig>");
        var result = manager.Get<ExtendedTypesConfig>();

        // Assert
        LogAssert("Verificando que long foi resolvido corretamente");
        result.BigNumber.ShouldBe(9999999999L);
    }

    [Fact]
    public void GetSection_WithDoubleProperty_ShouldResolveCorrectly()
    {
        // Arrange
        LogArrange("Criando configuracao com propriedade double");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Extended:Ratio"] = "3.14"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<ExtendedTypesConfig>");
        var result = manager.Get<ExtendedTypesConfig>();

        // Assert
        LogAssert("Verificando que double foi resolvido corretamente");
        result.Ratio.ShouldBe(3.14);
    }

    [Fact]
    public void GetSection_WithDecimalProperty_ShouldResolveCorrectly()
    {
        // Arrange
        LogArrange("Criando configuracao com propriedade decimal");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Extended:Price"] = "99.99"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<ExtendedTypesConfig>");
        var result = manager.Get<ExtendedTypesConfig>();

        // Assert
        LogAssert("Verificando que decimal foi resolvido corretamente");
        result.Price.ShouldBe(99.99m);
    }

    [Fact]
    public void GetSection_WithReadOnlyProperty_ShouldSkipIt()
    {
        // Arrange
        LogArrange("Criando configuracao com classe que tem propriedade somente leitura");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Extended:BigNumber"] = "42"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Chamando Get<ExtendedTypesConfig>");
        var result = manager.Get<ExtendedTypesConfig>();

        // Assert
        LogAssert("Verificando que propriedade read-only nao causou erro e valor computado mantido");
        result.ReadOnlyComputed.ShouldBe("computed");
        result.BigNumber.ShouldBe(42L);
    }

    #endregion

    #region T032: Integration tests — full pipeline with handlers

    [Fact]
    public void GetSection_WithGlobalHandler_ShouldExecuteForAllKeys()
    {
        // Arrange
        LogArrange("Criando manager com handler global que transforma strings");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "host=localhost",
            ["Persistence:PostgreSql:Schema"] = "public"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<UpperCaseHandler>().AtPosition(1);
        });

        // Act
        LogAct("Chamando Get<PostgreSqlConfig>");
        var result = manager.Get<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que handler global transformou todas as strings (P2-7)");
        result.ConnectionString.ShouldBe("HOST=LOCALHOST");
        result.Schema.ShouldBe("PUBLIC");
    }

    [Fact]
    public void GetSection_WithPropertyScopedHandler_ShouldExecuteOnlyForMatchingKey()
    {
        // Arrange
        LogArrange("Criando manager com handler escopado para propriedade (P2-5)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "original",
            ["Persistence:PostgreSql:Schema"] = "public"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<UpperCaseHandler>()
                .AtPosition(1)
                .ToClass<PostgreSqlConfig>()
                .ToProperty(c => c.ConnectionString);
        });

        // Act
        LogAct("Chamando Get<PostgreSqlConfig>");
        var result = manager.Get<PostgreSqlConfig>();

        // Assert
        LogAssert("Verificando que handler executou apenas para ConnectionString");
        result.ConnectionString.ShouldBe("ORIGINAL");
        result.Schema.ShouldBe("public"); // Nao transformado
    }

    [Fact]
    public void GetSection_WithClassScopedHandler_ShouldExecuteForAllPropertiesInSection()
    {
        // Arrange
        LogArrange("Criando manager com handler escopado para classe (P2-6/P2-9)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "pg-conn",
            ["Persistence:PostgreSql:Schema"] = "pg-schema",
            ["Persistence:MySql:ConnectionString"] = "mysql-conn"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<UpperCaseHandler>()
                .AtPosition(1)
                .ToClass<PostgreSqlConfig>();
        });

        // Act
        LogAct("Chamando Get para PostgreSql e MySql");
        var pgResult = manager.Get<PostgreSqlConfig>();
        var mysqlResult = manager.Get<MySqlConfig>();

        // Assert
        LogAssert("Verificando que handler executou para PostgreSql mas nao para MySql");
        pgResult.ConnectionString.ShouldBe("PG-CONN");
        pgResult.Schema.ShouldBe("PG-SCHEMA");
        mysqlResult.ConnectionString.ShouldBe("mysql-conn"); // Nao transformado
    }

    [Fact]
    public void GetSection_WithMultipleHandlersInOrder_ShouldChainCorrectly()
    {
        // Arrange
        LogArrange("Criando manager com multiplos handlers em cadeia (P2-2)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "value"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<PrefixHandler>().AtPosition(1);
            options.AddHandler<UpperCaseHandler>().AtPosition(2);
        });

        // Act
        LogAct("Chamando Get — handlers devem encadear em ordem");
        var result = manager.Get<PostgreSqlConfig, string>(c => c.ConnectionString);

        // Assert
        LogAssert("Verificando que handlers encadearam: prefix primeiro, depois uppercase");
        result.ShouldBe("PREFIX_VALUE");
    }

    [Fact]
    public void GetProperty_WithScopedHandler_ShouldDerivePathCorrectly()
    {
        // Arrange
        LogArrange("Criando manager com handler escopado via fluent API (P2-8)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "host=localhost",
            ["Persistence:PostgreSql:Port"] = "5432"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<UpperCaseHandler>()
                .AtPosition(1)
                .ToClass<PostgreSqlConfig>()
                .ToProperty(c => c.ConnectionString);
        });

        // Act
        LogAct("Chamando Get para propriedade com e sem handler escopado");
        var connStr = manager.Get<PostgreSqlConfig, string>(c => c.ConnectionString);
        var port = manager.Get<PostgreSqlConfig, int>(c => c.Port);

        // Assert
        LogAssert("Verificando que handler escopado atuou apenas na ConnectionString");
        connStr.ShouldBe("HOST=LOCALHOST");
        port.ShouldBe(5432);
    }

    [Fact]
    public void GetSection_HandlerWithForGetOnly_ShouldNotAffectSetPipeline()
    {
        // Arrange
        LogArrange("Criando manager com handler registrado apenas no Get");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:Port"] = "5432"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<UpperCaseHandler>().AtPosition(1).ForGet();
        });

        // Act
        LogAct("Escrevendo valor via Set e lendo via Get");
        manager.Set<PostgreSqlConfig, int>(c => c.Port, 9999);
        var result = manager.Get<PostgreSqlConfig, int>(c => c.Port);

        // Assert
        LogAssert("Verificando que Set nao passou pelo handler de Get");
        result.ShouldBe(9999);
    }

    #endregion

    #region T039: Set<TSection, TProperty>() tests

    [Fact]
    public void Set_ShouldStoreValueInMemory()
    {
        // Arrange
        LogArrange("Criando manager para teste de Set (edge case 6)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:Port"] = "5432"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Escrevendo valor via Set");
        manager.Set<PostgreSqlConfig, int>(c => c.Port, 9999);

        // Assert
        LogAssert("Verificando que Get retorna valor escrito via Set");
        var result = manager.Get<PostgreSqlConfig, int>(c => c.Port);
        result.ShouldBe(9999);
    }

    [Fact]
    public void Set_WithNoHandlers_ShouldApplyValueDirectly()
    {
        // Arrange
        LogArrange("Criando manager sem handlers de Set (edge case 6)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "original"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Escrevendo valor via Set sem handlers");
        manager.Set<PostgreSqlConfig, string>(c => c.ConnectionString, "new-value");

        // Assert
        LogAssert("Verificando que valor foi aplicado sem erro");
        var result = manager.Get<PostgreSqlConfig, string>(c => c.ConnectionString);
        result.ShouldBe("new-value");
    }

    [Fact]
    public void Set_WithHandler_ShouldFlowThroughSetPipeline()
    {
        // Arrange
        LogArrange("Criando manager com handler que transforma no Set (P4-1)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "original"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<SetUpperCaseHandler>().AtPosition(1);
        });

        // Act
        LogAct("Escrevendo valor via Set — handler deve transformar no pipeline de Set");
        manager.Set<PostgreSqlConfig, string>(c => c.ConnectionString, "new-value");

        // Assert
        LogAssert("Verificando que handler de Set transformou o valor armazenado");
        var result = manager.Get<PostgreSqlConfig, string>(c => c.ConnectionString);
        result.ShouldBe("NEW-VALUE");
    }

    [Fact]
    public void Set_GetAfterSet_ShouldReturnUpdatedValue()
    {
        // Arrange
        LogArrange("Criando manager para testar Get apos Set (P4-3)");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Security:Jwt:Secret"] = "old-secret",
            ["Security:Jwt:ExpirationMinutes"] = "30"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object);

        // Act
        LogAct("Escrevendo novo secret e lendo via Get");
        manager.Set<JwtConfig, string>(c => c.Secret, "new-secret");
        var result = manager.Get<JwtConfig>();

        // Assert
        LogAssert("Verificando que Get retorna valor atualizado pelo Set");
        result.Secret.ShouldBe("new-secret");
        result.ExpirationMinutes.ShouldBe(30); // Nao alterado
    }

    [Fact]
    public void Set_WithForSetOnlyHandler_ShouldNotAffectGetPipeline()
    {
        // Arrange
        LogArrange("Criando manager com handler apenas no Set pipeline");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "original"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<UpperCaseHandler>().AtPosition(1).ForSet();
        });

        // Act
        LogAct("Lendo via Get sem Set — handler ForSet nao deve afetar Get");
        var getResult = manager.Get<PostgreSqlConfig, string>(c => c.ConnectionString);

        // Assert
        LogAssert("Verificando que handler ForSet nao transformou valor no Get");
        getResult.ShouldBe("original");
    }

    [Fact]
    public void Set_MultipleHandlersInChain_ShouldExecuteInOrder()
    {
        // Arrange
        LogArrange("Criando manager com multiplos handlers de Set em cadeia");
        var config = BuildInMemoryConfiguration(new Dictionary<string, string?>
        {
            ["Persistence:PostgreSql:ConnectionString"] = "original"
        });
        var manager = new TestConfigurationManager(config, _loggerMock.Object, options =>
        {
            options.AddHandler<SetPrefixHandler>().AtPosition(1);
            options.AddHandler<SetUpperCaseHandler>().AtPosition(2);
        });

        // Act
        LogAct("Escrevendo valor via Set — handlers devem encadear");
        manager.Set<PostgreSqlConfig, string>(c => c.ConnectionString, "value");
        var result = manager.Get<PostgreSqlConfig, string>(c => c.ConnectionString);

        // Assert
        LogAssert("Verificando que handlers de Set encadearam na ordem correta");
        result.ShouldBe("PREFIX_VALUE");
    }

    #endregion
}

#region Integration Test Handlers

public sealed class SetUpperCaseHandler : ConfigurationHandlerBase
{
    public SetUpperCaseHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue) => currentValue;

    public override object? HandleSet(string key, object? currentValue)
    {
        if (currentValue is string str)
        {
            return str.ToUpperInvariant();
        }

        return currentValue;
    }
}

public sealed class SetPrefixHandler : ConfigurationHandlerBase
{
    public SetPrefixHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue) => currentValue;

    public override object? HandleSet(string key, object? currentValue)
    {
        if (currentValue is string str)
        {
            return "prefix_" + str;
        }

        return currentValue;
    }
}

public sealed class UpperCaseHandler : ConfigurationHandlerBase
{
    public UpperCaseHandler() : base(LoadStrategy.AllTime) { }

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

public sealed class PrefixHandler : ConfigurationHandlerBase
{
    public PrefixHandler() : base(LoadStrategy.AllTime) { }

    public override object? HandleGet(string key, object? currentValue)
    {
        if (currentValue is string str)
        {
            return "prefix_" + str;
        }

        return currentValue;
    }

    public override object? HandleSet(string key, object? currentValue) => currentValue;
}

#endregion
