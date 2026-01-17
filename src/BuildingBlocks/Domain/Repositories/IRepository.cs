using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;

namespace Bedrock.BuildingBlocks.Domain.Repositories;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Separação Entre BuildingBlocks.Domain.Entities e BuildingBlocks.Domain
═══════════════════════════════════════════════════════════════════════════════

Existem dois building blocks separados: Domain.Entities e Domain.
Essa separação mantém as dependências de Domain.Entities o mais enxutas possível.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Domain.Entities - Zero Dependências de Infraestrutura
───────────────────────────────────────────────────────────────────────────────

Domain.Entities pode ser referenciado por QUALQUER camada:
✅ Application, Domain, Infrastructure, Presentation
✅ Plugins para Office, extensões de browser, apps mobile
✅ Blazor WASM, Xamarin/MAUI, Unity

Isso é possível porque NÃO leva dependências de infraestrutura:
❌ MongoDB, PostgreSQL, SQL Server
❌ RabbitMQ, Kafka, Azure Service Bus
❌ Redis, Elasticsearch

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: BuildingBlocks.Domain - Integração com Mundo Externo
───────────────────────────────────────────────────────────────────────────────

BuildingBlocks.Domain PODE ter dependências de infraestrutura.
Não será referenciado por camadas que precisam ser portáteis.

Esta camada contém:
- Repositórios (persistência de agregados)
- Serviços de domínio (orquestração entre agregados)
- Integrações com mundo externo

═══════════════════════════════════════════════════════════════════════════════
*/
/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Abstrações de Repositórios no Domínio
═══════════════════════════════════════════════════════════════════════════════

O BuildingBlocks.Domain contém as ABSTRAÇÕES de repositórios.
A infraestrutura contém as IMPLEMENTAÇÕES concretas.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Dependency Inversion Principle (DIP)
───────────────────────────────────────────────────────────────────────────────

Abstrações de repositórios são definidas no DOMÍNIO, não na infraestrutura:
✅ Domínio define contratos (IRepository<T>)
✅ Infraestrutura implementa (MongoRepository<T>, PostgresRepository<T>)
✅ Serviços de domínio dependem de abstrações, não de implementações

BENEFÍCIOS:
- Domínio não conhece detalhes de persistência
- Múltiplas implementações podem coexistir
- Testes usam implementações in-memory

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Open/Closed Principle (OCP)
───────────────────────────────────────────────────────────────────────────────

Domínio está ABERTO para extensão, FECHADO para modificação:
✅ Novas implementações de repositórios não alteram contratos
✅ Adicionar suporte a novo banco não modifica o domínio
✅ Infraestrutura evolui independentemente

RAZÃO: Alterar domínio é mais crítico que alterar infraestrutura.
Abstrações estáveis no domínio promovem design robusto e flexível.

═══════════════════════════════════════════════════════════════════════════════
*/
/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Aggregate Root como Ponto de Entrada
═══════════════════════════════════════════════════════════════════════════════

Aggregate Root é a entidade que serve como ÚNICO ponto de entrada para um agregado.
Um agregado é um cluster de entidades relacionadas com regras de consistência.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Manipulação Sempre Via Aggregate Root
───────────────────────────────────────────────────────────────────────────────

Toda manipulação de dados de um agregado passa pelo seu Aggregate Root:
✅ Para alterar OrderItem, acesse através de Order
✅ Regras de negócio são aplicadas pelo Aggregate Root
✅ Consistência transacional garantida dentro do agregado

EXEMPLO:
- Order (Aggregate Root) → OrderItem, Discount, Payment
- Não é possível alterar OrderItem de um Order já concluído
- Order valida e aplica todas as regras antes de modificar seus filhos

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Repositórios Apenas Para Aggregate Roots
───────────────────────────────────────────────────────────────────────────────

IRepository<TAggregateRoot> é genérico APENAS para Aggregate Roots:
✅ OrderRepository - Order é Aggregate Root
✅ CustomerRepository - Customer é Aggregate Root
❌ OrderItemRepository - OrderItem NÃO é Aggregate Root

