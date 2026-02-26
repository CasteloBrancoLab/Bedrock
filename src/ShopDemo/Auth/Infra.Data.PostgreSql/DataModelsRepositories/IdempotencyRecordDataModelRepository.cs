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

public sealed class IdempotencyRecordDataModelRepository
    : DataModelRepositoryBase<IdempotencyRecordDataModel>,
      IIdempotencyRecordDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<IdempotencyRecordDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public IdempotencyRecordDataModelRepository(
        ILogger<IdempotencyRecordDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<IdempotencyRecordDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IdempotencyRecordDataModel?> GetByKeyAsync(
        ExecutionContext executionContext,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (IdempotencyRecordDataModel x) => x.IdempotencyKey)
            & _mapper.Where(static (IdempotencyRecordDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (IdempotencyRecordDataModel x) => x.IdempotencyKey, idempotencyKey);
        _mapper.AddParameterForCommand(command, static (IdempotencyRecordDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        IdempotencyRecordDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<int> RemoveExpiredAsync(
        ExecutionContext executionContext,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (IdempotencyRecordDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause)
            + " AND expires_at < @expires_at";

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (IdempotencyRecordDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);
        _mapper.AddParameterForCommand(command, static (IdempotencyRecordDataModel x) => x.ExpiresAt, now);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    // Stryker restore all
}
