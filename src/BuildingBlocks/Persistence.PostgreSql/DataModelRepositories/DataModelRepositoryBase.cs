using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: DataModelRepositoryBase - Base para Repositórios de DataModel
═══════════════════════════════════════════════════════════════════════════════

DataModelRepositoryBase é a classe base para repositórios PostgreSQL que trabalham
exclusivamente com DataModels. Não tem conhecimento de entidades de domínio.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Separação de Responsabilidades
───────────────────────────────────────────────────────────────────────────────

Esta classe trabalha APENAS com DataModels:
✅ CRUD operations em DataModelBase
✅ Multi-tenancy via ExecutionContext.TenantInfo.Code
✅ Paginação e ordenação
✅ Optimistic locking via EntityVersion

A conversão Entity ↔ DataModel será feita por factories em camada superior.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: UnitOfWork e Mapper como Dependências
───────────────────────────────────────────────────────────────────────────────

O repositório recebe:
- IPostgreSqlUnitOfWork: gerencia conexão e transação
- IDataModelMapper<TDataModel>: gera comandos SQL e mapeia propriedades

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Handler Pattern (Issue #60)
───────────────────────────────────────────────────────────────────────────────

Métodos de enumeração usam Handler Pattern em vez de IAsyncEnumerable:
- EnumerateAllAsync: recebe DataModelItemHandler<TDataModel>
- EnumerateModifiedSinceAsync: recebe DataModelItemHandler<TDataModel>

Benefícios:
✅ Exceções capturadas no repositório, não propagadas ao cliente
✅ Cliente só vê bool de sucesso/falha
✅ Handler pode retornar false para interromper iteração
✅ Abstração limpa sem vazamento de detalhes de infraestrutura

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Tratamento de Erros
───────────────────────────────────────────────────────────────────────────────

Todos os métodos:
1. Recebem ExecutionContext para rastreamento distribuído
2. Usam try-catch para capturar exceções
3. Logam exceções via LogExceptionForDistributedTracing
4. Adicionam exceções ao ExecutionContext via AddException
5. Retornam false/null em caso de erro

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Evitar Closures - Expression Trees vs Delegates
───────────────────────────────────────────────────────────────────────────────

As lambdas usadas com _mapper.Where() e _mapper.OrderByAscending() são
EXPRESSION TREES (Expression<Func<T, object>>), NÃO delegates comuns.

✅ CORRETO - Expression tree, sem closure:
   _mapper.Where(x => x.Id)
   _mapper.OrderByAscending(x => x.LastChangedAt)

❌ INCORRETO - Captura de variável criaria closure:
   var propertyName = "Id";
   _mapper.Where(x => GetProperty(x, propertyName))  // closure!

Por que evitar closures em código de infraestrutura:
1. Closures alocam objetos no heap (classe gerada pelo compilador)
2. Em hot paths (loops de leitura), isso causa pressão no GC
3. Expression trees com member access são analisadas em compile-time
4. O mapper extrai o nome da propriedade da expression, não executa a lambda

Padrão seguro para adicionar parâmetros:
   _mapper.AddParameterForCommand(command, x => x.Id, id)
   // A lambda é expression tree, 'id' é passado como argumento separado

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Abstract base class for PostgreSQL repositories that work with data models.
/// Provides standard CRUD operations using the UnitOfWork and DataModelMapper patterns.
/// </summary>
/// <typeparam name="TDataModel">The data model type for persistence.</typeparam>
public abstract class DataModelRepositoryBase<TDataModel>
    : IPostgreSqlDataModelRepository<TDataModel>
    where TDataModel : DataModelBase, new()
{
    // Fields
    private readonly ILogger _logger;
    private readonly IPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<TDataModel> _mapper;

    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DataModelRepositoryBase{TDataModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for recording operations and errors.</param>
    /// <param name="unitOfWork">The unit of work for managing database connections and transactions.</param>
    /// <param name="mapper">The data model mapper for SQL generation and property mapping.</param>
    protected DataModelRepositoryBase(
        ILogger logger,
        IPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<TDataModel> mapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Protected Properties

    /// <summary>
    /// Gets the logger instance for recording operations and errors.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// Gets the unit of work for managing database connections and transactions.
    /// </summary>
    protected IPostgreSqlUnitOfWork UnitOfWork => _unitOfWork;

    /// <summary>
    /// Gets the data model mapper for SQL generation and property mapping.
    /// </summary>
    protected IDataModelMapper<TDataModel> Mapper => _mapper;

    // Private Helper Methods

    /// <summary>
    /// Executes the reader loop and calls the handler for each data model.
    /// Shared helper method used by enumeration methods to avoid code duplication.
    /// </summary>
    // Stryker disable all : NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao")]
    private async Task ExecuteReaderWithHandlerAsync(
        NpgsqlDataReader reader,
        DataModelItemHandler<TDataModel> handler,
        CancellationToken cancellationToken)
    {
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            TDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

            bool shouldContinue = await handler(dataModel, cancellationToken).ConfigureAwait(false);
            if (!shouldContinue)
            {
                break;
            }
        }
    }
    // Stryker restore all

    // Public Methods - IDataModelRepository Implementation

    /// <summary>
    /// Retrieves a data model by its unique identifier.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteReaderAsync.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao")]
    public async Task<TDataModel?> GetByIdAsync(
        ExecutionContext executionContext,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            WhereClause whereClause = _mapper.Where(x => x.Id);
            string commandText = _mapper.GenerateSelectCommand(whereClause);

            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
            _mapper.AddParameterForCommand(command, x => x.TenantCode, executionContext.TenantInfo.Code);
            _mapper.AddParameterForCommand(command, x => x.Id, id);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            TDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

            return dataModel;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return null;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Enumerates all data models with pagination support, calling the handler for each item.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteReaderAsync.
    /// Uses the handler pattern instead of IAsyncEnumerable to avoid leaky abstractions.
    /// Exceptions are caught, logged, and added to ExecutionContext.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao")]
    public async Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        DataModelItemHandler<TDataModel> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            string commandText = _mapper.GenerateSelectCommand(paginationInfo);

            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
            _mapper.AddParameterForCommand(command, x => x.TenantCode, executionContext.TenantInfo.Code);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            await ExecuteReaderWithHandlerAsync(reader, handler, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return false;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Checks if a data model exists by its unique identifier.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteScalarAsync.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> ExistsAsync(
        ExecutionContext executionContext,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            WhereClause whereClause = _mapper.Where(x => x.Id);
            string commandText = _mapper.GenerateExistsCommand(whereClause);

            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
            _mapper.AddParameterForCommand(command, x => x.TenantCode, executionContext.TenantInfo.Code);
            _mapper.AddParameterForCommand(command, x => x.Id, id);

            object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return result is bool exists && exists;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return false;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Inserts a new data model.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteNonQueryAsync.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> InsertAsync(
        ExecutionContext executionContext,
        TDataModel dataModel,
        CancellationToken cancellationToken)
    {
        try
        {
            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(_mapper.InsertCommand);
            _mapper.ConfigureCommandFromDataModelBase(command, _mapper, dataModel);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return false;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Updates an existing data model using optimistic concurrency control.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteNonQueryAsync.
    /// The UPDATE command includes version check for optimistic concurrency.
    /// If the expectedVersion doesn't match the current version in the database,
    /// no rows are affected and the method returns false.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        TDataModel dataModel,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            // Generate UPDATE command with EntityVersion check for optimistic concurrency
            WhereClause versionClause = _mapper.Where(x => x.EntityVersion);
            string commandText = _mapper.GenerateUpdateCommand(versionClause);

            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
            _mapper.ConfigureCommandFromDataModelBase(command, _mapper, dataModel);

            // Add the expected version parameter for the WHERE clause
            _mapper.AddParameterForCommand(command, x => x.EntityVersion, expectedVersion);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return false;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Deletes a data model by its unique identifier using optimistic concurrency control.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteNonQueryAsync.
    /// The DELETE command includes version check for optimistic concurrency.
    /// If the expectedVersion doesn't match the current version in the database,
    /// no rows are affected and the method returns false.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        Guid id,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            // Generate DELETE command with Id and EntityVersion check for optimistic concurrency
            WhereClause idClause = _mapper.Where(x => x.Id);
            WhereClause versionClause = _mapper.Where(x => x.EntityVersion);
            WhereClause combinedClause = idClause & versionClause;
            string commandText = _mapper.GenerateDeleteCommand(combinedClause);

            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
            _mapper.AddParameterForCommand(command, x => x.TenantCode, executionContext.TenantInfo.Code);
            _mapper.AddParameterForCommand(command, x => x.Id, id);
            _mapper.AddParameterForCommand(command, x => x.EntityVersion, expectedVersion);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return false;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Enumerates data models modified since a specific timestamp, calling the handler for each item.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteReaderAsync.
    /// Uses the handler pattern instead of IAsyncEnumerable to avoid leaky abstractions.
    /// Exceptions are caught, logged, and added to ExecutionContext.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao")]
    public async Task<bool> EnumerateModifiedSinceAsync(
        ExecutionContext executionContext,
        DateTimeOffset since,
        DataModelItemHandler<TDataModel> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            WhereClause whereClause = _mapper.Where(x => x.LastChangedAt, RelationalOperator.GreaterThanOrEqual);
            OrderByClause orderByClause = _mapper.OrderByAscending(x => x.LastChangedAt);
            string commandText = _mapper.GenerateSelectCommand(whereClause, orderByClause);

            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
            _mapper.AddParameterForCommand(command, x => x.TenantCode, executionContext.TenantInfo.Code);
            _mapper.AddParameterForCommand(command, x => x.LastChangedAt, since);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            await ExecuteReaderWithHandlerAsync(reader, handler, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogExceptionForDistributedTracing(executionContext, ex);
            executionContext.AddException(ex);
            return false;
        }
    }
    // Stryker restore all
}
