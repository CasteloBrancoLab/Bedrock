using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class LoginAttemptDataModelMapper
    : DataModelMapperBase<LoginAttemptDataModel>
{
    protected override void ConfigureInternal(MapperOptions<LoginAttemptDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_login_attempts")
            .MapColumn(static x => x.Username)
            .MapColumn(static x => x.IpAddress)
            .MapColumn(static x => x.AttemptedAt)
            .MapColumn(static x => x.IsSuccessful)
            .MapColumn(static x => x.FailureReason);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, LoginAttemptDataModel model)
    {
        // DataModelBase columns
        importer.Write(model.Id, NpgsqlDbType.Uuid);
        importer.Write(model.TenantCode, NpgsqlDbType.Uuid);
        importer.Write(model.CreatedBy, NpgsqlDbType.Varchar);
        importer.Write(model.CreatedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.CreatedCorrelationId, NpgsqlDbType.Uuid);
        importer.Write(model.CreatedExecutionOrigin, NpgsqlDbType.Varchar);
        importer.Write(model.CreatedBusinessOperationCode, NpgsqlDbType.Varchar);
        importer.Write(model.LastChangedBy, NpgsqlDbType.Varchar);
        importer.Write(model.LastChangedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.LastChangedExecutionOrigin, NpgsqlDbType.Varchar);
        importer.Write(model.LastChangedCorrelationId, NpgsqlDbType.Uuid);
        importer.Write(model.LastChangedBusinessOperationCode, NpgsqlDbType.Varchar);
        importer.Write(model.EntityVersion, NpgsqlDbType.Bigint);

        // LoginAttempt-specific columns
        importer.Write(model.Username, NpgsqlDbType.Varchar);
        importer.Write(model.IpAddress, NpgsqlDbType.Varchar);
        importer.Write(model.AttemptedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.IsSuccessful, NpgsqlDbType.Boolean);
        importer.Write(model.FailureReason, NpgsqlDbType.Varchar);
    }
    // Stryker restore all
}
