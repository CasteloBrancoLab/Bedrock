using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class SigningKeyDataModelMapper
    : DataModelMapperBase<SigningKeyDataModel>
{
    protected override void ConfigureInternal(MapperOptions<SigningKeyDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_signing_keys")
            .MapColumn(static x => x.Kid)
            .MapColumn(static x => x.Algorithm)
            .MapColumn(static x => x.PublicKey)
            .MapColumn(static x => x.EncryptedPrivateKey)
            .MapColumn(static x => x.Status)
            .MapColumn(static x => x.RotatedAt)
            .MapColumn(static x => x.ExpiresAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, SigningKeyDataModel model)
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

        // SigningKey-specific columns
        importer.Write(model.Kid, NpgsqlDbType.Varchar);
        importer.Write(model.Algorithm, NpgsqlDbType.Varchar);
        importer.Write(model.PublicKey, NpgsqlDbType.Varchar);
        importer.Write(model.EncryptedPrivateKey, NpgsqlDbType.Varchar);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
        importer.Write(model.RotatedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
