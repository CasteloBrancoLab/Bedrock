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

public sealed class LoginAttemptDataModelRepository
    : DataModelRepositoryBase<LoginAttemptDataModel>,
      ILoginAttemptDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<LoginAttemptDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public LoginAttemptDataModelRepository(
        ILogger<LoginAttemptDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<LoginAttemptDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<LoginAttemptDataModel>> GetRecentByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (LoginAttemptDataModel x) => x.Username)
            & _mapper.Where(static (LoginAttemptDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause)
            + " AND attempted_at >= @attempted_at";

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (LoginAttemptDataModel x) => x.Username, username);
        _mapper.AddParameterForCommand(command, static (LoginAttemptDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);
        _mapper.AddParameterForCommand(command, static (LoginAttemptDataModel x) => x.AttemptedAt, since);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<LoginAttemptDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            LoginAttemptDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<LoginAttemptDataModel>> GetRecentByIpAddressAsync(
        ExecutionContext executionContext,
        string ipAddress,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (LoginAttemptDataModel x) => x.IpAddress)
            & _mapper.Where(static (LoginAttemptDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause)
            + " AND attempted_at >= @attempted_at";

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (LoginAttemptDataModel x) => x.IpAddress, ipAddress);
        _mapper.AddParameterForCommand(command, static (LoginAttemptDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);
        _mapper.AddParameterForCommand(command, static (LoginAttemptDataModel x) => x.AttemptedAt, since);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<LoginAttemptDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            LoginAttemptDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all
}
