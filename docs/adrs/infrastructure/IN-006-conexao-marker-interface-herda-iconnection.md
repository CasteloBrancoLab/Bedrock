# IN-006: Conexao do Bounded Context Deve Ter Marker Interface de Conexao

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN006_ConnectionMarkerInterfaceRule**, que verifica:

- Projetos `*.Infra.Data.{Tech}` devem declarar pelo menos uma interface
  no namespace `*.Connections.Interfaces`.
- Essa interface deve herdar, direta ou indiretamente, de
  `Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces.IConnection`.
- A interface deve ser um **marker** (corpo vazio — sem membros adicionais).

## Contexto

### O Problema (Analogia)

Imagine um predio comercial com 10 empresas. Cada empresa tem seu proprio
cartao de acesso, mas todos os cartoes funcionam no sistema central de
catracas do predio. O cartao da empresa A abre a porta da empresa A; o
da empresa B abre a da empresa B. Se todas as empresas usassem o mesmo
cartao generico "cartao do predio", qualquer funcionario abriria qualquer
porta — e ninguem saberia a qual empresa cada cartao pertence.

### O Problema Tecnico

Em um monorepo com multiplos bounded contexts, cada BC possui sua propria
conexao com o banco de dados (potencialmente bancos diferentes, connection
strings diferentes, lifecycles diferentes). O framework Bedrock define
`IConnection` como abstracacao base para conexoes
([Persistence.Abstractions](../../../src/BuildingBlocks/Persistence.Abstractions/)),
e cada tecnologia estende com sua interface especifica (ex:
`IPostgreSqlConnection`).

Se todos os BCs registrarem suas conexoes no container DI usando a mesma
interface `IPostgreSqlConnection`, o container nao consegue distinguir
qual conexao pertence a qual BC. O `AuthPostgreSqlUnitOfWork` pode
receber a conexao do `CatalogPostgreSqlConnection` por engano — um bug
silencioso e dificil de diagnosticar.

A solucao e cada BC declarar uma **marker interface** propria que estende
a interface tecnologica. O container DI resolve cada BC pela sua marker,
garantindo isolamento.

## Como Normalmente E Feito

### Abordagem Tradicional

A maioria dos projetos registra conexoes diretamente com a interface
da tecnologia ou com `DbContext`:

```csharp
// Registro direto — todos os BCs usam a mesma interface
services.AddScoped<IPostgreSqlConnection, AuthConnection>();
services.AddScoped<IPostgreSqlConnection, CatalogConnection>();
// Qual IPostgreSqlConnection o container resolve? Depende da ordem.
```

Ou com DbContext nomeado:

```csharp
services.AddDbContext<AuthDbContext>(o => o.UseNpgsql(authConnStr));
services.AddDbContext<CatalogDbContext>(o => o.UseNpgsql(catalogConnStr));
```

### Por Que Nao Funciona Bem

- **Resolucao ambigua**: Se dois BCs registram a mesma interface, o
  container DI resolve o ultimo registrado — ou lanca excecao, dependendo
  do framework.
- **Sem type safety**: Nao ha garantia em tempo de compilacao de que o
  `AuthUnitOfWork` recebe a conexao do Auth e nao a do Catalog.
- **Code agents confusos**: LLMs nao tem indicacao clara de que cada BC
  precisa de sua propria interface de conexao — e reutilizam a generica.

## A Decisao

### Nossa Abordagem

Cada bounded context que persiste dados em `Infra.Data.{Tech}` deve
declarar uma **marker interface** de conexao no namespace
`*.Connections.Interfaces`:

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/Connections/Interfaces/IAuthPostgreSqlConnection.cs
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;

public interface IAuthPostgreSqlConnection
    : IPostgreSqlConnection     // que por sua vez herda IConnection
{
    // Corpo vazio — marker interface
}
```

**Cadeia de heranca (exemplo com PostgreSQL):**

```
IConnection (Persistence.Abstractions)           ← raiz — a regra valida esta heranca
  └── IPostgreSqlConnection (Persistence.PostgreSql)   ← interface da tecnologia
        └── IAuthPostgreSqlConnection (ShopDemo.Auth — marker do BC)
