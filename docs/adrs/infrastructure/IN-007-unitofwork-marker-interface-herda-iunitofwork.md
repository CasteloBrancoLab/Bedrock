# IN-007: Bounded Context Deve Ter Marker Interface de UnitOfWork

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN007_UnitOfWorkMarkerInterfaceRule**, que verifica:

- Projetos `*.Infra.Data.{Tech}` devem declarar pelo menos uma interface
  no namespace `*.UnitOfWork.Interfaces`.
- Essa interface deve herdar, direta ou indiretamente, de
  `Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces.IUnitOfWork`.
- A interface deve ser um **marker** (corpo vazio — sem membros adicionais).

## Contexto

### O Problema (Analogia)

Imagine um banco com varias agencias. Cada agencia tem seu proprio cofre.
Quando um cliente faz uma transferencia, o caixa abre o cofre da agencia
correta, executa a operacao e fecha o cofre. Se todas as agencias
compartilhassem o mesmo cofre sem rotulos, o caixa nao saberia qual cofre
abrir — e poderia misturar operacoes de agencias diferentes na mesma
sessao.

### O Problema Tecnico

O UnitOfWork gerencia o ciclo de vida de uma transacao: abre conexao,
inicia transacao, executa operacoes, faz commit ou rollback, e fecha a
conexao. Em um monorepo com multiplos bounded contexts, cada BC pode ter
seu proprio banco de dados, seu proprio schema ou ate sua propria
instancia de servidor.

Se todos os BCs usarem a interface tecnologica diretamente no container
DI (ex: `IPostgreSqlUnitOfWork` para BCs que usam PostgreSQL), o mesmo
problema da [IN-006](./IN-006-conexao-marker-interface-herda-iconnection.md)
ocorre: resolucao ambigua. O `UserDataModelRepository` do Auth pode
receber o UnitOfWork do Catalog — executando queries no banco errado.

A marker interface de UnitOfWork complementa a marker de conexao
([IN-006](./IN-006-conexao-marker-interface-herda-iconnection.md)),
garantindo que toda a cadeia transacional esta vinculada ao BC correto.

## Como Normalmente E Feito

### Abordagem Tradicional

A maioria dos projetos usa o DbContext do ORM como UnitOfWork implicito:

```csharp
// DbContext como UnitOfWork implicito
services.AddDbContext<AuthDbContext>(o => o.UseNpgsql(connStr));

public class AuthService
{
    private readonly AuthDbContext _db;

    public async Task RegisterUser(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(); // commit implicito
    }
}
```

Ou com um UnitOfWork generico:

```csharp
services.AddScoped<IUnitOfWork, PostgreSqlUnitOfWork>();
// Todos os BCs compartilham a mesma interface
```

### Por Que Nao Funciona Bem

- **DbContext como UnitOfWork**: Mistura responsabilidades — o DbContext
  e ao mesmo tempo change tracker, query builder e unit of work. Trocar
  de tecnologia exige reescrever toda a camada de dados.
- **UnitOfWork generico**: Sem marker interface, o container DI nao
  distingue entre UnitOfWorks de diferentes BCs.
- **Ausencia de contrato explicito**: O ciclo de vida transacional
  (open/begin/commit/rollback/close) fica implicito dentro do ORM, sem
  visibilidade para quem consome.

## A Decisao

### Nossa Abordagem

Cada bounded context que persiste dados deve declarar uma **marker
interface** de UnitOfWork no namespace `*.UnitOfWork.Interfaces`:

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/UnitOfWork/Interfaces/IAuthPostgreSqlUnitOfWork.cs
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

public interface IAuthPostgreSqlUnitOfWork
    : IPostgreSqlUnitOfWork     // que por sua vez herda IUnitOfWork
{
    // Corpo vazio — marker interface
}
```

**Cadeia de heranca (exemplo com PostgreSQL):**

```
IUnitOfWork (Persistence.Abstractions)           ← raiz — a regra valida esta heranca
  └── IPostgreSqlUnitOfWork (Persistence.PostgreSql)   ← interface da tecnologia
        └── IAuthPostgreSqlUnitOfWork (ShopDemo.Auth — marker do BC)
