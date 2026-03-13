using Bedrock.BuildingBlocks.Messages;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Outbox;

/// <summary>
/// Facade que implementa <see cref="IAuthOutboxWriter"/> e delega
/// para <see cref="IAuthOutboxPostgreSqlWriter"/>.
/// </summary>
public sealed class AuthOutboxWriter : IAuthOutboxWriter
{
    private readonly IAuthOutboxPostgreSqlWriter _postgreSqlWriter;

    public AuthOutboxWriter(IAuthOutboxPostgreSqlWriter postgreSqlWriter)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlWriter);
        _postgreSqlWriter = postgreSqlWriter;
    }

    /// <inheritdoc />
    public Task EnqueueAsync(MessageBase payload, CancellationToken cancellationToken)
        => _postgreSqlWriter.EnqueueAsync(payload, cancellationToken);
}
