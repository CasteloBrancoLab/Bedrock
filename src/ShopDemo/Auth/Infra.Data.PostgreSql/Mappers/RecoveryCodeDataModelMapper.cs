using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class RecoveryCodeDataModelMapper
    : DataModelMapperBase<RecoveryCodeDataModel>
{
    protected override void ConfigureInternal(MapperOptions<RecoveryCodeDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_recovery_codes")
            .MapColumn(static x => x.UserId)
            .MapColumn(static x => x.CodeHash)
            .MapColumn(static x => x.IsUsed)
            .MapColumn(static x => x.UsedAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, RecoveryCodeDataModel model)
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

        // RecoveryCode-specific columns
        importer.Write(model.UserId, NpgsqlDbType.Uuid);
        importer.Write(model.CodeHash, NpgsqlDbType.Varchar);
        importer.Write(model.IsUsed, NpgsqlDbType.Boolean);
        importer.Write(model.UsedAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