RAZÃO: Entidades filhas são persistidas junto com seu Aggregate Root.

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Base interface for all repository implementations that persist aggregate roots.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the essential operations for persisting and retrieving aggregate roots
/// following Domain-Driven Design (DDD) principles. Repositories should only be created for
/// aggregate roots, never for child entities.
/// </para>
/// <para>
/// All methods receive an <see cref="ExecutionContext"/> to support multi-tenancy and audit trails.
/// </para>
/// </remarks>
/// <typeparam name="TAggregateRoot">
/// The type of the aggregate root. Must implement <see cref="IAggregateRoot"/>.
/// </typeparam>
public interface IRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos Essenciais vs CRUD Genérico
    ═══════════════════════════════════════════════════════════════════════════════

    Seguindo Behavior Driven-Development (BDD), focamos no comportamento desejado.
    Métodos CRUD genéricos (Add, Update, Delete) são evitados - podem nem ser necessários.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Apenas Métodos Universalmente Necessários na Interface Base
    ───────────────────────────────────────────────────────────────────────────────

    IRepository<TAggregateRoot> define APENAS métodos essenciais para qualquer repositório:
    ✅ GetByIdAsync - Recuperar agregado por identificador
    ✅ ExistsAsync - Verificar existência sem carregar entidade
    ✅ GetModifiedSinceAsync - Sincronização e auditoria

    ❌ Add, Update, Delete genéricos - defina apenas se necessário no repositório concreto

    ═══════════════════════════════════════════════════════════════════════════════
    */

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: GetByIdAsync Sempre Carrega Agregado Completo
    ───────────────────────────────────────────────────────────────────────────────

    GetByIdAsync SEMPRE recupera o Aggregate Root com TODOS os filhos carregados.
    Isso garante integridade do agregado e aplicação correta das regras de negócio.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Consultas Otimizadas na Interface Específica
    ───────────────────────────────────────────────────────────────────────────────

    Se uma operação precisa de consulta otimizada (sem carregar agregado completo):
    ✅ Defina na interface específica do repositório (IOrderRepository)
    ✅ Use nome que represente intenção de negócio clara
    ✅ Exemplo: GetOrderForPaymentAsync(), GetOrderSummaryAsync()

    ❌ Não adicione consultas otimizadas na interface base IRepository<T>

    ───────────────────────────────────────────────────────────────────────────────
    */

    /// <summary>
    /// Retrieves an aggregate root by its unique identifier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method always loads the complete aggregate with all child entities.
    /// This ensures aggregate integrity and correct business rule enforcement.
    /// </para>
    /// <para>
    /// For optimized queries that don't require the complete aggregate, define
    /// specific methods in the concrete repository interface (e.g., <c>GetOrderSummaryAsync</c>).
    /// </para>
    /// </remarks>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The aggregate root if found; otherwise, <c>null</c>.
    /// </returns>
    Task<TAggregateRoot?> GetByIdAsync(ExecutionContext executionContext, Id id, CancellationToken cancellationToken);

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_GUIDANCE: GetAllAsync - Listagem Paginada de Agregados
    ───────────────────────────────────────────────────────────────────────────────

    Embora nem toda raiz de agregação precise ser listada, este método é
    universalmente aplicável para repositórios que suportam paginação.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: GetAllAsync na Interface Base (Quase Universal)
    ───────────────────────────────────────────────────────────────────────────────

    Caso não colocássemos este método na interface base, cada repositório específico
    teria que defini-lo individualmente. Embora não seja 100% usado, é muito usado
    (quase 100% das vezes). Por isso, permanece na interface base.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: PaginationInfo é Obrigatório - Usar All Para Todos os Registros
    ───────────────────────────────────────────────────────────────────────────────

    Nenhuma listagem de agregados deve ser feita SEM o objeto de paginação.
    Caso o cliente queira, intencionalmente, carregar todos os agregados,
    ele pode fazer isso usando os recursos de PaginationInfo:

    ✅ PaginationInfo.All - Retorna todos os registros sem paginação
    ✅ PaginationInfo.CreateAll(sorts, filters) - Todos com ordenação e filtros
    ✅ pagination.IsUnbounded - Verifica se é query sem limite

    EXEMPLO:
      var allItems = repository.GetAllAsync(PaginationInfo.All, cancellationToken);
      var allSorted = repository.GetAllAsync(
          PaginationInfo.All.WithSortCollection(sorts),
          cancellationToken);

    ───────────────────────────────────────────────────────────────────────────────
    */

    /// <summary>
    /// Retrieves all aggregate roots with pagination support.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pagination is always required to prevent unbounded queries. Use
    /// <see cref="PaginationInfo.All"/> to explicitly request all records.
    /// </para>
    /// <para>
    /// The <see cref="PaginationInfo.IsUnbounded"/> property can be used to
    /// detect if the query has no limits.
    /// </para>
    /// </remarks>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="paginationInfo">The pagination parameters including page, size, sorts, and filters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// An async enumerable of aggregate roots matching the pagination criteria.
    /// </returns>
    IAsyncEnumerable<TAggregateRoot> GetAllAsync(ExecutionContext executionContext, PaginationInfo paginationInfo, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if an aggregate root with the specified identifier exists.
    /// </summary>
    /// <remarks>
    /// This method is optimized to check existence without loading the entire aggregate.
    /// Use this when you only need to verify if an entity exists.
    /// </remarks>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="id">The unique identifier to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>true</c> if an aggregate root with the specified ID exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync(ExecutionContext executionContext, Id id, CancellationToken cancellationToken);

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: TimeProvider Explícito Para Operações Temporais
    ───────────────────────────────────────────────────────────────────────────────

    Métodos que dependem do tempo atual DEVEM receber TimeProvider como parâmetro:
    ✅ Testabilidade - Injetar tempos fixos ou simulados em testes
    ✅ Consistência - Todas as operações usam mesma fonte de tempo
    ✅ Flexibilidade - Facilita mudanças (fuso horário, NTP, relógios customizados)

    ❌ DateTime.Now / DateTime.UtcNow - não testável, não consistente

    ───────────────────────────────────────────────────────────────────────────────
    */

    /// <summary>
    /// Retrieves aggregate roots that have been modified since a specified timestamp.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is useful for synchronization and audit scenarios where you need
    /// to track changes over time.
    /// </para>
    /// <para>
    /// The <paramref name="timeProvider"/> parameter is required for testability and
    /// consistency across operations.
    /// </para>
    /// </remarks>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="timeProvider">The time provider for consistent time operations.</param>
    /// <param name="since">The timestamp to compare against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// An async enumerable of aggregate roots modified after the specified timestamp.
    /// </returns>
    IAsyncEnumerable<TAggregateRoot> GetModifiedSinceAsync(ExecutionContext executionContext, TimeProvider timeProvider, DateTimeOffset since, CancellationToken cancellationToken);

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: RegisterNewAsync - Operação Universal de Criação
    ───────────────────────────────────────────────────────────────────────────────

    Seguindo BDD, contratos refletem operações de negócio, não CRUD genérico.
    Porém, registrar novo Aggregate Root é operação UNIVERSAL em qualquer repositório.

    ✅ RegisterNewAsync - incluído na interface base (sempre necessário)
    ✅ Nome expressa intenção de negócio (registrar), não operação técnica (insert)

    ❌ Add, Create, Insert genéricos - evite nomenclatura técnica/CRUD

    ───────────────────────────────────────────────────────────────────────────────
    */

    /// <summary>
    /// Registers a new aggregate root in the repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method persists a new aggregate root. The name reflects business intent
    /// (register) rather than technical operation (insert/add).
    /// </para>
    /// <para>
    /// The aggregate root and all its child entities will be persisted together.
    /// </para>
    /// </remarks>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="aggregateRoot">The aggregate root to register.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>true</c> if the registration was successful; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> RegisterNewAsync(ExecutionContext executionContext, TAggregateRoot aggregateRoot, CancellationToken cancellationToken);
}
