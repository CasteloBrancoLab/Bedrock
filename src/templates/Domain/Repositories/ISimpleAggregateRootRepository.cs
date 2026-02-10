using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Templates.Domain.Entities.SimpleAggregateRoots;

namespace Templates.Domain.Repositories;

public interface ISimpleAggregateRootRepository
    : IRepository<SimpleAggregateRoot>
{
}
