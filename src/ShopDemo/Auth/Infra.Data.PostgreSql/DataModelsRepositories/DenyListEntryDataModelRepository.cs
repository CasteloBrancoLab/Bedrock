using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;

public sealed class DenyListEntryDataModelRepository
    : DataModelRepositoryBase<DenyListEntryDataModel>,
      IDenyListEntryDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<DenyListEntryDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public DenyListEntryDataModelRepository(
        ILogger<DenyListEntryDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<DenyListEntryDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<bool> ExistsByTypeAndValueAsync(
        ExecutionContext executionContext,
        short type,
        string value,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (DenyListEntryDataModel x) => x.Type)
            & _mapper.Where(static (DenyListEntryDataModel x) => x.Value)
            & _mapper.Where(static (DenyListEntryDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateExistsCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.Type, type);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.Value, value);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is not null && (bool)result;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<DenyListEntryDataModel?> GetByTypeAndValueAsync(
        ExecutionContext executionContext,
        short type,
        string value,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (DenyListEntryDataModel x) => x.Type)
            & _mapper.Where(static (DenyListEntryDataModel x) => x.Value)
            & _mapper.Where(static (DenyListEntryDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.Type, type);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.Value, value);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        DenyListEntryDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<int> DeleteExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (DenyListEntryDataModel x) => x.ExpiresAt, RelationalOperator.LessThanOrEqual)
            & _mapper.Where(static (DenyListEntryDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.ExpiresAt, referenceDate);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<bool> DeleteByTypeAndValueAsync(
        ExecutionContext executionContext,
        short type,
        string value,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (DenyListEntryDataModel x) => x.Type)
            & _mapper.Where(static (DenyListEntryDataModel x) => x.Value)
            & _mapper.Where(static (DenyListEntryDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.Type, type);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.Value, value);
        _mapper.AddParameterForCommand(command, static (DenyListEntryDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected > 0;
    }
    // Stryker restore all
}
