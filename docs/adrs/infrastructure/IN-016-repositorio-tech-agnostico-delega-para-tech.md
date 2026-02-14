# IN-016: Repositorio Tech-Agnostico Delega Para Repositorio Tecnologico

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN016_TechAgnosticRepositoryDelegatesRule**, que verifica:

- Classes em `*.Infra.Data` no namespace `*.Repositories` que herdam
  de `RepositoryBase<TAggregateRoot>` devem:
  - Ser `sealed`.
  - Implementar a interface de repositorio do Domain (ex:
    `IUserRepository`).
  - Ter como dependencia no construtor o repositorio tecnologico
    correspondente (ex: `IUserPostgreSqlRepository`).

## Contexto

### O Problema (Analogia)

Imagine uma recepcionista de hotel. O hospede pede "preciso de um
taxi para o aeroporto". A recepcionista nao dirige o taxi — ela liga
para a empresa de taxi, confirma o horario e informa o hospede. Se
a empresa de taxi mudar (de UberBlack para uma cooperativa local), o
hospede nao percebe — a recepcionista continua sendo o ponto de contato
unico. Se algo der errado (taxi nao aparece), a recepcionista trata
o problema sem expor o hospede aos detalhes.

### O Problema Tecnico

A arquitetura do Bedrock define tres camadas de repositorio
([IN-005](./IN-005-infra-data-facade-persistencia.md)):

1. **Domain**: `IUserRepository` — contrato de negocio.
2. **Infra.Data**: `UserRepository` — facade que implementa o contrato
   e delega para a tecnologia.
3. **Infra.Data.{Tech}**: `IUserPostgreSqlRepository` — implementacao
   tecnologica.

A camada `Infra.Data` e crucial como **boundary de excecoes**: toda
excecao de infraestrutura (timeout, connection refused, constraint
violation) e capturada aqui e transformada em resposta segura (null,
false) com logging para distributed tracing.

Se a camada `Infra.Data` nao existir e o Domain referenciar diretamente
`Infra.Data.PostgreSql`, excecoes de infraestrutura vazam para o dominio
— violando o principio de que o dominio nao conhece detalhes tecnicos.

## Como Normalmente E Feito

### Abordagem Tradicional

O service de aplicacao chama o repositorio tecnologico diretamente:

```csharp
public class AuthService
{
    private readonly IUserPostgreSqlRepository _repo; // ← tecnologico

    public async Task<User?> Authenticate(string email)
    {
        try
        {
            return await _repo.GetByEmailAsync(email);
        }
        catch (NpgsqlException ex) // excecao de infra no service
        {
            _logger.LogError(ex, "Erro");
            return null;
        }
    }
}
```

### Por Que Nao Funciona Bem

- **Excecoes de infra no service**: `NpgsqlException` vazando para
  a Application.
- **Catch duplicado**: Cada service que usa o repositorio precisa
  tratar excecoes de banco.
- **Sem distributed tracing**: O logging nao inclui correlation ID,
  tenant, ou execution origin.
- **Acoplamento tecnologico**: Application conhece PostgreSQL.

## A Decisao

### Nossa Abordagem

O repositorio em `Infra.Data` herda de `RepositoryBase<T>`, implementa
a interface do Domain, e delega toda operacao para o repositorio
tecnologico com tratamento de excecoes:

```csharp
// ShopDemo.Auth.Infra.Data/Repositories/UserRepository.cs
public sealed class UserRepository
    : RepositoryBase<User>,       // base class do framework
      IUserRepository             // interface do Domain
{
    private readonly IUserPostgreSqlRepository _postgreSqlRepository;

    public UserRepository(
        ILogger<UserRepository> logger,
        IUserPostgreSqlRepository postgreSqlRepository
    ) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    // Metodo especifico — wrappeia com exception handling
    public async Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByEmailAsync(
                executionContext, email, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogExceptionForDistributedTracing(
                executionContext, ex,
                "An error occurred while getting user by email.");
            return null;
        }
    }

    // Template Method — delega para o tech repository
    protected override Task<User?> GetByIdInternalAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.GetByIdAsync(
            executionContext, id, cancellationToken);
    }

    protected override Task<bool> RegisterNewInternalAsync(
        ExecutionContext executionContext,
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        return _postgreSqlRepository.RegisterNewAsync(
            executionContext, aggregateRoot, cancellationToken);
    }

    // ... demais metodos delegam para _postgreSqlRepository
}
```

**Regras fundamentais:**

1. **`sealed`**: A classe nao pode ser estendida.
2. **Herda `RepositoryBase<TAggregateRoot>`**: Herda o exception handling
   padrao para metodos base (GetByIdAsync, ExistsAsync, RegisterNewAsync,
   EnumerateAllAsync).
3. **Implementa interface do Domain**: `IUserRepository` — o contrato que
   a Application conhece.
