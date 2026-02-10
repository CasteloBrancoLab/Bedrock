using Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;

public interface IPostgreSqlConnection
    : IConnection,
    IAsyncDisposable
{
    public NpgsqlConnection? GetConnectionObject();
}
