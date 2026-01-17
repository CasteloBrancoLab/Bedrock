using Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;

public interface IPostgreSqlUnitOfWork
    : IUnitOfWork
{
    public NpgsqlConnection? GetCurrentConnection();
    public NpgsqlTransaction? GetCurrentTransaction();
    public NpgsqlCommand CreateNpgsqlCommand(string commandText);
}
