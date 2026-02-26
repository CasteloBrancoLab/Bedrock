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

public sealed class RoleClaimDataModelRepository
    : DataModelRepositoryBase<RoleClaimDataModel>,
      IRoleClaimDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<RoleClaimDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public RoleClaimDataModelRepository(
        ILogger<RoleClaimDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<RoleClaimDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<RoleClaimDataModel>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RoleClaimDataModel x) => x.RoleId)
            & _mapper.Where(static (RoleClaimDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RoleClaimDataModel x) => x.RoleId, roleId);
        _mapper.AddParameterForCommand(command, static (RoleClaimDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<RoleClaimDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            RoleClaimDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<RoleClaimDataModel?> GetByRoleIdAndClaimIdAsync(
        ExecutionContext executionContext,
        Guid roleId,
        Guid claimId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RoleClaimDataModel x) => x.RoleId)
            & _mapper.Where(static (RoleClaimDataModel x) => x.ClaimId)
            & _mapper.Where(static (RoleClaimDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RoleClaimDataModel x) => x.RoleId, roleId);
        _mapper.AddParameterForCommand(command, static (RoleClaimDataModel x) => x.ClaimId, claimId);
        _mapper.AddParameterForCommand(command, static (RoleClaimDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        RoleClaimDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all
}
