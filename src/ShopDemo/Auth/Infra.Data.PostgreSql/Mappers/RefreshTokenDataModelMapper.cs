using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class RefreshTokenDataModelMapper
    : DataModelMapperBase<RefreshTokenDataModel>
{
    protected override void ConfigureInternal(MapperOptions<RefreshTokenDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_refresh_tokens")
            .MapColumn(static x => x.UserId)
            .MapColumn(static x => x.TokenHash)
            .MapColumn(static x => x.FamilyId)
            .MapColumn(static x => x.ExpiresAt)
            .MapColumn(static x => x.Status)
            .MapColumn(static x => x.RevokedAt)
            .MapColumn(static x => x.ReplacedByTokenId);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, RefreshTokenDataModel model)
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

        // RefreshToken-specific columns
        importer.Write(model.UserId, NpgsqlDbType.Uuid);
        importer.Write(model.TokenHash, NpgsqlDbType.Bytea);
        importer.Write(model.FamilyId, NpgsqlDbType.Uuid);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
        importer.Write(model.RevokedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.ReplacedByTokenId, NpgsqlDbType.Uuid);
    }
    // Stryker restore all
}
