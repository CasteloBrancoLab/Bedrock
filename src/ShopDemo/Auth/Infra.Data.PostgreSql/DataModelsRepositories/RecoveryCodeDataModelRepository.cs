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

public sealed class RecoveryCodeDataModelRepository
    : DataModelRepositoryBase<RecoveryCodeDataModel>,
      IRecoveryCodeDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<RecoveryCodeDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public RecoveryCodeDataModelRepository(
        ILogger<RecoveryCodeDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<RecoveryCodeDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<RecoveryCodeDataModel?> GetByUserIdAndCodeHashAsync(
        ExecutionContext executionContext,
        Guid userId,
        string codeHash,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RecoveryCodeDataModel x) => x.UserId)
            & _mapper.Where(static (RecoveryCodeDataModel x) => x.CodeHash)
            & _mapper.Where(static (RecoveryCodeDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RecoveryCodeDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (RecoveryCodeDataModel x) => x.CodeHash, codeHash);
        _mapper.AddParameterForCommand(command, static (RecoveryCodeDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        RecoveryCodeDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<RecoveryCodeDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RecoveryCodeDataModel x) => x.UserId)
            & _mapper.Where(static (RecoveryCodeDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RecoveryCodeDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (RecoveryCodeDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<RecoveryCodeDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            RecoveryCodeDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<int> DeleteAllByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RecoveryCodeDataModel x) => x.UserId)
            & _mapper.Where(static (RecoveryCodeDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RecoveryCodeDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (RecoveryCodeDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    // Stryker restore all
}
