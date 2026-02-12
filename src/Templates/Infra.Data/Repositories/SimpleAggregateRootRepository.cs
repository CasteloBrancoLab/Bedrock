using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Data.Repositories;
using Microsoft.Extensions.Logging;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Domain.Repositories.Interfaces;

namespace Templates.Infra.Data.Repositories;

public class SimpleAggregateRootRepository
    : RepositoryBase<SimpleAggregateRoot>,
    ISimpleAggregateRootRepository
{
    public SimpleAggregateRootRepository(
        ILogger<SimpleAggregateRootRepository> logger
    ) : base(logger)
    {
    }

    protected override Task<bool> ExistsInternalAsync(ExecutionContext executionContext, Id id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override IAsyncEnumerable<SimpleAggregateRoot> GetAllInternalAsync(PaginationInfo paginationInfo, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<SimpleAggregateRoot?> GetByIdInternalAsync(ExecutionContext executionContext, Id id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override IAsyncEnumerable<SimpleAggregateRoot> GetModifiedSinceInternalAsync(ExecutionContext executionContext, TimeProvider timeProvider, DateTimeOffset since, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> RegisterNewInternalAsync(ExecutionContext executionContext, SimpleAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
