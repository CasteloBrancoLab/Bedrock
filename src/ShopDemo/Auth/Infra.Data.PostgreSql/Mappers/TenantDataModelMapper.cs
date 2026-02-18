using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class TenantDataModelMapper
    : DataModelMapperBase<TenantDataModel>
{
    protected override void ConfigureInternal(MapperOptions<TenantDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "tenant_lookup")
            .MapColumn(static x => x.Name)
            .MapColumn(static x => x.Domain)
            .MapColumn(static x => x.SchemaName)
            .MapColumn(static x => x.Status)
            .MapColumn(static x => x.Tier)
            .MapColumn(static x => x.DbVersion);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, TenantDataModel model)
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

        // Tenant-specific columns
        importer.Write(model.Name, NpgsqlDbType.Varchar);
        importer.Write(model.Domain, NpgsqlDbType.Varchar);
        importer.Write(model.SchemaName, NpgsqlDbType.Varchar);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
        importer.Write(model.Tier, NpgsqlDbType.Smallint);
        importer.Write(model.DbVersion, NpgsqlDbType.Varchar);
    }
    // Stryker restore all
}