```

> **Nota**: A cadeia intermediaria depende da tecnologia. Para MongoDB
> seria `IMongoDbConnection`; para Redis, `IRedisConnection`. O ponto
> arquitetural e que a marker do BC deve herdar, direta ou indiretamente,
> de `IConnection` — a interface raiz definida em
> `Persistence.Abstractions`.

**Regras fundamentais:**

1. **Uma marker interface por BC por tecnologia**: Cada BC declara a sua
   propria (ex: `IAuthPostgreSqlConnection`, `ICatalogPostgreSqlConnection`).
2. **Corpo vazio**: A marker nao adiciona membros — apenas distingue o BC
   no container DI.
3. **Herda transitivamente de `IConnection`**: Deve herdar, direta ou
   indiretamente, da interface raiz `IConnection` definida em
   `Persistence.Abstractions`. A heranca pode passar pela interface da
   tecnologia (ex: `IPostgreSqlConnection`) — o importante e que
   `IConnection` esteja na cadeia.
4. **Namespace canonico**: Reside em `*.Connections.Interfaces`.
5. **Nomenclatura**: `I{BoundedContext}{Tech}Connection`.

**Registro no DI:**

```csharp
// Cada BC registra com sua propria marker interface
services.AddSingleton<IAuthPostgreSqlConnection, AuthPostgreSqlConnection>();
services.AddSingleton<ICatalogPostgreSqlConnection, CatalogPostgreSqlConnection>();

// O UnitOfWork do Auth recebe IAuthPostgreSqlConnection — sem ambiguidade
public class AuthPostgreSqlUnitOfWork(IAuthPostgreSqlConnection connection) { }
```

### Por Que Funciona Melhor

- **Type safety em tempo de compilacao**: O construtor do
  `AuthPostgreSqlUnitOfWork` aceita apenas `IAuthPostgreSqlConnection`.
  Passar a conexao errada e um erro de compilacao.
- **Isolamento de BCs**: Cada BC tem sua propria "porta de entrada" para
  o banco, identificada de forma unica pelo tipo.
- **Previsibilidade para code agents**: A regra e simples — "crie uma
  marker interface que herda de `IPostgreSqlConnection`".
- **Consistencia**: Todos os BCs seguem o mesmo padrao, facilitando
  navegacao e compreensao.

## Consequencias

### Beneficios

- Resolucao DI sem ambiguidade entre multiplos bounded contexts.
- Erros de wiring detectados em tempo de compilacao, nao em runtime.
- Code agents geram conexoes com o padrao correto sem inventar variacoes.
- UnitOfWork de cada BC recebe exatamente a conexao que lhe pertence.

### Trade-offs (Com Perspectiva)

- **Uma interface "vazia" por BC**: Parece codigo desnecessario — uma
  interface sem membros. Na pratica, e uma unica linha que previne uma
  classe inteira de bugs de DI. O custo e negligivel, o beneficio e
  concreto.
- **Mais tipos no container**: Cada BC adiciona uma interface ao DI. O
  overhead de resolucao e zero — o container indexa por tipo em O(1).

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Marker Interface** (Bloch, Effective Java): Interfaces vazias que
  servem para classificar tipos sem adicionar comportamento. No Java,
  `Serializable` e o exemplo classico. Aqui, a marker classifica a
  conexao por bounded context.
- **Separated Interface** (Fowler, POEAA): A interface vive no projeto
  do BC, separada da implementacao base no framework.

### O Que o DDD Diz

> "Each Bounded Context should have its own infrastructure."
>
> *Cada Bounded Context deve ter sua propria infraestrutura.*

Vernon (2013) enfatiza que BCs sao autonomos. Uma marker interface por
BC materializa essa autonomia na camada de conexao.

### O Que o Clean Architecture Diz

> "Source code dependencies must point only inward."
>
> *Dependencias de codigo-fonte devem apontar apenas para dentro.*

Robert C. Martin (2017). A marker interface e definida no BC
(`Infra.Data.{Tech}`), apontando para dentro (framework). A
implementacao concreta depende da marker, nunca o contrario.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Por que cada bounded context precisa de sua propria interface de
   conexao no Bedrock?"
2. "O que e uma marker interface e quando ela deve ser usada?"
3. "Como o container DI resolve conexoes de diferentes BCs sem
   ambiguidade?"

### Leitura Recomendada

- Joshua Bloch, *Effective Java* (2018), Item 41 — Use marker interfaces
  to define types
- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 18 — Separated Interface
- Vaughn Vernon, *Implementing Domain-Driven Design* (2013), Cap. 4 —
  Architecture

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.Abstractions | Define `IConnection` — a raiz da cadeia de heranca |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `IPostgreSqlConnection` e `PostgreSqlConnectionBase` — a camada tecnologica |

## Referencias no Codigo

- Marker interface de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Connections/Interfaces/IAuthPostgreSqlConnection.cs`
- Implementacao de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Connections/AuthPostgreSqlConnection.cs`
- Interface base: `src/BuildingBlocks/Persistence.Abstractions/Connections/Interfaces/IConnection.cs`
- ADR relacionada: [IN-001 — Camadas Canonicas](./IN-001-camadas-canonicas-bounded-context.md)
