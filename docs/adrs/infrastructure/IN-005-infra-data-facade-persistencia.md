# IN-005: Infra.Data Atua como Facade de Persistencia

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine uma agencia de viagens. O cliente diz "quero ir para Paris na
segunda semana de julho". Ele nao diz "reserve o voo TAM 3421, o hotel
Marriott quarto 502, e o transfer da empresa XYZ". A agencia recebe o
pedido de alto nivel e decide internamente qual companhia aerea, qual
hotel e qual transfer usar. Se o cliente tivesse que lidar com cada
fornecedor diretamente, a complexidade seria enorme. A agencia é uma
fachada que simplifica a interacao.

### O Problema Tecnico

Com entidades isoladas ([IN-002](./IN-002-domain-entities-projeto-separado.md)),
contratos no Domain ([IN-003](./IN-003-domain-projeto-separado.md)) e
DataModels nas camadas tecnologicas ([IN-004](./IN-004-modelo-dados-detalhe-implementacao.md)),
surge a questao: quem implementa as interfaces de repositorio definidas
no Domain?

Se `Infra.Data.PostgreSql` implementar diretamente `IUserRepository`,
o Domain passa a depender de uma tecnologia especifica. Se a aplicacao
precisar de cache-aside (Redis + PostgreSQL), quem orquestra a
estrategia? Se uma operacao de negocio precisar buscar em dois bancos
diferentes, quem decide a ordem?

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos implementa repositorios diretamente na camada
tecnologica com metodos CRUD genericos:

```csharp
// Implementacao diretamente na camada PostgreSQL
public class UserRepository : IUserRepository
{
    public Task<User> GetById(Guid id) { ... }
    public Task Insert(User user) { ... }
    public Task Update(User user) { ... }
    public Task Delete(Guid id) { ... }
}
```

### Por Que Nao Funciona Bem

- Operacoes sao CRUD generico — nao expressam intencao de negocio.
- `GetById` nao diz se é para autenticacao, para exibicao de perfil ou
  para auditoria — cada caso pode ter necessidades diferentes de dados.
- Nao ha lugar natural para orquestrar multiplas tecnologias (cache +
  banco, leitura de um banco + escrita em outro).
- O Domain depende diretamente da camada tecnologica, violando o
  Dependency Inversion Principle.
- Trocar ou adicionar uma tecnologia exige mudar quem implementa a
  interface — impacto em toda a aplicacao.

## A Decisao

### Nossa Abordagem

`{BC}.Infra.Data` atua como uma Facade de persistencia que:

1. **Implementa as interfaces de repositorio** definidas no Domain.
2. **Orquestra as tecnologias especificas** (PostgreSQL, Redis, etc.).
3. **Expoe operacoes de negocio** (high-level), nao CRUD (low-level).

```
samples/ShopDemo/Auth/
  Domain/
    Repositories/
      Interfaces/
        IUserRepository.cs           # Contrato de negocio

  Infra.Data/
    Repositories/
      UserRepository.cs              # Facade — implementa IUserRepository
                                     # Orquestra Infra.Data.PostgreSql (e futuro Redis, etc.)

  Infra.Data.PostgreSql/
    Repositories/
      Interfaces/
        IUserPostgreSqlRepository.cs # Contrato tecnologico
      UserPostgreSqlRepository.cs    # Implementacao PostgreSQL pura
```

**Operacoes de negocio vs. CRUD:**

```csharp
// Domain — interface de negocio (high-level)
public interface IUserRepository
{
    Task<User> GetUserForAuthenticationAsync(Email email);
    Task RegisterNewUserAsync(User user);
}

// Infra.Data — Facade que orquestra tecnologias
public class UserRepository : IUserRepository
{
    private readonly IUserPostgreSqlRepository _postgreSql;
    // futuro: private readonly IUserRedisRepository _redis;

    public async Task<User> GetUserForAuthenticationAsync(Email email)
    {
        // Estrategia: busca direto no OLTP (autenticacao é critica)
        return await _postgreSql.GetByEmailAsync(email);
    }

    public async Task RegisterNewUserAsync(User user)
    {
        // Estrategia: persiste no OLTP
        await _postgreSql.AddAsync(user);
    }
}

// Infra.Data.PostgreSql — implementacao tecnologica (low-level)
public class UserPostgreSqlRepository : IUserPostgreSqlRepository
{
    public Task<User> GetByEmailAsync(Email email) { ... }
    public Task AddAsync(User user) { ... }
}
```

**Regras fundamentais:**

1. **Domain nunca diz como persistir**: Diz "registra este usuario" e
   pronto. Nao diz "persiste no PostgreSQL" ou "invalida o cache".
2. **Infra.Data implementa as interfaces do Domain**: É o unico projeto
   que implementa `IUserRepository`.
3. **Infra.Data orquestra as tecnologias**: Decide a estrategia de
   persistencia para cada operacao (cache-aside, write-through,
   fallback, etc.).
4. **Operacoes sao de negocio**: `RegisterNewUser`, `GetUserForAuthentication`
   — nao `Insert`, `GetById`.
