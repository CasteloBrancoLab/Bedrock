using System.Collections.Concurrent;
using NpgsqlTypes;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Utils;

public static class NpgsqlDbTypeUtils
{
    // Cache of known mappings for quick lookup
    private static readonly ConcurrentDictionary<Type, NpgsqlDbType> _cache = new()
    {
        [typeof(Guid)] = NpgsqlDbType.Uuid,
        [typeof(string)] = NpgsqlDbType.Varchar,
        [typeof(DateTimeOffset)] = NpgsqlDbType.TimestampTz,
        [typeof(DateTime)] = NpgsqlDbType.Timestamp,
        [typeof(bool)] = NpgsqlDbType.Boolean,
        [typeof(long)] = NpgsqlDbType.Bigint,
        [typeof(int)] = NpgsqlDbType.Integer,
        [typeof(short)] = NpgsqlDbType.Smallint,
        [typeof(double)] = NpgsqlDbType.Double,
        [typeof(float)] = NpgsqlDbType.Real,
        [typeof(decimal)] = NpgsqlDbType.Numeric,
    };

    /// <summary>
    /// Maps a CLR <paramref name="type"/> to the equivalent <see cref="NpgsqlDbType"/>.
    /// Nullable types are resolved to their underlying type. Throws an exception
    /// for unsupported mappings.
    /// </summary>
    public static NpgsqlDbType MapToNpgsqlDbType(Type type)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (_cache.TryGetValue(underlying, out NpgsqlDbType dbType))
        {
            return dbType;
        }

        throw new ArgumentOutOfRangeException(nameof(type), type, "Type not supported");
    }
}
