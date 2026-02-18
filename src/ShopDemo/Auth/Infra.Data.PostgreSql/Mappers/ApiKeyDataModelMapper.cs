using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class ApiKeyDataModelMapper
    : DataModelMapperBase<ApiKeyDataModel>
{
    protected override void ConfigureInternal(MapperOptions<ApiKeyDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_api_keys")
            .MapColumn(static x => x.ServiceClientId)
            .MapColumn(static x => x.KeyPrefix)
            .MapColumn(static x => x.KeyHash)
            .MapColumn(static x => x.Status)
            .MapColumn(static x => x.ExpiresAt)
            .MapColumn(static x => x.LastUsedAt)
            .MapColumn(static x => x.RevokedAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, ApiKeyDataModel model)
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

        // ApiKey-specific columns
        importer.Write(model.ServiceClientId, NpgsqlDbType.Uuid);
        importer.Write(model.KeyPrefix, NpgsqlDbType.Varchar);
        importer.Write(model.KeyHash, NpgsqlDbType.Varchar);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.LastUsedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.RevokedAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
