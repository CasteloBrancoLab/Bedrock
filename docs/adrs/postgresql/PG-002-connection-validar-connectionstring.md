# PG-002: ConfigureInternal da Connection Deve Validar Connection String

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**PG002_ConnectionValidatesConnectionStringRule**, que verifica:

- Para cada classe que herda `PostgreSqlConnectionBase`, o metodo
  `ConfigureInternal` deve conter uma chamada a
  `ThrowIfNullOrWhiteSpace` ou `ThrowIfNullOrEmpty`.

## Contexto

### O Problema (Analogia)

Imagine um motorista que entra no carro, da a partida e comeca a
dirigir sem verificar se tem combustivel. O carro pode parar no meio
da estrada — e o problema so aparece quando ja e tarde demais. Melhor
verificar antes de sair.

### O Problema Tecnico

Cada bounded context tem sua propria connection string configurada via
`IConfiguration`. Se a connection string nao estiver configurada (null
ou vazia), a conexao falha em runtime com erros pouco descritivos:

1. **Null connection string**: `NpgsqlConnection` lanca
   `ArgumentNullException` sem indicar qual connection string falta.
2. **String vazia**: O Npgsql tenta conectar com parametros invalidos,
   gerando timeout ou erro de autenticacao confuso.
3. **Diagnostico dificil**: Em producao, o erro aparece no primeiro
   acesso ao banco — potencialmente minutos apos o deploy.

## A Decisao

Todo `ConfigureInternal` de uma Connection deve validar a connection
string com `ThrowIfNullOrWhiteSpace` ou `ThrowIfNullOrEmpty` antes
de usa-la:

```csharp
public sealed class AuthPostgreSqlConnection
    : PostgreSqlConnectionBase, IAuthPostgreSqlConnection
{
    private const string ConnectionStringConfigKey =
        "ConnectionStrings:AuthPostgreSql";

    private readonly IConfiguration _configuration;

    public AuthPostgreSqlConnection(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void ConfigureInternal(
        PostgreSqlConnectionOptions options)
    {
        string? connectionString =
            _configuration[ConnectionStringConfigKey];

        // Validacao obrigatoria — fail-fast
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString,
            nameof(connectionString)
        );

        options.WithConnectionString(connectionString);
    }
}
```

### Por Que Funciona

- **Fail-fast**: O erro aparece imediatamente ao tentar abrir a
  conexao, com mensagem clara indicando que a connection string
  esta ausente ou vazia.
- **Mensagem descritiva**: `ThrowIfNullOrWhiteSpace` inclui o nome
  do parametro na excecao, facilitando diagnostico.
- **Deteccao em build**: A regra de arquitetura garante que todo
  novo bounded context valida sua connection string.

## Consequencias

### Beneficios

- Erros de configuracao detectados imediatamente (fail-fast).
- Mensagens de erro claras e acionaveis.
- Zero risco de timeout ou erros cripticos por connection string
  faltante.

### Trade-offs

- **Nenhum significativo**: A validacao e uma unica linha de codigo
  que previne problemas significativos em runtime.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `PostgreSqlConnectionBase` e `PostgreSqlConnectionOptions` |

## Referencias no Codigo

- Connection de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Connections/AuthPostgreSqlConnection.cs`
- ADR relacionada: [IN-006 — Connection Interface Herda IConnection](../infrastructure/IN-006-connection-interface-herda-iconnection.md)
