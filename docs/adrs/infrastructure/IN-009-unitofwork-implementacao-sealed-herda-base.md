# IN-009: Implementacao de UnitOfWork Deve Ser Sealed e Herdar Base Class

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN009_UnitOfWorkImplementationSealedRule**, que verifica:

- Classes concretas que implementam uma marker interface de UnitOfWork
  (`*UnitOfWork` no namespace `*.UnitOfWork`) devem ser `sealed`.
- Devem herdar, direta ou indiretamente, da base class fornecida por
  `Bedrock.BuildingBlocks.Persistence.{Tech}` (ex:
  `PostgreSqlUnitOfWorkBase` para PostgreSQL).
- Devem implementar a marker interface do BC (ex:
  `IAuthPostgreSqlUnitOfWork`).

## Contexto

### O Problema (Analogia)

Imagine um caixa de banco. Cada caixa segue exatamente o mesmo
procedimento para abrir e fechar o cofre: autenticar, registrar no log,
abrir, executar operacoes, fechar, registrar encerramento. Se cada caixa
inventasse seu proprio procedimento, um poderia esquecer de fechar o
cofre, outro poderia esquecer de registrar no log. O procedimento e
padronizado e nao pode ser alterado pelo caixa individual.

### O Problema Tecnico

O UnitOfWork coordena o ciclo transacional completo:

1. Abrir conexao
2. Iniciar transacao
3. Executar operacoes
4. Commit ou rollback
5. Fechar conexao

A base class fornecida pelo building block da tecnologia (ex:
`PostgreSqlUnitOfWorkBase` para PostgreSQL) implementa esse ciclo
completo, incluindo tratamento de excecoes com rollback automatico e
logging com distributed tracing.

> **Nota**: A base class depende da tecnologia. Para PostgreSQL e
> `PostgreSqlUnitOfWorkBase`; para MongoDB seria `MongoDbUnitOfWorkBase`;
> para Redis, `RedisUnitOfWorkBase`. O ponto arquitetural e que cada
> tecnologia define sua propria base class em
> `Bedrock.BuildingBlocks.Persistence.{Tech}`, e a implementacao do BC
> deve herda-la.

Se a implementacao concreta permitir heranca, classes derivadas podem
quebrar o ciclo transacional — por exemplo, omitindo o rollback em caso
de excecao.

## Como Normalmente E Feito

### Abordagem Tradicional

Projetos com ORM delegam o UnitOfWork ao DbContext, que gerencia
transacoes implicitamente. Projetos sem ORM frequentemente criam
UnitOfWorks genericos com ciclos de vida inconsistentes:

```csharp
public class UnitOfWork : IUnitOfWork
{
    public virtual async Task CommitAsync()
    {
        await _transaction.CommitAsync();
    }
    // Subclasse pode sobrescrever CommitAsync e esquecer de tratar excecoes
}
```

### Por Que Nao Funciona Bem

- **Ciclo transacional inconsistente**: Cada implementacao pode inventar
  sua propria ordem de open/begin/commit/rollback/close.
- **Heranca fragil**: Subclasses podem sobrescrever etapas criticas
  (ex: rollback) sem perceber as consequencias.
- **Ausencia de rollback automatico**: Sem a garantia da base class,
  excecoes podem deixar transacoes abertas.

## A Decisao

### Nossa Abordagem

A implementacao de UnitOfWork segue tres regras:

