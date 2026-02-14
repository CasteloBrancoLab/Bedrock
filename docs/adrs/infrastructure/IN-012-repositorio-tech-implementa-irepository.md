# IN-012: Repositorio Tecnologico Deve Implementar IRepository

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN012_TechRepositoryImplementsIRepositoryRule**, que verifica:

- Interfaces no namespace `*.Repositories.Interfaces` de projetos
  `*.Infra.Data.{Tech}` devem herdar, direta ou indiretamente, de
  `IRepository<TAggregateRoot>` (via `IPostgreSqlRepository<T>`).
- O type parameter deve ser um aggregate root do dominio (tipo que herda
  de `IAggregateRoot`).

## Contexto

### O Problema (Analogia)

Imagine uma empresa de logistica com dois tipos de armazem: um
frigorificado (para pereciveis) e um convencional (para secos). Ambos
seguem o mesmo contrato de operacao com o cliente: receber encomenda,
armazenar, consultar e despachar. O cliente nao sabe (nem precisa saber)
em qual armazem o produto esta — ele fala com a central. A central sabe
qual armazem usar para cada tipo de produto.

### O Problema Tecnico

A arquitetura do Bedrock separa repositories em tres niveis
([IN-005](./IN-005-infra-data-facade-persistencia.md)):

1. **Domain**: Define `IUserRepository` — contrato de negocio.
2. **Infra.Data.{Tech}**: Define `IUserPostgreSqlRepository` — contrato
   tecnologico que opera com tipos de dominio (`User`, `EmailAddress`).
3. **Infra.Data.{Tech}**: Define `IUserDataModelRepository` — contrato
   de dados que opera com DataModels (`UserDataModel`).

O repositorio tecnologico (nivel 2) e a ponte entre o dominio e os
DataModels. Ele recebe tipos de dominio, converte para DataModels via
Factories/Adapters, delega para o DataModelRepository, e converte o
resultado de volta.

Se esse repositorio nao implementar `IRepository<TAggregateRoot>` (via
`IPostgreSqlRepository<T>`), a camada `Infra.Data` nao consegue
referencia-lo de forma type-safe — e o contrato entre dominio e
infraestrutura fica quebrado.

## Como Normalmente E Feito

### Abordagem Tradicional

Projetos sem essa separacao criam repositorios que misturam tipos de
dominio com tipos de infraestrutura:

```csharp
public class UserRepository : IUserRepository
{
    private readonly NpgsqlConnection _connection;

    public async Task<User?> GetByIdAsync(Guid id)
    {
        // SQL, materializacao e logica de dominio no mesmo lugar
        var cmd = new NpgsqlCommand("SELECT ...", _connection);
        // ... monta User diretamente do reader
    }
}
```

### Por Que Nao Funciona Bem

- **Sem camada intermediaria**: A conversao DataModel ↔ Entity fica
  espalhada dentro de cada metodo do repositorio.
- **Sem contrato tecnologico**: A camada `Infra.Data` nao tem interface
  para referenciar — depende diretamente da implementacao.
- **Testes dificeis**: Nao ha como mockar o repositorio tecnologico
  isoladamente.

## A Decisao

### Nossa Abordagem

Cada aggregate root persistido deve ter um repositorio tecnologico:

```csharp
// Interface — estende o contrato base com queries de dominio
public interface IUserPostgreSqlRepository
    : IPostgreSqlRepository<User>     // marker que herda IRepository<User>
{
    Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,                // tipo de dominio, nao string
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken);
}

// Implementacao — ponte entre tipos de dominio e DataModels
public sealed class UserPostgreSqlRepository
    : IUserPostgreSqlRepository
{
    private readonly IUserDataModelRepository _dataModelRepository;

    public async Task<User?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        // 1. Delega para DataModelRepository
        UserDataModel? dataModel = await _dataModelRepository
            .GetByIdAsync(executionContext, id, cancellationToken);

        if (dataModel is null) return null;

        // 2. Converte DataModel → Entity via Factory
        return UserFactory.Create(dataModel);
    }

    public Task<bool> RegisterNewAsync(
        ExecutionContext executionContext,
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        // 1. Converte Entity → DataModel via Factory
        UserDataModel dataModel =
            UserDataModelFactory.Create(aggregateRoot);

        // 2. Delega para DataModelRepository
        return _dataModelRepository.InsertAsync(
            executionContext, dataModel, cancellationToken);
    }
}
```

