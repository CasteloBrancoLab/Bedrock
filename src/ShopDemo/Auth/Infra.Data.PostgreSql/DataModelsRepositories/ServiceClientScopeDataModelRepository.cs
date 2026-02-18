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

public sealed class ServiceClientScopeDataModelRepository
    : DataModelRepositoryBase<ServiceClientScopeDataModel>,
      IServiceClientScopeDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<ServiceClientScopeDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public ServiceClientScopeDataModelRepository(
        ILogger<ServiceClientScopeDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<ServiceClientScopeDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<ServiceClientScopeDataModel>> GetByServiceClientIdAsync(
        ExecutionContext executionContext,
        Guid serviceClientId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (ServiceClientScopeDataModel x) => x.ServiceClientId)
            & _mapper.Where(static (ServiceClientScopeDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (ServiceClientScopeDataModel x) => x.ServiceClientId, serviceClientId);
        _mapper.AddParameterForCommand(command, static (ServiceClientScopeDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<ServiceClientScopeDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            ServiceClientScopeDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<bool> DeleteByServiceClientIdAsync(
        ExecutionContext executionContext,
        Guid serviceClientId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (ServiceClientScopeDataModel x) => x.ServiceClientId)
            & _mapper.Where(static (ServiceClientScopeDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (ServiceClientScopeDataModel x) => x.ServiceClientId, serviceClientId);
        _mapper.AddParameterForCommand(command, static (ServiceClientScopeDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        int result = await command.ExecuteNonQueryAsync(cancellationToken);

        return result > 0;
    }
    // Stryker restore all
}
