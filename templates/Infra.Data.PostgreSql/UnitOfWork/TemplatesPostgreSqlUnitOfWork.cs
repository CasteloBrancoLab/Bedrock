using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;
using Microsoft.Extensions.Logging;
using Templates.Infra.Data.PostgreSql.Connections.Interfaces;
using Templates.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace Templates.Infra.Data.PostgreSql.UnitOfWork;

public sealed class TemplatesPostgreSqlUnitOfWork
    : PostgreSqlUnitOfWorkBase,
    ITemplatesPostgreSqlUnitOfWork
{
    // Constants
    private const string UnitOfWorkName = "TemplatesPostgreSqlUnitOfWork";

    // Constructors
    public TemplatesPostgreSqlUnitOfWork(
        ILogger<TemplatesPostgreSqlUnitOfWork> logger,
        ITemplatesPostgreSqlConnection postgreSqlConnection
    ) : base(
        logger,
        UnitOfWorkName,
        postgreSqlConnection
    )
    {
    }
}
