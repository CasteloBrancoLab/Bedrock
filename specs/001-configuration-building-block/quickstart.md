# Quickstart: Configuration BuildingBlock

**Phase 1 Output** | **Date**: 2026-02-15

## 1. Criar uma Classe de Configuracao (POCO)

```csharp
// Classe simples que mapeia para uma secao do appsettings.json
public class PostgreSqlConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Schema { get; set; } = "public";
    public string[] AllowedSchemas { get; set; } = [];
    public int? CommandTimeout { get; set; }  // nullable
}
```

Correspondente no `appsettings.json`:

```json
{
  "Persistence": {
    "PostgreSql": {
      "ConnectionString": "Host=localhost;Database=mydb",
      "Port": 5432,
      "Schema": "public",
      "AllowedSchemas": [ "public", "tenant_a" ],
      "CommandTimeout": 30
    }
  }
}
```

## 2. Criar um ConfigurationManager Concreto

```csharp
public sealed class AppConfigurationManager : ConfigurationManagerBase
{
    public AppConfigurationManager(IConfiguration configuration, ILogger<AppConfigurationManager> logger)
        : base(configuration, logger)
    {
    }

    protected override void ConfigureInternal(ConfigurationOptions options)
    {
        // Mapeia classes para secoes
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");
        options.MapSection<JwtConfig>("Security:Jwt");

        // Sem handlers customizados — comportamento equivalente ao IConfiguration padrao
    }
}
```

## 3. Registrar no IoC

```csharp
// Em Program.cs ou Startup
services.AddBedrockConfiguration<AppConfigurationManager>();
```

## 4. Usar

```csharp
public class MeuServico
{
    private readonly AppConfigurationManager _config;

    public MeuServico(AppConfigurationManager config)
    {
        _config = config;
    }

    public void Executar()
    {
        // Ler secao inteira
        var pgConfig = _config.Get<PostgreSqlConfig>();
        Console.WriteLine(pgConfig.ConnectionString);
        Console.WriteLine(pgConfig.Port);
        Console.WriteLine(pgConfig.AllowedSchemas.Length);

        // Ler propriedade especifica
        var connStr = _config.Get<PostgreSqlConfig, string>(c => c.ConnectionString);

        // Escrever (in-memory por padrao)
        _config.Set<PostgreSqlConfig, int>(c => c.Port, 5433);
    }
}
```

## 5. Adicionar Handlers Customizados

### 5a. Criar um Handler

```csharp
public sealed class MeuHandlerCustomizado : ConfigurationHandlerBase
{
    public MeuHandlerCustomizado()
        : base(LoadStrategy.StartupOnly)  // executa uma vez, cache permanente
    {
    }

    public override object? HandleGet(string key, object? currentValue)
    {
        // key = "Persistence:PostgreSql:ConnectionString"
        // currentValue = valor resolvido pelo IConfiguration (ou handler anterior)

        // Exemplo: transformar, substituir ou repassar
        if (currentValue is string str && str.StartsWith("vault://"))
        {
            return BuscarDoVault(str);  // substitui
        }

        return currentValue;  // repassa sem alteracao
    }

    public override object? HandleSet(string key, object? currentValue)
    {
        return currentValue;  // repassa
    }

    private static string BuscarDoVault(string reference)
    {
        // Logica de busca no vault (exemplo ilustrativo)
        return "Host=prod-server;Database=mydb;Password=secret";
    }
}
```

### 5b. Registrar com Escopo

```csharp
public sealed class AppConfigurationManager : ConfigurationManagerBase
{
    public AppConfigurationManager(IConfiguration configuration, ILogger<AppConfigurationManager> logger)
        : base(configuration, logger)
    {
    }

    protected override void ConfigureInternal(ConfigurationOptions options)
    {
        options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");
        options.MapSection<JwtConfig>("Security:Jwt");

        // Handler global (todas as chaves)
        options.AddHandler<LoggingHandler>()
            .AtPosition(1)
            .WithLoadStrategy(LoadStrategy.AllTime);

        // Handler por propriedade especifica
        options.AddHandler<MeuHandlerCustomizado>()
            .AtPosition(2)
            .WithLoadStrategy(LoadStrategy.StartupOnly)
            .ToClass<PostgreSqlConfig>()
            .ToProperty(c => c.ConnectionString);

        // Handler por classe inteira (todas as propriedades de JwtConfig)
        options.AddHandler<EncryptionHandler>()
            .AtPosition(3)
            .WithLoadStrategy(LoadStrategy.LazyStartupOnly)
            .ToClass<JwtConfig>();

        // Handler apenas no pipeline de Set
        options.AddHandler<AuditHandler>()
            .AtPosition(1)
            .ForSet();
    }
}
```

## 6. Fluxo de Execucao (Get)

```
manager.Get<PostgreSqlConfig>()
  │
  ├── Propriedade "ConnectionString":
  │   ├── IConfiguration le: "vault://connection-string-key"
  │   ├── Handler 1 (LoggingHandler, global): loga e repassa
  │   ├── Handler 2 (MeuHandlerCustomizado, property match): busca do vault
  │   └── Valor final: "Host=prod-server;Database=mydb;Password=secret"
  │
  ├── Propriedade "Port":
  │   ├── IConfiguration le: 5432
  │   ├── Handler 1 (LoggingHandler, global): loga e repassa
  │   ├── Handler 2 (MeuHandlerCustomizado, NO MATCH — skip)
  │   └── Valor final: 5432
  │
  └── Retorna PostgreSqlConfig { ConnectionString="Host=...", Port=5432, ... }
```

## Notas

- **Sem strings manuais**: O caminho `Persistence:PostgreSql:ConnectionString` e derivado automaticamente do `MapSection<PostgreSqlConfig>("Persistence:PostgreSql")` + nome da propriedade `ConnectionString`.
- **Sem colisao**: Classes diferentes com propriedade `ConnectionString` (ex: `PostgreSqlConfig` e `MySqlConfig`) tem paths diferentes porque a secao e diferente.
- **Arrays**: Propriedades `string[]` sao resolvidas a partir de secoes indexadas no JSON (`"Key:0"`, `"Key:1"`, etc.).
- **Nullable**: Propriedades `int?` retornam `null` se a chave nao existe em nenhuma fonte.
- **LoadStrategy.StartupOnly**: Se o handler falhar no Initialize(), a aplicacao nao sobe (fail-fast).
- **LoadStrategy.LazyStartupOnly**: Se falhar no primeiro acesso, o erro e propagado. O proximo acesso retenta.
