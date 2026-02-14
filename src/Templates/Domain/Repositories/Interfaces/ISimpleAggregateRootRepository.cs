using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Templates.Domain.Entities.SimpleAggregateRoots;

namespace Templates.Domain.Repositories.Interfaces;

public interface ISimpleAggregateRootRepository
    : IRepository<SimpleAggregateRoot>
{
}
