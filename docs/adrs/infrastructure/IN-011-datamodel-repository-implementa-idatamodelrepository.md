# IN-011: DataModelRepository Deve Implementar IDataModelRepository

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN011_DataModelRepositoryImplementsBaseRule**, que verifica:

- Interfaces no namespace `*.DataModelsRepositories.Interfaces` de
  projetos `*.Infra.Data.{Tech}` devem herdar, direta ou indiretamente,
  de `IDataModelRepository<TDataModel>`.
- O type parameter `TDataModel` deve ser um tipo que herda de
  `DataModelBase`.
- A classe concreta deve herdar de `DataModelRepositoryBase<TDataModel>`.

## Contexto

### O Problema (Analogia)

Imagine um almoxarifado de uma fabrica. Todos os almoxarifados da rede
seguem o mesmo protocolo de operacao: receber material, registrar
entrada, consultar estoque, despachar material, registrar saida. Cada
almoxarifado pode ter particularidades (armazena pecas ou materia-prima),
mas o protocolo base e o mesmo. Se cada almoxarifado inventasse seu
proprio protocolo, a central nao conseguiria auditar nem controlar
nenhum deles.

### O Problema Tecnico

O DataModelRepository e o nivel mais baixo de acesso a dados — opera
diretamente com DataModels e SQL parametrizado. O framework define
`IDataModelRepository<T>` com operacoes CRUD padronizadas:

- `GetByIdAsync` — busca por ID com filtro de tenant.
- `ExistsAsync` — verifica existencia.
- `InsertAsync` — insere novo registro.
- `UpdateAsync` — atualiza com concorrencia otimista (version check).
- `DeleteAsync` — remove com concorrencia otimista.
- `EnumerateAllAsync` — itera com handler pattern e paginacao.
- `EnumerateModifiedSinceAsync` — itera registros alterados desde data.

Se repositorios de DataModel nao implementarem esse contrato, o
`DataModelRepositoryBase` nao pode fornecer a implementacao padrao de
CRUD — e cada BC reimplementa SQL manualmente, com inconsistencias
inevitaveis.

## Como Normalmente E Feito

### Abordagem Tradicional

Projetos sem framework definem repositorios ad-hoc com assinaturas
inconsistentes:

```csharp
public class UserRepository
{
    public User? FindById(int id) { ... }     // "Find" vs. "Get"
    public void Save(User user) { ... }       // "Save" = insert ou update?
    public List<User> GetAll() { ... }        // Retorna lista em vez de handler
}

public class OrderRepository
{
    public Order? Get(Guid orderId) { ... }   // "Get" vs. "FindById"
    public bool Create(Order order) { ... }   // "Create" vs. "Save"
    public IEnumerable<Order> List() { ... }  // "List" vs. "GetAll"
}
```

### Por Que Nao Funciona Bem

- **Assinaturas inconsistentes**: `Find` vs. `Get`, `Save` vs. `Create`,
  `GetAll` vs. `List`.
- **Sem concorrencia otimista**: `Update` e `Delete` nao verificam
  versao — atualizacoes concorrentes causam lost updates.
- **Sem handler pattern**: `GetAll()` retorna lista em memoria — perigoso
  para tabelas grandes.
- **Sem multi-tenancy**: Queries nao filtram por tenant automaticamente.

## A Decisao

### Nossa Abordagem

Cada aggregate root persistido deve ter um DataModelRepository que:

1. Declara interface no namespace `*.DataModelsRepositories.Interfaces`
   herdando de `IPostgreSqlDataModelRepository<TDataModel>`.
2. Implementa classe concreta herdando de
   `DataModelRepositoryBase<TDataModel>`.

```csharp
// Interface — estende o contrato base com queries especificas do BC
public interface IUserDataModelRepository
    : IPostgreSqlDataModelRepository<UserDataModel>
{
    Task<UserDataModel?> GetByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken);
}

// Implementacao — herda CRUD padrao, implementa queries especificas
public sealed class UserDataModelRepository
    : DataModelRepositoryBase<UserDataModel>,
      IUserDataModelRepository
{
    public UserDataModelRepository(
        ILogger<UserDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<UserDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
    }

    public async Task<UserDataModel?> GetByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (UserDataModel x) => x.Email)
            & _mapper.Where(static (UserDataModel x) => x.TenantCode);
        // ... execucao type-safe via mapper
    }
}
```

