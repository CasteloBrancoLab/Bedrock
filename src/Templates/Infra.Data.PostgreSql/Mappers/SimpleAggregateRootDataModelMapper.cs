using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;
using Templates.Infra.Data.PostgreSql.DataModels;

namespace Templates.Infra.Data.PostgreSql.Mappers;

public sealed class SimpleAggregateRootDataModelMapper
    : DataModelMapperBase<SimpleAggregateRootDataModel>
{
    protected override void ConfigureInternal(MapperOptions<SimpleAggregateRootDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "simple_aggregate_roots")
            .MapColumn(x => x.FirstName)
            .MapColumn(x => x.LastName)
            .MapColumn(x => x.FullName)
            .MapColumn(x => x.BirthDate);
    }

    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, SimpleAggregateRootDataModel model)
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

        // SimpleAggregateRoot specific columns
        importer.Write(model.FirstName, NpgsqlDbType.Varchar);
        importer.Write(model.LastName, NpgsqlDbType.Varchar);
        importer.Write(model.FullName, NpgsqlDbType.Varchar);
        importer.Write(model.BirthDate, NpgsqlDbType.TimestampTz);
    }
}
