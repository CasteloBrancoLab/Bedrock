using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Outbox.PostgreSql;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Outbox;

// Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
[ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
public sealed class AuthOutboxRepository
    : OutboxPostgreSqlRepositoryBase,
    IAuthOutboxRepository
{
    public AuthOutboxRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override void ConfigureInternal(OutboxPostgreSqlOptions options)
    {
        options.WithTableName("auth_outbox");
    }
}
// Stryker restore all