**Cadeia de heranca:**

```
IDataModelRepository<T> (Persistence.Abstractions)
  └── IPostgreSqlDataModelRepository<T> (Persistence.PostgreSql)
        └── IUserDataModelRepository (ShopDemo.Auth)
```

**Regras fundamentais:**

1. **Interface herda de `IPostgreSqlDataModelRepository<TDataModel>`**:
   Garante compatibilidade com o contrato CRUD padrao.
2. **Classe concreta herda de `DataModelRepositoryBase<TDataModel>`**:
   Herda implementacao de `GetByIdAsync`, `ExistsAsync`, `InsertAsync`,
   `UpdateAsync`, `DeleteAsync`, `EnumerateAllAsync`.
3. **Classe concreta e `sealed`**: Sem heranca adicional.
4. **Recebe UnitOfWork do BC**: Via marker interface (ex:
   `IAuthPostgreSqlUnitOfWork`), garantindo isolamento transacional.
5. **Recebe Mapper**: `IDataModelMapper<TDataModel>` para geracao
   type-safe de SQL.
6. **Queries especificas adicionadas na interface**: `GetByEmailAsync`,
   `ExistsByUsernameAsync`, etc.

### Por Que Funciona Melhor

- **CRUD padrao herdado**: `GetByIdAsync`, `InsertAsync`, etc. sao
  implementados uma unica vez na base — todos os BCs compartilham.
- **Concorrencia otimista automatica**: `UpdateAsync` e `DeleteAsync`
  verificam `EntityVersion` — herdado da base.
- **Multi-tenancy automatica**: Todas as queries filtram por `TenantCode`
  — herdado da base.
- **Handler pattern**: `EnumerateAllAsync` usa handler delegate em vez de
  retornar listas — herdado da base.

## Consequencias

### Beneficios

- CRUD padronizado em todos os BCs com concorrencia otimista e
  multi-tenancy.
- Queries especificas do BC vivem em metodos adicionais na interface.
- Base class elimina duplicacao de SQL para operacoes comuns.
- Code agents geram repositorios corretos declarando apenas queries
  especificas.

### Trade-offs (Com Perspectiva)

- **Mais uma interface e classe por aggregate**: DataModelRepository +
  interface + queries especificas. Na pratica, a interface e pequena
  (2-4 metodos extras) e a classe herda quase tudo da base.
- **Acoplamento com a tecnologia**: `IPostgreSqlDataModelRepository` e
  especifico de PostgreSQL. Isso e intencional — DataModelRepositories
  vivem na camada tecnologica.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Repository Pattern** (Fowler, POEAA): DataModelRepository e a
  implementacao concreta do padrao Repository na camada mais baixa.
- **Template Method** (GoF): `DataModelRepositoryBase` define o
  algoritmo CRUD; a classe concreta adiciona queries especificas.
- **Layer Supertype** (Fowler, POEAA): `DataModelRepositoryBase` e o
  supertipo da camada de DataModelRepositories.

### O Que o DDD Diz

> "The Repository pattern provides the illusion of an in-memory
> collection."
>
> *O padrao Repository fornece a ilusao de uma colecao em memoria.*

Evans (2003). O DataModelRepository fornece essa ilusao para DataModels
— operacoes CRUD com assinatura simples que escondem SQL parametrizado.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Quais operacoes CRUD o DataModelRepositoryBase fornece
   automaticamente?"
2. "Como adicionar uma query especifica ao DataModelRepository?"
3. "Como o handler pattern funciona no EnumerateAllAsync?"

### Leitura Recomendada

- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 10 — Repository
- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Repositories

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.Abstractions | Define `IDataModelRepository<T>` — contrato CRUD base |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelRepositoryBase<T>` — implementacao CRUD padrao |

## Referencias no Codigo

- Interface de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/Interfaces/IUserDataModelRepository.cs`
- Implementacao de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/DataModelsRepositories/UserDataModelRepository.cs`
- Base class: `src/BuildingBlocks/Persistence.PostgreSql/DataModelRepositories/DataModelRepositoryBase.cs`
- ADR relacionada: [IN-010 — DataModel Herda DataModelBase](./IN-010-datamodel-herda-datamodelbase.md)
