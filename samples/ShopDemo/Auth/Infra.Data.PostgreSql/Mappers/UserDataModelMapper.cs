using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using ShopDemo.Auth.Infra.Persistence.DataModels;

namespace ShopDemo.Auth.Infra.Persistence.Mappers;

public sealed class UserDataModelMapper
    : DataModelMapperBase<UserDataModel>
{
    protected override void ConfigureInternal(MapperOptions<UserDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_users")
            .MapColumn(static x => x.Username)
            .MapColumn(static x => x.Email)
            .MapColumn(static x => x.PasswordHash)
            .MapColumn(static x => x.Status);
    }

    // Stryker disable all : Requer NpgsqlBinaryImporter real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer NpgsqlBinaryImporter real - coberto por testes de integracao")]
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, UserDataModel model)
    {
        // DataModelBase columns
        importer.Write(model.Id, NpgsqlDbType.Uuid);
        importer.Write(model.TenantCode, NpgsqlDbType.Uuid);
        importer.Write(model.CreatedBy, NpgsqlDbType.Varchar);
        importer.Write(model.CreatedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.LastChangedBy, NpgsqlDbType.Varchar);
        importer.Write(model.LastChangedAt, NpgsqlDbType.TimestampTz);
        importer.Write(model.LastChangedExecutionOrigin, NpgsqlDbType.Varchar);
        importer.Write(model.LastChangedCorrelationId, NpgsqlDbType.Uuid);
        importer.Write(model.LastChangedBusinessOperationCode, NpgsqlDbType.Varchar);
        importer.Write(model.EntityVersion, NpgsqlDbType.Bigint);

        // User-specific columns
        importer.Write(model.Username, NpgsqlDbType.Varchar);
        importer.Write(model.Email, NpgsqlDbType.Varchar);
        importer.Write(model.PasswordHash, NpgsqlDbType.Bytea);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
    }
    // Stryker restore all
}
