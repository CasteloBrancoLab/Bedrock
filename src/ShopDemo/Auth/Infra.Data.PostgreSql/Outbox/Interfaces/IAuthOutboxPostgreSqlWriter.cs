using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;

public interface IAuthOutboxPostgreSqlWriter
    : IAuthOutboxWriter
{
}