5. **Cada operacao pode ter sua propria estrategia**: Autenticacao busca
   direto no OLTP; listagem pode usar cache; registro pode invalidar
   cache apos escrita.

**Grafo de dependencias:**

```mermaid
graph TD
    Domain[Domain] --> DE[Domain.Entities]
    InfraData["Infra.Data (Facade)"] --> Domain
    InfraData --> DE
    InfraData --> InfraDataPg[Infra.Data.PostgreSql]
    InfraDataPg --> DE
```

### Por Que Funciona Melhor

- **Facade Pattern**: Simplifica a interacao com multiplas tecnologias
  atras de uma interface unica.
- **Estrategia por operacao**: Cada operacao de negocio pode ter sua
  propria logica de persistencia, sem impactar o dominio.
- **Troca de tecnologia transparente**: Adicionar Redis como cache é
  invisivel para o Domain — so a Facade muda.
- **Operacoes expressivas**: Metodos de repositorio comunicam intencao
  de negocio, facilitando compreensao e manutencao.

## Consequencias

### Beneficios

- Domain depende apenas de abstracoes de negocio — zero conhecimento de
  tecnologia.
- Adicionar ou trocar tecnologias de persistencia nao impacta o Domain
  nem a Application.
- Operacoes de repositorio sao auto-documentadas pela nomenclatura de
  negocio.
- Estrategias de persistencia complexas (cache-aside, CQRS, event
  sourcing) ficam encapsuladas na Facade.
- Code agents geram repositorios com nomenclatura de negocio em vez de
  CRUD generico.

### Trade-offs (Com Perspectiva)

- **Camada extra**: Infra.Data entre Domain e Infra.Data.PostgreSql
  adiciona uma indirection. Na pratica, a Facade é uma classe fina que
  delega para a tecnologia — o overhead de codigo é minimo.
- **Mais interfaces**: `IUserRepository` (negocio) +
  `IUserPostgreSqlRepository` (tecnico) para a mesma entidade. A
  separacao é justificada: uma expressa o que o negocio precisa, a
  outra o que a tecnologia oferece.
- **Nomenclatura de negocio exige analise**: Definir
  `GetUserForAuthentication` em vez de `GetById` requer pensar na
  intencao. Esse "custo" é na verdade um beneficio — forca o
  entendimento do dominio.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Facade Pattern** (GoF): Infra.Data é uma fachada que simplifica a
  interacao com o subsistema de persistencia (PostgreSQL, Redis, etc.).
- **Strategy Pattern** (GoF): Cada operacao de negocio pode usar uma
  estrategia de persistencia diferente, encapsulada na Facade.
- **Repository Pattern** (Fowler, POEAA): A separacao entre interface
  de negocio (Domain) e implementacao (Infra.Data) é a essencia do
  Repository Pattern.

### O Que o DDD Diz

> "A Repository represents all objects of a certain type as a
> conceptual set. It acts like a collection, except with more elaborate
> querying capability."
>
> *Um Repositorio representa todos os objetos de um determinado tipo
> como um conjunto conceitual. Ele age como uma colecao, porem com
> capacidade de consulta mais elaborada.*

Evans (2003) define repositorios como abstraccoes de negocio, nao
acessos a banco. A Facade respeita isso: `RegisterNewUser` é uma
operacao conceitual, nao um `INSERT INTO`.

### O Que o Clean Architecture Diz

> "The architecture should scream the intent of the system."
>
> *A arquitetura deve gritar a intencao do sistema.*

Robert C. Martin (2017). Repositorios com nomenclatura de negocio
(`GetUserForAuthentication`) fazem a arquitetura "gritar" a intencao
— em vez de CRUD generico que nao diz nada sobre o dominio.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre Infra.Data e Infra.Data.PostgreSql no
   Bedrock?"
2. "Por que usar operacoes de negocio em repositorios em vez de CRUD
   generico?"
3. "Como implementar cache-aside pattern usando a Facade de
   persistencia?"
4. "O que acontece quando preciso adicionar Redis como cache ao
   Bedrock?"

### Leitura Recomendada

- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Repositories
- GoF, *Design Patterns* (1994) — Facade Pattern
- Robert C. Martin, *Clean Architecture* (2017), Cap. 22 — The Clean
  Architecture
- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 10 — Repository

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Data | Framework base para Infra.Data (RepositoryBase, interfaces base de repositorio) |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Framework base para Infra.Data.PostgreSql (connections, data models, unit of work) |

## Referencias no Codigo

- Facade de exemplo: `samples/ShopDemo/Auth/Infra.Data/Repositories/UserRepository.cs`
- Interface de negocio: `samples/ShopDemo/Auth/Domain/Repositories/Interfaces/IUserRepository.cs`
- Repositorio tecnologico: `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs`
- ADR relacionada: [IN-003 — Domain Projeto Separado](./IN-003-domain-projeto-separado.md)
- ADR relacionada: [IN-004 — Modelo de Dados É Detalhe de Implementacao](./IN-004-modelo-dados-detalhe-implementacao.md)
