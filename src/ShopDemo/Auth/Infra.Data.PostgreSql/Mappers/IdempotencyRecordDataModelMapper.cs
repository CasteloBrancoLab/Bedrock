using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class IdempotencyRecordDataModelMapper
    : DataModelMapperBase<IdempotencyRecordDataModel>
{
    protected override void ConfigureInternal(MapperOptions<IdempotencyRecordDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_idempotency_records")
            .MapColumn(static x => x.IdempotencyKey)
            .MapColumn(static x => x.RequestHash)
            .MapColumn(static x => x.ResponseBody)
            .MapColumn(static x => x.StatusCode)
            .MapColumn(static x => x.ExpiresAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, IdempotencyRecordDataModel model)
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

        // IdempotencyRecord-specific columns
        importer.Write(model.IdempotencyKey, NpgsqlDbType.Varchar);
        importer.Write(model.RequestHash, NpgsqlDbType.Varchar);
        importer.Write(model.ResponseBody, NpgsqlDbType.Varchar);
        importer.Write(model.StatusCode, NpgsqlDbType.Integer);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
