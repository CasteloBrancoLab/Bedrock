# IN-008: Implementacao de Conexao Deve Ser Sealed e Herdar Base Class

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN008_ConnectionImplementationSealedRule**, que verifica:

- Classes concretas que implementam uma marker interface de conexao
  (`*Connection` no namespace `*.Connections`) devem ser `sealed`.
- Devem herdar, direta ou indiretamente, da base class fornecida por
  `Bedrock.BuildingBlocks.Persistence.{Tech}` (ex:
  `PostgreSqlConnectionBase` para PostgreSQL).
- Devem implementar a marker interface do BC (ex:
  `IAuthPostgreSqlConnection`).

## Contexto

### O Problema (Analogia)

Imagine uma rede de franquias. Cada franquia tem sua propria fechadura,
mas todas as fechaduras sao do mesmo modelo e fabricante — garantindo
compatibilidade com o sistema de seguranca central. Se uma franquia
instalar uma fechadura de modelo diferente, o sistema central nao
consegue monitorar. E se a fechadura for "extensivel" (qualquer um pode
modificar o mecanismo interno), a seguranca fica comprometida.

### O Problema Tecnico

A conexao com o banco de dados e um ponto critico de infraestrutura.
Ela gerencia recursos caros (sockets TCP, connection pools, SSL
handshakes). A base class fornecida pelo building block da tecnologia
(ex: `PostgreSqlConnectionBase` para PostgreSQL) implementa:

- **Double-check locking** para abertura thread-safe.
- **Interlocked.Exchange** para troca atomica de conexoes.
- **Dispose pattern** para liberacao garantida de recursos.

> **Nota**: A base class depende da tecnologia. Para PostgreSQL e
> `PostgreSqlConnectionBase`; para MongoDB seria `MongoDbConnectionBase`;
> para Redis, `RedisConnectionBase`. O ponto arquitetural e que cada
> tecnologia define sua propria base class em
> `Bedrock.BuildingBlocks.Persistence.{Tech}`, e a implementacao do BC
> deve herda-la.

Se a implementacao concreta permitir heranca (nao for `sealed`), classes
derivadas podem:

- Sobrescrever metodos de lifecycle, quebrando o padrao de dispose.
- Introduzir estado adicional que nao e protegido pelo lock.
- Alterar o comportamento de open/close de formas inesperadas.

O `sealed` garante que a implementacao e final — ninguem estende, ninguem
quebra os invariantes de thread-safety.

## Como Normalmente E Feito

### Abordagem Tradicional

A maioria dos projetos cria conexoes como classes abertas ou usa
connection strings diretamente:

```csharp
// Classe aberta — qualquer um pode herdar e sobrescrever
public class AuthConnection
{
    private readonly string _connectionString;

    public AuthConnection(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Auth");
    }

    public virtual NpgsqlConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
}

// Heranca perigosa
public class AuthConnectionWithLogging : AuthConnection
{
    public override NpgsqlConnection CreateConnection()
    {
        // Esqueceu de chamar base, ou alterou comportamento
        var conn = new NpgsqlConnection("...");
        Console.WriteLine("Conexao criada");
        return conn;
    }
}
```

### Por Que Nao Funciona Bem

- **Heranca fragil**: Classes derivadas podem quebrar invariantes da
  base sem que o compilador detecte.
- **Estado nao protegido**: Campos adicionados em subclasses nao sao
  cobertos pelo lock da base.
- **Sem padrao consistente**: Cada implementacao inventa sua propria
  forma de gerenciar o lifecycle da conexao.

## A Decisao

### Nossa Abordagem

A implementacao de conexao deve seguir tres regras:

1. **Ser `sealed`** — nao permite heranca.
2. **Herdar da base class tecnologica** (ex: `PostgreSqlConnectionBase`).
3. **Implementar a marker interface do BC** (ex:
   `IAuthPostgreSqlConnection`).

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/Connections/AuthPostgreSqlConnection.cs
public sealed class AuthPostgreSqlConnection
    : PostgreSqlConnectionBase,          // base class da tecnologia
      IAuthPostgreSqlConnection          // marker interface do BC
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

        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString,
            nameof(connectionString));

        options.WithConnectionString(connectionString);
    }
}
```

**Regras fundamentais:**

1. **`sealed`**: A classe nao pode ser estendida.
2. **Herda a base class da tecnologia**: O lifecycle da conexao (open,
   close, dispose, thread-safety) e herdado da base — nao reimplementado.
   A base class e fornecida por `Bedrock.BuildingBlocks.Persistence.{Tech}`
   (ex: `PostgreSqlConnectionBase` para PostgreSQL).
3. **Implementa a marker do BC**: Garante resolucao DI correta.
4. **Unico ponto de customizacao**: `ConfigureInternal` — o Template
   Method que a base chama para obter a connection string.
5. **Dependencia minima**: Apenas `IConfiguration` no construtor.
6. **Connection string key padrao**: `ConnectionStrings:{BC}{Tech}`.

### Por Que Funciona Melhor

- **Thread-safety garantida**: A base class implementa double-check
  locking e atomic swap. A classe `sealed` nao pode quebrar esses
  invariantes.
- **Template Method**: A classe concreta so decide a connection string;
  todo o lifecycle e responsabilidade da base.
- **Previsibilidade**: Code agents sabem exatamente o que criar — classe
  sealed, herda base, override de `ConfigureInternal`.
- **Troca de banco isolada**: Para trocar de PostgreSQL para MySQL, basta
  criar um novo `AuthMySqlConnection` que herda de `MySqlConnectionBase`.

## Consequencias

### Beneficios

- Implementacoes de conexao sao imutaveis (sealed) e consistentes.
- Thread-safety garantida pela base class sem reimplementacao.
- Template Method Pattern limita a customizacao ao estritamente necessario.
- Code agents geram conexoes corretas com apenas 3 informacoes: nome do
  BC, tecnologia e chave da connection string.

### Trade-offs (Com Perspectiva)

- **Menos flexibilidade**: `sealed` impede cenarios de extensao. Na
  pratica, extensoes de conexao sao extremamente raras — e quando
  necessarias, devem ser feitas na base class do framework, nao no BC.
- **Dependencia de `IConfiguration`**: A connection string vem do
  `IConfiguration`. Isso e intencional — centraliza o acesso a
  configuracao em um unico ponto.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Template Method** (GoF): A base class da tecnologia (ex:
  `PostgreSqlConnectionBase`) define o algoritmo de lifecycle; a classe
  concreta do BC (ex: `AuthPostgreSqlConnection`) preenche o passo de
  configuracao.
- **Favor Composition over Inheritance** / **Design for Inheritance or
  Prohibit It** (Bloch, Effective Java): Classes nao projetadas para
  heranca devem ser `sealed` para evitar quebras de contrato.

### O Que o DDD Diz

> "Infrastructure should be a stable, reliable foundation."
>
> *Infraestrutura deve ser uma fundacao estavel e confiavel.*

Evans (2003). Conexoes sealed com lifecycle herdado da base garantem
estabilidade — nao ha como um BC "inventar" um lifecycle alternativo
que quebre transacoes.

### O Que o Clean Code Diz

> "Prefer final classes."
>
> *Prefira classes finais.*

Martin (2008). Classes sealed (final) comunicam intencao: "esta classe
esta completa, nao deve ser estendida". Isso reduz a carga cognitiva
de quem le o codigo.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Por que a conexao deve ser sealed e nao aberta a heranca?"
2. "Como o Template Method Pattern se aplica a conexoes de banco?"
3. "Qual o risco de permitir heranca em classes de infraestrutura?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Template Method
- Joshua Bloch, *Effective Java* (2018), Item 19 — Design and document
  for inheritance or else prohibit it
- Robert C. Martin, *Clean Code* (2008), Cap. 10 — Classes

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.Abstractions | Define `IConnection` — o contrato raiz |
| Bedrock.BuildingBlocks.Persistence.{Tech} | Define a base class da tecnologia (ex: `PostgreSqlConnectionBase`) — implementa lifecycle e thread-safety |

## Referencias no Codigo

- Implementacao de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Connections/AuthPostgreSqlConnection.cs`
- Base class: `src/BuildingBlocks/Persistence.PostgreSql/Connections/PostgreSqlConnectionBase.cs`
- ADR relacionada: [IN-006 — Marker Interface de Conexao](./IN-006-conexao-marker-interface-herda-iconnection.md)
