using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class PasswordResetTokenDataModelMapper
    : DataModelMapperBase<PasswordResetTokenDataModel>
{
    protected override void ConfigureInternal(MapperOptions<PasswordResetTokenDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_password_reset_tokens")
            .MapColumn(static x => x.UserId)
            .MapColumn(static x => x.TokenHash)
            .MapColumn(static x => x.ExpiresAt)
            .MapColumn(static x => x.IsUsed)
            .MapColumn(static x => x.UsedAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, PasswordResetTokenDataModel model)
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

        // PasswordResetToken-specific columns
        importer.Write(model.UserId, NpgsqlDbType.Uuid);
        importer.Write(model.TokenHash, NpgsqlDbType.Varchar);
        importer.Write(model.ExpiresAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.IsUsed, NpgsqlDbType.Boolean);
        importer.Write(model.UsedAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
