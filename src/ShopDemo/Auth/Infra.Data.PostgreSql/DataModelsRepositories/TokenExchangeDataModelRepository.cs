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

public sealed class TokenExchangeDataModelRepository
    : DataModelRepositoryBase<TokenExchangeDataModel>,
      ITokenExchangeDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<TokenExchangeDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public TokenExchangeDataModelRepository(
        ILogger<TokenExchangeDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<TokenExchangeDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<TokenExchangeDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (TokenExchangeDataModel x) => x.UserId)
            & _mapper.Where(static (TokenExchangeDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (TokenExchangeDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (TokenExchangeDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<TokenExchangeDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            TokenExchangeDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<TokenExchangeDataModel?> GetByIssuedTokenJtiAsync(
        ExecutionContext executionContext,
        string issuedTokenJti,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (TokenExchangeDataModel x) => x.IssuedTokenJti)
            & _mapper.Where(static (TokenExchangeDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (TokenExchangeDataModel x) => x.IssuedTokenJti, issuedTokenJti);
        _mapper.AddParameterForCommand(command, static (TokenExchangeDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        TokenExchangeDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all
}
