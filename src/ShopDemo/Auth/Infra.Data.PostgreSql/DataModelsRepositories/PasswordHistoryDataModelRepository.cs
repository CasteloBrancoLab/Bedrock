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

public sealed class PasswordHistoryDataModelRepository
    : DataModelRepositoryBase<PasswordHistoryDataModel>,
      IPasswordHistoryDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<PasswordHistoryDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public PasswordHistoryDataModelRepository(
        ILogger<PasswordHistoryDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<PasswordHistoryDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<PasswordHistoryDataModel>> GetLatestByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        int count,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (PasswordHistoryDataModel x) => x.UserId)
            & _mapper.Where(static (PasswordHistoryDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause)
            + $" ORDER BY changed_at DESC LIMIT {count}";

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (PasswordHistoryDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (PasswordHistoryDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<PasswordHistoryDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            PasswordHistoryDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all
}
