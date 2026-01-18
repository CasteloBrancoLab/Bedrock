using Bedrock.BuildingBlocks.Domain.Repositories;
using Templates.Domain.Entities.SimpleAggregateRoots;

namespace Templates.Domain.Repositories;

public interface ISimpleAggregateRootRepository
    : IRepository<SimpleAggregateRoot>
{
}
