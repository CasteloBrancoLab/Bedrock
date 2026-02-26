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

public sealed class RoleDataModelRepository
    : DataModelRepositoryBase<RoleDataModel>,
      IRoleDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<RoleDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public RoleDataModelRepository(
        ILogger<RoleDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<RoleDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<RoleDataModel?> GetByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RoleDataModel x) => x.Name)
            & _mapper.Where(static (RoleDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RoleDataModel x) => x.Name, name);
        _mapper.AddParameterForCommand(command, static (RoleDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        RoleDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<bool> ExistsByNameAsync(
        ExecutionContext executionContext,
        string name,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (RoleDataModel x) => x.Name)
            & _mapper.Where(static (RoleDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateExistsCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (RoleDataModel x) => x.Name, name);
        _mapper.AddParameterForCommand(command, static (RoleDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is true;
    }
    // Stryker restore all
}
