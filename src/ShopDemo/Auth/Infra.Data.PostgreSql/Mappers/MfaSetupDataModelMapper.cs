using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class MfaSetupDataModelMapper
    : DataModelMapperBase<MfaSetupDataModel>
{
    protected override void ConfigureInternal(MapperOptions<MfaSetupDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_mfa_setups")
            .MapColumn(static x => x.UserId)
            .MapColumn(static x => x.EncryptedSharedSecret)
            .MapColumn(static x => x.IsEnabled)
            .MapColumn(static x => x.EnabledAt);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, MfaSetupDataModel model)
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

        // MfaSetup-specific columns
        importer.Write(model.UserId, NpgsqlDbType.Uuid);
        importer.Write(model.EncryptedSharedSecret, NpgsqlDbType.Varchar);
        importer.Write(model.IsEnabled, NpgsqlDbType.Boolean);
        importer.Write(model.EnabledAt, NpgsqlDbType.TimestampTz);
    }
    // Stryker restore all
}
