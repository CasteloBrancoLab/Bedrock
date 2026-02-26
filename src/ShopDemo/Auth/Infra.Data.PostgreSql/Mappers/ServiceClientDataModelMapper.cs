using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class ServiceClientDataModelMapper
    : DataModelMapperBase<ServiceClientDataModel>
{
    protected override void ConfigureInternal(MapperOptions<ServiceClientDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_service_clients")
            .MapColumn(static x => x.ClientId)
            .MapColumn(static x => x.ClientSecretHash)
            .MapColumn(static x => x.Name)
            .MapColumn(static x => x.Status)
            .MapColumn(static x => x.CreatedByUserId)
            .MapColumn(static x => x.ExpiresAt)
            .MapColumn(static x => x.RevokedAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, ServiceClientDataModel model)
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

        // ServiceClient-specific columns
        importer.Write(model.ClientId, NpgsqlDbType.Varchar);
        importer.Write(model.ClientSecretHash, NpgsqlDbType.Bytea);
        importer.Write(model.Name, NpgsqlDbType.Varchar);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
        importer.Write(model.CreatedByUserId, NpgsqlDbType.Uuid);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.RevokedAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