**Cadeia de heranca:**

```
IRepository<T> (BuildingBlocks.Domain)
  └── IPostgreSqlRepository<T> (Persistence.PostgreSql — marker)
        └── IUserPostgreSqlRepository (ShopDemo.Auth)
```

**Regras fundamentais:**

1. **Interface herda de `IPostgreSqlRepository<TAggregateRoot>`**:
   Garante compatibilidade com `IRepository<T>`.
2. **Type parameter e aggregate root**: `User`, nao `UserDataModel`.
3. **Metodos recebem tipos de dominio**: `EmailAddress`, `Id` — nao
   `string`, `Guid`.
4. **Classe concreta e `sealed`**: Sem heranca adicional.
5. **Converte via Factories/Adapters**: DataModel ↔ Entity.
6. **Delega para DataModelRepository**: Nunca executa SQL diretamente.

### Por Que Funciona Melhor

- **Separacao clara**: Tipos de dominio no repositorio tecnologico,
  tipos de infra no DataModelRepository.
- **Conversao centralizada**: Factories e Adapters sao o unico ponto
  de conversao — nao ha SQL misturado com logica de dominio.
- **Testabilidade**: `Infra.Data` pode mockar
  `IUserPostgreSqlRepository` sem precisar de banco real.
- **Previsibilidade**: Code agents sabem que o repositorio tecnologico
  converte tipos e delega — nada mais.

## Consequencias

### Beneficios

- Contrato type-safe entre `Infra.Data` e `Infra.Data.{Tech}`.
- Conversao DataModel ↔ Entity isolada em Factories e Adapters.
- Testabilidade em todos os niveis (mock por interface).
- Code agents geram repositorios tecnologicos seguindo o padrao
  "converter e delegar".

### Trade-offs (Com Perspectiva)

- **Tres niveis de repositorio**: `IUserRepository` → `IUserPostgreSqlRepository` → `IUserDataModelRepository`.
  Parece muito, mas cada nivel tem responsabilidade distinta:
  negocio, conversao de tipos, e acesso a dados.
- **Boilerplate de conversao**: Cada metodo faz "buscar DataModel,
  converter, retornar". Na pratica, e previsivel e facil de gerar
  automaticamente.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Adapter Pattern** (GoF): O repositorio tecnologico adapta a
  interface de dominio (`IRepository<User>`) para a interface de dados
  (`IDataModelRepository<UserDataModel>`).
- **Mediator Pattern** (GoF): Media a comunicacao entre tipos de dominio
  e tipos de persistencia.

### O Que o DDD Diz

> "Repositories should operate in terms of Aggregates."
>
> *Repositorios devem operar em termos de Aggregates.*

Evans (2003). O repositorio tecnologico opera com `User` (aggregate
root), nao com `UserDataModel` (detalhe de persistencia).

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre IUserPostgreSqlRepository e
   IUserDataModelRepository?"
2. "Por que o repositorio tecnologico recebe EmailAddress e nao string?"
3. "Como os tres niveis de repositorio se conectam no Bedrock?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Adapter Pattern
- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Repositories

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Domain | Define `IRepository<T>` — contrato base de dominio |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `IPostgreSqlRepository<T>` — marker tecnologico |

## Referencias no Codigo

- Interface de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/Interfaces/IUserPostgreSqlRepository.cs`
- Implementacao de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Repositories/UserPostgreSqlRepository.cs`
- ADR relacionada: [IN-005 — Infra.Data Facade de Persistencia](./IN-005-infra-data-facade-persistencia.md)
- ADR relacionada: [IN-011 — DataModelRepository Implementa Base](./IN-011-datamodel-repository-implementa-idatamodelrepository.md)
