using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Npgsql;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Mappers;

/// <summary>
/// Data model mapper for TestEntityDataModel.
/// Maps to the test_entities table in the PostgreSQL test database.
/// </summary>
public class TestEntityDataModelMapper : DataModelMapperBase<TestEntityDataModel>
{
    /// <inheritdoc />
    protected override void ConfigureInternal(MapperOptions<TestEntityDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: null, name: "test_entities")
            .MapColumn(x => x.Name);
    }

    /// <inheritdoc />
    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, TestEntityDataModel model)
    {
        // Not used in integration tests
    }
}
