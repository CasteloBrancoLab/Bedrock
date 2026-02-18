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

public sealed class PasswordResetTokenDataModelRepository
    : DataModelRepositoryBase<PasswordResetTokenDataModel>,
      IPasswordResetTokenDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<PasswordResetTokenDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public PasswordResetTokenDataModelRepository(
        ILogger<PasswordResetTokenDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<PasswordResetTokenDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<PasswordResetTokenDataModel?> GetByTokenHashAsync(
        ExecutionContext executionContext,
        string tokenHash,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (PasswordResetTokenDataModel x) => x.TokenHash)
            & _mapper.Where(static (PasswordResetTokenDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (PasswordResetTokenDataModel x) => x.TokenHash, tokenHash);
        _mapper.AddParameterForCommand(command, static (PasswordResetTokenDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        PasswordResetTokenDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
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
            _mapper.Where(static (PasswordResetTokenDataModel x) => x.UserId)
            & _mapper.Where(static (PasswordResetTokenDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (PasswordResetTokenDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (PasswordResetTokenDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        return await command.ExecuteNonQueryAsync(cancellationToken);
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
            _mapper.Where(static (PasswordResetTokenDataModel x) => x.ExpiresAt, RelationalOperator.LessThanOrEqual)
            & _mapper.Where(static (PasswordResetTokenDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (PasswordResetTokenDataModel x) => x.ExpiresAt, referenceDate);
        _mapper.AddParameterForCommand(command, static (PasswordResetTokenDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    // Stryker restore all
}
