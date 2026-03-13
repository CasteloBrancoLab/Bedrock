using Bedrock.BuildingBlocks.Outbox.Models;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;

namespace ShopDemo.Auth.Infra.Data.Outbox;

/// <summary>
/// Facade que implementa <see cref="IAuthOutboxRepository"/> e delega
/// para <see cref="IAuthOutboxPostgreSqlRepository"/>.
/// </summary>
public sealed class AuthOutboxRepository : IAuthOutboxRepository
{
    private readonly IAuthOutboxPostgreSqlRepository _postgreSqlRepository;

    public AuthOutboxRepository(IAuthOutboxPostgreSqlRepository postgreSqlRepository)
    {
        ArgumentNullException.ThrowIfNull(postgreSqlRepository);
        _postgreSqlRepository = postgreSqlRepository;
    }

    /// <inheritdoc />
    public Task AddAsync(OutboxEntry entry, CancellationToken cancellationToken)
        => _postgreSqlRepository.AddAsync(entry, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<OutboxEntry>> ClaimNextBatchAsync(
        int batchSize, TimeSpan processingTimeout, CancellationToken cancellationToken)
        => _postgreSqlRepository.ClaimNextBatchAsync(batchSize, processingTimeout, cancellationToken);

    /// <inheritdoc />
    public Task MarkAsSentAsync(Guid entryId, CancellationToken cancellationToken)
        => _postgreSqlRepository.MarkAsSentAsync(entryId, cancellationToken);

    /// <inheritdoc />
    public Task MarkAsFailedAsync(Guid entryId, CancellationToken cancellationToken)
        => _postgreSqlRepository.MarkAsFailedAsync(entryId, cancellationToken);
}
