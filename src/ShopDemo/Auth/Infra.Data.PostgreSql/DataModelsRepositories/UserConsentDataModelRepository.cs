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

public sealed class UserConsentDataModelRepository
    : DataModelRepositoryBase<UserConsentDataModel>,
      IUserConsentDataModelRepository
{
    private readonly IAuthPostgreSqlUnitOfWork _unitOfWork;
    private readonly IDataModelMapper<UserConsentDataModel> _mapper;

    // Stryker disable once Block : Construtor delega para base class e armazena campos privados - testado indiretamente
    public UserConsentDataModelRepository(
        ILogger<UserConsentDataModelRepository> logger,
        IAuthPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<UserConsentDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<IReadOnlyList<UserConsentDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (UserConsentDataModel x) => x.UserId)
            & _mapper.Where(static (UserConsentDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (UserConsentDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (UserConsentDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<UserConsentDataModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            UserConsentDataModel dataModel = new();
            _mapper.PopulateDataModelBaseFromReader(reader, dataModel);
            results.Add(dataModel);
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    public async Task<UserConsentDataModel?> GetActiveByUserIdAndConsentTermIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        Guid consentTermId,
        CancellationToken cancellationToken)
    {
        WhereClause whereClause =
            _mapper.Where(static (UserConsentDataModel x) => x.UserId)
            & _mapper.Where(static (UserConsentDataModel x) => x.ConsentTermId)
            & _mapper.Where(static (UserConsentDataModel x) => x.Status)
            & _mapper.Where(static (UserConsentDataModel x) => x.TenantCode);

        string sql = _mapper.GenerateSelectCommand(whereClause);

        await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
        _mapper.AddParameterForCommand(command, static (UserConsentDataModel x) => x.UserId, userId);
        _mapper.AddParameterForCommand(command, static (UserConsentDataModel x) => x.ConsentTermId, consentTermId);
        _mapper.AddParameterForCommand(command, static (UserConsentDataModel x) => x.Status, (short)1); // Active = 1
        _mapper.AddParameterForCommand(command, static (UserConsentDataModel x) => x.TenantCode, executionContext.TenantInfo.Code);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        UserConsentDataModel dataModel = new();
        _mapper.PopulateDataModelBaseFromReader(reader, dataModel);

        return dataModel;
    }
    // Stryker restore all
}
