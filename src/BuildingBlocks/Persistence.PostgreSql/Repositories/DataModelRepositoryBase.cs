using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories;

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
✅ Multi-tenancy via TenantCode
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
        Guid tenantCode,
        Guid id,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause = _mapper.Where(x => x.Id);
        string commandText = _mapper.GenerateSelectCommand(whereClause);

        using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
        _mapper.AddParameterForCommand(command, x => x.TenantCode, tenantCode);
        _mapper.AddParameterForCommand(command, x => x.Id, id);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        TDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel, PopulateAdditionalProperties);

        return dataModel;
    }
    // Stryker restore all

    /// <summary>
    /// Enumerates all data models with pagination support, calling the handler for each item.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteReaderAsync.
    /// Uses the handler pattern instead of IAsyncEnumerable to avoid leaky abstractions.
    /// Exceptions are caught and logged, returning false on failure.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao")]
    public async Task<bool> EnumerateAllAsync(
        Guid tenantCode,
        PaginationInfo paginationInfo,
        DataModelItemHandler<TDataModel> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            string commandText = _mapper.GenerateSelectCommand(paginationInfo);

            using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
            _mapper.AddParameterForCommand(command, x => x.TenantCode, tenantCode);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                TDataModel dataModel = new();
                _mapper.PopulateDataModelBaseFromReader(reader, dataModel, PopulateAdditionalProperties);

                bool shouldContinue = await handler(dataModel, cancellationToken);
                if (!shouldContinue)
                {
                    break;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating all data models for tenant {TenantCode}", tenantCode);
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
        Guid tenantCode,
        Guid id,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause = _mapper.Where(x => x.Id);
        string commandText = _mapper.GenerateExistsCommand(whereClause);

        using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
        _mapper.AddParameterForCommand(command, x => x.TenantCode, tenantCode);
        _mapper.AddParameterForCommand(command, x => x.Id, id);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is bool exists && exists;
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
        TDataModel dataModel,
        CancellationToken cancellationToken)
    {
        using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(_mapper.InsertCommand);
        _mapper.ConfigureCommandFromDataModelBase(command, _mapper, dataModel);
        ConfigureAdditionalParameters(command, dataModel);

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }
    // Stryker restore all

    /// <summary>
    /// Updates an existing data model using optimistic locking.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteNonQueryAsync.
    /// The UPDATE command includes version check for optimistic locking.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> UpdateAsync(
        TDataModel dataModel,
        CancellationToken cancellationToken)
    {
        using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(_mapper.UpdateCommand);
        _mapper.ConfigureCommandFromDataModelBase(command, _mapper, dataModel);
        ConfigureAdditionalParameters(command, dataModel);

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }
    // Stryker restore all

    /// <summary>
    /// Deletes a data model by its unique identifier.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteNonQueryAsync.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public async Task<bool> DeleteAsync(
        Guid tenantCode,
        Guid id,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause = _mapper.Where(x => x.Id);
        string commandText = _mapper.GenerateDeleteCommand(whereClause);

        using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(commandText);
        _mapper.AddParameterForCommand(command, x => x.TenantCode, tenantCode);
        _mapper.AddParameterForCommand(command, x => x.Id, id);

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }
    // Stryker restore all

    /// <summary>
    /// Enumerates data models modified since a specific timestamp, calling the handler for each item.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - requires active NpgsqlConnection for ExecuteReaderAsync.
    /// Uses the handler pattern instead of IAsyncEnumerable to avoid leaky abstractions.
    /// Exceptions are caught and logged, returning false on failure.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e NpgsqlDataReader sao sealed e nao podem ser mockados - requer testes de integracao")]
    public async Task<bool> EnumerateModifiedSinceAsync(
        Guid tenantCode,
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
            _mapper.AddParameterForCommand(command, x => x.TenantCode, tenantCode);
            _mapper.AddParameterForCommand(command, x => x.LastChangedAt, since);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                TDataModel dataModel = new();
                _mapper.PopulateDataModelBaseFromReader(reader, dataModel, PopulateAdditionalProperties);

                bool shouldContinue = await handler(dataModel, cancellationToken);
                if (!shouldContinue)
                {
                    break;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating modified data models for tenant {TenantCode} since {Since}", tenantCode, since);
            return false;
        }
    }
    // Stryker restore all

    // Protected Virtual Methods - Extension Points

    /// <summary>
    /// When overridden in a derived class, populates additional properties from the reader
    /// that are not part of DataModelBase.
    /// </summary>
    /// <param name="reader">The data reader with current row.</param>
    /// <param name="dataModel">The data model to populate.</param>
    /// <param name="mapper">The data model mapper.</param>
    protected virtual void PopulateAdditionalProperties(
        NpgsqlDataReader reader,
        TDataModel dataModel,
        IDataModelMapper<TDataModel> mapper)
    {
        // Default implementation does nothing.
        // Derived classes can override to populate additional properties.
    }

    /// <summary>
    /// When overridden in a derived class, configures additional command parameters
    /// that are not part of DataModelBase.
    /// </summary>
    /// <param name="command">The command to configure.</param>
    /// <param name="dataModel">The data model with values.</param>
    protected virtual void ConfigureAdditionalParameters(
        NpgsqlCommand command,
        TDataModel dataModel)
    {
        // Default implementation does nothing.
        // Derived classes can override to add additional parameters.
    }
}
