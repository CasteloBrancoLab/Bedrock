using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class DenyListEntryDataModelMapper
    : DataModelMapperBase<DenyListEntryDataModel>
{
    protected override void ConfigureInternal(MapperOptions<DenyListEntryDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_deny_list_entries")
            .MapColumn(static x => x.Type)
            .MapColumn(static x => x.Value)
            .MapColumn(static x => x.ExpiresAt)
            .MapColumn(static x => x.Reason);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, DenyListEntryDataModel model)
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

        // DenyListEntry-specific columns
        importer.Write(model.Type, NpgsqlDbType.Smallint);
        importer.Write(model.Value, NpgsqlDbType.Varchar);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.Reason, NpgsqlDbType.Varchar);
    }
    // Stryker restore all
}