```

> **Nota**: A cadeia intermediaria depende da tecnologia. Para MongoDB
> seria `IMongoDbUnitOfWork`; para Redis, `IRedisUnitOfWork`. O ponto
> arquitetural e que a marker do BC deve herdar, direta ou indiretamente,
> de `IUnitOfWork` — a interface raiz definida em
> `Persistence.Abstractions`.

**Regras fundamentais:**

1. **Uma marker interface por BC por tecnologia**: Cada BC declara a sua.
2. **Corpo vazio**: Apenas distingue o BC no container DI.
3. **Herda transitivamente de `IUnitOfWork`**: Deve herdar, direta ou
   indiretamente, da interface raiz `IUnitOfWork` definida em
   `Persistence.Abstractions`. A heranca pode passar pela interface da
   tecnologia (ex: `IPostgreSqlUnitOfWork`) — o importante e que
   `IUnitOfWork` esteja na cadeia.
4. **Namespace canonico**: Reside em `*.UnitOfWork.Interfaces`.
5. **Nomenclatura**: `I{BoundedContext}{Tech}UnitOfWork`.

**Vinculo com a conexao (exemplo PostgreSQL):**

```csharp
// O UnitOfWork do Auth recebe a conexao do Auth — type-safe
public sealed class AuthPostgreSqlUnitOfWork
    : PostgreSqlUnitOfWorkBase,       // base class da tecnologia (varia por tech)
      IAuthPostgreSqlUnitOfWork       // marker interface do BC
{
    public AuthPostgreSqlUnitOfWork(
        ILogger<AuthPostgreSqlUnitOfWork> logger,
        IAuthPostgreSqlConnection postgreSqlConnection  // conexao do Auth
    ) : base(logger, "AuthPostgreSqlUnitOfWork", postgreSqlConnection)
    {
    }
}
```

O UnitOfWork recebe a marker interface de conexao do mesmo BC
([IN-006](./IN-006-conexao-marker-interface-herda-iconnection.md)),
formando uma cadeia type-safe: Connection → UnitOfWork → Repositories.
A implementacao acima e especifica de PostgreSQL; para outra tecnologia,
a base class e a conexao mudariam, mas a marker interface e o vinculo
com o BC permanecem.

### Por Que Funciona Melhor

- **Cadeia transacional type-safe**: Connection, UnitOfWork e
  Repositories de cada BC sao vinculados em tempo de compilacao.
- **Isolamento total**: Operacoes do Auth nunca executam no banco do
  Catalog — o compilador garante.
- **Ciclo de vida explicito**: O contrato `IUnitOfWork` torna visivel
  cada etapa da transacao (open, begin, commit, rollback, close).
- **Previsibilidade**: Code agents seguem o padrao "marker interface +
  heranca da base tecnologica" sem inventar variacoes.

## Consequencias

### Beneficios

- Transacoes isoladas por bounded context com garantia em tempo de
  compilacao.
- Repositories recebem o UnitOfWork correto via DI sem ambiguidade.
- Ciclo de vida transacional explicito e testavel.
- Padrao consistente em todos os BCs do monorepo.

### Trade-offs (Com Perspectiva)

- **Interface vazia por BC**: O mesmo trade-off da IN-006 — uma linha
  de codigo que previne bugs de wiring. Custo negligivel.
- **Acoplamento com a tecnologia no nome**: `IAuthPostgreSqlUnitOfWork`
  menciona PostgreSql. Isso e intencional — a marker vive na camada
  tecnologica (`Infra.Data.PostgreSql`), nao no dominio.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Unit of Work** (Fowler, POEAA): Mantem uma lista de objetos afetados
  por uma transacao e coordena a escrita de mudancas e a resolucao de
  problemas de concorrencia.
- **Marker Interface** (Bloch, Effective Java): Interface vazia usada
  para classificacao de tipos.

### O Que o DDD Diz

> "A transaction should be scoped to a single Aggregate."
>
> *Uma transacao deve ser limitada a um unico Aggregate.*

Evans (2003). O UnitOfWork por BC garante que transacoes sao limitadas
ao escopo correto — sem misturar aggregates de BCs diferentes.

### O Que o Clean Architecture Diz

> "The database is a detail."
>
> *O banco de dados e um detalhe.*

Robert C. Martin (2017). A marker interface de UnitOfWork vive na camada
tecnologica — o dominio nunca a ve. O detalhe transacional fica contido
na camada mais externa.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre IUnitOfWork e IPostgreSqlUnitOfWork no
   Bedrock?"
2. "Por que cada bounded context precisa de seu proprio UnitOfWork?"
3. "Como a cadeia Connection → UnitOfWork → Repository garante
   isolamento transacional?"

### Leitura Recomendada

- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 11 — Unit of Work
- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Aggregates and
  Transactions
- Vaughn Vernon, *Implementing Domain-Driven Design* (2013), Cap. 10 —
  Aggregates

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.Abstractions | Define `IUnitOfWork` — a raiz da cadeia de heranca |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `IPostgreSqlUnitOfWork` e `PostgreSqlUnitOfWorkBase` — a camada tecnologica |

## Referencias no Codigo

- Marker interface de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/UnitOfWork/Interfaces/IAuthPostgreSqlUnitOfWork.cs`
- Implementacao de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/UnitOfWork/AuthPostgreSqlUnitOfWork.cs`
- Interface base: `src/BuildingBlocks/Persistence.Abstractions/UnitOfWork/Interfaces/IUnitOfWork.cs`
- ADR relacionada: [IN-006 — Marker Interface de Conexao](./IN-006-conexao-marker-interface-herda-iconnection.md)