4. **Recebe repositorio tecnologico via construtor**: Via marker interface
   (`IUserPostgreSqlRepository`).
5. **Metodos especificos wrapeiam com try-catch**: Logging com
   distributed tracing + retorno seguro.
6. **Metodos base usam Template Method**: `GetByIdInternalAsync` etc.
   delegam; `RepositoryBase` ja trata excecoes.

**Fluxo de excecoes:**

```
Application chama IUserRepository.GetByEmailAsync(...)
  └── UserRepository (Infra.Data)
        ├── try: delega para IUserPostgreSqlRepository
        │     └── UserPostgreSqlRepository (Infra.Data.PostgreSql)
        │           └── IUserDataModelRepository (SQL real)
        └── catch: LogExceptionForDistributedTracing + return null
```

Nenhuma excecao de infraestrutura chega a Application.

### Por Que Funciona Melhor

- **Boundary de excecoes**: Toda excecao de infra e capturada e logada
  com distributed tracing antes de retornar para a Application.
- **Zero catch na Application**: Application recebe null/false — nunca
  NpgsqlException, TimeoutException, etc.
- **Logging rico**: CorrelationId, TenantCode, ExecutionOrigin — tudo
  extraido do ExecutionContext automaticamente.
- **Troca de tecnologia transparente**: Se trocar de PostgreSQL para
  MongoDB, so muda o repositorio tecnologico — `UserRepository` em
  `Infra.Data` continua intacto.

## Consequencias

### Beneficios

- Application nunca ve excecoes de infraestrutura.
- Logging padronizado com distributed tracing em todos os repositorios.
- Troca de tecnologia nao impacta Domain nem Application.
- Code agents geram repositorios tech-agnosticos seguindo o padrao
  "delegar e tratar excecoes".

### Trade-offs (Com Perspectiva)

- **Camada extra de indirection**: Cada chamada passa por UserRepository
  antes de chegar ao PostgreSQL. O custo de runtime e uma chamada de
  metodo virtual — nanossegundos, negligivel.
- **Boilerplate de delegacao**: Cada metodo e um try-catch que delega.
  Na pratica, o padrao e identico em todos os metodos — previsivel e
  facil de gerar.
- **Erros "silenciados"**: Excecoes viram null/false em vez de propagar.
  Isso e intencional — excecoes sao logadas com distributed tracing e
  adicionadas ao ExecutionContext. O chamador decide como lidar com
  null/false.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Facade Pattern** (GoF): `UserRepository` em `Infra.Data` e uma
  facade que simplifica o acesso a multiplas tecnologias de persistencia.
- **Proxy Pattern** (GoF): Intercepta chamadas ao repositorio real para
  adicionar exception handling e logging.
- **Template Method** (GoF): `RepositoryBase` define o algoritmo
  (try-catch-log); a classe concreta preenche o passo de delegacao.

### O Que o DDD Diz

> "The infrastructure layer should shield the domain from technical
> details."
>
> *A camada de infraestrutura deve proteger o dominio de detalhes
> tecnicos.*

Evans (2003). O repositorio tech-agnostico e o escudo: absorve excecoes,
loga com contexto, e retorna respostas seguras para o dominio.

### O Que o Clean Architecture Diz

> "Don't let outer layer details leak into inner layers."
>
> *Nao deixe detalhes de camadas externas vazarem para camadas internas.*

Robert C. Martin (2017). `NpgsqlException` e um detalhe da camada mais
externa (PostgreSQL). O repositorio em `Infra.Data` impede que esse
detalhe alcance Application ou Domain.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre UserRepository (Infra.Data) e
   UserPostgreSqlRepository (Infra.Data.PostgreSql)?"
2. "Como o RepositoryBase trata excecoes nos metodos Template Method?"
3. "Por que retornar null em vez de propagar excecoes?"
4. "Como adicionar cache (Redis) ao fluxo de persistencia sem alterar
   o Domain?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Facade Pattern, Proxy Pattern
- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Repositories
- Robert C. Martin, *Clean Architecture* (2017), Cap. 22 — The Clean
  Architecture

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Data | Define `RepositoryBase<T>` — base class com exception handling e Template Method |
| Bedrock.BuildingBlocks.Observability | Define `LogExceptionForDistributedTracing` — logging rico com contexto |

## Referencias no Codigo

- Repositorio facade de exemplo: `src/ShopDemo/Auth/Infra.Data/Repositories/UserRepository.cs`
- Interface de dominio: `src/ShopDemo/Auth/Domain/Repositories/Interfaces/IUserRepository.cs`
- Repositorio tecnologico: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs`
- ADR relacionada: [IN-005 — Infra.Data Facade de Persistencia](./IN-005-infra-data-facade-persistencia.md)
- ADR relacionada: [IN-012 — Repositorio Tech Implementa IRepository](./IN-012-repositorio-tech-implementa-irepository.md)
