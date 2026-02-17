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

public sealed class RefreshTokenDataModelRepository
    : DataModelRepositoryBase<RefreshTokenDataModel>,
      IRefreshTokenDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<RefreshTokenDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public RefreshTokenDataModelRepository(
        ILogger<RefreshTokenDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<RefreshTokenDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<RefreshTokenDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RefreshTokenDataModel x) => x.UserId)
            & _mapper.Where(static (RefreshTokenDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RefreshTokenDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (RefreshTokenDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<RefreshTokenDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            RefreshTokenDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<RefreshTokenDataModel?> GetByTokenHashAsync(
        ExecutionContext executionContext,
        byte[] tokenHash,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RefreshTokenDataModel x) => x.TokenHash)
            & _mapper.Where(static (RefreshTokenDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RefreshTokenDataModel x) => x.TokenHash, tokenHash);
        _mapper.AddParameterForCommand(command, static (RefreshTokenDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        RefreshTokenDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<RefreshTokenDataModel>> GetActiveByFamilyIdAsync(
        ExecutionContext executionContext,
        Guid familyId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RefreshTokenDataModel x) => x.FamilyId)
            & _mapper.Where(static (RefreshTokenDataModel x) => x.Status)
            & _mapper.Where(static (RefreshTokenDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RefreshTokenDataModel x) => x.FamilyId, familyId);
        _mapper.AddParameterForCommand(command, static (RefreshTokenDataModel x) => x.Status, (short)1); // Active = 1
        _mapper.AddParameterForCommand(command, static (RefreshTokenDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<RefreshTokenDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            RefreshTokenDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all
}
