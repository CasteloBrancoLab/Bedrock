using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork;

public sealed class AuthPostgreSqlUnitOfWork
    : PostgreSqlUnitOfWorkBase,
    IAuthPostgreSqlUnitOfWork
{
    // Constants
    private const string UnitOfWorkName = "AuthPostgreSqlUnitOfWork";

    // Constructors
    public AuthPostgreSqlUnitOfWork(
        ILogger<AuthPostgreSqlUnitOfWork> logger,
        IAuthPostgreSqlConnection postgreSqlConnection
    ) : base(
        logger,
        UnitOfWorkName,
        postgreSqlConnection
    )
    {
    }
}
