using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace Templates.Infra.Data.PostgreSql.Repositories;

public class SimpleAggregateRootPostgreSqlRepository
    : ISimpleAggregateRootPostgreSqlRepository
{
    public Task<bool> EnumerateAllAsync(ExecutionContext executionContext, PaginationInfo paginationInfo, ItemHandler<SimpleAggregateRoot> handler, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EnumerateModifiedSinceAsync(ExecutionContext executionContext, TimeProvider timeProvider, DateTimeOffset since, ItemHandler<SimpleAggregateRoot> handler, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(ExecutionContext executionContext, Id id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<SimpleAggregateRoot?> GetByIdAsync(ExecutionContext executionContext, Id id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RegisterNewAsync(ExecutionContext executionContext, SimpleAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
