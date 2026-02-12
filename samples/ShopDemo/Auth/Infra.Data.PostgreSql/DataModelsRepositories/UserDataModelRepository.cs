using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;

public sealed class UserDataModelRepository
    : DataModelRepositoryBase<UserDataModel>,
      IUserDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<UserDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public UserDataModelRepository(
        ILogger<UserDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<UserDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<UserDataModel?> GetByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (UserDataModel x) => x.Email)
            & _mapper.Where(static (UserDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.Email, email);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        UserDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<UserDataModel?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (UserDataModel x) => x.Username)
            & _mapper.Where(static (UserDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.Username, username);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        UserDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        string email,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (UserDataModel x) => x.Email)
            & _mapper.Where(static (UserDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateExistsCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.Email, email);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is true;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (UserDataModel x) => x.Username)
            & _mapper.Where(static (UserDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateExistsCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.Username, username);
        _mapper.AddParameterForCommand(command, static (UserDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is true;
    }
    // Stryker restore all
}
