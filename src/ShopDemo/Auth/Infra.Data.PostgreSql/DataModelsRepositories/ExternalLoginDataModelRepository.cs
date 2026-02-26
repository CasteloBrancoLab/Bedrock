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

public sealed class ExternalLoginDataModelRepository
    : DataModelRepositoryBase<ExternalLoginDataModel>,
      IExternalLoginDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<ExternalLoginDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public ExternalLoginDataModelRepository(
        ILogger<ExternalLoginDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<ExternalLoginDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<ExternalLoginDataModel?> GetByProviderAndProviderUserIdAsync(
        ExecutionContext executionContext,
        string provider,
        string providerUserId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (ExternalLoginDataModel x) => x.Provider)
            & _mapper.Where(static (ExternalLoginDataModel x) => x.ProviderUserId)
            & _mapper.Where(static (ExternalLoginDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.Provider, provider);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.ProviderUserId, providerUserId);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        ExternalLoginDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<ExternalLoginDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (ExternalLoginDataModel x) => x.UserId)
            & _mapper.Where(static (ExternalLoginDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<ExternalLoginDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            ExternalLoginDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<bool> DeleteByUserIdAndProviderAsync(
        ExecutionContext executionContext,
        Guid userId,
        string provider,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (ExternalLoginDataModel x) => x.UserId)
            & _mapper.Where(static (ExternalLoginDataModel x) => x.Provider)
            & _mapper.Where(static (ExternalLoginDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateDeleteCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.Provider, provider);
        _mapper.AddParameterForCommand(command, static (ExternalLoginDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected > 0;
    }
    // Stryker restore all
}
