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

public sealed class ImpersonationSessionDataModelRepository
    : DataModelRepositoryBase<ImpersonationSessionDataModel>,
      IImpersonationSessionDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<ImpersonationSessionDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public ImpersonationSessionDataModelRepository(
        ILogger<ImpersonationSessionDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<ImpersonationSessionDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<ImpersonationSessionDataModel?> GetActiveByOperatorUserIdAsync(
        ExecutionContext executionContext,
        Guid operatorUserId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (ImpersonationSessionDataModel x) => x.OperatorUserId)
            & _mapper.Where(static (ImpersonationSessionDataModel x) => x.Status)
            & _mapper.Where(static (ImpersonationSessionDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (ImpersonationSessionDataModel x) => x.OperatorUserId, operatorUserId);
        _mapper.AddParameterForCommand(command, static (ImpersonationSessionDataModel x) => x.Status, (short)1); // Active = 1
        _mapper.AddParameterForCommand(command, static (ImpersonationSessionDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        ImpersonationSessionDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<ImpersonationSessionDataModel?> GetActiveByTargetUserIdAsync(
        ExecutionContext executionContext,
        Guid targetUserId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (ImpersonationSessionDataModel x) => x.TargetUserId)
            & _mapper.Where(static (ImpersonationSessionDataModel x) => x.Status)
            & _mapper.Where(static (ImpersonationSessionDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (ImpersonationSessionDataModel x) => x.TargetUserId, targetUserId);
        _mapper.AddParameterForCommand(command, static (ImpersonationSessionDataModel x) => x.Status, (short)1); // Active = 1
        _mapper.AddParameterForCommand(command, static (ImpersonationSessionDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        ImpersonationSessionDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all
}
