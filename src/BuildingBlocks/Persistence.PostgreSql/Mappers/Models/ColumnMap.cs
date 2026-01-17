using Bedrock.BuildingBlocks.Persistence.PostgreSql.Utils;
using NpgsqlTypes;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

public readonly struct ColumnMap
{
    // Properties
    public string ColumnName { get; }
    public Type Type { get; }
    public NpgsqlDbType NpgsqlDbType { get; }

    // Constructors
    private ColumnMap(string columnName, Type type, NpgsqlDbType npgsqlDbType)
    {
        ColumnName = columnName;
        Type = type;
        NpgsqlDbType = npgsqlDbType;
    }

    // Builders
    public static ColumnMap Create(string columnName, Type type)
    {
        return new ColumnMap(columnName, type, NpgsqlDbTypeUtils.MapToNpgsqlDbType(type));
    }
    public static ColumnMap Create(string columnName, Type type, NpgsqlDbType npgsqlDbType)
    {
        return new ColumnMap(columnName, type, npgsqlDbType);
    }
}