1. **Ser `sealed`** — nao permite heranca.
2. **Herdar da base class tecnologica** (ex: `PostgreSqlUnitOfWorkBase`).
3. **Implementar a marker interface do BC** (ex:
   `IAuthPostgreSqlUnitOfWork`).

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/UnitOfWork/AuthPostgreSqlUnitOfWork.cs
public sealed class AuthPostgreSqlUnitOfWork
    : PostgreSqlUnitOfWorkBase,          // base class da tecnologia
      IAuthPostgreSqlUnitOfWork          // marker interface do BC
{
    private const string UnitOfWorkName = "AuthPostgreSqlUnitOfWork";

    public AuthPostgreSqlUnitOfWork(
        ILogger<AuthPostgreSqlUnitOfWork> logger,
        IAuthPostgreSqlConnection postgreSqlConnection
    ) : base(
        logger,
        UnitOfWorkName,
        postgreSqlConnection
    )
    {
    }
}
```

**Regras fundamentais:**

1. **`sealed`**: A classe nao pode ser estendida.
2. **Herda a base class da tecnologia**: O ciclo transacional completo
   e herdado — nao reimplementado. A base class e fornecida por
   `Bedrock.BuildingBlocks.Persistence.{Tech}` (ex:
   `PostgreSqlUnitOfWorkBase` para PostgreSQL).
3. **Implementa a marker do BC**: Garante resolucao DI correta.
4. **Recebe a marker de conexao do BC**: `IAuthPostgreSqlConnection` —
   nao `IPostgreSqlConnection` generico. Isso vincula UnitOfWork e
   conexao no mesmo BC.
5. **Define `UnitOfWorkName`**: Usado para logging e tracing.
6. **Sem logica adicional**: A classe concreta so conecta os pontos
   (nome, logger, conexao). Toda a logica transacional esta na base.

### Por Que Funciona Melhor

- **Ciclo transacional garantido**: `ExecuteAsync` da base sempre faz
  open → begin → handler → commit/rollback → close. Ninguem pode alterar
  essa sequencia.
- **Vinculo type-safe com conexao**: O construtor aceita apenas a conexao
  do mesmo BC — impossivel misturar.
- **Zero logica no BC**: A classe concreta e apenas um "conector" entre
  a base e o DI do BC. Toda a complexidade esta no framework.

## Consequencias

### Beneficios

- Ciclo transacional consistente e confiavel em todos os BCs.
- Rollback automatico em caso de excecao — sem possibilidade de bypass.
- Code agents geram UnitOfWorks corretos com 3 informacoes: nome do BC,
  logger e conexao.
- Logging com distributed tracing integrado via base class.

### Trade-offs (Com Perspectiva)

- **Menos flexibilidade**: `sealed` impede customizacao do ciclo
  transacional. Isso e intencional — o ciclo nao deve ser customizado
  por BC. Se houver necessidade de um ciclo diferente, ele deve ser
  implementado na base class do framework.
- **Classe "quase vazia"**: A classe concreta tem apenas construtor.
  Parece codigo desnecessario, mas e o ponto de entrada do DI e o vinculo
  type-safe com a conexao do BC.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Unit of Work** (Fowler, POEAA): Mantem uma lista de objetos afetados
  por uma transacao e coordena a escrita.
- **Template Method** (GoF): A base class da tecnologia (ex:
  `PostgreSqlUnitOfWorkBase.ExecuteAsync`) define o algoritmo
  transacional; a classe concreta do BC fornece os parametros.

### O Que o DDD Diz

> "Keep infrastructure concerns out of the domain."
>
> *Mantenha preocupacoes de infraestrutura fora do dominio.*

Evans (2003). O UnitOfWork sealed na camada `Infra.Data.{Tech}` garante
que o ciclo transacional nao vaza para camadas superiores.

### O Que o Clean Code Diz

> "A class should have only one reason to change."
>
> *Uma classe deve ter apenas um motivo para mudar.*

Martin (2008). A classe concreta de UnitOfWork muda apenas se o BC
precisar de uma nova conexao ou um novo nome. O ciclo transacional muda
na base class — Single Responsibility Principle.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre a base class da tecnologia (ex:
   PostgreSqlUnitOfWorkBase) e a implementacao do BC (ex:
   AuthPostgreSqlUnitOfWork)?"
2. "Por que o UnitOfWork recebe a marker interface de conexao e nao a
   interface generica?"
3. "Como o ExecuteAsync da base class garante rollback automatico?"

### Leitura Recomendada

- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 11 — Unit of Work
- GoF, *Design Patterns* (1994) — Template Method
- Robert C. Martin, *Clean Code* (2008), Cap. 10 — Classes

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.Abstractions | Define `IUnitOfWork` — o contrato raiz |
| Bedrock.BuildingBlocks.Persistence.{Tech} | Define a base class da tecnologia (ex: `PostgreSqlUnitOfWorkBase`) — implementa o ciclo transacional |

## Referencias no Codigo

- Implementacao de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/UnitOfWork/AuthPostgreSqlUnitOfWork.cs`
- Base class: `src/BuildingBlocks/Persistence.PostgreSql/UnitOfWork/PostgreSqlUnitOfWorkBase.cs`
- ADR relacionada: [IN-007 — Marker Interface de UnitOfWork](./IN-007-unitofwork-marker-interface-herda-iunitofwork.md)
- ADR relacionada: [IN-006 — Marker Interface de Conexao](./IN-006-conexao-marker-interface-herda-iconnection.md)
