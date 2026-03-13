using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Outbox.Messages;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Outbox;

/// <summary>
/// Outbox writer do Auth BC. Compoe <see cref="MessageOutboxWriter"/> internamente
/// e recebe <see cref="IAuthOutboxPostgreSqlRepository"/> (marker do BC) em vez da interface
/// generica <see cref="IOutboxRepository"/>.
/// </summary>
public sealed class AuthOutboxPostgreSqlWriter : IAuthOutboxPostgreSqlWriter
{
    private readonly MessageOutboxWriter _writer;

    public AuthOutboxPostgreSqlWriter(
        IAuthOutboxPostgreSqlRepository repository,
        IOutboxSerializer<MessageBase> serializer,
        TimeProvider timeProvider)
    {
        _writer = new MessageOutboxWriter(repository, serializer, timeProvider);
    }

    /// <inheritdoc />
    public Task EnqueueAsync(MessageBase payload, CancellationToken cancellationToken)
        => _writer.EnqueueAsync(payload, cancellationToken);
}
