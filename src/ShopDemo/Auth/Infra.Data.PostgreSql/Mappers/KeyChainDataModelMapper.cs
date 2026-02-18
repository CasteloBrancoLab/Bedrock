using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class KeyChainDataModelMapper
    : DataModelMapperBase<KeyChainDataModel>
{
    protected override void ConfigureInternal(MapperOptions<KeyChainDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_key_chains")
            .MapColumn(static x => x.UserId)
            .MapColumn(static x => x.KeyId)
            .MapColumn(static x => x.PublicKey)
            .MapColumn(static x => x.EncryptedSharedSecret)
            .MapColumn(static x => x.Status)
            .MapColumn(static x => x.ExpiresAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, KeyChainDataModel model)
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

        // KeyChain-specific columns
        importer.Write(model.UserId, NpgsqlDbType.Uuid);
        importer.Write(model.KeyId, NpgsqlDbType.Varchar);
        importer.Write(model.PublicKey, NpgsqlDbType.Varchar);
        importer.Write(model.EncryptedSharedSecret, NpgsqlDbType.Varchar);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
