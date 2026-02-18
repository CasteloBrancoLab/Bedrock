using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;

public sealed class ExternalLoginDataModelMapper
    : DataModelMapperBase<ExternalLoginDataModel>
{
    protected override void ConfigureInternal(MapperOptions<ExternalLoginDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_external_logins")
            .MapColumn(static x => x.UserId)
            .MapColumn(static x => x.Provider)
            .MapColumn(static x => x.ProviderUserId)
            .MapColumn(static x => x.Email);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, ExternalLoginDataModel model)
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

        // ExternalLogin-specific columns
        importer.Write(model.UserId, NpgsqlDbType.Uuid);
        importer.Write(model.Provider, NpgsqlDbType.Varchar);
        importer.Write(model.ProviderUserId, NpgsqlDbType.Varchar);
        importer.Write(model.Email, NpgsqlDbType.Varchar);
    }
    // Stryker restore all
}
